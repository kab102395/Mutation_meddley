using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_EvolutionChoice
    {
        public string Id;
        public string Name;
        public string Description;
        public int RequiredLevel;
        public int Tier;
        public string PrerequisiteId;

        public MutationMeddley_EvolutionChoice(
            string id,
            string name,
            string description,
            int requiredLevel,
            int tier,
            string prerequisiteId = "")
        {
            Id = id;
            Name = name;
            Description = description;
            RequiredLevel = requiredLevel;
            Tier = tier;
            PrerequisiteId = prerequisiteId ?? "";
        }
    }

    [Serializable]
    public abstract class MutationMeddley_EvolvingMutationBase : BaseMutation
    {
        // Keep these public serialized fields stable. Future framework state should
        // preferably be encoded inside EvolutionState unless an explicit save
        // migration is introduced.
        public string MutationMeddley_EvolutionState = "";
        public Guid MutationMeddley_EvolveAbilityID = Guid.Empty;

        public abstract string MutationMeddley_EvolutionDisplayName { get; }

        protected abstract List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices();

        protected virtual string MutationMeddley_GetEvolutionCommand()
        {
            return "MutationMeddley_Evolve_" + GetType().Name;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, MutationMeddley_GetEvolutionCommand());
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == MutationMeddley_GetEvolutionCommand())
            {
                MutationMeddley_ShowEvolutionPicker();
                return false;
            }

            return base.FireEvent(E);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            MutationMeddley_AddEvolutionAbility();
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref MutationMeddley_EvolveAbilityID);
            return base.Unmutate(GO);
        }

        protected virtual void MutationMeddley_OnEvolutionChosen(MutationMeddley_EvolutionChoice choice)
        {
        }

        protected bool MutationMeddley_HasEvolution(string id)
        {
            List<string> selected = MutationMeddley_GetSelectedEvolutionIds();
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        protected string MutationMeddley_GetEvolutionSummary()
        {
            List<string> selected = MutationMeddley_GetSelectedEvolutionIds();
            if (selected.Count == 0)
            {
                return "No evolutions chosen.";
            }

            List<MutationMeddley_EvolutionChoice> choices = MutationMeddley_GetEvolutionChoices();
            StringBuilder result = new StringBuilder("Evolutions: ");
            bool wroteAny = false;

            for (int i = 0; i < choices.Count; i++)
            {
                if (!MutationMeddley_HasEvolution(choices[i].Id))
                {
                    continue;
                }

                if (wroteAny)
                {
                    result.Append(" -> ");
                }

                result.Append(choices[i].Name);
                wroteAny = true;
            }

            return result.ToString();
        }

        private void MutationMeddley_AddEvolutionAbility()
        {
            if (MutationMeddley_EvolveAbilityID != Guid.Empty)
            {
                return;
            }

            MutationMeddley_EvolveAbilityID = AddMyActivatedAbility(
                Name: "Evolve " + MutationMeddley_EvolutionDisplayName,
                Command: MutationMeddley_GetEvolutionCommand(),
                Class: "Physical Mutation",
                Description: "Choose an available Mutation Meddley evolution. New tiers unlock at mutation rank milestones."
            );
        }

        private List<string> MutationMeddley_GetSelectedEvolutionIds()
        {
            List<string> selected = new List<string>();
            if (string.IsNullOrEmpty(MutationMeddley_EvolutionState))
            {
                return selected;
            }

            string[] ids = MutationMeddley_EvolutionState.Split(
                new char[] { ';' },
                StringSplitOptions.RemoveEmptyEntries
            );

            for (int i = 0; i < ids.Length; i++)
            {
                selected.Add(ids[i]);
            }

            return selected;
        }

        private bool MutationMeddley_HasSelectionAtTier(int tier)
        {
            List<MutationMeddley_EvolutionChoice> choices = MutationMeddley_GetEvolutionChoices();
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].Tier == tier && MutationMeddley_HasEvolution(choices[i].Id))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MutationMeddley_IsEvolutionAvailable(MutationMeddley_EvolutionChoice choice)
        {
            if (Level < choice.RequiredLevel)
            {
                return false;
            }

            if (MutationMeddley_HasEvolution(choice.Id))
            {
                return false;
            }

            if (MutationMeddley_HasSelectionAtTier(choice.Tier))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(choice.PrerequisiteId)
                && !MutationMeddley_HasEvolution(choice.PrerequisiteId))
            {
                return false;
            }

            return true;
        }

        private List<MutationMeddley_EvolutionChoice> MutationMeddley_GetAvailableEvolutions()
        {
            List<MutationMeddley_EvolutionChoice> result = new List<MutationMeddley_EvolutionChoice>();
            List<MutationMeddley_EvolutionChoice> choices = MutationMeddley_GetEvolutionChoices();

            for (int i = 0; i < choices.Count; i++)
            {
                if (MutationMeddley_IsEvolutionAvailable(choices[i]))
                {
                    result.Add(choices[i]);
                }
            }

            return result;
        }

        private void MutationMeddley_ShowEvolutionPicker()
        {
            List<MutationMeddley_EvolutionChoice> available = MutationMeddley_GetAvailableEvolutions();
            if (available.Count == 0)
            {
                Popup.Show(
                    "No evolution is currently available for "
                    + MutationMeddley_EvolutionDisplayName
                    + ".\n\n"
                    + MutationMeddley_GetEvolutionSummary()
                );
                return;
            }

            StringBuilder prompt = new StringBuilder();
            prompt.Append("Choose an evolution for ");
            prompt.Append(MutationMeddley_EvolutionDisplayName);
            prompt.Append(":\n\n");

            for (int i = 0; i < available.Count; i++)
            {
                prompt.Append(i + 1);
                prompt.Append(") ");
                prompt.Append(available[i].Name);
                prompt.Append("\n   ");
                prompt.Append(available[i].Description);
                prompt.Append("\n\n");
            }

            prompt.Append("Enter a number, or press Escape to cancel.");

            string response = Popup.AskString(
                prompt.ToString(),
                ReturnNullForEscape: true
            );

            if (string.IsNullOrEmpty(response))
            {
                return;
            }

            int selection;
            if (!int.TryParse(response, out selection)
                || selection < 1
                || selection > available.Count)
            {
                Popup.ShowFail("That is not a valid evolution choice.");
                return;
            }

            MutationMeddley_EvolutionChoice chosen = available[selection - 1];
            MutationMeddley_SelectEvolution(chosen);
        }

        private void MutationMeddley_SelectEvolution(MutationMeddley_EvolutionChoice choice)
        {
            if (!MutationMeddley_IsEvolutionAvailable(choice))
            {
                Popup.ShowFail("That evolution is not currently available.");
                return;
            }

            if (string.IsNullOrEmpty(MutationMeddley_EvolutionState))
            {
                MutationMeddley_EvolutionState = choice.Id;
            }
            else
            {
                MutationMeddley_EvolutionState += ";" + choice.Id;
            }

            MutationMeddley_OnEvolutionChosen(choice);
            Popup.Show(
                MutationMeddley_EvolutionDisplayName
                + " evolved: "
                + choice.Name
                + "\n\n"
                + choice.Description
            );
        }
    }
}
