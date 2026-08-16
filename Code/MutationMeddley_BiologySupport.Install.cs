using XRL;
using XRL.Core;

namespace XRL.World.Parts
{
    [PlayerMutator]
    public class MutationMeddley_BiologyPlayerMutator : IPlayerMutator
    {
        public void mutate(GameObject player)
        {
            if (player != null)
            {
                player.RequirePart<MutationMeddley_BiologySupport>();
            }
        }
    }

    [HasCallAfterGameLoadedAttribute]
    public class MutationMeddley_BiologyLoadHandler
    {
        [CallAfterGameLoadedAttribute]
        public static void MutationMeddley_InstallBiologySupport()
        {
            GameObject player = XRLCore.Core?.Game?.Player?.Body;
            if (player != null)
            {
                player.RequirePart<MutationMeddley_BiologySupport>();
            }
        }
    }
}
