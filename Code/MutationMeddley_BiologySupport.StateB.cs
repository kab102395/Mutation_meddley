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
        internal int MutationMeddley_GetCarapaceBraceCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (!MutationMeddley_HasEvolution(mutation, "fortress"))
            {
                return 2 + (Math.Max(1, mutation.Level) >= 2 ? 1 : 0);
            }
            return MutationMeddley_HasEvolution(mutation, "living_fortress") ? 5 : 4;
        }

        private int MutationMeddley_GetCarapaceImpactCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_HasEvolution(mutation, "pursuit_predator") ? 6 : 4;
        }

        private int MutationMeddley_GetCarapaceHeatCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_HasEvolution(mutation, "storm_carapace") ? 6 : 4;
        }

        private int MutationMeddley_GetCarapaceMireCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_HasEvolution(mutation, "bog_shell") ? 6 : 4;
        }

        private int MutationMeddley_GetCarapaceRimeCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_HasEvolution(mutation, "storm_carapace") ? 6 : 4;
        }

        private int MutationMeddley_GetCrystalStressCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (!MutationMeddley_HasEvolution(mutation, "diamond_lattice"))
            {
                return 2 + (Math.Max(1, mutation.Level) >= 2 ? 1 : 0);
            }
            return MutationMeddley_HasEvolution(mutation, "impact_cathedral")
                || MutationMeddley_HasEvolution(mutation, "anchor_maze") ? 6 : 4;
        }

        private int MutationMeddley_GetCrystalDawnCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_HasEvolution(mutation, "mirrorshard_halo") ? 6 : 4;
        }

        private int MutationMeddley_GetCrystalDuskCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_HasEvolution(mutation, "eclipse_veil") ? 6 : 4;
        }

        private int MutationMeddley_GetCrystalReleaseCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_HasEvolution(mutation, "song_of_fracture")
                || MutationMeddley_HasEvolution(mutation, "stilltone_engine") ? 6 : 4;
        }

        private int MutationMeddley_GetEffectiveCadenceForDisplay(MutationMeddley_AdaptiveMutationBase mutation)
        {
            int cadence = MutationMeddley_GetStateInt(mutation, "lc_cadence");
            if (MutationMeddley_HasMutationByName("Heightened Hearing")) cadence += 1;
            if (MutationMeddley_HasMutationByName("Brineborn") && MutationMeddley_IsCurrentCellSaline()) cadence += 1;

            MutationMeddley_AdaptiveMutationBase carapace = MutationMeddley_GetMutation("Carapace Evolution");
            if (carapace != null
                && carapace.MutationMeddley_PeekIsFunctionallyActive()
                && MutationMeddley_HasEvolution(carapace, "hunter_shell"))
            {
                cadence += 1;
            }

            if (MutationMeddley_HasEvolution(mutation, "fractured_choir")) cadence += 1;
            return Math.Min(cadence, 8);
        }

        private int MutationMeddley_GetBrineReserveCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            int cap = 6;
            if (MutationMeddley_HasMutationByName("Amphibious")) cap += 1;
            if (MutationMeddley_HasMutationByName("Photosynthetic Skin") && MutationMeddley_IsCurrentCellLit()) cap += 1;
            return cap;
        }

        private int MutationMeddley_GetAshEmberCap(MutationMeddley_AdaptiveMutationBase mutation)
        {
            // Match Ash Metabolism's actual live cap exactly. The first 0.7.1
            // telemetry copy omitted Photosynthetic Skin in lit cells and could show
            // 7/7 while the mutation itself was using an 8-point cap.
            int cap = 6;
            if (MutationMeddley_HasMutationByName("Flaming Ray")) cap += 1;
            if (MutationMeddley_HasMutationByName("Photosynthetic Skin") && MutationMeddley_IsCurrentCellLit()) cap += 1;
            return cap;
        }

        private int MutationMeddley_GetColonyPressureCap()
        {
            int cap = 6;
            if (MutationMeddley_HasMutationByName("Multiple Legs")) cap += 1;

            // Walking Colony's real rule is semantic: one additional compatible
            // BODY_PART_INTERACTION mutation other than Multiple Legs. Until the
            // telemetry provider is fully moved into the concrete mutation, mirror
            // the complete known registry instead of a loose shell approximation.
            if (MutationMeddley_HasMutationByName("Carapace")
                || MutationMeddley_HasMutationByName("Quills")
                || MutationMeddley_HasMutationByName("Burrowing Claws"))
            {
                cap += 1;
            }
            return cap;
        }

        private bool MutationMeddley_HasMutationByName(string name)
        {
            if (ParentObject == null)
            {
                return false;
            }

            global::XRL.World.Parts.Mutations mutations =
                ParentObject.GetPart("Mutations") as global::XRL.World.Parts.Mutations;
            return mutations != null && mutations.GetMutationByName(name) != null;
        }

        private bool MutationMeddley_IsCurrentCellLit()
        {
            return ParentObject != null
                && ParentObject.CurrentCell != null
                && ParentObject.CurrentCell.IsLit();
        }

        private bool MutationMeddley_IsCurrentCellSaline()
        {
            if (ParentObject == null || ParentObject.CurrentCell == null)
            {
                return false;
            }

            string description = ParentObject.CurrentCell.ToString();
            if (!string.IsNullOrEmpty(description))
            {
                string lowered = description.ToLowerInvariant();
                if (lowered.Contains("salt") || lowered.Contains("brine"))
                {
                    return true;
                }
            }

            object liquid = ParentObject.CurrentCell.GetOpenLiquidVolume();
            if (liquid != null)
            {
                string lowered = liquid.ToString().ToLowerInvariant();
                return lowered.Contains("salt") || lowered.Contains("brine");
            }

            return false;
        }
    }
}
