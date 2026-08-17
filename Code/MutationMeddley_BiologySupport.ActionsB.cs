using System.Collections.Generic;
using System.Text;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    public partial class MutationMeddley_BiologySupport : IPart
    {
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
