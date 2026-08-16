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
        private string MutationMeddley_GetResourceSummary(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (mutation == null)
            {
                return "No resource data.";
            }

            string name = mutation.MutationMeddley_EvolutionDisplayName;
            if (name == "Carapace Evolution")
            {
                if (!mutation.MutationMeddley_PeekIsFunctionallyActive())
                {
                    return "Dormant - vanilla Carapace is absent.";
                }

                if (MutationMeddley_HasEvolution(mutation, "hunter_shell"))
                {
                    return "Impact " + MutationMeddley_GetStateInt(mutation, "carapace_impact") + "/" + MutationMeddley_GetCarapaceImpactCap(mutation);
                }
                if (MutationMeddley_HasEvolution(mutation, "adaptive_carapace"))
                {
                    return "Heat " + MutationMeddley_GetStateInt(mutation, "carapace_attune_heat") + "/" + MutationMeddley_GetCarapaceHeatCap(mutation)
                        + "\nMire " + MutationMeddley_GetStateInt(mutation, "carapace_attune_mire") + "/" + MutationMeddley_GetCarapaceMireCap(mutation)
                        + "\nRime " + MutationMeddley_GetStateInt(mutation, "carapace_attune_rime") + "/" + MutationMeddley_GetCarapaceRimeCap(mutation);
                }
                return "Brace " + MutationMeddley_GetStateInt(mutation, "carapace_brace") + "/" + MutationMeddley_GetCarapaceBraceCap(mutation);
            }

            if (name == "Living Crystal")
            {
                if (MutationMeddley_HasEvolution(mutation, "prismatic_matrix"))
                {
                    return "Dawn " + MutationMeddley_GetStateInt(mutation, "lc_dawn") + "/" + MutationMeddley_GetCrystalDawnCap(mutation)
                        + "\nDusk " + MutationMeddley_GetStateInt(mutation, "lc_dusk") + "/" + MutationMeddley_GetCrystalDuskCap(mutation);
                }
                if (MutationMeddley_HasEvolution(mutation, "resonant_crystal"))
                {
                    return "Cadence " + MutationMeddley_GetStateInt(mutation, "lc_cadence") + "/6"
                        + "\nEffective cadence " + MutationMeddley_GetEffectiveCadenceForDisplay(mutation) + "/8"
                        + "\nRelease " + MutationMeddley_GetStateInt(mutation, "lc_release") + "/" + MutationMeddley_GetCrystalReleaseCap(mutation);
                }
                return "Stress " + MutationMeddley_GetStateInt(mutation, "lc_stress") + "/" + MutationMeddley_GetCrystalStressCap(mutation);
            }

            if (name == "Brineborn")
            {
                StringBuilder result = new StringBuilder();
                result.Append("Reserve ");
                result.Append(MutationMeddley_GetStateInt(mutation, "brine_reserve"));
                result.Append("/");
                result.Append(MutationMeddley_GetBrineReserveCap(mutation));

                if (MutationMeddley_HasEvolution(mutation, "wellspring_flesh"))
                {
                    result.Append("\nMend ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "brine_mend"));
                    result.Append("/3");
                }
                else if (MutationMeddley_HasEvolution(mutation, "saltglass_bloom"))
                {
                    result.Append("\nBastion ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "brine_bastion"));
                    result.Append("/4");
                }
                else if (MutationMeddley_HasEvolution(mutation, "scouring_estuary"))
                {
                    result.Append("\nWake ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "brine_wake"));
                    result.Append("/4");
                }

                return result.ToString();
            }

            if (name == "Ash Metabolism")
            {
                StringBuilder result = new StringBuilder();
                result.Append("Embers ");
                result.Append(MutationMeddley_GetStateInt(mutation, "ash_embers"));
                result.Append("/");
                result.Append(MutationMeddley_GetAshEmberCap(mutation));

                if (MutationMeddley_HasEvolution(mutation, "furnace_skin"))
                {
                    result.Append("\nKiln ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "ash_kiln"));
                    result.Append("/4");
                }
                else if (MutationMeddley_HasEvolution(mutation, "cinder_gut"))
                {
                    result.Append("\nRush ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "ash_rush"));
                    result.Append("/4");
                }
                else if (MutationMeddley_HasEvolution(mutation, "smoke_organ"))
                {
                    result.Append("\nHaze ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "ash_haze"));
                    result.Append("/4");
                }

                return result.ToString();
            }

            if (name == "Walking Colony")
            {
                StringBuilder result = new StringBuilder();
                result.Append("Pressure ");
                result.Append(MutationMeddley_GetStateInt(mutation, "colony_charge"));
                result.Append("/");
                result.Append(MutationMeddley_GetColonyPressureCap());

                if (MutationMeddley_HasEvolution(mutation, "marrow_hive"))
                {
                    result.Append("\nStitch ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "colony_stitch"));
                    result.Append("/4");
                }
                else if (MutationMeddley_HasEvolution(mutation, "surveyor_swarm"))
                {
                    result.Append("\nScout ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "colony_scout"));
                    result.Append("/4");
                    result.Append("\nStride ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "colony_stride_streak"));
                    result.Append("/8");
                }
                else if (MutationMeddley_HasEvolution(mutation, "graft_parliament"))
                {
                    result.Append("\nDelegated load ");
                    result.Append(MutationMeddley_GetStateInt(mutation, "colony_parliament"));
                    result.Append("/4");
                }

                return result.ToString();
            }

            return "No resource data.";
        }

        private string MutationMeddley_GetResourceFlow(MutationMeddley_AdaptiveMutationBase mutation)
        {
            string name = mutation.MutationMeddley_EvolutionDisplayName;
            StringBuilder text = new StringBuilder();
            text.Append(MutationMeddley_GetResourceSummary(mutation));
            text.Append("\n\n");

            if (name == "Carapace Evolution")
            {
                if (MutationMeddley_HasEvolution(mutation, "hunter_shell"))
                {
                    text.Append("Gain: movement and pursuit build Impact; Hunter specializations and Multiple Legs can accelerate it.\n");
                    text.Append("Decay: remaining still bleeds Impact.\n");
                    text.Append("Active spend: Drive Shell spends 1 Impact for deliberate recovery.\n");
                    text.Append("Reaction spend: adjacent engaged melee contact automatically spends Impact; Ramming Gait can spend 2.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "adaptive_carapace"))
                {
                    text.Append("Gain: heat/light feeds Heat; wet/saline terrain feeds Mire; dim/rime posture feeds Rime.\n");
                    text.Append("Decay: attunements decay outside their environments.\n");
                    text.Append("Active spend: Discharge Attunement consumes 1 available attunement for deliberate recovery.\n");
                    text.Append("Reaction spend: matching environmental pressure or contact automatically vents matching attunement.");
                }
                else
                {
                    text.Append("Gain: end a turn without moving to gain Brace. Rank 2 raises baseline capacity from 2 to 3.\n");
                    text.Append("Decay: movement removes Brace.\n");
                    text.Append("Active: Brace Shell can deliberately set one Brace when not full, or spend one Brace for recovery while wounded.\n");
                    text.Append("Reaction: qualifying incoming damage while wounded automatically spends 1 Brace.");
                }
            }
            else if (name == "Living Crystal")
            {
                if (MutationMeddley_HasEvolution(mutation, "prismatic_matrix"))
                {
                    text.Append("Gain: lit turns build Dawn and drain Dusk; dim turns build Dusk and drain Dawn.\n");
                    text.Append("Active: Refract Lattice consumes 1 currently dominant alignment for recovery.\n");
                    text.Append("Reaction: matching light-state pressure/contact automatically spends the matching alignment.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "resonant_crystal"))
                {
                    text.Append("Gain: movement builds Cadence; sufficient Cadence and branch behavior create Release.\n");
                    text.Append("Active: Resonant Release spends 1 Release for recovery; if Release is empty, 2 Cadence can be converted into 1 Release.\n");
                    text.Append("Reaction: contact spends Release; Humming Guard can spend Release on incoming pressure.");
                }
                else
                {
                    text.Append("Gain: stillness or close pressure builds Stress.\n");
                    text.Append("Decay: moving without close pressure drains Stress.\n");
                    text.Append("Active: Resolve Crystal Stress spends 1 Stress for recovery.\n");
                    text.Append("Reaction: qualifying pressure/contact spends Stress.");
                }
            }
            else if (name == "Brineborn")
            {
                text.Append("Reserve builds in saline conditions and decays away from them.\n");
                if (MutationMeddley_HasEvolution(mutation, "wellspring_flesh"))
                {
                    text.Append("Active: Draw Brine converts 1 Reserve into up to 2 Mend.\n");
                    text.Append("Reaction: Mend and Reserve feed automatic recovery under pressure.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "saltglass_bloom"))
                {
                    text.Append("Active: Raise Saltglass converts 1 Reserve into 1 Bastion.\n");
                    text.Append("Reaction: Bastion is spent by shell pressure/contact behavior.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "scouring_estuary"))
                {
                    text.Append("Active: Release Wake converts 1 Reserve into 1 Wake.\n");
                    text.Append("Reaction: moving adjacent melee contact spends Wake.");
                }
                else
                {
                    text.Append("Active: Draw Brine spends 1 Reserve for deliberate recovery.\n");
                    text.Append("Reaction: qualifying incoming damage while wounded spends 1 Reserve.");
                }
            }
            else if (name == "Ash Metabolism")
            {
                text.Append("Embers build in hot/smoky conditions and decay in calm environments.\n");
                if (MutationMeddley_HasEvolution(mutation, "furnace_skin"))
                {
                    text.Append("Active: Bank Kiln converts 1 Ember into 1 Kiln layer.\n");
                    text.Append("Reaction: Kiln is spent by defensive heat/pressure behavior.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "cinder_gut"))
                {
                    text.Append("Active: Stoke Rush converts 1 Ember into 1 Rush.\n");
                    text.Append("Reaction: moving adjacent melee contact spends Rush.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "smoke_organ"))
                {
                    text.Append("Active: Gather Haze converts 1 Ember into 1 Haze.\n");
                    text.Append("Reaction: smoke pressure/contact spends Haze.");
                }
                else
                {
                    text.Append("Active: Cauterize spends 1 Ember for deliberate recovery.\n");
                    text.Append("Reaction: qualifying incoming damage while wounded spends 1 Ember.");
                }
            }
            else if (name == "Walking Colony")
            {
                text.Append("Pressure builds from movement and drains while inert.\n");
                if (MutationMeddley_HasEvolution(mutation, "marrow_hive"))
                {
                    text.Append("Active: Knit Flesh converts 1 Pressure into 1 Stitch and heals when wounded.\n");
                    text.Append("Reaction: incoming pressure spends Stitch to heal.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "surveyor_swarm"))
                {
                    text.Append("Active: Map Pursuit converts 1 Pressure into 1 Scout.\n");
                    text.Append("Reaction: moving adjacent melee contact spends Scout.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "graft_parliament"))
                {
                    text.Append("Active: Delegate Load converts 1 Pressure into 1 Delegated Load.\n");
                    text.Append("Reaction: incoming pressure spends Delegated Load to answer or redistribute strain.");
                }
                else
                {
                    text.Append("Active: Redistribute Pressure spends 2 Pressure for deliberate recovery.\n");
                    text.Append("Reaction: qualifying incoming damage while wounded spends 1 Pressure.");
                }
            }

            return text.ToString();
        }

    }
}
