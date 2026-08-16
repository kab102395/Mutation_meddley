using XRL;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;

// Keep the lifecycle hooks in the global namespace and in the exact shape used by
// Qud's documented player-mutator/load-hook pattern. The Biology support itself
// remains namespaced under XRL.World.Parts.

[PlayerMutator]
public class MutationMeddley_BiologyPlayerMutator : IPlayerMutator
{
    public void mutate(GameObject player)
    {
        // PlayerMutator supplies the player GameObject by contract. During very early
        // character construction, do not require IsPlayer() to have become observable
        // yet; this is the strongest guaranteed new-game installation path.
        MutationMeddley_BiologySupport support =
            MutationMeddley_BiologySupport.MutationMeddley_EnsureInstalled(
                player,
                trustedPlayerObject: true);
        if (support != null)
        {
            support.MutationMeddley_RefreshAbilitySurface();
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
        MutationMeddley_BiologySupport support =
            MutationMeddley_BiologySupport.MutationMeddley_EnsureInstalled(
                player,
                trustedPlayerObject: true);
        if (support != null)
        {
            support.MutationMeddley_RefreshAbilitySurface();
        }
    }
}
