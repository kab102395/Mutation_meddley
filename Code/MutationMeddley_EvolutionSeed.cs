using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_EvolutionSeed : MutationMeddley_EvolvingMutationBase
    {
        public override string MutationMeddley_EvolutionDisplayName
        {
            get { return "Evolution Seed [DEV]"; }
        }

        public override string GetDescription()
        {
            return "A developer proof-of-concept mutation used to validate Mutation Meddley's branching evolution framework.";
        }

        public override string GetLevelText(int Level)
        {
            string text = "Framework test mutation. It intentionally has no final combat effect.\n"
                + "Rank 3: choose a primary adaptation.\n"
                + "Rank 6: specialize that adaptation.\n"
                + "Rank 9: choose its capstone.\n\n"
                + MutationMeddley_GetEvolutionSummary();

            return text;
        }

        protected override List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices()
        {
            return new List<MutationMeddley_EvolutionChoice>
            {
                new MutationMeddley_EvolutionChoice(
                    "bulwark",
                    "Bulwark",
                    "The mutation develops toward armor, stability, and retaliation.",
                    3,
                    1
                ),
                new MutationMeddley_EvolutionChoice(
                    "predator",
                    "Predator",
                    "The mutation develops toward mobility, aggression, and close-range pressure.",
                    3,
                    1
                ),
                new MutationMeddley_EvolutionChoice(
                    "symbiote",
                    "Symbiote",
                    "The mutation develops toward recovery, metabolism, and environmental interaction.",
                    3,
                    1
                ),

                new MutationMeddley_EvolutionChoice(
                    "layered_plates",
                    "Layered Plates",
                    "Bulwark specializes into reliable passive protection.",
                    6,
                    2,
                    "bulwark"
                ),
                new MutationMeddley_EvolutionChoice(
                    "reactive_plates",
                    "Reactive Plates",
                    "Bulwark specializes into triggered defenses and retaliation.",
                    6,
                    2,
                    "bulwark"
                ),
                new MutationMeddley_EvolutionChoice(
                    "spined_hunter",
                    "Spined Hunter",
                    "Predator specializes into punishing adjacent enemies.",
                    6,
                    2,
                    "predator"
                ),
                new MutationMeddley_EvolutionChoice(
                    "pursuit_lattice",
                    "Pursuit Lattice",
                    "Predator specializes into repositioning and sticking to prey.",
                    6,
                    2,
                    "predator"
                ),
                new MutationMeddley_EvolutionChoice(
                    "regenerative_lattice",
                    "Regenerative Lattice",
                    "Symbiote specializes into recovery after surviving pressure.",
                    6,
                    2,
                    "symbiote"
                ),
                new MutationMeddley_EvolutionChoice(
                    "chemosynthetic_shell",
                    "Chemosynthetic Shell",
                    "Symbiote specializes into extracting value from hostile environments.",
                    6,
                    2,
                    "symbiote"
                ),

                new MutationMeddley_EvolutionChoice(
                    "living_fortress",
                    "Living Fortress",
                    "Layered Plates culminates in a build-defining defensive capstone.",
                    9,
                    3,
                    "layered_plates"
                ),
                new MutationMeddley_EvolutionChoice(
                    "counter_carapace",
                    "Counter-Carapace",
                    "Reactive Plates culminates in a build centered on retaliation and timing.",
                    9,
                    3,
                    "reactive_plates"
                ),
                new MutationMeddley_EvolutionChoice(
                    "apex_spines",
                    "Apex Spines",
                    "Spined Hunter culminates in an aggressive contact-damage capstone.",
                    9,
                    3,
                    "spined_hunter"
                ),
                new MutationMeddley_EvolutionChoice(
                    "relentless_pursuit",
                    "Relentless Pursuit",
                    "Pursuit Lattice culminates in a mobility and action-economy capstone.",
                    9,
                    3,
                    "pursuit_lattice"
                ),
                new MutationMeddley_EvolutionChoice(
                    "deathless_tissue",
                    "Deathless Tissue",
                    "Regenerative Lattice culminates in a recovery-focused survival capstone.",
                    9,
                    3,
                    "regenerative_lattice"
                ),
                new MutationMeddley_EvolutionChoice(
                    "hostile_ecology",
                    "Hostile Ecology",
                    "Chemosynthetic Shell culminates in turning dangerous terrain and effects into resources.",
                    9,
                    3,
                    "chemosynthetic_shell"
                )
            };
        }
    }
}
