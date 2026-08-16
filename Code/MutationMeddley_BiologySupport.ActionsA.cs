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
    public partial class MutationMeddley_BiologySupport : IPart
    {
        private bool MutationMeddley_UsePrimaryAction(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (mutation == null || ParentObject == null)
            {
                return false;
            }

            if (!mutation.MutationMeddley_PeekIsFunctionallyActive())
            {
                Popup.ShowFail(mutation.MutationMeddley_EvolutionDisplayName + " is currently dormant.");
                return false;
            }

            string signature = MutationMeddley_GetActionSignature(mutation);
            bool success = false;
            string message = "";

            if (signature == "carapace_baseline")
            {
                int brace = MutationMeddley_GetStateInt(mutation, "carapace_brace");
                int cap = MutationMeddley_GetCarapaceBraceCap(mutation);
                if (ParentObject.hitpoints < ParentObject.baseHitpoints && brace > 0)
                {
                    MutationMeddley_SetStateInt(mutation, "carapace_brace", brace - 1);
                    success = mutation.MutationMeddley_TryBiologyHeal(1);
                    message = "You deliberately settle stored brace around your wounds.";
                }
                else if (brace < cap)
                {
                    MutationMeddley_SetStateInt(mutation, "carapace_brace", brace + 1);
                    success = true;
                    message = "You set your shell and bank a deliberate brace.";
                }
            }
            else if (signature == "carapace_fortress")
            {
                success = MutationMeddley_SpendForHeal(mutation, "carapace_brace", 1);
                message = "You spend brace to stabilize the fortified shell.";
            }
            else if (signature == "carapace_hunter")
            {
                success = MutationMeddley_SpendForHeal(mutation, "carapace_impact", 1);
                message = "You bleed stored impact back through the articulated shell.";
            }
            else if (signature == "carapace_adaptive")
            {
                if (ParentObject.hitpoints < ParentObject.baseHitpoints)
                {
                    string key = MutationMeddley_GetHighestAttunementKey(mutation);
                    if (!string.IsNullOrEmpty(key))
                    {
                        int current = MutationMeddley_GetStateInt(mutation, key);
                        MutationMeddley_SetStateInt(mutation, key, current - 1);
                        success = mutation.MutationMeddley_TryBiologyHeal(1);
                        message = "You discharge stored environmental attunement through your shell.";
                    }
                }
            }
            else if (signature == "crystal_baseline" || signature == "crystal_diamond")
            {
                success = MutationMeddley_SpendForHeal(mutation, "lc_stress", 1);
                message = "You resolve stored crystal stress into a stabilizing lattice.";
            }
            else if (signature == "crystal_prismatic")
            {
                if (ParentObject.hitpoints < ParentObject.baseHitpoints)
                {
                    int dawn = MutationMeddley_GetStateInt(mutation, "lc_dawn");
                    int dusk = MutationMeddley_GetStateInt(mutation, "lc_dusk");
                    string key = dawn >= dusk ? "lc_dawn" : "lc_dusk";
                    int current = Math.Max(dawn, dusk);
                    if (current > 0)
                    {
                        MutationMeddley_SetStateInt(mutation, key, current - 1);
                        success = mutation.MutationMeddley_TryBiologyHeal(1);
                        message = "You fold stored alignment inward through the living lattice.";
                    }
                }
            }
            else if (signature == "crystal_resonant")
            {
                int release = MutationMeddley_GetStateInt(mutation, "lc_release");
                if (ParentObject.hitpoints < ParentObject.baseHitpoints && release > 0)
                {
                    MutationMeddley_SetStateInt(mutation, "lc_release", release - 1);
                    success = mutation.MutationMeddley_TryBiologyHeal(1);
                    message = "You resolve a stored resonance into a stabilizing hum.";
                }
                else if (release == 0)
                {
                    int cadence = MutationMeddley_GetStateInt(mutation, "lc_cadence");
                    if (cadence >= 2)
                    {
                        MutationMeddley_SetStateInt(mutation, "lc_cadence", cadence - 2);
                        MutationMeddley_SetStateInt(mutation, "lc_release", 1);
                        success = true;
                        message = "You compress two beats of cadence into one stored release.";
                    }
                }
            }
            else if (signature == "brine_baseline")
            {
                success = MutationMeddley_SpendForHeal(mutation, "brine_reserve", 1);
                message = "You draw stored brine across your wounds.";
            }
            else if (signature == "brine_wellspring")
            {
                success = MutationMeddley_ConvertResource(mutation, "brine_reserve", 1, "brine_mend", 2, 3);
                message = "You draw saline reserve inward and bank it as mend.";
            }
            else if (signature == "brine_saltglass")
            {
                success = MutationMeddley_ConvertResource(mutation, "brine_reserve", 1, "brine_bastion", 1, 4);
                message = "You settle reserve into a deliberate saltglass bastion.";
            }
            else if (signature == "brine_scouring")
            {
                success = MutationMeddley_ConvertResource(mutation, "brine_reserve", 1, "brine_wake", 1, 4);
                message = "You cast stored estuary pressure forward as wake.";
            }
            else if (signature == "ash_baseline")
            {
                success = MutationMeddley_SpendForHeal(mutation, "ash_embers", 1);
                message = "You deliberately cauterize your wounds with an ember.";
            }
            else if (signature == "ash_furnace")
            {
                success = MutationMeddley_ConvertResource(mutation, "ash_embers", 1, "ash_kiln", 1, 4);
                message = "You bank an ember into a kiln layer.";
            }
            else if (signature == "ash_cinder")
            {
                success = MutationMeddley_ConvertResource(mutation, "ash_embers", 1, "ash_rush", 1, 4);
                message = "You stoke an ember into predatory rush.";
            }
            else if (signature == "ash_smoke")
            {
                success = MutationMeddley_ConvertResource(mutation, "ash_embers", 1, "ash_haze", 1, 4);
                message = "You gather an ember into a bank of haze.";
            }
            else if (signature == "colony_baseline")
            {
                success = MutationMeddley_SpendForHeal(mutation, "colony_charge", 2);
                message = "You redistribute colonial pressure into deliberate recovery.";
            }
            else if (signature == "colony_marrow")
            {
                int pressure = MutationMeddley_GetStateInt(mutation, "colony_charge");
                int stitch = MutationMeddley_GetStateInt(mutation, "colony_stitch");
                bool canHeal = ParentObject.hitpoints < ParentObject.baseHitpoints;
                if (pressure > 0 && (stitch < 4 || canHeal))
                {
                    MutationMeddley_SetStateInt(mutation, "colony_charge", pressure - 1);
                    if (stitch < 4)
                    {
                        MutationMeddley_SetStateInt(mutation, "colony_stitch", stitch + 1);
                    }
                    if (canHeal)
                    {
                        mutation.MutationMeddley_TryBiologyHeal(1);
                    }
                    success = true;
                    message = "You order the colony to knit pressure into flesh and stitch.";
                }
            }
            else if (signature == "colony_surveyor")
            {
                success = MutationMeddley_ConvertResource(mutation, "colony_charge", 1, "colony_scout", 1, 4);
                message = "You redistribute colony pressure into a mapped pursuit line.";
            }
            else if (signature == "colony_parliament")
            {
                success = MutationMeddley_ConvertResource(mutation, "colony_charge", 1, "colony_parliament", 1, 4);
                message = "You delegate colony pressure across the body's parliament.";
            }

            if (!success)
            {
                Popup.ShowFail(
                    MutationMeddley_GetActionName(mutation)
                    + " cannot be used right now.\n\n"
                    + MutationMeddley_GetActionDescription(mutation));
                return false;
            }

            mutation.MutationMeddley_RefreshForBiology();
            MutationMeddley_RefreshAbilitySurface();
            ParentObject.UseEnergy(1000, "Physical Mutation");
            if (!string.IsNullOrEmpty(message))
            {
                MessageQueue.AddPlayerMessage(message);
            }
            return true;
        }

        private bool MutationMeddley_SpendForHeal(
            MutationMeddley_AdaptiveMutationBase mutation,
            string resourceKey,
            int cost)
        {
            if (ParentObject == null
                || ParentObject.hitpoints >= ParentObject.baseHitpoints
                || MutationMeddley_GetStateInt(mutation, resourceKey) < cost)
            {
                return false;
            }

            MutationMeddley_SetStateInt(
                mutation,
                resourceKey,
                MutationMeddley_GetStateInt(mutation, resourceKey) - cost);
            return mutation.MutationMeddley_TryBiologyHeal(1);
        }

        private bool MutationMeddley_ConvertResource(
            MutationMeddley_AdaptiveMutationBase mutation,
            string sourceKey,
            int sourceCost,
            string targetKey,
            int targetGain,
            int targetCap)
        {
            int source = MutationMeddley_GetStateInt(mutation, sourceKey);
            int target = MutationMeddley_GetStateInt(mutation, targetKey);
            if (source < sourceCost || target >= targetCap)
            {
                return false;
            }

            MutationMeddley_SetStateInt(mutation, sourceKey, source - sourceCost);
            MutationMeddley_SetStateInt(mutation, targetKey, Math.Min(targetCap, target + targetGain));
            return true;
        }

    }
}
