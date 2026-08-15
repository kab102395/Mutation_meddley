using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_LivingCrystal : MutationMeddley_AdaptiveMutationBase
    {
        public override string MutationMeddley_EvolutionDisplayName
        {
            get { return "Living Crystal"; }
        }

        protected override string MutationMeddley_ModeAbilityName
        {
            get { return "Retune Living Crystal"; }
        }

        protected override string MutationMeddley_ModeAbilityDescription
        {
            get { return "Shift your crystalline posture to emphasize your current evolution path."; }
        }

        public override string GetDescription()
        {
            return "Your body is slowly replacing pliant tissue with living crystal.\n\n"
                + "Living Crystal is a build-defining mutation focused on branch identity, posture changes, and hard tradeoffs.";
        }

        public override string GetLevelText(int Level)
        {
            return "Rank 3: choose a crystalline identity.\n"
                + "Rank 6: specialize that lattice.\n"
                + "Rank 9: secure its capstone.\n\n"
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState();
        }

        protected override List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices()
        {
            return new List<MutationMeddley_EvolutionChoice>
            {
                new MutationMeddley_EvolutionChoice(
                    "diamond_lattice",
                    "Diamond Lattice",
                    "Harden toward impact, structure, and immovable force.",
                    3,
                    1,
                    detailText: "Heavy defensive path. Stronger AV, slower repositioning."
                ),
                new MutationMeddley_EvolutionChoice(
                    "prismatic_matrix",
                    "Prismatic Matrix",
                    "Split light and threat through refractive geometry.",
                    3,
                    1,
                    detailText: "Evasive resistance path. Balances DV with elemental resilience."
                ),
                new MutationMeddley_EvolutionChoice(
                    "resonant_crystal",
                    "Resonant Crystal",
                    "Turn your body into a humming instrument of stress and motion.",
                    3,
                    1,
                    detailText: "Mobile control path. Prioritizes speed, rhythm, and flexible defense."
                ),
                new MutationMeddley_EvolutionChoice(
                    "reinforced_facets",
                    "Reinforced Facets",
                    "Dense growth spreads load across interlocking crystal plates.",
                    6,
                    2,
                    "diamond_lattice",
                    "Deepens the tank role with still greater structural bias."
                ),
                new MutationMeddley_EvolutionChoice(
                    "sunlens_array",
                    "Sunlens Array",
                    "Facet geometry bends glare and thermal pressure away from you.",
                    6,
                    2,
                    "prismatic_matrix",
                    "Leans harder into heat and cold mitigation with reflective motion."
                ),
                new MutationMeddley_EvolutionChoice(
                    "choral_spines",
                    "Choral Spines",
                    "Resonant growths turn movement into a constant predatory cadence.",
                    6,
                    2,
                    "resonant_crystal",
                    "Sharpens tempo and stance-shifting mobility."
                ),
                new MutationMeddley_EvolutionChoice(
                    "impact_cathedral",
                    "Impact Cathedral",
                    "Your body becomes a shrine to pure structural refusal.",
                    9,
                    3,
                    "reinforced_facets",
                    "Capstone fortress line with severe weight and speed tradeoffs."
                ),
                new MutationMeddley_EvolutionChoice(
                    "mirrorshard_halo",
                    "Mirrorshard Halo",
                    "You diffuse hostile energy through a corona of mirrored splinters.",
                    9,
                    3,
                    "sunlens_array",
                    "Capstone refractive line with strong thermal adaptation."
                ),
                new MutationMeddley_EvolutionChoice(
                    "song_of_fracture",
                    "Song of Fracture",
                    "Your crystal body vibrates just ahead of danger.",
                    9,
                    3,
                    "choral_spines",
                    "Capstone resonance line with strong mobility and footwork bias."
                )
            };
        }

        protected override List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices()
        {
            if (MutationMeddley_HasEvolution("diamond_lattice"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("facet_lock", "Facet Lock", "Compress into a dense, anchored shell."),
                    new MutationMeddley_ModeChoice("saw_edges", "Saw Edges", "Open sharp seams for more agile defense.")
                };
            }

            if (MutationMeddley_HasEvolution("prismatic_matrix"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("dawn_glare", "Dawn Glare", "Favor heat-shedding and nimble reflection."),
                    new MutationMeddley_ModeChoice("dusk_glare", "Dusk Glare", "Favor cold-shedding and angled evasion.")
                };
            }

            if (MutationMeddley_HasEvolution("resonant_crystal"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("pulse_step", "Pulse Step", "Let vibration carry you into motion."),
                    new MutationMeddley_ModeChoice("humming_guard", "Humming Guard", "Stabilize your rhythm into guarded movement.")
                };
            }

            return new List<MutationMeddley_ModeChoice>();
        }

        protected override void MutationMeddley_RefreshPassiveEffects()
        {
            MutationMeddley_ClearCommonStatShifts();

            if (MutationMeddley_HasEvolution("diamond_lattice"))
            {
                int depth = MutationMeddley_GetPathDepth("diamond_lattice", "reinforced_facets", "impact_cathedral");
                MutationMeddley_SetShift("AV", 1 + depth);
                MutationMeddley_SetShift("MoveSpeed", -10 * depth);

                if (MutationMeddley_ModeState == "saw_edges")
                {
                    MutationMeddley_SetShift("AV", depth);
                    MutationMeddley_SetShift("DV", depth);
                }
            }
            else if (MutationMeddley_HasEvolution("prismatic_matrix"))
            {
                int depth = MutationMeddley_GetPathDepth("prismatic_matrix", "sunlens_array", "mirrorshard_halo");
                MutationMeddley_SetShift("DV", 1 + depth);

                if (MutationMeddley_ModeState == "dusk_glare")
                {
                    MutationMeddley_SetShift("ColdResistance", 10 + (10 * depth));
                    MutationMeddley_SetShift("HeatResistance", 5 * depth);
                }
                else
                {
                    MutationMeddley_SetShift("HeatResistance", 10 + (10 * depth));
                    MutationMeddley_SetShift("ColdResistance", 5 * depth);
                }
            }
            else if (MutationMeddley_HasEvolution("resonant_crystal"))
            {
                int depth = MutationMeddley_GetPathDepth("resonant_crystal", "choral_spines", "song_of_fracture");
                MutationMeddley_SetShift("MoveSpeed", 10 + (10 * depth));

                if (MutationMeddley_ModeState == "humming_guard")
                {
                    MutationMeddley_SetShift("DV", 1 + depth);
                }
                else
                {
                    MutationMeddley_SetShift("Quickness", 1 + depth);
                }
            }
        }
    }
}
