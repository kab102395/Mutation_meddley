using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using XRL;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    [Serializable]
    public partial class MutationMeddley_BiologySupport : IPart
    {
        private string MutationMeddley_GetActionSignature(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (mutation == null)
            {
                return "";
            }

            string name = mutation.MutationMeddley_EvolutionDisplayName;
            if (name == "Carapace Evolution")
            {
                if (MutationMeddley_HasEvolution(mutation, "fortress")) return "carapace_fortress";
                if (MutationMeddley_HasEvolution(mutation, "hunter_shell")) return "carapace_hunter";
                if (MutationMeddley_HasEvolution(mutation, "adaptive_carapace")) return "carapace_adaptive";
                return "carapace_baseline";
            }

            if (name == "Living Crystal")
            {
                if (MutationMeddley_HasEvolution(mutation, "diamond_lattice")) return "crystal_diamond";
                if (MutationMeddley_HasEvolution(mutation, "prismatic_matrix")) return "crystal_prismatic";
                if (MutationMeddley_HasEvolution(mutation, "resonant_crystal")) return "crystal_resonant";
                return "crystal_baseline";
            }

            if (name == "Brineborn")
            {
                if (MutationMeddley_HasEvolution(mutation, "wellspring_flesh")) return "brine_wellspring";
                if (MutationMeddley_HasEvolution(mutation, "saltglass_bloom")) return "brine_saltglass";
                if (MutationMeddley_HasEvolution(mutation, "scouring_estuary")) return "brine_scouring";
                return "brine_baseline";
            }

            if (name == "Ash Metabolism")
            {
                if (MutationMeddley_HasEvolution(mutation, "furnace_skin")) return "ash_furnace";
                if (MutationMeddley_HasEvolution(mutation, "cinder_gut")) return "ash_cinder";
                if (MutationMeddley_HasEvolution(mutation, "smoke_organ")) return "ash_smoke";
                return "ash_baseline";
            }

            if (name == "Walking Colony")
            {
                if (MutationMeddley_HasEvolution(mutation, "marrow_hive")) return "colony_marrow";
                if (MutationMeddley_HasEvolution(mutation, "surveyor_swarm")) return "colony_surveyor";
                if (MutationMeddley_HasEvolution(mutation, "graft_parliament")) return "colony_parliament";
                return "colony_baseline";
            }

            return "";
        }

        private string MutationMeddley_GetActionName(MutationMeddley_AdaptiveMutationBase mutation)
        {
            switch (MutationMeddley_GetActionSignature(mutation))
            {
                case "carapace_fortress": return "Fortify Shell";
                case "carapace_hunter": return "Drive Shell";
                case "carapace_adaptive": return "Discharge Attunement";
                case "carapace_baseline": return "Brace Shell";
                case "crystal_diamond": return "Resolve Crystal Stress";
                case "crystal_prismatic": return "Refract Lattice";
                case "crystal_resonant": return "Resonant Release";
                case "crystal_baseline": return "Resolve Crystal Stress";
                case "brine_wellspring": return "Draw Brine";
                case "brine_saltglass": return "Raise Saltglass";
                case "brine_scouring": return "Release Wake";
                case "brine_baseline": return "Draw Brine";
                case "ash_furnace": return "Bank Kiln";
                case "ash_cinder": return "Stoke Rush";
                case "ash_smoke": return "Gather Haze";
                case "ash_baseline": return "Cauterize";
                case "colony_marrow": return "Knit Flesh";
                case "colony_surveyor": return "Map Pursuit";
                case "colony_parliament": return "Delegate Load";
                case "colony_baseline": return "Redistribute Pressure";
                default: return "Use Mutation Meddley Biology";
            }
        }

        private string MutationMeddley_GetActionDescription(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (mutation == null)
            {
                return "Mutation Meddley action.";
            }

            StringBuilder text = new StringBuilder();
            text.Append(MutationMeddley_GetResourceSummary(mutation));
            text.Append("\n\n");
            text.Append("Active action: ");
            text.Append(MutationMeddley_GetActionName(mutation));
            text.Append("\nChance: 100% when requirements are met.\nEnergy: 1000.\n");
            text.Append(MutationMeddley_GetActiveActionRules(mutation));
            text.Append("\n\nAutomatic reactions, exact trigger conditions, and current Mutation Meddley modifiers are listed in Mutation Meddley Biology.");
            return text.ToString();
        }

        private string MutationMeddley_GetBiologyAbilityDescription()
        {
            List<MutationMeddley_AdaptiveMutationBase> owned = MutationMeddley_GetOwnedMutations();
            StringBuilder text = new StringBuilder("Free inspection. No turn or energy is spent.\n");

            for (int i = 0; i < owned.Count; i++)
            {
                text.Append("\n");
                text.Append(owned[i].MutationMeddley_EvolutionDisplayName);
                text.Append(": ");
                text.Append(MutationMeddley_GetResourceSummary(owned[i]).Replace("\n", " | "));
            }

            return text.ToString();
        }

        private void MutationMeddley_ShowBiology()
        {
            while (true)
            {
                MutationMeddley_RefreshAbilitySurface();
                List<MutationMeddley_AdaptiveMutationBase> owned = MutationMeddley_GetOwnedMutations();
                if (owned.Count == 0)
                {
                    Popup.Show("You do not currently possess any Mutation Meddley biology.");
                    return;
                }

                List<string> options = new List<string>();
                for (int i = 0; i < owned.Count; i++)
                {
                    MutationMeddley_AdaptiveMutationBase mutation = owned[i];
                    StringBuilder option = new StringBuilder();
                    option.Append(mutation.MutationMeddley_EvolutionDisplayName);
                    option.Append(" - rank ");
                    option.Append(mutation.Level);
                    option.Append("\n");
                    option.Append(MutationMeddley_GetPathAndStanceSummary(mutation));
                    option.Append("\n");
                    option.Append(MutationMeddley_GetResourceSummary(mutation).Replace("\n", " | "));
                    options.Add(option.ToString());
                }

                options.Add("Current MM modifiers\nSee Mutation Meddley stat contributions by source.");
                options.Add("All reactions\nSee automatic reaction triggers, current chances, costs, and effects.");
                options.Add("Active ecology\nSee current pair synergies and triads.");

                int selection = MutationMeddley_ShowOptions(
                    "Mutation Meddley Biology",
                    options);

                if (selection < 0)
                {
                    return;
                }

                if (selection < owned.Count)
                {
                    MutationMeddley_ShowMutationDetail(owned[selection]);
                    continue;
                }

                int special = selection - owned.Count;
                if (special == 0)
                {
                    MutationMeddley_ShowAllModifiers(owned);
                }
                else if (special == 1)
                {
                    MutationMeddley_ShowAllReactions(owned);
                }
                else if (special == 2)
                {
                    MutationMeddley_ShowAllEcology(owned);
                }
            }
        }

        private void MutationMeddley_ShowMutationDetail(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (mutation == null)
            {
                return;
            }

            while (true)
            {
                mutation.MutationMeddley_RefreshForBiology();
                List<string> options = new List<string>
                {
                    "Use " + MutationMeddley_GetActionName(mutation) + "\n" + MutationMeddley_GetActiveActionRules(mutation),
                    "Resources and flow\n" + MutationMeddley_GetResourceFlow(mutation),
                    "Automatic reactions\n" + MutationMeddley_GetReactionSummary(mutation),
                    "Current MM modifiers\n" + mutation.MutationMeddley_PeekPassiveBonusSummary(),
                    "Active ecology\n" + mutation.MutationMeddley_PeekSynergySummary(),
                    "Current mechanics\n" + mutation.MutationMeddley_PeekCurrentMechanicsSummary()
                };

                string title = mutation.MutationMeddley_EvolutionDisplayName
                    + " - rank " + mutation.Level
                    + "\n" + MutationMeddley_GetPathAndStanceSummary(mutation)
                    + "\n" + MutationMeddley_GetResourceSummary(mutation);

                int selection = MutationMeddley_ShowOptions(title, options);
                if (selection < 0)
                {
                    return;
                }

                if (selection == 0)
                {
                    if (MutationMeddley_UsePrimaryAction(mutation))
                    {
                        MutationMeddley_RefreshAbilitySurface();
                        return;
                    }
                }
                else if (selection == 1)
                {
                    Popup.Show(MutationMeddley_GetResourceFlow(mutation));
                }
                else if (selection == 2)
                {
                    Popup.Show(MutationMeddley_GetReactionSummary(mutation));
                }
                else if (selection == 3)
                {
                    Popup.Show(mutation.MutationMeddley_PeekPassiveBonusSummary());
                }
                else if (selection == 4)
                {
                    Popup.Show(mutation.MutationMeddley_PeekSynergySummary());
                }
                else if (selection == 5)
                {
                    Popup.Show(mutation.MutationMeddley_PeekCurrentMechanicsSummary());
                }
            }
        }

        private int MutationMeddley_ShowOptions(string title, List<string> options)
        {
            if (options == null || options.Count == 0)
            {
                return -1;
            }

            string[] optionArray = options.ToArray();
            char[] hotkeys = new char[optionArray.Length];
            for (int i = 0; i < hotkeys.Length; i++)
            {
                hotkeys[i] = i < 26 ? (char)('A' + i) : ' ';
            }

            return Popup.ShowOptionList(
                title,
                Options: optionArray,
                Hotkeys: hotkeys,
                AllowEscape: true);
        }

        private string MutationMeddley_GetPathAndStanceSummary(MutationMeddley_AdaptiveMutationBase mutation)
        {
            string path = mutation.MutationMeddley_PeekEvolutionSummary();
            string stance = mutation.MutationMeddley_PeekCurrentModeName();
            if (string.IsNullOrEmpty(stance) || stance == "Unaligned")
            {
                return path;
            }

            return path + " | Stance: " + stance;
        }

    }
}
