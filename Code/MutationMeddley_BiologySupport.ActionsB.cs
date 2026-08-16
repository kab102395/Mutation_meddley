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
        private string MutationMeddley_GetHighestAttunementKey(MutationMeddley_AdaptiveMutationBase mutation)
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

        private void MutationMeddley_UseCarapaceAction()
        {
            MutationMeddley_UsePrimaryAction(MutationMeddley_GetMutation("Carapace Evolution"));
        }

        private void MutationMeddley_UseCrystalAction()
        {
            MutationMeddley_UsePrimaryAction(MutationMeddley_GetMutation("Living Crystal"));
        }

        private void MutationMeddley_UseBrineAction()
        {
            MutationMeddley_UsePrimaryAction(MutationMeddley_GetMutation("Brineborn"));
        }

        private void MutationMeddley_UseAshAction()
        {
            MutationMeddley_UsePrimaryAction(MutationMeddley_GetMutation("Ash Metabolism"));
        }

        private void MutationMeddley_UseColonyAction()
        {
            MutationMeddley_UsePrimaryAction(MutationMeddley_GetMutation("Walking Colony"));
        }

        private void MutationMeddley_ShowAllModifiers(List<MutationMeddley_AdaptiveMutationBase> owned)
        {
            StringBuilder text = new StringBuilder("CURRENT MUTATION MEDDLEY MODIFIERS\n");
            text.Append("Only Mutation Meddley contributions are claimed here; vanilla and other-mod contributions are not guessed.");

            for (int i = 0; i < owned.Count; i++)
            {
                owned[i].MutationMeddley_RefreshForBiology();
                text.Append("\n\n");
                text.Append(owned[i].MutationMeddley_EvolutionDisplayName);
                text.Append("\n");
                text.Append(owned[i].MutationMeddley_PeekPassiveBonusSummary());
            }

            Popup.Show(text.ToString());
        }

        private void MutationMeddley_ShowAllReactions(List<MutationMeddley_AdaptiveMutationBase> owned)
        {
            StringBuilder text = new StringBuilder("AUTOMATIC MUTATION MEDDLEY REACTIONS");
            for (int i = 0; i < owned.Count; i++)
            {
                text.Append("\n\n");
                text.Append(owned[i].MutationMeddley_EvolutionDisplayName);
                text.Append("\n");
                text.Append(MutationMeddley_GetReactionSummary(owned[i]));
            }
            Popup.Show(text.ToString());
        }

        private void MutationMeddley_ShowAllEcology(List<MutationMeddley_AdaptiveMutationBase> owned)
        {
            StringBuilder text = new StringBuilder("ACTIVE MUTATION MEDDLEY ECOLOGY");
            for (int i = 0; i < owned.Count; i++)
            {
                text.Append("\n\n");
                text.Append(owned[i].MutationMeddley_EvolutionDisplayName);
                text.Append("\n");
                text.Append(owned[i].MutationMeddley_PeekSynergySummary());
            }
            Popup.Show(text.ToString());
        }

    }
}
