using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_ModeChoice
    {
        public string Id;
        public string Name;
        public string Description;

        public MutationMeddley_ModeChoice(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }

    [Serializable]
    public abstract class MutationMeddley_AdaptiveMutationBase : MutationMeddley_EvolvingMutationBase
    {
        public Guid MutationMeddley_ModeAbilityID = Guid.Empty;

        protected virtual string MutationMeddley_ModeAbilityClass
        {
            get { return "Physical Mutation"; }
        }

        protected abstract string MutationMeddley_ModeAbilityName { get; }
        protected abstract string MutationMeddley_ModeAbilityDescription { get; }

        protected abstract List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices();

        protected abstract void MutationMeddley_RefreshPassiveEffects();

        protected virtual string MutationMeddley_GetModeCommand()
        {
            return "MutationMeddley_Mode_" + GetType().Name;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, MutationMeddley_GetModeCommand());
            base.Register(Object);
            MutationMeddley_OnEvolutionStateChanged();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == MutationMeddley_GetModeCommand())
            {
                MutationMeddley_ShowModePicker();
                return false;
            }

            return base.FireEvent(E);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            MutationMeddley_AddModeAbility();
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            MutationMeddley_ClearCommonStatShifts();
            RemoveMyActivatedAbility(ref MutationMeddley_ModeAbilityID);
            return base.Unmutate(GO);
        }

        protected override void MutationMeddley_OnEvolutionStateChanged()
        {
            MutationMeddley_AddModeAbility();
            MutationMeddley_EnsureValidMode();
            MutationMeddley_RefreshPassiveEffects();
        }

        protected string MutationMeddley_GetCurrentModeName()
        {
            List<MutationMeddley_ModeChoice> modes = MutationMeddley_GetModeChoices();
            for (int i = 0; i < modes.Count; i++)
            {
                if (modes[i].Id == MutationMeddley_GetCurrentModeId())
                {
                    return modes[i].Name;
                }
            }

            return "Unaligned";
        }

        protected bool MutationMeddley_HasAnyEvolution()
        {
            return MutationMeddley_GetSelectedEvolutionIds().Count > 0;
        }

        protected int MutationMeddley_GetPathDepth(string tier1Id, string tier2Id, string tier3Id)
        {
            int result = 0;

            if (MutationMeddley_HasEvolution(tier1Id))
            {
                result += 1;
            }

            if (MutationMeddley_HasEvolution(tier2Id))
            {
                result += 1;
            }

            if (MutationMeddley_HasEvolution(tier3Id))
            {
                result += 1;
            }

            return result;
        }

        protected void MutationMeddley_ClearCommonStatShifts()
        {
            string[] stats = new string[]
            {
                "AV",
                "DV",
                "MoveSpeed",
                "HeatResistance",
                "ColdResistance",
                "Quickness"
            };

            for (int i = 0; i < stats.Length; i++)
            {
                StatShifter.SetStatShift(stats[i], 0, true);
            }
        }

        protected void MutationMeddley_SetShift(string stat, int amount)
        {
            StatShifter.SetStatShift(stat, amount, true);
        }

        protected string MutationMeddley_DescribeModeState()
        {
            if (!MutationMeddley_HasAnyEvolution())
            {
                return "No path chosen yet.";
            }

            return "Current stance: " + MutationMeddley_GetCurrentModeName() + ".";
        }

        protected string MutationMeddley_GetCurrentModeId()
        {
            return MutationMeddley_GetStateValue("mode");
        }

        protected void MutationMeddley_SetCurrentModeId(string id)
        {
            MutationMeddley_SetStateValue("mode", id);
        }

        private void MutationMeddley_AddModeAbility()
        {
            if (MutationMeddley_ModeAbilityID != Guid.Empty)
            {
                return;
            }

            MutationMeddley_ModeAbilityID = AddMyActivatedAbility(
                Name: MutationMeddley_ModeAbilityName,
                Command: MutationMeddley_GetModeCommand(),
                Class: MutationMeddley_ModeAbilityClass,
                Description: MutationMeddley_ModeAbilityDescription
            );
        }

        private void MutationMeddley_EnsureValidMode()
        {
            List<MutationMeddley_ModeChoice> modes = MutationMeddley_GetModeChoices();
            if (modes.Count == 0)
            {
                MutationMeddley_SetCurrentModeId("");
                return;
            }

            for (int i = 0; i < modes.Count; i++)
            {
                if (modes[i].Id == MutationMeddley_GetCurrentModeId())
                {
                    return;
                }
            }

            MutationMeddley_SetCurrentModeId(modes[0].Id);
        }

        private void MutationMeddley_ShowModePicker()
        {
            if (!MutationMeddley_IsFunctionallyActive())
            {
                Popup.Show(MutationMeddley_GetInactiveReason());
                return;
            }

            List<MutationMeddley_ModeChoice> modes = MutationMeddley_GetModeChoices();
            if (modes.Count == 0)
            {
                Popup.Show(
                    "No stance is available for "
                    + MutationMeddley_EvolutionDisplayName
                    + " yet.\n\n"
                    + "Choose an evolution path first."
                );
                return;
            }

            string[] options = new string[modes.Count];
            char[] hotkeys = new char[modes.Count];

            for (int i = 0; i < modes.Count; i++)
            {
                StringBuilder option = new StringBuilder();
                option.Append(modes[i].Name);
                option.Append("\n");
                option.Append(modes[i].Description);

                if (modes[i].Id == MutationMeddley_GetCurrentModeId())
                {
                    option.Append("\n(Current)");
                }

                options[i] = option.ToString();
                hotkeys[i] = (char)('A' + i);
            }

            int selection = Popup.ShowOptionList(
                MutationMeddley_ModeAbilityName,
                Options: options,
                Hotkeys: hotkeys,
                AllowEscape: true
            );

            if (selection < 0 || selection >= modes.Count)
            {
                return;
            }

            MutationMeddley_SetCurrentModeId(modes[selection].Id);
            MutationMeddley_RefreshPassiveEffects();
            UseEnergy(1000, "Physical Mutation");
        }
    }
}
