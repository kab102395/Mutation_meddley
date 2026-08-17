using System.Text;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    internal static class MutationMeddley_PrimaryActionCatalog
    {
        internal static string GetSignature(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (mutation == null)
            {
                return "";
            }

            string name = mutation.MutationMeddley_EvolutionDisplayName;
            if (name == "Carapace Evolution")
            {
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "fortress")) return "carapace_fortress";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "hunter_shell")) return "carapace_hunter";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "adaptive_carapace")) return "carapace_adaptive";
                return "carapace_baseline";
            }

            if (name == "Living Crystal")
            {
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "diamond_lattice")) return "crystal_diamond";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "prismatic_matrix")) return "crystal_prismatic";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "resonant_crystal")) return "crystal_resonant";
                return "crystal_baseline";
            }

            if (name == "Brineborn")
            {
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "wellspring_flesh")) return "brine_wellspring";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "saltglass_bloom")) return "brine_saltglass";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "scouring_estuary")) return "brine_scouring";
                return "brine_baseline";
            }

            if (name == "Ash Metabolism")
            {
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "furnace_skin")) return "ash_furnace";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "cinder_gut")) return "ash_cinder";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "smoke_organ")) return "ash_smoke";
                return "ash_baseline";
            }

            if (name == "Walking Colony")
            {
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "marrow_hive")) return "colony_marrow";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "surveyor_swarm")) return "colony_surveyor";
                if (MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "graft_parliament")) return "colony_parliament";
                return "colony_baseline";
            }

            return "";
        }

        internal static string GetName(MutationMeddley_AdaptiveMutationBase mutation)
        {
            switch (GetSignature(mutation))
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
                default: return "";
            }
        }

        internal static string GetRules(MutationMeddley_AdaptiveMutationBase mutation)
        {
            switch (GetSignature(mutation))
            {
                case "carapace_baseline":
                    return "If wounded and Brace is available, spend 1 Brace to heal. Otherwise spend the turn deliberately setting +1 Brace, up to the current cap.";
                case "carapace_fortress":
                    return "Spend 1 Brace to deliberately stabilize the shell and heal while wounded. If healthy, the action fails without spending.";
                case "carapace_hunter":
                    return "Spend 1 Impact to deliberately convert pursuit load into recovery. If healthy, the action fails without spending.";
                case "carapace_adaptive":
                    return "Spend 1 available Heat, Mire, or Rime attunement (highest store wins ties in that order) to stabilize the shell. If healthy, the action fails without spending.";
                case "crystal_baseline":
                case "crystal_diamond":
                    return "Spend 1 Stress for deliberate recovery. If healthy or empty, the action fails without spending.";
                case "crystal_prismatic":
                    return "Spend 1 currently dominant Dawn/Dusk alignment for deliberate recovery. If healthy or empty, the action fails without spending.";
                case "crystal_resonant":
                    return "Spend 1 Release for deliberate recovery. If Release is empty and raw Cadence is at least 2, convert 2 Cadence into 1 Release instead.";
                case "brine_baseline":
                    return "Spend 1 Reserve for deliberate recovery. If healthy or empty, the action fails without spending.";
                case "brine_wellspring":
                    return "Spend 1 Reserve to gain up to 2 Mend. If Mend is full, the action fails without spending.";
                case "brine_saltglass":
                    return "Spend 1 Reserve to gain 1 Bastion. If Bastion is full, the action fails without spending.";
                case "brine_scouring":
                    return "Spend 1 Reserve to gain 1 Wake. If Wake is full, the action fails without spending.";
                case "ash_baseline":
                    return "Spend 1 Ember to cauterize wounds. If healthy or empty, the action fails without spending.";
                case "ash_furnace":
                    return "Spend 1 Ember to bank 1 Kiln layer. If Kiln is full, the action fails without spending.";
                case "ash_cinder":
                    return "Spend 1 Ember to bank 1 Rush. If Rush is full, the action fails without spending.";
                case "ash_smoke":
                    return "Spend 1 Ember to bank 1 Haze. If Haze is full, the action fails without spending.";
                case "colony_baseline":
                    return "Spend 2 Pressure for deliberate recovery. If wounded recovery is impossible or Pressure is below 2, the action fails without spending.";
                case "colony_marrow":
                    return "Spend 1 Pressure to gain 1 Stitch and, if wounded, heal. If Stitch is full and no healing can occur, the action fails without spending.";
                case "colony_surveyor":
                    return "Spend 1 Pressure to gain 1 Scout. If Scout is full, the action fails without spending.";
                case "colony_parliament":
                    return "Spend 1 Pressure to gain 1 Delegated Load. If Delegated Load is full, the action fails without spending.";
                default:
                    return "No deliberate action is currently available.";
            }
        }

        internal static string GetDescription(
            MutationMeddley_AdaptiveMutationBase mutation,
            string currentResourceSummary = "")
        {
            if (mutation == null)
            {
                return "Mutation Meddley action.";
            }

            StringBuilder text = new StringBuilder();
            if (!string.IsNullOrEmpty(currentResourceSummary))
            {
                text.Append(currentResourceSummary);
                text.Append("\n\n");
            }

            text.Append("Active action: ");
            text.Append(GetName(mutation));
            text.Append("\nChance: 100% when requirements are met.\nEnergy: 1000.\n");
            text.Append(GetRules(mutation));
            text.Append("\n\nAutomatic reactions and exact current resource details are available through Mutation Meddley Biology when the inspector is present.");
            return text.ToString();
        }
    }
}
