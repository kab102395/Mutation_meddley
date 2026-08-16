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
        private string MutationMeddley_GetReactionSummary(MutationMeddley_AdaptiveMutationBase mutation)
        {
            StringBuilder text = new StringBuilder();
            string name = mutation.MutationMeddley_EvolutionDisplayName;

            text.Append("Percentages describe the next qualifying trigger. Existing 0.7 reactions remain deterministic; 0.7.1 does not add random proc rolls.\n\n");

            if (name == "Carapace Evolution")
            {
                if (!mutation.MutationMeddley_PeekIsFunctionallyActive())
                {
                    return "Carapace Evolution is dormant because vanilla Carapace is absent.\nCurrent chance for shell reactions: 0%.";
                }

                if (MutationMeddley_HasEvolution(mutation, "fortress"))
                {
                    int brace = MutationMeddley_GetStateInt(mutation, "carapace_brace");
                    int cost = MutationMeddley_HasEvolution(mutation, "entrenched_bastion")
                        && MutationMeddley_GetStateInt(mutation, "carapace_stationary") > 0 ? 1 : 2;
                    text.Append("Shell Pressure Response\nType: automatic reaction\nTrigger: qualifying incoming damage while Brace is available.\nCurrent chance: ");
                    text.Append(brace > 0 ? "100%" : "0%");
                    text.Append("\nCost: up to ");
                    text.Append(cost);
                    text.Append(" Brace.\nEffect: recovery; Spiteful Wall can answer with bonus damage.\n");
                    if (MutationMeddley_HasEvolution(mutation, "porcupine_redoubt"))
                    {
                        text.Append("\nPorcupine Redoubt\nTrigger: rooted qualifying incoming damage with Quills while engaged.\nCurrent chance: 100% when trigger conditions are met.\nCost: no separate meter spend.\nEffect: quill-backed retaliation.\n");
                    }
                }
                else if (MutationMeddley_HasEvolution(mutation, "hunter_shell"))
                {
                    int impact = MutationMeddley_GetStateInt(mutation, "carapace_impact");
                    text.Append("Impact Contact Spend\nTrigger: adjacent engaged melee contact.\nCurrent chance: ");
                    text.Append(impact > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Impact in Skirmish Gait; up to 2 in Ramming Gait.\nEffect: recovery or shell-slam bonus damage.\n");
                }
                else if (MutationMeddley_HasEvolution(mutation, "adaptive_carapace"))
                {
                    int total = MutationMeddley_GetStateInt(mutation, "carapace_attune_heat")
                        + MutationMeddley_GetStateInt(mutation, "carapace_attune_mire")
                        + MutationMeddley_GetStateInt(mutation, "carapace_attune_rime");
                    text.Append("Attunement Reflex\nTrigger: matching environmental pressure or close contact.\nCurrent chance: ");
                    text.Append(total > 0 ? "100% when a matching attunement exists" : "0%");
                    text.Append("\nCost: 1 matching attunement.\nEffect: shell stabilization/recovery and branch follow-through.\n");
                }
                else
                {
                    int brace = MutationMeddley_GetStateInt(mutation, "carapace_brace");
                    text.Append("Baseline Wound Closure\nTrigger: qualifying incoming damage while wounded.\nCurrent chance: ");
                    text.Append(brace > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Brace.\nEffect: heal 1 base HP plus continuous verb growth.");
                }
            }
            else if (name == "Living Crystal")
            {
                if (MutationMeddley_HasEvolution(mutation, "diamond_lattice"))
                {
                    int stress = MutationMeddley_GetStateInt(mutation, "lc_stress");
                    text.Append("Stress Discharge\nTrigger: qualifying incoming pressure or adjacent engaged melee contact.\nCurrent chance: ");
                    text.Append(stress > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Stress on contact; incoming pressure can spend 1-2 depending on Dense Core/stillness.\nEffect: lattice retaliation or stabilization.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "prismatic_matrix"))
                {
                    int current = MutationMeddley_IsCurrentCellLit()
                        ? MutationMeddley_GetStateInt(mutation, "lc_dawn")
                        : MutationMeddley_GetStateInt(mutation, "lc_dusk");
                    text.Append("Prismatic Glare\nTrigger: pressure/contact matching the current light state.\nCurrent chance: ");
                    text.Append(current > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Dawn in light or 1 Dusk in dim conditions.\nEffect: refractive bonus damage or stabilization.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "resonant_crystal"))
                {
                    int release = MutationMeddley_GetStateInt(mutation, "lc_release");
                    text.Append("Resonant Discharge\nTrigger: adjacent engaged melee contact; Humming Guard can also answer incoming pressure.\nCurrent chance: ");
                    text.Append(release > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Release in Pulse Step; Humming Guard/contact can spend up to 2.\nEffect: resonant bonus damage or stabilizing discharge.");
                }
                else
                {
                    int stress = MutationMeddley_GetStateInt(mutation, "lc_stress");
                    text.Append("Baseline Fracture Seal\nTrigger: qualifying incoming damage while wounded.\nCurrent chance: ");
                    text.Append(stress > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Stress.\nEffect: heal 1 base HP plus continuous verb growth.");
                }
            }
            else if (name == "Brineborn")
            {
                int reserve = MutationMeddley_GetStateInt(mutation, "brine_reserve");
                if (!MutationMeddley_HasAnyEvolution(mutation))
                {
                    text.Append("Baseline Wound Closure\nTrigger: qualifying incoming damage while wounded.\nCurrent chance: ");
                    text.Append(reserve > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Reserve.\nEffect: heal 1 base HP plus continuous verb growth.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "wellspring_flesh"))
                {
                    int mend = MutationMeddley_GetStateInt(mutation, "brine_mend");
                    text.Append("Wellspring Recovery\nTrigger: qualifying incoming pressure while Mend/Reserve recovery state is available.\nCurrent chance: ");
                    text.Append(mend > 0 || reserve > 0 ? "100% when the branch trigger qualifies" : "0%");
                    text.Append("\nCost: branch-dependent Mend/Reserve.\nEffect: recovery routing and reserve recycling.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "saltglass_bloom"))
                {
                    int bastion = MutationMeddley_GetStateInt(mutation, "brine_bastion");
                    text.Append("Saltglass Response\nTrigger: qualifying pressure/contact with Bastion.\nCurrent chance: ");
                    text.Append(bastion > 0 ? "100%" : "0%");
                    text.Append("\nCost: Bastion as defined by the current shell route.\nEffect: mineral defense or edge retaliation.");
                }
                else
                {
                    int wake = MutationMeddley_GetStateInt(mutation, "brine_wake");
                    text.Append("Estuary Wake Spend\nTrigger: moving adjacent engaged melee contact.\nCurrent chance: ");
                    text.Append(wake > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Wake.\nEffect: pursuit bonus damage/pressure.");
                }
            }
            else if (name == "Ash Metabolism")
            {
                int embers = MutationMeddley_GetStateInt(mutation, "ash_embers");
                if (!MutationMeddley_HasAnyEvolution(mutation))
                {
                    text.Append("Baseline Cauterization\nTrigger: qualifying incoming damage while wounded.\nCurrent chance: ");
                    text.Append(embers > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Ember.\nEffect: heal 1 base HP plus continuous verb growth.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "furnace_skin"))
                {
                    int kiln = MutationMeddley_GetStateInt(mutation, "ash_kiln");
                    text.Append("Kiln Response\nTrigger: qualifying pressure with stored Kiln/heat state.\nCurrent chance: ");
                    text.Append(kiln > 0 || embers > 0 ? "100% when the branch trigger qualifies" : "0%");
                    text.Append("\nCost: branch-defined Kiln/Ember state.\nEffect: defensive heat response or retaliation.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "cinder_gut"))
                {
                    int rush = MutationMeddley_GetStateInt(mutation, "ash_rush");
                    text.Append("Cinder Rush Spend\nTrigger: moving adjacent engaged melee contact.\nCurrent chance: ");
                    text.Append(rush > 0 ? "100%" : "0%");
                    text.Append("\nCost: Rush.\nEffect: pursuit bonus damage/tempo.");
                }
                else
                {
                    int haze = MutationMeddley_GetStateInt(mutation, "ash_haze");
                    text.Append("Haze Response\nTrigger: qualifying smoky pressure/contact.\nCurrent chance: ");
                    text.Append(haze > 0 || embers > 0 ? "100% when the smoke trigger qualifies" : "0%");
                    text.Append("\nCost: Haze/Ember state.\nEffect: concealment pressure, recovery, or draft follow-through.");
                }
            }
            else if (name == "Walking Colony")
            {
                int pressure = MutationMeddley_GetStateInt(mutation, "colony_charge");
                if (!MutationMeddley_HasAnyEvolution(mutation))
                {
                    text.Append("Baseline Wound Closure\nTrigger: qualifying incoming damage while wounded.\nCurrent chance: ");
                    text.Append(pressure > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Pressure.\nEffect: heal 1 base HP plus continuous verb growth.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "marrow_hive"))
                {
                    int stitch = MutationMeddley_GetStateInt(mutation, "colony_stitch");
                    text.Append("Stitch Wound\nTrigger: qualifying incoming pressure with Stitch available.\nCurrent chance: ");
                    text.Append(stitch > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Stitch.\nEffect: recovery; branch specializations can recycle Stitch/Pressure.");
                }
                else if (MutationMeddley_HasEvolution(mutation, "surveyor_swarm"))
                {
                    int scout = MutationMeddley_GetStateInt(mutation, "colony_scout");
                    text.Append("Surveyor Line Spend\nTrigger: moving adjacent engaged melee contact.\nCurrent chance: ");
                    text.Append(scout > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Scout.\nEffect: surveyor-line bonus damage/pressure.");
                }
                else
                {
                    int parliament = MutationMeddley_GetStateInt(mutation, "colony_parliament");
                    text.Append("Delegated Strain\nTrigger: qualifying incoming pressure with Delegated Load available.\nCurrent chance: ");
                    text.Append(parliament > 0 ? "100%" : "0%");
                    text.Append("\nCost: 1 Delegated Load.\nEffect: retaliation when a source resolves; otherwise recovery/redistribution.");
                }
            }

            return text.ToString();
        }

        private string MutationMeddley_GetActiveActionRules(MutationMeddley_AdaptiveMutationBase mutation)
        {
            switch (MutationMeddley_GetActionSignature(mutation))
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

    }
}
