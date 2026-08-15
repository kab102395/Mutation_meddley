using System;
using System.Collections.Generic;
using System.Globalization;
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
        public string DetailText;
        public int RequiredLevel;
        public int Tier;
        public string PrerequisiteId;

        public MutationMeddley_EvolutionChoice(
            string id,
            string name,
            string description,
            int requiredLevel,
            int tier,
            string prerequisiteId = "",
            string detailText = "")
        {
            Id = id;
            Name = name;
            Description = description;
            DetailText = detailText ?? "";
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
            MutationMeddley_OnEvolutionStateChanged();
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref MutationMeddley_EvolveAbilityID);
            return base.Unmutate(GO);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            bool result = base.ChangeLevel(NewLevel);
            MutationMeddley_OnEvolutionStateChanged();
            return result;
        }

        protected virtual void MutationMeddley_OnEvolutionChosen(MutationMeddley_EvolutionChoice choice)
        {
        }

        protected virtual void MutationMeddley_OnEvolutionStateChanged()
        {
        }

        protected virtual bool MutationMeddley_IsFunctionallyActive()
        {
            return true;
        }

        protected virtual string MutationMeddley_GetInactiveReason()
        {
            return MutationMeddley_EvolutionDisplayName + " is currently inactive.";
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

        protected string MutationMeddley_GetStateValue(string key)
        {
            Dictionary<string, string> metadata = MutationMeddley_GetStateMetadata();
            string value;
            if (metadata.TryGetValue(key, out value))
            {
                return value;
            }

            return "";
        }

        protected int MutationMeddley_GetStateInt(string key, int defaultValue = 0)
        {
            int value;
            if (int.TryParse(MutationMeddley_GetStateValue(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return defaultValue;
        }

        protected void MutationMeddley_SetStateValue(string key, string value)
        {
            Dictionary<string, string> metadata = MutationMeddley_GetStateMetadata();
            if (string.IsNullOrEmpty(value))
            {
                metadata.Remove(key);
            }
            else
            {
                metadata[key] = value;
            }

            MutationMeddley_SetStateEnvelope(
                MutationMeddley_GetSelectedEvolutionIds(),
                metadata
            );
        }

        protected void MutationMeddley_SetStateInt(string key, int value)
        {
            MutationMeddley_SetStateValue(
                key,
                value.ToString(CultureInfo.InvariantCulture)
            );
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

        protected List<string> MutationMeddley_GetSelectedEvolutionIds()
        {
            string encodedIds = MutationMeddley_GetEvolutionSegment();
            List<string> selected = new List<string>();
            if (string.IsNullOrEmpty(encodedIds))
            {
                return selected;
            }

            string[] ids = encodedIds.Split(
                new char[] { ';' },
                StringSplitOptions.RemoveEmptyEntries
            );

            for (int i = 0; i < ids.Length; i++)
            {
                selected.Add(ids[i]);
            }

            return selected;
        }

        protected void MutationMeddley_SetSelectedEvolutionIds(List<string> selected)
        {
            MutationMeddley_SetStateEnvelope(selected, MutationMeddley_GetStateMetadata());
        }

        protected bool MutationMeddley_HasSelectionAtTier(int tier)
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

        protected bool MutationMeddley_IsEvolutionAvailable(MutationMeddley_EvolutionChoice choice)
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

        protected List<MutationMeddley_EvolutionChoice> MutationMeddley_GetAvailableEvolutions()
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
            if (!MutationMeddley_IsFunctionallyActive())
            {
                Popup.Show(MutationMeddley_GetInactiveReason());
                return;
            }

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

            string[] options = new string[available.Count];
            char[] hotkeys = new char[available.Count];
            for (int i = 0; i < available.Count; i++)
            {
                StringBuilder option = new StringBuilder();
                option.Append(available[i].Name);
                option.Append("\n");
                option.Append(available[i].Description);

                if (!string.IsNullOrEmpty(available[i].DetailText))
                {
                    option.Append("\n");
                    option.Append(available[i].DetailText);
                }

                options[i] = option.ToString();
                hotkeys[i] = (char)('A' + i);
            }

            int selection = Popup.ShowOptionList(
                "Evolve: " + MutationMeddley_EvolutionDisplayName + " - rank " + Level,
                Options: options,
                Hotkeys: hotkeys,
                AllowEscape: true
            );

            if (selection < 0 || selection >= available.Count)
            {
                return;
            }

            MutationMeddley_EvolutionChoice chosen = available[selection];
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
                MutationMeddley_SetSelectedEvolutionIds(
                    new List<string> { choice.Id }
                );
            }
            else
            {
                List<string> selected = MutationMeddley_GetSelectedEvolutionIds();
                selected.Add(choice.Id);
                MutationMeddley_SetSelectedEvolutionIds(selected);
            }

            MutationMeddley_OnEvolutionChosen(choice);
            MutationMeddley_OnEvolutionStateChanged();
            Popup.Show(
                MutationMeddley_EvolutionDisplayName
                + " evolved: "
                + choice.Name
                + "\n\n"
                + choice.Description
            );
        }

        private string MutationMeddley_GetEvolutionSegment()
        {
            if (string.IsNullOrEmpty(MutationMeddley_EvolutionState))
            {
                return "";
            }

            int metadataIndex = MutationMeddley_EvolutionState.IndexOf('|');
            if (metadataIndex < 0)
            {
                return MutationMeddley_EvolutionState;
            }

            return MutationMeddley_EvolutionState.Substring(0, metadataIndex);
        }

        private Dictionary<string, string> MutationMeddley_GetStateMetadata()
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(MutationMeddley_EvolutionState))
            {
                return metadata;
            }

            string[] segments = MutationMeddley_EvolutionState.Split('|');
            for (int i = 1; i < segments.Length; i++)
            {
                if (string.IsNullOrEmpty(segments[i]))
                {
                    continue;
                }

                int separatorIndex = segments[i].IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = segments[i].Substring(0, separatorIndex);
                string value = segments[i].Substring(separatorIndex + 1);
                metadata[key] = value;
            }

            return metadata;
        }

        private void MutationMeddley_SetStateEnvelope(List<string> selected, Dictionary<string, string> metadata)
        {
            StringBuilder state = new StringBuilder();
            if (selected != null && selected.Count > 0)
            {
                for (int i = 0; i < selected.Count; i++)
                {
                    if (i > 0)
                    {
                        state.Append(';');
                    }

                    state.Append(selected[i]);
                }
            }

            if (metadata != null && metadata.Count > 0)
            {
                List<string> keys = new List<string>(metadata.Keys);
                keys.Sort(StringComparer.Ordinal);

                for (int i = 0; i < keys.Count; i++)
                {
                    if (string.IsNullOrEmpty(metadata[keys[i]]))
                    {
                        continue;
                    }

                    state.Append('|');
                    state.Append(keys[i]);
                    state.Append('=');
                    state.Append(metadata[keys[i]]);
                }
            }

            MutationMeddley_EvolutionState = state.ToString();
        }
    }
}
