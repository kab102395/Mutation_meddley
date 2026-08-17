using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    public partial class MutationMeddley_BiologySupport : IPart
    {
        private bool MutationMeddley_UsePrimaryAction(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_PrimaryActionService.MutationMeddley_TryUse(
                this,
                mutation,
                ParentObject,
                MutationMeddley_GetActionSignature(mutation),
                MutationMeddley_GetActionName(mutation),
                MutationMeddley_GetActionDescription(mutation));
        }
    }
}
