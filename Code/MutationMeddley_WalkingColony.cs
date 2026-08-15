using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_WalkingColony : MutationMeddley_AdaptiveMutationBase
    {
        private const string MutationMeddley_ColonyKey = "colony_charge";
        private const string MutationMeddley_MovedKey = "colony_moved";
        private const string MutationMeddley_StreakKey = "colony_stride_streak";
        private const string MutationMeddley_StitchKey = "colony_stitch";
        private const string MutationMeddley_ScoutKey = "colony_scout";
        private const string MutationMeddley_ParliamentKey = "colony_parliament";
        private const string MutationMeddley_MoltUnlockedKey = "colony_hidden_molt";
        private const string MutationMeddley_MoltProgressKey = "colony_hidden_molt_progress";
        private const string MutationMeddley_WakeTrailUnlockedKey = "colony_hidden_waketrail";
        private const string MutationMeddley_WakeTrailProgressKey = "colony_hidden_waketrail_progress";
        private const string MutationMeddley_BurrowedUnlockedKey = "colony_hidden_burrowed";
        private const string MutationMeddley_BurrowedProgressKey = "colony_hidden_burrowed_progress";
        private const string MutationMeddley_ChoirUnlockedKey = "colony_hidden_choir";
        private const string MutationMeddley_ChoirProgressKey = "colony_hidden_choir_progress";
        private const int MutationMeddley_MaxColony = 6;

        public override string MutationMeddley_EvolutionDisplayName
        {
            get { return "Walking Colony"; }
        }

        protected override string MutationMeddley_ModeAbilityName
        {
            get { return "Retune Walking Colony"; }
        }

        protected override string MutationMeddley_ModeAbilityDescription
        {
            get { return "Shift the colony between recovery, ranging, and distributed-body priorities."; }
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
                MutationMeddley_ProcessColonyTurn();
            }

            return base.FireEvent(E);
        }

        public override string GetDescription()
        {
            return "A living colony now redistributes labor, recovery, and pressure across your body.\n\n"
                + "Walking Colony is a symbiotic body-plan mutation about cadence, distributed burden, anatomy-aware sustain, and pursuit ecology.";
        }

        public override string GetLevelText(int Level)
        {
            return "Rank 3: choose how the colony organizes your body.\n"
                + "Rank 6: specialize the hive, swarm, or parliament.\n"
                + "Rank 9: secure the colonial capstone.\n\n"
                + MutationMeddley_GetUsageSummary()
                + "\n\n"
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState()
                + "\n"
                + "Colony pressure: "
                + MutationMeddley_GetColonyPressure()
                + "/"
                + MutationMeddley_GetMaxColonyPressure()
                + "\n"
                + MutationMeddley_GetCurrentMechanicsSummary()
                + "\n"
                + MutationMeddley_GetPassiveBonusSummary()
                + "\n"
                + MutationMeddley_GetSynergySummary();
        }

        protected override IEnumerable<string> MutationMeddley_GetCurrentMechanicNotes()
        {
            yield return "Colony pressure builds from movement and drains when you stay inert.";

            if (MutationMeddley_HasEvolution("marrow_hive"))
            {
                yield return "Marrow Hive turns pressure into stitch reserve. Knit Flesh only spends when healing can actually happen; Bank Scars turns stillness into tougher recovery windows.";
                yield return "Current stitch: " + MutationMeddley_GetStateInt(MutationMeddley_StitchKey, 0) + ".";
            }
            else if (MutationMeddley_HasEvolution("surveyor_swarm"))
            {
                yield return "Surveyor Swarm turns movement into scout pressure. Range Ahead likes hostile ground; Harry Line spends pressure into chase control.";
                yield return "Current scout pressure: " + MutationMeddley_GetStateInt(MutationMeddley_ScoutKey, 0) + ".";
            }
            else if (MutationMeddley_HasEvolution("graft_parliament"))
            {
                yield return "Graft Parliament stores delegated load while your body plan is crowded. Delegate Load favors composure; Override Frame favors opportunistic body theft.";
                yield return "Current delegated load: " + MutationMeddley_GetStateInt(MutationMeddley_ParliamentKey, 0) + ".";
            }
            else
            {
                yield return "Choose a rank-3 colony identity first to unlock pressure-spending behavior.";
            }
        }

        protected override List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices()
        {
            return new List<MutationMeddley_EvolutionChoice>
            {
                new MutationMeddley_EvolutionChoice(
                    "marrow_hive",
                    "Marrow Hive",
                    "The colony nests deep and reinforces durability through internal husbandry.",
                    3,
                    1,
                    detailText: "Sustain identity. Wants attrition, recovery windows, and bodily persistence."
                ),
                new MutationMeddley_EvolutionChoice(
                    "surveyor_swarm",
                    "Surveyor Swarm",
                    "The colony spreads scouts through your stride and reads the world by movement.",
                    3,
                    1,
                    detailText: "Mobile identity. Wants repeated movement, pursuit, and traversal."
                ),
                new MutationMeddley_EvolutionChoice(
                    "graft_parliament",
                    "Graft Parliament",
                    "The colony becomes a distributed decision-maker that treats your frame as negotiable.",
                    3,
                    1,
                    detailText: "Body-plan identity. Wants structural burden, body mutation overlap, and tactical repositioning."
                ),
                new MutationMeddley_EvolutionChoice(
                    "bone_nursery",
                    "Bone Nursery",
                    "The colony seeds recovery in deep structural tissue and waits for strain to feed it.",
                    6,
                    2,
                    "marrow_hive",
                    "Best when you absorb pressure and keep enough colony mass in reserve."
                ),
                new MutationMeddley_EvolutionChoice(
                    "scar_feeders",
                    "Scar Feeders",
                    "Damage and recovery become edible history for the colony.",
                    6,
                    2,
                    "marrow_hive",
                    "Best when you trade clean tempo for stubborn persistence."
                ),
                new MutationMeddley_EvolutionChoice(
                    "tendon_scouts",
                    "Tendon Scouts",
                    "Movement routes become the colony's map and first source of advantage.",
                    6,
                    2,
                    "surveyor_swarm",
                    "Best when you keep remapping lines through contested ground."
                ),
                new MutationMeddley_EvolutionChoice(
                    "latch_runners",
                    "Latch Runners",
                    "Your pursuit line carries colonial pressure forward from cell to cell.",
                    6,
                    2,
                    "surveyor_swarm",
                    "Best when you refuse long stationary breaks."
                ),
                new MutationMeddley_EvolutionChoice(
                    "nerve_delegation",
                    "Nerve Delegation",
                    "The colony learns to distribute tension and reaction timing across the frame.",
                    6,
                    2,
                    "graft_parliament",
                    "Best when the body stays composed under layered demands."
                ),
                new MutationMeddley_EvolutionChoice(
                    "borrowed_hands",
                    "Borrowed Hands",
                    "The colony treats worn structure and limb burden as negotiable labor.",
                    6,
                    2,
                    "graft_parliament",
                    "Best when body-plan pressure becomes something to exploit."
                ),
                new MutationMeddley_EvolutionChoice(
                    "ossuary_bloom",
                    "Ossuary Bloom",
                    "The colony turns deep structure into a patient recovery garden.",
                    9,
                    3,
                    "bone_nursery",
                    "Capstone deep sustain line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "burrowed_nursery",
                    "Burrowed Nursery",
                    "The colony learns to stabilize itself through subterranean recovery and traversal.",
                    9,
                    3,
                    "bone_nursery",
                    "UNUSUAL ADAPTATION. Requires repeated recovery-routing while carrying Burrowing Claws.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "catacomb_metabolism",
                    "Catacomb Metabolism",
                    "Scars and recovery become a slow but inexhaustible colonial ration.",
                    9,
                    3,
                    "scar_feeders",
                    "Capstone scar-economy line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "march_cartography",
                    "March Cartography",
                    "The colony now remembers where motion becomes advantage.",
                    9,
                    3,
                    "tendon_scouts",
                    "Capstone route-mapping line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "pack_pursuit",
                    "Pack Pursuit",
                    "Repeated movement compounds the colony's pressure on anything trying to escape you.",
                    9,
                    3,
                    "latch_runners",
                    "Capstone chase-pressure line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "wake_trail",
                    "Wake Trail",
                    "Your movement history itself becomes a colonial hunting line.",
                    9,
                    3,
                    "latch_runners",
                    "UNUSUAL ADAPTATION. Requires repeated high-cadence movement through hostile terrain.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "distributed_verdict",
                    "Distributed Verdict",
                    "The colony turns composure under strain into coordinated judgment.",
                    9,
                    3,
                    "nerve_delegation",
                    "Capstone composure line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "choir_of_tendons",
                    "Choir of Tendons",
                    "Composed strain teaches the colony to coordinate the whole body like a rhythmic instrument.",
                    9,
                    3,
                    "nerve_delegation",
                    "UNUSUAL ADAPTATION. Requires repeated composed turns while carrying resonance-friendly anatomy.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "colony_interface",
                    "Colony Interface",
                    "The colony fully adopts your burdened frame as shared machinery.",
                    9,
                    3,
                    "borrowed_hands",
                    "Capstone body-interface line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "molt_parliament",
                    "Molt Parliament",
                    "Worn structure is no longer separate from the colony's own body politic.",
                    9,
                    3,
                    "borrowed_hands",
                    "UNUSUAL ADAPTATION. Requires sustained structural strain while carrying shell-compatible anatomy.",
                    true
                )
            };
        }

        protected override IEnumerable<string> MutationMeddley_GetIntrinsicSemanticTags()
        {
            return new string[] { "SYMBIOTIC", "BODY_PLAN", "BIOLOGICAL", "COLONIAL", "BODY_PART_INTERACTION" };
        }

        protected override IEnumerable<string> MutationMeddley_GetEvolutionSemanticTags()
        {
            List<string> tags = new List<string>();

            if (MutationMeddley_HasEvolution("marrow_hive"))
            {
                tags.Add("REGENERATIVE");
                tags.Add("STRUCTURAL");
            }

            if (MutationMeddley_HasEvolution("surveyor_swarm"))
            {
                tags.Add("MOBILE");
                tags.Add("PURSUIT");
                tags.Add("TERRAIN_INTERACTION");
            }

            if (MutationMeddley_HasEvolution("graft_parliament"))
            {
                tags.Add("CONTROL");
                tags.Add("BODY_PART_INTERACTION");
                tags.Add("STRUCTURAL");
            }

            return tags;
        }

        protected override List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices()
        {
            if (MutationMeddley_HasEvolution("marrow_hive"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("knit_flesh", "Knit Flesh", "Spend colony mass on direct recovery and holding together."),
                    new MutationMeddley_ModeChoice("bank_scars", "Bank Scars", "Hold colonial pressure for deeper structural payoff.")
                };
            }

            if (MutationMeddley_HasEvolution("surveyor_swarm"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("range_ahead", "Range Ahead", "Favor route reading and rapid remapping."),
                    new MutationMeddley_ModeChoice("harry_line", "Harry Line", "Favor persistent pursuit pressure.")
                };
            }

            if (MutationMeddley_HasEvolution("graft_parliament"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("delegate_load", "Delegate Load", "Favor calm redistribution of strain."),
                    new MutationMeddley_ModeChoice("override_frame", "Override Frame", "Favor aggressive use of a burdened body plan.")
                };
            }

            return new List<MutationMeddley_ModeChoice>();
        }

        protected override List<MutationMeddley_SynergyDefinition> MutationMeddley_GetSynergyDefinitions()
        {
            return new List<MutationMeddley_SynergyDefinition>
            {
                new MutationMeddley_SynergyDefinition("regeneration", "Regeneration", "The colony becomes harder to dislodge once recovery loops begin."),
                new MutationMeddley_SynergyDefinition("multiple_legs", "Multiple Legs", "Extra locomotion gives the colony more routing data and chase leverage."),
                new MutationMeddley_SynergyDefinition("quills", "Quills", "Reactive anatomy gives the colony more dangerous surface labor."),
                new MutationMeddley_SynergyDefinition("burrowing_claws", "Burrowing Claws", "Subterranean routes become a direct colonial resource."),
                new MutationMeddley_SynergyDefinition("amphibious", "Amphibious", "Wet routing changes how the colony spends movement and recovery pressure."),
                new MutationMeddley_SynergyDefinition("heightened_hearing", "Heightened Hearing", "The colony reads vibration and timing more cleanly."),
                new MutationMeddley_SynergyDefinition("phasing", "Phasing", "Distributed burden becomes stranger when parts of the body slip out of sync."),
                new MutationMeddley_SynergyDefinition("carapace", "Carapace", "A live shell changes how the colony negotiates burden even before shell evolution."),
                new MutationMeddley_SynergyDefinition("ash_pair", "Ash Metabolism", "Heat and colonial pressure start recycling each other."),
                new MutationMeddley_SynergyDefinition("living_crystal_pair", "Living Crystal", "Crystalline structure gives the colony a harsher but cleaner frame to inhabit."),
                new MutationMeddley_SynergyDefinition("brineborn_pair", "Brineborn", "Saline routes and colonial movement start reinforcing each other."),
                new MutationMeddley_SynergyDefinition("carapace_pair", "Carapace Evolution", "A live shell gives the colony better structure to negotiate against."),
                new MutationMeddley_SynergyDefinition("ossuary_rampart", "Ossuary Rampart", "Shell, crystal, and marrow ecology thicken into one anti-burst wall.", isTriad: true),
                new MutationMeddley_SynergyDefinition("drift_parliament", "Drift Parliament", "Movement history, shallow-liquid routing, and colonial pressure become one chase logic.", isTriad: true),
                new MutationMeddley_SynergyDefinition("undertow_furnace", "Undertow Furnace", "Reserve, recovery, and heat debt become one survival engine.", isTriad: true),
                new MutationMeddley_SynergyDefinition("bone_kiln_parliament", "Bone Kiln Parliament", "Colonial burden, heat, and shell structure become one heavy frame.", isTriad: true),
                new MutationMeddley_SynergyDefinition("resonant_undertow", "Resonant Undertow", "Cadence, reserve, and movement history begin feeding one another.", isTriad: true),
                new MutationMeddley_SynergyDefinition("chorus_husk", "Chorus Husk", "Distributed strain, rhythm, and membrane-shell logic become one unstable body.", isTriad: true),
                new MutationMeddley_SynergyDefinition("whitewater_ossuary", "Whitewater Ossuary", "Harsh traversal and deep sustain form a slow, relentless wall.", isTriad: true),
                new MutationMeddley_SynergyDefinition("blackglass_pursuit", "Blackglass Pursuit", "Route memory, pursuit, and impact now hunt as one frame.", isTriad: true),
                new MutationMeddley_SynergyDefinition("burrowed_nursery_state", "Burrowed Nursery", "The colony now treats burrowing recovery routes as home territory.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("wake_trail_state", "Wake Trail", "Your movement history now carries a colonial hunting trace.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("molt_parliament_state", "Molt Parliament", "The colony has started treating worn structure as part of itself.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("choir_of_tendons_state", "Choir of Tendons", "The colony now coordinates strain like a rhythmic body-choir.", isUnusual: true)
            };
        }

        protected override bool MutationMeddley_IsSynergyActive(MutationMeddley_SynergyDefinition synergy)
        {
            switch (synergy.Id)
            {
                case "regeneration":
                    return MutationMeddley_HasMutation("Regeneration") && MutationMeddley_HasEvolution("marrow_hive");
                case "multiple_legs":
                    return MutationMeddley_HasMutation("Multiple Legs") && MutationMeddley_HasEvolution("surveyor_swarm");
                case "quills":
                    return MutationMeddley_HasMutation("Quills")
                        && (MutationMeddley_HasEvolution("graft_parliament") || MutationMeddley_HasEvolution("marrow_hive"));
                case "burrowing_claws":
                    return MutationMeddley_HasMutation("Burrowing Claws")
                        && (MutationMeddley_HasEvolution("surveyor_swarm") || MutationMeddley_HasEvolution("marrow_hive"));
                case "amphibious":
                    return MutationMeddley_HasMutation("Amphibious")
                        && (MutationMeddley_HasEvolution("surveyor_swarm") || MutationMeddley_HasEvolution("marrow_hive"));
                case "heightened_hearing":
                    return MutationMeddley_HasMutation("Heightened Hearing")
                        && (MutationMeddley_HasEvolution("graft_parliament") || MutationMeddley_HasEvolution("surveyor_swarm"));
                case "phasing":
                    return MutationMeddley_HasMutation("Phasing") && MutationMeddley_HasEvolution("graft_parliament");
                case "carapace":
                    return MutationMeddley_HasMutation("Carapace")
                        && (MutationMeddley_HasEvolution("marrow_hive") || MutationMeddley_HasEvolution("graft_parliament"));
                case "ash_pair":
                    return MutationMeddley_HasMutation("Ash Metabolism") && MutationMeddley_HasAnyEvolution();
                case "living_crystal_pair":
                    return MutationMeddley_HasMutation("Living Crystal") && MutationMeddley_HasAnyEvolution();
                case "brineborn_pair":
                    return MutationMeddley_HasMutation("Brineborn") && MutationMeddley_HasAnyEvolution();
                case "carapace_pair":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution") && MutationMeddley_HasAnyEvolution();
                case "ossuary_rampart":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("marrow_hive")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice");
                case "drift_parliament":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("surveyor_swarm")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary");
                case "undertow_furnace":
                    return MutationMeddley_HasEvolution("marrow_hive")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "wellspring_flesh")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "cinder_gut");
                case "bone_kiln_parliament":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("graft_parliament")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "furnace_skin")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress");
                case "resonant_undertow":
                    return MutationMeddley_HasEvolution("surveyor_swarm")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "wellspring_flesh");
                case "chorus_husk":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("graft_parliament")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace");
                case "whitewater_ossuary":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("marrow_hive")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress");
                case "blackglass_pursuit":
                    return MutationMeddley_HasEvolution("surveyor_swarm")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "cinder_gut")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice");
                case "burrowed_nursery_state":
                    return MutationMeddley_HasEvolution("burrowed_nursery");
                case "wake_trail_state":
                    return MutationMeddley_HasEvolution("wake_trail");
                case "molt_parliament_state":
                    return MutationMeddley_HasEvolution("molt_parliament");
                case "choir_of_tendons_state":
                    return MutationMeddley_HasEvolution("choir_of_tendons");
                default:
                    return false;
            }
        }

        protected override bool MutationMeddley_IsChoiceUnlocked(MutationMeddley_EvolutionChoice choice)
        {
            if (!choice.IsUnusual)
            {
                return true;
            }

            if (choice.Id == "molt_parliament")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_MoltUnlockedKey);
            }

            if (choice.Id == "wake_trail")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_WakeTrailUnlockedKey);
            }

            if (choice.Id == "burrowed_nursery")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_BurrowedUnlockedKey);
            }

            if (choice.Id == "choir_of_tendons")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_ChoirUnlockedKey);
            }

            return false;
        }

        protected override void MutationMeddley_RefreshPassiveEffects()
        {
            MutationMeddley_ClearCommonStatShifts();

            int pressure = MutationMeddley_GetColonyPressure();
            bool moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0;
            bool hostileTerrain = MutationMeddley_IsCurrentCellHostileTraversal();
            int stitch = MutationMeddley_GetStateInt(MutationMeddley_StitchKey, 0);
            int scout = MutationMeddley_GetStateInt(MutationMeddley_ScoutKey, 0);
            int parliament = MutationMeddley_GetStateInt(MutationMeddley_ParliamentKey, 0);

            if (MutationMeddley_HasEvolution("marrow_hive"))
            {
                MutationMeddley_SetShift("Toughness", 1);
                MutationMeddley_SetShift("AV", 1 + (pressure / 3) + stitch);
                MutationMeddley_SetShift("DV", (pressure / 4) + (stitch / 2));

                if (MutationMeddley_HasEvolution("bone_nursery"))
                {
                    MutationMeddley_SetShift("AV", 1 + (pressure / 2) + stitch);
                    MutationMeddley_SetShift("ColdResistance", 5 + (pressure * 2));
                }
                else if (MutationMeddley_HasEvolution("scar_feeders"))
                {
                    MutationMeddley_SetShift("HeatResistance", 5 + (pressure * 2));
                    MutationMeddley_SetShift("DV", 1 + (pressure / 4) + (stitch / 2));
                }

                if (MutationMeddley_HasEvolution("ossuary_bloom"))
                {
                    MutationMeddley_SetShift("AV", 1 + (stitch / 2));
                }

                if (MutationMeddley_HasEvolution("catacomb_metabolism"))
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }
            else if (MutationMeddley_HasEvolution("surveyor_swarm"))
            {
                MutationMeddley_SetShift("Agility", 1);
                MutationMeddley_SetShift("Quickness", 1 + (pressure / 3) + scout);
                MutationMeddley_SetShift("DV", (moved ? 1 + (pressure / 4) : pressure / 5) + (scout / 2));

                if (MutationMeddley_HasEvolution("tendon_scouts"))
                {
                    MutationMeddley_SetShift("DV", (moved ? 2 + (pressure / 4) : 1) + scout);
                }
                else if (MutationMeddley_HasEvolution("latch_runners"))
                {
                    MutationMeddley_SetShift("Quickness", (moved ? 2 + (pressure / 3) : 1) + scout);
                    MutationMeddley_SetShift("DV", (MutationMeddley_GetCurrentModeId() == "harry_line" && pressure > 0 ? 1 : 0) + (scout / 2));
                }

                if (MutationMeddley_HasEvolution("march_cartography") && hostileTerrain)
                {
                    MutationMeddley_SetShift("DV", 1);
                }

                if (MutationMeddley_HasEvolution("pack_pursuit") && moved)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }
            else if (MutationMeddley_HasEvolution("graft_parliament"))
            {
                MutationMeddley_SetShift("Intelligence", 1);
                MutationMeddley_SetShift("DV", 1 + (pressure / 4) + parliament);

                if (MutationMeddley_HasOtherMutationWithTag("BODY_PART_INTERACTION"))
                {
                    MutationMeddley_SetShift("AV", 1);
                }

                if (MutationMeddley_HasEvolution("nerve_delegation"))
                {
                    MutationMeddley_SetShift("DV", 1 + (pressure / 3) + parliament);
                    MutationMeddley_SetShift("Quickness", (MutationMeddley_GetCurrentModeId() == "delegate_load" ? 1 : 0) + (parliament / 2));
                }
                else if (MutationMeddley_HasEvolution("borrowed_hands"))
                {
                    MutationMeddley_SetShift("AV", 1 + (pressure / 4) + (parliament / 2));
                    MutationMeddley_SetShift("Quickness", (MutationMeddley_GetCurrentModeId() == "override_frame" ? 1 + (pressure / 5) : 0) + parliament);
                }

                if (MutationMeddley_HasEvolution("distributed_verdict"))
                {
                    MutationMeddley_SetShift("DV", 1);
                }

                if (MutationMeddley_HasEvolution("colony_interface"))
                {
                    MutationMeddley_SetShift("AV", 1);
                }
            }

            if (MutationMeddley_HasMutation("Regeneration") && MutationMeddley_HasEvolution("marrow_hive"))
            {
                MutationMeddley_SetShift("AV", pressure > 0 ? 1 : 0);
            }

            if (MutationMeddley_HasMutation("Multiple Legs") && MutationMeddley_HasEvolution("surveyor_swarm") && moved)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_HasMutation("Quills") && MutationMeddley_HasEvolution("graft_parliament"))
            {
                MutationMeddley_SetShift("AV", 1);
            }

            if (MutationMeddley_HasMutation("Burrowing Claws")
                && (MutationMeddley_HasEvolution("marrow_hive") || MutationMeddley_HasEvolution("surveyor_swarm")))
            {
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasMutation("Amphibious") && MutationMeddley_IsCurrentCellWet())
            {
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasMutation("Heightened Hearing"))
            {
                if (MutationMeddley_HasEvolution("surveyor_swarm"))
                {
                    MutationMeddley_SetShift("Quickness", moved ? 1 : 0);
                }
                else if (MutationMeddley_HasEvolution("graft_parliament"))
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_HasMutation("Phasing") && MutationMeddley_HasEvolution("graft_parliament"))
            {
                MutationMeddley_SetShift("DV", 1);
                MutationMeddley_SetShift("Quickness", MutationMeddley_GetCurrentModeId() == "override_frame" ? 1 : 0);
            }

            if (MutationMeddley_HasMutation("Carapace") && MutationMeddley_HasEvolution("marrow_hive"))
            {
                MutationMeddley_SetShift("AV", 1);
            }

            if (MutationMeddley_HasMutation("Ash Metabolism"))
            {
                if (MutationMeddley_HasEvolution("surveyor_swarm") && moved)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("graft_parliament") && MutationMeddley_IsCurrentCellHot())
                {
                    MutationMeddley_SetShift("AV", 1);
                }
            }

            if (MutationMeddley_HasMutation("Living Crystal"))
            {
                if (MutationMeddley_HasEvolution("marrow_hive")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice"))
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("surveyor_swarm")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("graft_parliament")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix"))
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_HasMutation("Brineborn"))
            {
                if (MutationMeddley_HasEvolution("surveyor_swarm") && MutationMeddley_IsCurrentCellWet())
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("marrow_hive") && MutationMeddley_IsCurrentCellSaline())
                {
                    MutationMeddley_SetShift("ColdResistance", 5);
                }
            }

            if (MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution"))
            {
                if (MutationMeddley_HasEvolution("marrow_hive")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress"))
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("surveyor_swarm")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("graft_parliament")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace"))
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_IsTriadActive("ossuary_rampart"))
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("drift_parliament") && moved && MutationMeddley_IsCurrentCellWet())
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_IsTriadActive("undertow_furnace") && MutationMeddley_IsCurrentCellWet())
            {
                MutationMeddley_SetShift("AV", 1);
                MutationMeddley_SetShift("HeatResistance", 5);
            }

            if (MutationMeddley_IsTriadActive("bone_kiln_parliament") && MutationMeddley_IsCurrentCellHot())
            {
                MutationMeddley_SetShift("AV", 2);
            }

            if (MutationMeddley_IsTriadActive("resonant_undertow") && moved)
            {
                MutationMeddley_SetShift("Quickness", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("chorus_husk"))
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("whitewater_ossuary") && MutationMeddley_IsCurrentCellWet())
            {
                MutationMeddley_SetShift("AV", 2);
            }

            if (MutationMeddley_IsTriadActive("blackglass_pursuit") && moved)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_HasEvolution("burrowed_nursery"))
            {
                MutationMeddley_SetShift("AV", 1);
                MutationMeddley_SetShift("DV", MutationMeddley_HasMutation("Burrowing Claws") ? 1 : 0);
            }

            if (MutationMeddley_HasEvolution("wake_trail") && moved)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_HasEvolution("molt_parliament") && MutationMeddley_HasOtherMutationWithTag("STRUCTURAL"))
            {
                MutationMeddley_SetShift("AV", 2);
            }

            if (MutationMeddley_HasEvolution("choir_of_tendons"))
            {
                MutationMeddley_SetShift("DV", 1);
                MutationMeddley_SetShift("Quickness", moved ? 1 : 0);
            }
        }

        private int MutationMeddley_GetColonyPressure()
        {
            return MutationMeddley_GetStateInt(MutationMeddley_ColonyKey, 0);
        }

        private int MutationMeddley_GetMaxColonyPressure()
        {
            int result = MutationMeddley_MaxColony;
            if (MutationMeddley_HasMutation("Multiple Legs"))
            {
                result += 1;
            }

            if (MutationMeddley_HasOtherMutationWithTagExcept("BODY_PART_INTERACTION", "Multiple Legs"))
            {
                result += 1;
            }

            return result;
        }

        private void MutationMeddley_ProcessColonyTurn()
        {
            bool moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0;
            bool hostileTerrain = MutationMeddley_IsCurrentCellHostileTraversal();
            int pressure = MutationMeddley_GetColonyPressure();
            int maxPressure = MutationMeddley_GetMaxColonyPressure();
            int strideStreak = MutationMeddley_GetStateInt(MutationMeddley_StreakKey, 0);
            int stitch = Math.Max(0, MutationMeddley_GetStateInt(MutationMeddley_StitchKey, 0) - 1);
            int scout = Math.Max(0, MutationMeddley_GetStateInt(MutationMeddley_ScoutKey, 0) - 1);
            int parliament = Math.Max(0, MutationMeddley_GetStateInt(MutationMeddley_ParliamentKey, 0) - 1);

            if (moved)
            {
                pressure = Math.Min(maxPressure, pressure + 1);
                strideStreak += 1;
            }
            else
            {
                pressure = Math.Max(0, pressure - 1);
                strideStreak = 0;
            }

            if (MutationMeddley_HasEvolution("marrow_hive")
                && MutationMeddley_GetCurrentModeId() == "knit_flesh"
                && pressure > 0
                && ParentObject != null
                && !moved
                && ParentObject.hitpoints < ParentObject.baseHitpoints)
            {
                ParentObject.Heal(MutationMeddley_HasMutation("Regeneration") ? 2 : 1);
                pressure -= 1;
                stitch = Math.Min(4, stitch + 1 + (MutationMeddley_HasEvolution("bone_nursery") ? 1 : 0));
            }

            if (MutationMeddley_HasEvolution("scar_feeders")
                && MutationMeddley_GetCurrentModeId() == "bank_scars"
                && !moved)
            {
                pressure = Math.Min(maxPressure, pressure + 1);
                stitch = Math.Min(4, stitch + 1);
            }

            if (MutationMeddley_HasEvolution("surveyor_swarm")
                && MutationMeddley_GetCurrentModeId() == "range_ahead"
                && moved
                && (hostileTerrain || MutationMeddley_HasEvolution("tendon_scouts")))
            {
                pressure = Math.Min(maxPressure, pressure + 1);
                scout = Math.Min(4, scout + 1 + (MutationMeddley_HasEvolution("tendon_scouts") && hostileTerrain ? 1 : 0));
            }

            if (MutationMeddley_HasEvolution("surveyor_swarm")
                && MutationMeddley_GetCurrentModeId() == "harry_line"
                && moved
                && pressure > 0)
            {
                pressure -= 1;
                scout = Math.Min(4, scout + 1 + (MutationMeddley_HasEvolution("latch_runners") ? 1 : 0));
            }

            if (MutationMeddley_HasEvolution("graft_parliament")
                && MutationMeddley_GetCurrentModeId() == "delegate_load"
                && !moved
                && MutationMeddley_HasOtherMutationWithTag("BODY_PART_INTERACTION"))
            {
                parliament = Math.Min(4, parliament + 1 + (MutationMeddley_HasEvolution("nerve_delegation") ? 1 : 0));
            }

            if (MutationMeddley_HasEvolution("graft_parliament")
                && MutationMeddley_GetCurrentModeId() == "override_frame"
                && MutationMeddley_HasOtherMutationWithTag("STRUCTURAL"))
            {
                pressure = Math.Min(maxPressure, pressure + 1);
                parliament = Math.Min(4, parliament + 1 + (MutationMeddley_HasEvolution("borrowed_hands") ? 1 : 0));
            }

            MutationMeddley_TrackMoltParliamentDiscovery();
            MutationMeddley_TrackWakeTrailDiscovery(hostileTerrain, strideStreak);
            MutationMeddley_TrackBurrowedNurseryDiscovery(moved, pressure);
            MutationMeddley_TrackChoirOfTendonsDiscovery(!moved, pressure);

            MutationMeddley_SetStateInt(MutationMeddley_ColonyKey, Math.Max(0, Math.Min(pressure, maxPressure)));
            MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 0);
            MutationMeddley_SetStateInt(MutationMeddley_StreakKey, Math.Min(strideStreak, 8));
            MutationMeddley_SetStateInt(MutationMeddley_StitchKey, Math.Max(0, Math.Min(stitch, 4)));
            MutationMeddley_SetStateInt(MutationMeddley_ScoutKey, Math.Max(0, Math.Min(scout, 4)));
            MutationMeddley_SetStateInt(MutationMeddley_ParliamentKey, Math.Max(0, Math.Min(parliament, 4)));
            MutationMeddley_RefreshPassiveEffects();
        }

        private void MutationMeddley_TrackMoltParliamentDiscovery()
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_MoltUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("borrowed_hands")
                || MutationMeddley_GetCurrentModeId() != "override_frame"
                || !MutationMeddley_HasOtherMutationWithTag("STRUCTURAL"))
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_MoltProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_MoltUnlockedKey);
            }
        }

        private void MutationMeddley_TrackWakeTrailDiscovery(bool hostileTerrain, int strideStreak)
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_WakeTrailUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("latch_runners")
                || strideStreak < 3
                || !hostileTerrain)
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_WakeTrailProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_WakeTrailUnlockedKey);
            }
        }

        private void MutationMeddley_TrackBurrowedNurseryDiscovery(bool moved, int pressure)
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_BurrowedUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("bone_nursery")
                || !MutationMeddley_HasMutation("Burrowing Claws")
                || moved
                || pressure < 2)
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_BurrowedProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_BurrowedUnlockedKey);
            }
        }

        private void MutationMeddley_TrackChoirOfTendonsDiscovery(bool composedTurn, int pressure)
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_ChoirUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("nerve_delegation")
                || !composedTurn
                || pressure < 2
                || (!MutationMeddley_HasMutation("Heightened Hearing")
                    && !MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")))
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_ChoirProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_ChoirUnlockedKey);
            }
        }

        private bool MutationMeddley_IsTriadActive(string id)
        {
            return MutationMeddley_IsSynergyActive(new MutationMeddley_SynergyDefinition(id, "", ""));
        }
    }
}
