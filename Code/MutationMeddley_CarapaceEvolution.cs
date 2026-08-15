using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_CarapaceEvolution : MutationMeddley_AdaptiveMutationBase
    {
        private const string MutationMeddley_MovedKey = "carapace_moved";
        private const string MutationMeddley_StationaryKey = "carapace_stationary";

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

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "EndTurn");
            Object.RegisterPartEvent(this, "EnteredCell");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EnteredCell")
            {
                MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 1);
                MutationMeddley_RefreshPassiveEffects();
            }
            else if (E.ID == "EndTurn")
            {
                MutationMeddley_SetStateInt(
                    MutationMeddley_StationaryKey,
                    MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) == 0 ? 1 : 0
                );
                MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 0);
                MutationMeddley_RefreshPassiveEffects();
            }

            return base.FireEvent(E);
        }

        public override string GetDescription()
        {
            return "A Mutation Meddley companion evolution intended to pair with vanilla Carapace.\n\n"
                + "This mutation does not replace vanilla Carapace. Instead, it offers a separate branching shell-specialization layer that can be taken alongside it.";
        }

        public override string GetLevelText(int Level)
        {
            string intro = "Requires vanilla Carapace to activate.\n"
                + "Rank 3: choose the shell's identity.\n"
                + "Rank 6: specialize the shell.\n"
                + "Rank 9: claim its capstone.\n\n";

            if (!MutationMeddley_IsFunctionallyActive())
            {
                intro += "Dormant: vanilla Carapace is not currently present.\n\n";
            }

            return intro
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState();
        }

        protected override bool MutationMeddley_IsFunctionallyActive()
        {
            return MutationMeddley_HasVanillaCarapace();
        }

        protected override string MutationMeddley_GetInactiveReason()
        {
            return "Carapace Evolution is dormant.\n\nTake vanilla Carapace first, then evolve the shell through Mutation Meddley.";
        }

        protected override List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices()
        {
            return new List<MutationMeddley_EvolutionChoice>
            {
                new MutationMeddley_EvolutionChoice(
                    "fortress",
                    "Fortress",
                    "Become denser, more rooted, and brutally difficult to dislodge.",
                    3,
                    1,
                    detailText: "Static defense identity."
                ),
                new MutationMeddley_EvolutionChoice(
                    "hunter_shell",
                    "Hunter Shell",
                    "Articulate the shell for pursuit and close-range pressure.",
                    3,
                    1,
                    detailText: "Predatory shell identity."
                ),
                new MutationMeddley_EvolutionChoice(
                    "adaptive_carapace",
                    "Adaptive Carapace",
                    "Retune the shell toward weather, exposure, and hostile environments.",
                    3,
                    1,
                    detailText: "Environmental shell identity."
                ),

                new MutationMeddley_EvolutionChoice(
                    "faceted_keep",
                    "Faceted Keep",
                    "Hold contact lines by making the shell broad and punishing.",
                    6,
                    2,
                    "fortress",
                    "Fortress specialization for adjacent pressure."
                ),
                new MutationMeddley_EvolutionChoice(
                    "entrenched_bastion",
                    "Entrenched Bastion",
                    "Root the shell until stillness becomes its own defense.",
                    6,
                    2,
                    "fortress",
                    "Fortress specialization for planted defense."
                ),
                new MutationMeddley_EvolutionChoice(
                    "ravager_joints",
                    "Ravager Joints",
                    "Segment the shell for pursuit and close pursuit angles.",
                    6,
                    2,
                    "hunter_shell",
                    "Hunter specialization for sticky melee pursuit."
                ),
                new MutationMeddley_EvolutionChoice(
                    "spur_lattice",
                    "Spur Lattice",
                    "Use the shell as a forward-leaning killing frame rather than a pure chase tool.",
                    6,
                    2,
                    "hunter_shell",
                    "Hunter specialization for harder contact."
                ),
                new MutationMeddley_EvolutionChoice(
                    "thermal_baffles",
                    "Thermal Baffles",
                    "Grow reactive channels that steer heat and cold through the shell.",
                    6,
                    2,
                    "adaptive_carapace",
                    "Adaptive specialization for climate response."
                ),
                new MutationMeddley_EvolutionChoice(
                    "mire_sheath",
                    "Mire Sheath",
                    "Tune the shell to foul ground, liquid contact, and slogging weather.",
                    6,
                    2,
                    "adaptive_carapace",
                    "Adaptive specialization for terrain contact."
                ),

                new MutationMeddley_EvolutionChoice(
                    "living_fortress",
                    "Living Fortress",
                    "Your shell becomes a held line that refuses collapse.",
                    9,
                    3,
                    "faceted_keep",
                    "Capstone contact-defense line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "redoubt_engine",
                    "Redoubt Engine",
                    "Stillness hardens the shell into a patient defensive machine.",
                    9,
                    3,
                    "entrenched_bastion",
                    "Capstone rooted-defense line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "pursuit_predator",
                    "Pursuit Predator",
                    "Your shell becomes a continuous frame for pressure and chase.",
                    9,
                    3,
                    "ravager_joints",
                    "Capstone pursuit line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "hooked_pursuer",
                    "Hooked Pursuer",
                    "Your shell leans harder into committed, close-range violence.",
                    9,
                    3,
                    "spur_lattice",
                    "Capstone contact-hunter line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "storm_carapace",
                    "Storm Carapace",
                    "Climate and exposure become something the shell continually rebalances.",
                    9,
                    3,
                    "thermal_baffles",
                    "Capstone climate line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "bog_shell",
                    "Bog Shell",
                    "The shell learns to thrive when the ground is foul, wet, or choking.",
                    9,
                    3,
                    "mire_sheath",
                    "Capstone terrain-contact line."
                )
            };
        }

        protected override List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices()
        {
            if (MutationMeddley_HasEvolution("fortress"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("anchor_down", "Anchor Down", "Favor planted defense."),
                    new MutationMeddley_ModeChoice("spiteful_wall", "Spiteful Wall", "Favor defensive contact pressure.")
                };
            }

            if (MutationMeddley_HasEvolution("hunter_shell"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("skirmish_gait", "Skirmish Gait", "Favor pursuit and timing."),
                    new MutationMeddley_ModeChoice("ramming_gait", "Ramming Gait", "Favor committed contact.")
                };
            }

            if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("ember_veil", "Ember Veil", "Favor heat and dry exposure."),
                    new MutationMeddley_ModeChoice("rime_veil", "Rime Veil", "Favor cold and foul contact.")
                };
            }

            return new List<MutationMeddley_ModeChoice>();
        }

        protected override void MutationMeddley_RefreshPassiveEffects()
        {
            MutationMeddley_ClearCommonStatShifts();

            if (!MutationMeddley_IsFunctionallyActive())
            {
                return;
            }

            bool engaged = ParentObject != null && ParentObject.IsEngagedInMelee();
            bool stationary = MutationMeddley_GetStateInt(MutationMeddley_StationaryKey, 0) > 0;
            bool wetGround = MutationMeddley_IsWetGround();

            if (MutationMeddley_HasEvolution("fortress"))
            {
                MutationMeddley_SetShift("AV", 1);

                if (MutationMeddley_HasEvolution("faceted_keep"))
                {
                    MutationMeddley_SetShift("AV", engaged ? 4 : 2);
                    MutationMeddley_SetShift("DV", MutationMeddley_HasEvolution("living_fortress") && engaged ? 2 : 1);
                }
                else if (MutationMeddley_HasEvolution("entrenched_bastion"))
                {
                    MutationMeddley_SetShift("AV", stationary ? 5 : 2);
                    if (MutationMeddley_HasEvolution("redoubt_engine") && stationary)
                    {
                        MutationMeddley_SetShift("DV", 2);
                    }
                }
                else
                {
                    MutationMeddley_SetShift("AV", 2);
                }
            }
            else if (MutationMeddley_HasEvolution("hunter_shell"))
            {
                MutationMeddley_SetShift("DV", 1);

                if (MutationMeddley_HasEvolution("ravager_joints"))
                {
                    MutationMeddley_SetShift("Quickness", MutationMeddley_HasEvolution("pursuit_predator") ? 4 : 2);
                    MutationMeddley_SetShift("DV", engaged ? 3 : 2);
                }
                else if (MutationMeddley_HasEvolution("spur_lattice"))
                {
                    MutationMeddley_SetShift("AV", engaged ? (MutationMeddley_HasEvolution("hooked_pursuer") ? 3 : 2) : 1);
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else
                {
                    MutationMeddley_SetShift("Quickness", 1);
                    MutationMeddley_SetShift("DV", 2);
                }
            }
            else if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                if (MutationMeddley_HasEvolution("thermal_baffles"))
                {
                    MutationMeddley_SetShift(
                        "HeatResistance",
                        MutationMeddley_GetCurrentModeId() == "ember_veil"
                            ? (MutationMeddley_HasEvolution("storm_carapace") ? 35 : 20)
                            : 5
                    );
                    MutationMeddley_SetShift(
                        "ColdResistance",
                        MutationMeddley_GetCurrentModeId() == "rime_veil"
                            ? (MutationMeddley_HasEvolution("storm_carapace") ? 35 : 20)
                            : 5
                    );
                    MutationMeddley_SetShift("DV", 1 + (MutationMeddley_HasEvolution("storm_carapace") ? 1 : 0));
                }
                else if (MutationMeddley_HasEvolution("mire_sheath"))
                {
                    MutationMeddley_SetShift("DV", wetGround ? 3 : 1);
                    MutationMeddley_SetShift("AV", wetGround && MutationMeddley_HasEvolution("bog_shell") ? 2 : 0);
                    MutationMeddley_SetShift("ColdResistance", wetGround ? 10 : 0);
                }
                else
                {
                    MutationMeddley_SetShift("HeatResistance", MutationMeddley_GetCurrentModeId() == "ember_veil" ? 15 : 5);
                    MutationMeddley_SetShift("ColdResistance", MutationMeddley_GetCurrentModeId() == "rime_veil" ? 15 : 5);
                }
            }
        }

        private bool MutationMeddley_HasVanillaCarapace()
        {
            if (ParentObject == null)
            {
                return false;
            }

            global::XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations")
                as global::XRL.World.Parts.Mutations;

            return mutations != null
                && mutations.GetMutationByName("Carapace") != null;
        }

        private bool MutationMeddley_IsWetGround()
        {
            if (ParentObject == null || ParentObject.CurrentCell == null)
            {
                return false;
            }

            object liquid = ParentObject.CurrentCell.GetOpenLiquidVolume();
            if (liquid != null)
            {
                return true;
            }

            string cellDescription = ParentObject.CurrentCell.ToString();
            if (string.IsNullOrEmpty(cellDescription))
            {
                return false;
            }

            string loweredTerrain = cellDescription.ToLowerInvariant();
            return loweredTerrain.Contains("water")
                || loweredTerrain.Contains("pool")
                || loweredTerrain.Contains("mire")
                || loweredTerrain.Contains("bog")
                || loweredTerrain.Contains("marsh");
        }
    }
}
