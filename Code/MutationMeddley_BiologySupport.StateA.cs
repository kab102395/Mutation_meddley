using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    public partial class MutationMeddley_BiologySupport : IPart
    {
        internal bool MutationMeddley_HasAnyEvolution(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_StateEnvelopeAccess.HasAnyEvolution(mutation);
        }

        internal bool MutationMeddley_HasEvolution(MutationMeddley_AdaptiveMutationBase mutation, string id)
        {
            return MutationMeddley_StateEnvelopeAccess.HasEvolution(mutation, id);
        }

        internal int MutationMeddley_GetStateInt(MutationMeddley_AdaptiveMutationBase mutation, string key)
        {
            return MutationMeddley_StateEnvelopeAccess.GetInt(mutation, key);
        }

        internal void MutationMeddley_SetStateInt(
            MutationMeddley_AdaptiveMutationBase mutation,
            string key,
            int value)
        {
            MutationMeddley_StateEnvelopeAccess.SetInt(mutation, key, value);
        }
    }
}
