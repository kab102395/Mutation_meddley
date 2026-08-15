using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_Brineborn : MutationMeddley_AdaptiveMutationBase
    {
        private const string MutationMeddley_ReserveKey = "brine_reserve";
        private const string MutationMeddley_MovedKey = "brine_moved";
        private const string MutationMeddley_SalineKey = "brine_saline";
        private const string MutationMeddley_SaltGhostUnlockedKey = "brine_hidden_saltghost";
        private const string MutationMeddley_SaltGhostProgressKey = "brine_hidden_saltghost_progress";
        private const string MutationMeddley_ReliquaryUnlockedKey = "brine_hidden_reliquary";
        private const string MutationMeddley_ReliquaryProgressKey = "brine_hidden_reliquary_progress";
        private const int MutationMeddley_MaxReserve = 6;

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
            get { return "Shift your saline metabolism between recovery, crystallization, and scouring pressure."; }
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
            }
            else if (E.ID == "EndTurn")
            {
                MutationMeddley_ProcessBrineTurn();
            }

            return base.FireEvent(E);
        }

        public override string GetDescription()
        {
            return "Salt, brine, and mineral saturation have become core to your metabolism.\n\n"
                + "Brineborn is an environmental mutation about terrain affinity, conversion, abrasive survivability, and visible interaction with the rest of your biology.";
        }

        public override string GetLevelText(int Level)
        {
            return "Rank 3: choose how your saline biology expresses itself.\n"
                + "Rank 6: specialize the loop.\n"
                + "Rank 9: claim the estuarial capstone.\n\n"
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState()
                + "\n"
                + "Saline reserve: "
                + MutationMeddley_GetReserve()
                + "/"
                + MutationMeddley_GetMaxReserve()
                + (MutationMeddley_IsSalineEnvironment() ? " (saline ground)" : " (dry ground)")
                + "\n"
                + MutationMeddley_GetSynergySummary();
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
                    detailText: "Sustain identity. Reserve becomes recovery and temperature stability."
                ),
                new MutationMeddley_EvolutionChoice(
                    "saltglass_bloom",
                    "Saltglass Bloom",
                    "Minerals harden across you in layered, glassy crusts.",
                    3,
                    1,
                    detailText: "Crystallization identity. Reserve becomes shell and edge."
                ),
                new MutationMeddley_EvolutionChoice(
                    "scouring_estuary",
                    "Scouring Estuary",
                    "You process hostile conditions into a harsh, mobile ecology.",
                    3,
                    1,
                    detailText: "Pressure identity. Reserve becomes motion and hostile routing."
                ),

                new MutationMeddley_EvolutionChoice(
                    "tidal_marrows",
                    "Tidal Marrows",
                    "Brine pulses deeper into your frame and quietly closes wounds.",
                    6,
                    2,
                    "wellspring_flesh",
                    "Spend reserve for direct recovery."
                ),
                new MutationMeddley_EvolutionChoice(
                    "cool_sump",
                    "Cool Sump",
                    "Deep saline stores blunt thermal pressure when the world turns hostile.",
                    6,
                    2,
                    "wellspring_flesh",
                    "Spend less aggressively; convert reserve into weather buffering."
                ),
                new MutationMeddley_EvolutionChoice(
                    "saltglass_bastion",
                    "Saltglass Bastion",
                    "Stationary reserve blooms into a heavy mineral shell.",
                    6,
                    2,
                    "saltglass_bloom",
                    "Spend reserve while holding ground."
                ),
                new MutationMeddley_EvolutionChoice(
                    "knife_reef",
                    "Knife Reef",
                    "Reserve hardens into sharp, layered ridges as you shift position.",
                    6,
                    2,
                    "saltglass_bloom",
                    "Spend reserve while moving to keep the shell agile."
                ),
                new MutationMeddley_EvolutionChoice(
                    "desiccant_wake",
                    "Desiccant Wake",
                    "Leaving brine behind lets you spend reserve to keep pressure on dry ground.",
                    6,
                    2,
                    "scouring_estuary",
                    "Spend reserve to stay dangerous away from saline safety."
                ),
                new MutationMeddley_EvolutionChoice(
                    "brackish_jet",
                    "Brackish Jet",
                    "Reserve turns sudden saline surges into bursts of pursuit and repositioning.",
                    6,
                    2,
                    "scouring_estuary",
                    "Spend reserve immediately after fresh saline contact."
                ),

                new MutationMeddley_EvolutionChoice(
                    "sacred_reservoir",
                    "Sacred Reservoir",
                    "Your body becomes an estuary that refuses depletion.",
                    9,
                    3,
                    "tidal_marrows",
                    "Capstone recovery line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "glacier_brine",
                    "Glacier Brine",
                    "Cold, dense stores turn hardship into stillness and endurance.",
                    9,
                    3,
                    "cool_sump",
                    "Capstone thermal-buffer line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "cathedral_of_salt",
                    "Cathedral of Salt",
                    "Your shell rises in brilliant mineral terraces whenever you settle into place.",
                    9,
                    3,
                    "saltglass_bastion",
                    "Capstone rooted-shell line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "brine_reliquary",
                    "Brine Reliquary",
                    "Your reserve stops behaving like motion fuel and starts settling into anchored mineral memory.",
                    9,
                    3,
                    "saltglass_bastion",
                    "UNUSUAL ADAPTATION. Requires repeated saline fortification with crystal-compatible structure.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "reef_crown",
                    "Reef Crown",
                    "You wear reserve as moving saltglass edges rather than a single static bastion.",
                    9,
                    3,
                    "knife_reef",
                    "Capstone agile-shell line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "whitewater_predator",
                    "Whitewater Predator",
                    "Dry country becomes something you cross by consuming your own stored estuary.",
                    9,
                    3,
                    "desiccant_wake",
                    "Capstone dry-ground pressure line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "saltwind_hunter",
                    "Saltwind Hunter",
                    "Fresh contact with brine becomes the trigger for sudden predatory motion.",
                    9,
                    3,
                    "brackish_jet",
                    "Capstone fresh-contact burst line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "salt_ghost",
                    "Salt Ghost",
                    "Your saline body starts treating brine as a phase-anchor rather than mere nourishment.",
                    9,
                    3,
                    "brackish_jet",
                    "UNUSUAL ADAPTATION. Requires prolonged saline exposure while carrying a phase-compatible body.",
                    true
                )
            };
        }

        protected override IEnumerable<string> MutationMeddley_GetIntrinsicSemanticTags()
        {
            return new string[] { "ENVIRONMENTAL", "AQUATIC", "SALINE", "LIQUID_INTERACTION", "METABOLIC", "BIOLOGICAL" };
        }

        protected override IEnumerable<string> MutationMeddley_GetEvolutionSemanticTags()
        {
            List<string> tags = new List<string>();
            if (MutationMeddley_HasEvolution("wellspring_flesh"))
            {
                tags.Add("REGENERATIVE");
            }

            if (MutationMeddley_HasEvolution("saltglass_bloom"))
            {
                tags.Add("CONTROL");
                tags.Add("STRUCTURAL");
            }

            if (MutationMeddley_HasEvolution("scouring_estuary"))
            {
                tags.Add("TERRAIN_INTERACTION");
                tags.Add("MOBILE");
            }

            return tags;
        }

        protected override List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices()
        {
            if (MutationMeddley_HasEvolution("wellspring_flesh"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("draw_brine", "Draw Brine", "Spend reserve more readily on recovery."),
                    new MutationMeddley_ModeChoice("cool_reserve", "Cool Reserve", "Hold reserve longer for resilience and weather buffering.")
                };
            }

            if (MutationMeddley_HasEvolution("saltglass_bloom"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("shell_up", "Shell Up", "Favor rooted shell growth."),
                    new MutationMeddley_ModeChoice("knife_rind", "Knife Rind", "Favor moving mineral edges.")
                };
            }

            if (MutationMeddley_HasEvolution("scouring_estuary"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("dry_tide", "Dry Tide", "Spend reserve to keep pressure after leaving saline ground."),
                    new MutationMeddley_ModeChoice("surge_tide", "Surge Tide", "Spend reserve to capitalize on fresh saline contact.")
                };
            }

            return new List<MutationMeddley_ModeChoice>();
        }

        protected override List<MutationMeddley_SynergyDefinition> MutationMeddley_GetSynergyDefinitions()
        {
            return new List<MutationMeddley_SynergyDefinition>
            {
                new MutationMeddley_SynergyDefinition("amphibious", "Amphibious", "Aquatic familiarity increases reserve stability and wet-ground comfort."),
                new MutationMeddley_SynergyDefinition("regeneration", "Regeneration", "Reserve recycles into tougher, more reliable recovery loops."),
                new MutationMeddley_SynergyDefinition("photosynthetic_skin", "Photosynthetic Skin", "Sunlit brine becomes a metabolic estuary you can actually exploit."),
                new MutationMeddley_SynergyDefinition("multiple_legs", "Multiple Legs", "Shallow-liquid routing and reserve carry reward mobile saline builds."),
                new MutationMeddley_SynergyDefinition("electrical_generation", "Electrical Generation", "Conductive brine amplifies motion and pressure at a defensive cost."),
                new MutationMeddley_SynergyDefinition("burrowing_claws", "Burrowing Claws", "Harsh ground becomes easier to cross and hold with stored reserve."),
                new MutationMeddley_SynergyDefinition("ash_pair", "Ash Metabolism", "Steam, heat, and reserve now share one environmental loop."),
                new MutationMeddley_SynergyDefinition("walking_colony_pair", "Walking Colony", "Colonial routing changes how your reserve survives movement and recovery."),
                new MutationMeddley_SynergyDefinition("living_crystal_pair", "Living Crystal", "Saltglass physiology mineralizes the lattice according to your crystal branch."),
                new MutationMeddley_SynergyDefinition("carapace_pair", "Carapace Evolution", "Mineral deposition changes how your shell or body carries saline armor."),
                new MutationMeddley_SynergyDefinition("cathedral_organism", "Cathedral Organism", "Salt, shell, and crystal now fortify each other as one structure."),
                new MutationMeddley_SynergyDefinition("breakwater_predator", "Breakwater Predator", "Wet pursuit compounds cadence and reserve pressure together."),
                new MutationMeddley_SynergyDefinition("prism_estuary", "Prism Estuary", "Light and climate begin feeding the same saline metabolism."),
                new MutationMeddley_SynergyDefinition("salt_kiln_reliquary", "Salt Kiln Reliquary", "Thermal mineralization hardens your saline defense into a kiln-kept bastion."),
                new MutationMeddley_SynergyDefinition("steam_choir", "Steam Choir", "Wet pursuit, smoke, and resonance now compound one another."),
                new MutationMeddley_SynergyDefinition("drift_parliament", "Drift Parliament", "Shallow-liquid routing now carries colonial chase pressure."),
                new MutationMeddley_SynergyDefinition("salt_ghost_state", "Salt Ghost", "Brine and phase-state now overlap in a dangerous, temporary ecology.", isUnusual: true)
                ,
                new MutationMeddley_SynergyDefinition("brine_reliquary_state", "Brine Reliquary", "Reserve now settles into anchored saline structure instead of only circulating.", isUnusual: true)
            };
        }

        protected override bool MutationMeddley_IsSynergyActive(MutationMeddley_SynergyDefinition synergy)
        {
            switch (synergy.Id)
            {
                case "amphibious":
                    return MutationMeddley_HasMutation("Amphibious") && MutationMeddley_HasAnyEvolution();
                case "regeneration":
                    return MutationMeddley_HasMutation("Regeneration") && MutationMeddley_HasEvolution("wellspring_flesh");
                case "photosynthetic_skin":
                    return MutationMeddley_HasMutation("Photosynthetic Skin") && MutationMeddley_HasAnyEvolution();
                case "multiple_legs":
                    return MutationMeddley_HasMutation("Multiple Legs") && MutationMeddley_HasEvolution("scouring_estuary");
                case "electrical_generation":
                    return MutationMeddley_HasMutation("Electrical Generation") && MutationMeddley_HasAnyEvolution();
                case "burrowing_claws":
                    return MutationMeddley_HasMutation("Burrowing Claws")
                        && (MutationMeddley_HasEvolution("scouring_estuary") || MutationMeddley_HasEvolution("wellspring_flesh"));
                case "ash_pair":
                    return MutationMeddley_HasMutation("Ash Metabolism")
                        && (MutationMeddley_HasEvolution("saltglass_bloom") || MutationMeddley_HasEvolution("scouring_estuary"));
                case "walking_colony_pair":
                    return MutationMeddley_HasMutation("Walking Colony")
                        && (MutationMeddley_HasEvolution("wellspring_flesh") || MutationMeddley_HasEvolution("scouring_estuary"));
                case "living_crystal_pair":
                    return MutationMeddley_HasMutation("Living Crystal") && MutationMeddley_HasAnyEvolution();
                case "carapace_pair":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution") && MutationMeddley_HasAnyEvolution();
                case "cathedral_organism":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("saltglass_bloom")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress");
                case "breakwater_predator":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("scouring_estuary")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell");
                case "prism_estuary":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("wellspring_flesh")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace");
                case "salt_kiln_reliquary":
                    return MutationMeddley_HasEvolution("saltglass_bloom")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "furnace_skin");
                case "steam_choir":
                    return MutationMeddley_HasEvolution("scouring_estuary")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "smoke_organ");
                case "drift_parliament":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("scouring_estuary")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "surveyor_swarm");
                case "salt_ghost_state":
                    return MutationMeddley_HasEvolution("salt_ghost");
                case "brine_reliquary_state":
                    return MutationMeddley_HasEvolution("brine_reliquary");
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

            if (choice.Id == "salt_ghost")
            {
                return MutationMeddley_GetStateInt(MutationMeddley_SaltGhostUnlockedKey, 0) > 0;
            }

            if (choice.Id == "brine_reliquary")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_ReliquaryUnlockedKey);
            }

            return false;
        }

        protected override void MutationMeddley_RefreshPassiveEffects()
        {
            MutationMeddley_ClearCommonStatShifts();

            int reserve = MutationMeddley_GetReserve();
            bool saline = MutationMeddley_IsSalineEnvironment();
            bool wet = MutationMeddley_IsCurrentCellWet();
            bool lit = MutationMeddley_IsCurrentCellLit();

            if (MutationMeddley_HasEvolution("wellspring_flesh"))
            {
                MutationMeddley_SetShift("HeatResistance", 5 + (reserve * 2));
                MutationMeddley_SetShift("ColdResistance", 5 + (reserve * 2));

                if (MutationMeddley_HasEvolution("tidal_marrows"))
                {
                    MutationMeddley_SetShift("DV", reserve / 2);
                    if (MutationMeddley_HasEvolution("sacred_reservoir"))
                    {
                        MutationMeddley_SetShift("AV", reserve / 3);
                    }
                }
                else if (MutationMeddley_HasEvolution("cool_sump"))
                {
                    MutationMeddley_SetShift("DV", saline ? 2 : 1);
                    if (MutationMeddley_HasEvolution("glacier_brine"))
                    {
                        MutationMeddley_SetShift("AV", reserve / 3);
                    }
                }
                else
                {
                    MutationMeddley_SetShift("DV", 1 + (reserve / 3));
                }
            }
            else if (MutationMeddley_HasEvolution("saltglass_bloom"))
            {
                if (MutationMeddley_HasEvolution("saltglass_bastion"))
                {
                    MutationMeddley_SetShift("AV", 1 + (reserve / 2));
                    if (MutationMeddley_HasEvolution("cathedral_of_salt"))
                    {
                        MutationMeddley_SetShift("DV", reserve / 3);
                    }
                }
                else if (MutationMeddley_HasEvolution("knife_reef"))
                {
                    MutationMeddley_SetShift("DV", 1 + (reserve / 2));
                    if (MutationMeddley_HasEvolution("reef_crown"))
                    {
                        MutationMeddley_SetShift("Quickness", reserve / 2);
                    }
                }
                else
                {
                    MutationMeddley_SetShift("AV", 1 + (reserve / 3));
                    MutationMeddley_SetShift("DV", reserve / 4);
                }
            }
            else if (MutationMeddley_HasEvolution("scouring_estuary"))
            {
                if (MutationMeddley_HasEvolution("desiccant_wake"))
                {
                    MutationMeddley_SetShift("Quickness", reserve / 2);
                    if (MutationMeddley_HasEvolution("whitewater_predator"))
                    {
                        MutationMeddley_SetShift("DV", 1 + (reserve / 3));
                    }
                }
                else if (MutationMeddley_HasEvolution("brackish_jet"))
                {
                    MutationMeddley_SetShift("DV", saline ? 2 + (reserve / 3) : reserve / 4);
                    if (MutationMeddley_HasEvolution("saltwind_hunter"))
                    {
                        MutationMeddley_SetShift("Quickness", saline ? 2 + (reserve / 3) : 0);
                    }
                }
                else
                {
                    MutationMeddley_SetShift("Quickness", reserve / 3);
                    MutationMeddley_SetShift("DV", reserve / 4);
                }
            }

            if (MutationMeddley_HasMutation("Amphibious"))
            {
                MutationMeddley_SetShift("DV", wet ? 1 : 0);
            }

            if (MutationMeddley_HasMutation("Regeneration") && MutationMeddley_HasEvolution("wellspring_flesh"))
            {
                MutationMeddley_SetShift("AV", reserve > 0 ? 1 : 0);
            }

            if (MutationMeddley_HasMutation("Photosynthetic Skin") && lit && saline)
            {
                MutationMeddley_SetShift("DV", 1);
                MutationMeddley_SetShift("HeatResistance", 5);
            }

            if (MutationMeddley_HasMutation("Multiple Legs") && MutationMeddley_HasEvolution("scouring_estuary") && wet)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_HasMutation("Electrical Generation") && saline)
            {
                MutationMeddley_SetShift("Quickness", 1);
                MutationMeddley_SetShift("AV", -1);
            }

            if (MutationMeddley_HasMutation("Ash Metabolism"))
            {
                if (saline && lit)
                {
                    MutationMeddley_SetShift("HeatResistance", 5);
                }

                if (MutationMeddley_HasEvolution("scouring_estuary") && wet)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }

            if (MutationMeddley_HasMutation("Burrowing Claws") && !saline)
            {
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasMutation("Living Crystal"))
            {
                if (MutationMeddley_HasEvolution("saltglass_bloom"))
                {
                    MutationMeddley_SetShift("AV", saline ? 1 : 0);
                }
                else if (MutationMeddley_HasEvolution("wellspring_flesh") && lit && saline)
                {
                    MutationMeddley_SetShift("ColdResistance", 5);
                    MutationMeddley_SetShift("HeatResistance", 5);
                }
                else if (MutationMeddley_HasEvolution("scouring_estuary") && wet)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }

            if (MutationMeddley_HasMutation("Walking Colony"))
            {
                if (MutationMeddley_HasEvolution("wellspring_flesh")
                    && MutationMeddley_MutationHasEvolution("Walking Colony", "marrow_hive"))
                {
                    MutationMeddley_SetShift("AV", reserve > 0 ? 1 : 0);
                }
                else if (MutationMeddley_HasEvolution("scouring_estuary")
                    && MutationMeddley_MutationHasEvolution("Walking Colony", "surveyor_swarm")
                    && wet)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }

            if (MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution"))
            {
                if (MutationMeddley_HasEvolution("saltglass_bloom")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress"))
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("scouring_estuary")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("wellspring_flesh")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace")
                    && wet)
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_IsTriadActive("cathedral_organism") && saline && MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) == 0)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("breakwater_predator") && wet && MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_IsTriadActive("prism_estuary") && lit && saline)
            {
                MutationMeddley_SetShift("HeatResistance", 10);
                MutationMeddley_SetShift("ColdResistance", 10);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("salt_kiln_reliquary") && saline)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("HeatResistance", 5);
            }

            if (MutationMeddley_IsTriadActive("steam_choir") && wet)
            {
                MutationMeddley_SetShift("Quickness", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("drift_parliament") && wet && MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_HasEvolution("salt_ghost"))
            {
                MutationMeddley_SetShift("Quickness", saline ? 3 : 0);
                MutationMeddley_SetShift("DV", saline ? 2 : 0);
            }

            if (MutationMeddley_HasEvolution("brine_reliquary") && saline && MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) == 0)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }
        }

        private int MutationMeddley_GetReserve()
        {
            return MutationMeddley_GetStateInt(MutationMeddley_ReserveKey, 0);
        }

        private int MutationMeddley_GetMaxReserve()
        {
            int maxReserve = MutationMeddley_MaxReserve;
            if (MutationMeddley_HasMutation("Amphibious"))
            {
                maxReserve += 1;
            }

            if (MutationMeddley_HasMutation("Photosynthetic Skin") && MutationMeddley_IsCurrentCellLit())
            {
                maxReserve += 1;
            }

            return maxReserve;
        }

        private void MutationMeddley_ProcessBrineTurn()
        {
            bool saline = MutationMeddley_IsSalineEnvironment();
            int reserve = MutationMeddley_GetReserve();
            int moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0);
            int maxReserve = MutationMeddley_GetMaxReserve();

            if (saline)
            {
                int gain = MutationMeddley_HasEvolution("saltwind_hunter") ? 2 : 1;
                if (MutationMeddley_HasMutation("Photosynthetic Skin") && MutationMeddley_IsCurrentCellLit())
                {
                    gain += 1;
                }

                reserve = Math.Min(maxReserve, reserve + gain);
            }
            else
            {
                int decay = MutationMeddley_HasEvolution("glacier_brine") ? 0 : 1;
                if (MutationMeddley_HasMutation("Amphibious"))
                {
                    decay = Math.Max(decay - 1, 0);
                }
                if (MutationMeddley_HasMutation("Burrowing Claws") && MutationMeddley_HasEvolution("scouring_estuary"))
                {
                    decay = Math.Max(decay - 1, 0);
                }

                reserve = Math.Max(0, reserve - decay);
            }

            if (MutationMeddley_HasEvolution("tidal_marrows")
                && MutationMeddley_GetCurrentModeId() == "draw_brine"
                && reserve > 0
                && ParentObject != null)
            {
                ParentObject.Heal(MutationMeddley_HasMutation("Regeneration") ? 2 : 1);
                reserve -= 1;
            }

            if (MutationMeddley_HasEvolution("saltglass_bastion")
                && MutationMeddley_GetCurrentModeId() == "shell_up"
                && reserve > 0
                && moved == 0)
            {
                reserve -= 1;
                reserve = Math.Min(MutationMeddley_MaxReserve, reserve + (MutationMeddley_HasEvolution("cathedral_of_salt") ? 1 : 0));
            }

            if (MutationMeddley_HasEvolution("knife_reef")
                && MutationMeddley_GetCurrentModeId() == "knife_rind"
                && reserve > 0
                && moved > 0)
            {
                reserve -= 1;
            }

            if (MutationMeddley_HasEvolution("desiccant_wake")
                && MutationMeddley_GetCurrentModeId() == "dry_tide"
                && reserve > 0
                && moved > 0
                && !saline)
            {
                reserve -= 1;
            }

            if (MutationMeddley_HasEvolution("brackish_jet")
                && MutationMeddley_GetCurrentModeId() == "surge_tide"
                && reserve > 0
                && moved > 0
                && saline)
            {
                reserve -= 1;
            }

            if (MutationMeddley_HasMutation("Multiple Legs")
                && MutationMeddley_HasEvolution("scouring_estuary")
                && moved > 0
                && saline)
            {
                reserve = Math.Min(maxReserve, reserve + 1);
            }

            if (!MutationMeddley_HasSelectionAtTier(3)
                && MutationMeddley_HasMutation("Phasing")
                && saline
                && MutationMeddley_HasEvolution("brackish_jet"))
            {
                if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_SaltGhostProgressKey, 1, 6) >= 6)
                {
                    MutationMeddley_UnlockHiddenChoice(MutationMeddley_SaltGhostUnlockedKey);
                }
            }

            MutationMeddley_TrackBrineReliquaryDiscovery(saline, moved);

            MutationMeddley_SetStateInt(MutationMeddley_ReserveKey, Math.Max(0, Math.Min(reserve, maxReserve)));
            MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 0);
            MutationMeddley_SetStateInt(MutationMeddley_SalineKey, saline ? 1 : 0);
            MutationMeddley_RefreshPassiveEffects();
        }

        private bool MutationMeddley_IsSalineEnvironment()
        {
            return MutationMeddley_IsCurrentCellSaline();
        }

        private void MutationMeddley_TrackBrineReliquaryDiscovery(bool saline, int moved)
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_ReliquaryUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("saltglass_bastion")
                || !MutationMeddley_MutationHasSemanticTag("Living Crystal", "CRYSTALLINE")
                || !saline
                || moved != 0)
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_ReliquaryProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_ReliquaryUnlockedKey);
            }
        }

        private bool MutationMeddley_IsTriadActive(string id)
        {
            return MutationMeddley_IsSynergyActive(new MutationMeddley_SynergyDefinition(id, "", ""));
        }
    }
}
