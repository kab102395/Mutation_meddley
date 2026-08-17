using System;
using XRL.Messages;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    internal static class MutationMeddley_PrimaryActionService
    {
        internal static bool MutationMeddley_TryUse(
            MutationMeddley_AdaptiveMutationBase mutation,
            GameObject owner,
            string signature,
            string actionName,
            string actionDescription)
        {
            if (mutation == null || owner == null || !owner.IsPlayer())
            {
                return false;
            }

            if (!mutation.MutationMeddley_PeekIsFunctionallyActive())
            {
                Popup.ShowFail(mutation.MutationMeddley_EvolutionDisplayName + " is currently dormant.");
                return false;
            }

            bool success = false;
            string message = "";

            switch (signature)
            {
                case "carapace_baseline":
                {
                    int brace = MutationMeddley_GetStateInt(mutation, "carapace_brace");
                    int braceCap = MutationMeddley_GetCarapaceBraceCap(mutation);
                    if (owner.hitpoints < owner.baseHitpoints && brace > 0)
                    {
                        MutationMeddley_SetStateInt(mutation, "carapace_brace", brace - 1);
                        success = mutation.MutationMeddley_TryBiologyHeal(1);
                        message = "You deliberately settle stored brace around your wounds.";
                    }
                    else if (brace < braceCap)
                    {
                        MutationMeddley_SetStateInt(mutation, "carapace_brace", brace + 1);
                        success = true;
                        message = "You set your shell and bank a deliberate brace.";
                    }
                    break;
                }

                case "carapace_fortress":
                    success = MutationMeddley_SpendForHeal(mutation, owner, "carapace_brace", 1);
                    message = "You spend brace to stabilize the fortified shell.";
                    break;

                case "carapace_hunter":
                    success = MutationMeddley_SpendForHeal(mutation, owner, "carapace_impact", 1);
                    message = "You bleed stored impact back through the articulated shell.";
                    break;

                case "carapace_adaptive":
                {
                    if (owner.hitpoints < owner.baseHitpoints)
                    {
                        string attunementKey = MutationMeddley_GetHighestAttunementKey(mutation);
                        if (!string.IsNullOrEmpty(attunementKey))
                        {
                            int current = MutationMeddley_GetStateInt(mutation, attunementKey);
                            MutationMeddley_SetStateInt(mutation, attunementKey, current - 1);
                            success = mutation.MutationMeddley_TryBiologyHeal(1);
                            message = "You discharge stored environmental attunement through your shell.";
                        }
                    }
                    break;
                }

                case "crystal_baseline":
                case "crystal_diamond":
                    success = MutationMeddley_SpendForHeal(mutation, owner, "lc_stress", 1);
                    message = "You resolve stored crystal stress into a stabilizing lattice.";
                    break;

                case "crystal_prismatic":
                {
                    if (owner.hitpoints < owner.baseHitpoints)
                    {
                        int dawn = MutationMeddley_GetStateInt(mutation, "lc_dawn");
                        int dusk = MutationMeddley_GetStateInt(mutation, "lc_dusk");
                        string alignmentKey = dawn >= dusk ? "lc_dawn" : "lc_dusk";
                        int current = Math.Max(dawn, dusk);
                        if (current > 0)
                        {
                            MutationMeddley_SetStateInt(mutation, alignmentKey, current - 1);
                            success = mutation.MutationMeddley_TryBiologyHeal(1);
                            message = "You fold stored alignment inward through the living lattice.";
                        }
                    }
                    break;
                }

                case "crystal_resonant":
                {
                    int release = MutationMeddley_GetStateInt(mutation, "lc_release");
                    if (owner.hitpoints < owner.baseHitpoints && release > 0)
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
                    break;
                }

                case "brine_baseline":
                    success = MutationMeddley_SpendForHeal(mutation, owner, "brine_reserve", 1);
                    message = "You draw stored brine across your wounds.";
                    break;

                case "brine_wellspring":
                    success = MutationMeddley_ConvertResource(mutation, "brine_reserve", 1, "brine_mend", 2, 3);
                    message = "You draw saline reserve inward and bank it as mend.";
                    break;

                case "brine_saltglass":
                    success = MutationMeddley_ConvertResource(mutation, "brine_reserve", 1, "brine_bastion", 1, 4);
                    message = "You settle reserve into a deliberate saltglass bastion.";
                    break;

                case "brine_scouring":
                    success = MutationMeddley_ConvertResource(mutation, "brine_reserve", 1, "brine_wake", 1, 4);
                    message = "You cast stored estuary pressure forward as wake.";
                    break;

                case "ash_baseline":
                    success = MutationMeddley_SpendForHeal(mutation, owner, "ash_embers", 1);
                    message = "You deliberately cauterize your wounds with an ember.";
                    break;

                case "ash_furnace":
                    success = MutationMeddley_ConvertResource(mutation, "ash_embers", 1, "ash_kiln", 1, 4);
                    message = "You bank an ember into a kiln layer.";
                    break;

                case "ash_cinder":
                    success = MutationMeddley_ConvertResource(mutation, "ash_embers", 1, "ash_rush", 1, 4);
                    message = "You stoke an ember into predatory rush.";
                    break;

                case "ash_smoke":
                    success = MutationMeddley_ConvertResource(mutation, "ash_embers", 1, "ash_haze", 1, 4);
                    message = "You gather an ember into a bank of haze.";
                    break;

                case "colony_baseline":
                    success = MutationMeddley_SpendForHeal(mutation, owner, "colony_charge", 2);
                    message = "You redistribute colonial pressure into deliberate recovery.";
                    break;

                case "colony_marrow":
                {
                    int pressure = MutationMeddley_GetStateInt(mutation, "colony_charge");
                    int stitch = MutationMeddley_GetStateInt(mutation, "colony_stitch");
                    bool canHeal = owner.hitpoints < owner.baseHitpoints;
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
                    break;
                }

                case "colony_surveyor":
                    success = MutationMeddley_ConvertResource(mutation, "colony_charge", 1, "colony_scout", 1, 4);
                    message = "You redistribute colony pressure into a mapped pursuit line.";
                    break;

                case "colony_parliament":
                    success = MutationMeddley_ConvertResource(mutation, "colony_charge", 1, "colony_parliament", 1, 4);
                    message = "You delegate colony pressure across the body's parliament.";
                    break;
            }

            if (!success)
            {
                Popup.ShowFail(
                    actionName
                    + " cannot be used right now.\n\n"
                    + actionDescription);
                return false;
            }

            mutation.MutationMeddley_RefreshForBiology();

            // Biology is an optional aggregate inspector. The gameplay transaction is
            // complete without it; refresh it only when the support part is present.
            MutationMeddley_BiologySupport support =
                owner.GetPart("MutationMeddley_BiologySupport") as MutationMeddley_BiologySupport;
            if (support != null)
            {
                support.MutationMeddley_RefreshAbilitySurface();
            }

            owner.UseEnergy(1000, "Physical Mutation");
            if (!string.IsNullOrEmpty(message))
            {
                MessageQueue.AddPlayerMessage(message);
            }
            return true;
        }

        private static int MutationMeddley_GetStateInt(
            MutationMeddley_AdaptiveMutationBase mutation,
            string key)
        {
            return MutationMeddley_StateEnvelopeAccess.GetInt(mutation, key);
        }

        private static void MutationMeddley_SetStateInt(
            MutationMeddley_AdaptiveMutationBase mutation,
            string key,
            int value)
        {
            MutationMeddley_StateEnvelopeAccess.SetInt(mutation, key, value);
        }

        private static int MutationMeddley_GetCarapaceBraceCap(
            MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (!MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "fortress"))
            {
                return 2 + (Math.Max(1, mutation.Level) >= 2 ? 1 : 0);
            }

            return MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, "living_fortress") ? 5 : 4;
        }

        private static bool MutationMeddley_SpendForHeal(
            MutationMeddley_AdaptiveMutationBase mutation,
            GameObject owner,
            string resourceKey,
            int cost)
        {
            if (owner == null
                || owner.hitpoints >= owner.baseHitpoints
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

        private static bool MutationMeddley_ConvertResource(
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

        private static string MutationMeddley_GetHighestAttunementKey(
            MutationMeddley_AdaptiveMutationBase mutation)
        {
            int heat = MutationMeddley_GetStateInt(mutation, "carapace_attune_heat");
            int mire = MutationMeddley_GetStateInt(mutation, "carapace_attune_mire");
            int rime = MutationMeddley_GetStateInt(mutation, "carapace_attune_rime");

            if (heat <= 0 && mire <= 0 && rime <= 0)
            {
                return "";
            }

            if (heat >= mire && heat >= rime) return "carapace_attune_heat";
            if (mire >= rime) return "carapace_attune_mire";
            return "carapace_attune_rime";
        }
    }
}
