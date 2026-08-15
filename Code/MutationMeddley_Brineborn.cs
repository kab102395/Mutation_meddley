using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_Brineborn : MutationMeddley_AdaptiveMutationBase
    {
        public override string MutationMeddley_EvolutionDisplayName
        {
            get { return "Brineborn"; }
        }

        protected override string MutationMeddley_ModeAbilityName
        {
            get { return "Retune Brineborn"; }
        }

        protected override string MutationMeddley_ModeAbilityDescription
        {
            get { return "Shift your saline metabolism between sustain, crystal hardening, and scouring motion."; }
        }

        public override string GetDescription()
        {
            return "Salt, brine, and mineral saturation have become core to your metabolism.\n\n"
                + "Brineborn is an environmental mutation about terrain affinity, conversion, and abrasive survivability.";
        }

        public override string GetLevelText(int Level)
        {
            return "Rank 3: choose how your saline biology expresses itself.\n"
                + "Rank 6: specialize the loop.\n"
                + "Rank 9: claim the estuarial capstone.\n\n"
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState();
        }

        protected override List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices()
        {
            return new List<MutationMeddley_EvolutionChoice>
            {
                new MutationMeddley_EvolutionChoice(
                    "wellspring_flesh",
                    "Wellspring Flesh",
                    "Your tissues store and recycle brine as a reserve against hardship.",
                    3,
                    1,
                    detailText: "Sustain line. Better all-weather survivability and tempo recovery."
                ),
                new MutationMeddley_EvolutionChoice(
                    "saltglass_bloom",
                    "Saltglass Bloom",
                    "Minerals harden across you in layered, glassy crusts.",
                    3,
                    1,
                    detailText: "Crystallization line. Heavier shell, sharper tradeoff against motion."
                ),
                new MutationMeddley_EvolutionChoice(
                    "scouring_estuary",
                    "Scouring Estuary",
                    "You process hostile conditions into a harsh, mobile ecology.",
                    3,
                    1,
                    detailText: "Hostile-environment line. Favors aggressive repositioning and adaptation."
                ),
                new MutationMeddley_EvolutionChoice(
                    "tidal_marrows",
                    "Tidal Marrows",
                    "Brine pulses deeper into your frame and steadies your rhythm.",
                    6,
                    2,
                    "wellspring_flesh",
                    "Deepens the sustain loop with broader temperature tolerance."
                ),
                new MutationMeddley_EvolutionChoice(
                    "saltglass_bastion",
                    "Saltglass Bastion",
                    "Your shell thickens into a layered salt-ceramic bulwark.",
                    6,
                    2,
                    "saltglass_bloom",
                    "Further commits to hardening and rooted defense."
                ),
                new MutationMeddley_EvolutionChoice(
                    "desiccant_wake",
                    "Desiccant Wake",
                    "Movement leaves behind a dry, punishing metabolic trail.",
                    6,
                    2,
                    "scouring_estuary",
                    "Strengthens mobile pressure and extreme-condition readiness."
                ),
                new MutationMeddley_EvolutionChoice(
                    "sacred_reservoir",
                    "Sacred Reservoir",
                    "Your body becomes an estuary that refuses depletion.",
                    9,
                    3,
                    "tidal_marrows",
                    "Capstone sustain path with broad elemental stability."
                ),
                new MutationMeddley_EvolutionChoice(
                    "cathedral_of_salt",
                    "Cathedral of Salt",
                    "Your shell rises in heavy, brilliant terraces of mineral armor.",
                    9,
                    3,
                    "saltglass_bastion",
                    "Capstone control path with extreme hardening."
                ),
                new MutationMeddley_EvolutionChoice(
                    "whitewater_predator",
                    "Whitewater Predator",
                    "You hunt as a moving front of abrasion and hungry salinity.",
                    9,
                    3,
                    "desiccant_wake",
                    "Capstone mobility path with extreme environmental adaptation."
                )
            };
        }

        protected override List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices()
        {
            if (MutationMeddley_HasEvolution("wellspring_flesh"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("draw_brine", "Draw Brine", "Favor even, restorative saline circulation."),
                    new MutationMeddley_ModeChoice("cool_reserve", "Cool Reserve", "Hold deep mineral chill against hostile climates.")
                };
            }

            if (MutationMeddley_HasEvolution("saltglass_bloom"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("shell_up", "Shell Up", "Accrete a thick saltglass shell."),
                    new MutationMeddley_ModeChoice("knife_rind", "Knife Rind", "Keep the shell thinner and sharper at the edges.")
                };
            }

            if (MutationMeddley_HasEvolution("scouring_estuary"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("brackish_sprint", "Brackish Sprint", "Turn abrasive pressure into speed."),
                    new MutationMeddley_ModeChoice("dry_tide", "Dry Tide", "Lean into harsh climate tolerance while moving carefully.")
                };
            }

            return new List<MutationMeddley_ModeChoice>();
        }

        protected override void MutationMeddley_RefreshPassiveEffects()
        {
            MutationMeddley_ClearCommonStatShifts();

            if (MutationMeddley_HasEvolution("wellspring_flesh"))
            {
                int depth = MutationMeddley_GetPathDepth("wellspring_flesh", "tidal_marrows", "sacred_reservoir");
                MutationMeddley_SetShift("HeatResistance", 5 + (10 * depth));
                MutationMeddley_SetShift("ColdResistance", 5 + (10 * depth));

                if (MutationMeddley_ModeState == "draw_brine")
                {
                    MutationMeddley_SetShift("MoveSpeed", 5 * depth);
                }
                else
                {
                    MutationMeddley_SetShift("DV", 1 + depth);
                }
            }
            else if (MutationMeddley_HasEvolution("saltglass_bloom"))
            {
                int depth = MutationMeddley_GetPathDepth("saltglass_bloom", "saltglass_bastion", "cathedral_of_salt");
                MutationMeddley_SetShift("AV", 1 + depth);

                if (MutationMeddley_ModeState == "knife_rind")
                {
                    MutationMeddley_SetShift("DV", depth);
                }
                else
                {
                    MutationMeddley_SetShift("MoveSpeed", -10 * depth);
                }
            }
            else if (MutationMeddley_HasEvolution("scouring_estuary"))
            {
                int depth = MutationMeddley_GetPathDepth("scouring_estuary", "desiccant_wake", "whitewater_predator");
                MutationMeddley_SetShift("MoveSpeed", 10 + (10 * depth));

                if (MutationMeddley_ModeState == "dry_tide")
                {
                    MutationMeddley_SetShift("HeatResistance", 10 + (5 * depth));
                    MutationMeddley_SetShift("ColdResistance", 10 + (5 * depth));
                }
                else
                {
                    MutationMeddley_SetShift("DV", 1 + depth);
                }
            }
        }
    }
}
