using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_CarapaceEvolution : MutationMeddley_AdaptiveMutationBase
    {
        public override string MutationMeddley_EvolutionDisplayName
        {
            get { return "Carapace Evolution"; }
        }

        protected override string MutationMeddley_ModeAbilityName
        {
            get { return "Retune Carapace Evolution"; }
        }

        protected override string MutationMeddley_ModeAbilityDescription
        {
            get { return "Shift your augmented shell between fortress, pursuit, and adaptation stances."; }
        }

        public override string GetDescription()
        {
            return "A Mutation Meddley companion evolution intended to pair with vanilla Carapace.\n\n"
                + "This mutation does not replace vanilla Carapace. Instead, it offers a separate branching shell-specialization layer that can be taken alongside it.";
        }

        public override string GetLevelText(int Level)
        {
            string intro = "Intended to pair with vanilla Carapace for a full shell-focused build.\n"
                + "Rank 3: choose the shell's identity.\n"
                + "Rank 6: specialize the shell.\n"
                + "Rank 9: claim its capstone.\n\n";

            return intro
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState();
        }

        protected override List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices()
        {
            return new List<MutationMeddley_EvolutionChoice>
            {
                new MutationMeddley_EvolutionChoice(
                    "fortress",
                    "Fortress",
                    "Become slower, denser, and brutally difficult to dislodge.",
                    3,
                    1,
                    detailText: "Heavy defense path built around anchoring and attrition."
                ),
                new MutationMeddley_EvolutionChoice(
                    "hunter_shell",
                    "Hunter Shell",
                    "Articulate your shell for pursuit and predatory close combat.",
                    3,
                    1,
                    detailText: "Mobile shell path that trades some stillness for tempo."
                ),
                new MutationMeddley_EvolutionChoice(
                    "adaptive_carapace",
                    "Adaptive Carapace",
                    "Retune your shell toward weather, hostile substances, and ambient stress.",
                    3,
                    1,
                    detailText: "Environmental shell path with strong resistance pivots."
                ),
                new MutationMeddley_EvolutionChoice(
                    "entrenched_bastion",
                    "Entrenched Bastion",
                    "Layer your shell into a still heavier defensive redoubt.",
                    6,
                    2,
                    "fortress",
                    "Deepens the fortress path with stronger anchoring."
                ),
                new MutationMeddley_EvolutionChoice(
                    "ravager_joints",
                    "Ravager Joints",
                    "Segment the shell for faster lateral pressure and chase.",
                    6,
                    2,
                    "hunter_shell",
                    "Deepens the hunter line with harder commitment to movement."
                ),
                new MutationMeddley_EvolutionChoice(
                    "thermal_baffles",
                    "Thermal Baffles",
                    "Grow reactive channels that steer heat and cold through your shell.",
                    6,
                    2,
                    "adaptive_carapace",
                    "Deepens the adaptive line with stronger elemental retuning."
                ),
                new MutationMeddley_EvolutionChoice(
                    "living_fortress",
                    "Living Fortress",
                    "Your shell becomes a near-static fortress of layered certainty.",
                    9,
                    3,
                    "entrenched_bastion",
                    "Capstone fortress line with severe mobility sacrifice."
                ),
                new MutationMeddley_EvolutionChoice(
                    "pursuit_predator",
                    "Pursuit Predator",
                    "Your shell is now a weaponized frame for relentless pressure.",
                    9,
                    3,
                    "ravager_joints",
                    "Capstone hunter line with strong speed bias."
                ),
                new MutationMeddley_EvolutionChoice(
                    "storm_carapace",
                    "Storm Carapace",
                    "Your shell continually rebalances itself against climate and exposure.",
                    9,
                    3,
                    "thermal_baffles",
                    "Capstone adaptive line with strong temperature resilience."
                )
            };
        }

        protected override List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices()
        {
            if (MutationMeddley_HasEvolution("fortress"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("anchor_down", "Anchor Down", "Settle your shell into a rooted defensive posture."),
                    new MutationMeddley_ModeChoice("spiteful_wall", "Spiteful Wall", "Loosen the shell slightly for more reactive defense.")
                };
            }

            if (MutationMeddley_HasEvolution("hunter_shell"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("skirmish_gait", "Skirmish Gait", "Favor speed, angles, and stickiness."),
                    new MutationMeddley_ModeChoice("ramming_gait", "Ramming Gait", "Carry more shell mass into the chase.")
                };
            }

            if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("ember_veil", "Ember Veil", "Tune the shell for heat and dry stress."),
                    new MutationMeddley_ModeChoice("rime_veil", "Rime Veil", "Tune the shell for cold and biting exposure.")
                };
            }

            return new List<MutationMeddley_ModeChoice>();
        }

        protected override void MutationMeddley_RefreshPassiveEffects()
        {
            MutationMeddley_ClearCommonStatShifts();

            if (MutationMeddley_HasEvolution("fortress"))
            {
                int depth = MutationMeddley_GetPathDepth("fortress", "entrenched_bastion", "living_fortress");
                MutationMeddley_SetShift("AV", 1 + depth);

                if (MutationMeddley_ModeState == "spiteful_wall")
                {
                    MutationMeddley_SetShift("DV", depth);
                    MutationMeddley_SetShift("MoveSpeed", -5 * depth);
                }
                else
                {
                    MutationMeddley_SetShift("MoveSpeed", -10 * depth);
                }
            }
            else if (MutationMeddley_HasEvolution("hunter_shell"))
            {
                int depth = MutationMeddley_GetPathDepth("hunter_shell", "ravager_joints", "pursuit_predator");
                MutationMeddley_SetShift("DV", 1 + depth);
                MutationMeddley_SetShift("MoveSpeed", 5 + (10 * depth));

                if (MutationMeddley_ModeState == "ramming_gait")
                {
                    MutationMeddley_SetShift("AV", depth);
                }
            }
            else if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                int depth = MutationMeddley_GetPathDepth("adaptive_carapace", "thermal_baffles", "storm_carapace");

                if (MutationMeddley_ModeState == "rime_veil")
                {
                    MutationMeddley_SetShift("ColdResistance", 10 + (10 * depth));
                    MutationMeddley_SetShift("HeatResistance", 5 * depth);
                }
                else
                {
                    MutationMeddley_SetShift("HeatResistance", 10 + (10 * depth));
                    MutationMeddley_SetShift("ColdResistance", 5 * depth);
                }

                MutationMeddley_SetShift("DV", depth);
            }
        }
    }
}
