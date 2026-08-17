using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    internal static class MutationMeddley_AbilityRepair
    {
        private const string MutationMeddley_EvolutionDescription =
            "Choose an available Mutation Meddley evolution. New tiers unlock at mutation rank milestones.";

        internal static void MutationMeddley_EnsureEvolutionAbility(
            MutationMeddley_EvolvingMutationBase mutation,
            GameObject owner)
        {
            if (mutation == null || owner == null || !owner.IsPlayer())
            {
                return;
            }

            ActivatedAbilities abilities =
                owner.GetPart("ActivatedAbilities") as ActivatedAbilities;
            if (abilities == null)
            {
                owner.RequirePart<ActivatedAbilities>();
                abilities = owner.GetPart("ActivatedAbilities") as ActivatedAbilities;
            }

            if (abilities == null || abilities.AbilityByGuid == null)
            {
                return;
            }

            Guid abilityID = mutation.MutationMeddley_EvolveAbilityID;
            if (abilityID != Guid.Empty && !abilities.AbilityByGuid.ContainsKey(abilityID))
            {
                mutation.MutationMeddley_EvolveAbilityID = Guid.Empty;
                abilityID = Guid.Empty;
            }

            if (abilityID == Guid.Empty)
            {
                mutation.MutationMeddley_EvolveAbilityID = abilities.AddAbility(
                    Name: "Evolve " + mutation.MutationMeddley_EvolutionDisplayName,
                    Command: "MutationMeddley_Evolve_" + mutation.GetType().Name,
                    Class: "Physical Mutation",
                    Description: MutationMeddley_EvolutionDescription);
                return;
            }

            abilities.AbilityByGuid[abilityID].Description = MutationMeddley_EvolutionDescription;
        }
    }
}
