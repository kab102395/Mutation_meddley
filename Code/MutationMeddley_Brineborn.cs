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
        private const string MutationMeddley_ShellUpKey = "brine_shellup";
        private const string MutationMeddley_KnifeRindKey = "brine_kniferind";
        private const string MutationMeddley_DryTideKey = "brine_drytide";
        private const string MutationMeddley_SurgeTideKey = "brine_surgetide";
        private const string MutationMeddley_MendKey = "brine_mend";
        private const string MutationMeddley_BastionKey = "brine_bastion";
        private const string MutationMeddley_WakeKey = "brine_wake";
        private const string MutationMeddley_SaltGhostUnlockedKey = "brine_hidden_saltghost";
        private const string MutationMeddley_SaltGhostProgressKey = "brine_hidden_saltghost_progress";
        private const string MutationMeddley_ReliquaryUnlockedKey = "brine_hidden_reliquary";
        private const string MutationMeddley_ReliquaryProgressKey = "brine_hidden_reliquary_progress";
        private const string MutationMeddley_UndertowUnlockedKey = "brine_hidden_undertow";
        private const string MutationMeddley_UndertowProgressKey = "brine_hidden_undertow_progress";
        private const string MutationMeddley_AbyssalUnlockedKey = "brine_hidden_abyssal";
        private const string MutationMeddley_AbyssalProgressKey = "brine_hidden_abyssal_progress";
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
            Object.RegisterPartEvent(this, "AttackerDealtDamage");
            Object.RegisterPartEvent(this, "TookDamage");
            Object.RegisterPartEvent(this, "TookEnvironmentalDamage");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EnteredCell")
            {
                MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 1);
            }
            else if (E.ID == "AttackerDealtDamage")
            {
                MutationMeddley_HandleBrineStrike();
                MutationMeddley_RefreshPassiveEffects();
            }
            else if (E.ID == "TookDamage" || E.ID == "TookEnvironmentalDamage")
            {
                MutationMeddley_HandleBrinePressure();
                MutationMeddley_RefreshPassiveEffects();
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
                + MutationMeddley_GetUsageSummary()
                + "\n\n"
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
                + MutationMeddley_GetCurrentMechanicsSummary()
                + "\n"
                + MutationMeddley_GetPassiveBonusSummary()
                + "\n"
                + MutationMeddley_GetSynergySummary();
        }

        protected override IEnumerable<string> MutationMeddley_GetCurrentMechanicNotes()
        {
            yield return "Saline reserve builds in qualifying saline cells and decays away from them.";

            if (MutationMeddley_HasEvolution("wellspring_flesh"))
            {
                yield return "Wellspring Flesh uses Draw Brine to build mend, then spends mend when pressure arrives or wounds open.";
                yield return "Current mend: " + MutationMeddley_GetStateInt(MutationMeddley_MendKey, 0) + ". Cool Reserve banks the next mend into weatherproofing instead of raw sustain.";
            }
            else if (MutationMeddley_HasEvolution("saltglass_bloom"))
            {
                yield return "Saltglass Bloom turns reserve into bastion and spends bastion when the shell is tested or a sharp line connects.";
                yield return "Current bastion: " + MutationMeddley_GetStateInt(MutationMeddley_BastionKey, 0) + ". Shell Up likes stationary reserve; Knife Rind likes movement.";
            }
            else if (MutationMeddley_HasEvolution("scouring_estuary"))
            {
                yield return "Scouring Estuary turns reserve into wake pressure after movement, then spends wake on successful pursuit contact.";
                yield return "Current wake: " + MutationMeddley_GetStateInt(MutationMeddley_WakeKey, 0) + ".";
            }
            else
            {
                yield return "Choose a rank-3 saline identity first to unlock reserve-spending mechanics.";
            }
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
                    "undertow_heart",
                    "Undertow Heart",
                    "Recovery loops stop being passive and start pulling the rest of your build into their timing.",
                    9,
                    3,
                    "tidal_marrows",
                    "UNUSUAL ADAPTATION. Requires repeated reserve-spend recovery while carrying Regeneration.",
                    true
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
                    "abyssal_brine",
                    "Abyssal Brine",
                    "Cold wet pressure turns your reserve into deep stillness rather than mobile buffering.",
                    9,
                    3,
                    "cool_sump",
                    "UNUSUAL ADAPTATION. Requires long cold wet exposure while carrying a cryogenic profile.",
                    true
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
                new MutationMeddley_SynergyDefinition("cathedral_organism", "Cathedral Organism", "Salt, shell, and crystal now fortify each other as one structure.", isTriad: true),
                new MutationMeddley_SynergyDefinition("breakwater_predator", "Breakwater Predator", "Wet pursuit compounds cadence and reserve pressure together.", isTriad: true),
                new MutationMeddley_SynergyDefinition("prism_estuary", "Prism Estuary", "Light and climate begin feeding the same saline metabolism.", isTriad: true),
                new MutationMeddley_SynergyDefinition("salt_kiln_reliquary", "Salt Kiln Reliquary", "Thermal mineralization hardens your saline defense into a kiln-kept bastion.", isTriad: true),
                new MutationMeddley_SynergyDefinition("steam_choir", "Steam Choir", "Wet pursuit, smoke, and resonance now compound one another.", isTriad: true),
                new MutationMeddley_SynergyDefinition("drift_parliament", "Drift Parliament", "Shallow-liquid routing now carries colonial chase pressure.", isTriad: true),
                new MutationMeddley_SynergyDefinition("undertow_furnace", "Undertow Furnace", "Reserve, recovery, and heat debt become one survival engine.", isTriad: true),
                new MutationMeddley_SynergyDefinition("salt_eclipse", "Salt Eclipse", "Saline refraction and adaptive shell weathering become one dim-space defense.", isTriad: true),
                new MutationMeddley_SynergyDefinition("resonant_undertow", "Resonant Undertow", "Reserve and cadence now cycle through movement and recovery together.", isTriad: true),
                new MutationMeddley_SynergyDefinition("smoke_reef", "Smoke Reef", "Smoke, saltglass edges, and prismatic refraction mislead the whole battlefield.", isTriad: true),
                new MutationMeddley_SynergyDefinition("whitewater_ossuary", "Whitewater Ossuary", "Harsh traversal and deep sustain reinforce a slow, relentless wall.", isTriad: true),
                new MutationMeddley_SynergyDefinition("salt_ghost_state", "Salt Ghost", "Brine and phase-state now overlap in a dangerous, temporary ecology.", isUnusual: true)
                ,
                new MutationMeddley_SynergyDefinition("brine_reliquary_state", "Brine Reliquary", "Reserve now settles into anchored saline structure instead of only circulating.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("undertow_heart_state", "Undertow Heart", "Recovery loops now pull the rest of your build into tidal timing.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("abyssal_brine_state", "Abyssal Brine", "Reserve now prefers deep cold stillness over ordinary buffering.", isUnusual: true)
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
                case "undertow_furnace":
                    return MutationMeddley_HasEvolution("wellspring_flesh")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "cinder_gut")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "marrow_hive");
                case "salt_eclipse":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("saltglass_bloom")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace");
                case "resonant_undertow":
                    return MutationMeddley_HasEvolution("wellspring_flesh")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "surveyor_swarm");
                case "smoke_reef":
                    return MutationMeddley_HasEvolution("saltglass_bloom")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "smoke_organ")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix");
                case "whitewater_ossuary":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("scouring_estuary")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "marrow_hive")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress");
                case "salt_ghost_state":
                    return MutationMeddley_HasEvolution("salt_ghost");
                case "brine_reliquary_state":
                    return MutationMeddley_HasEvolution("brine_reliquary");
                case "undertow_heart_state":
                    return MutationMeddley_HasEvolution("undertow_heart");
                case "abyssal_brine_state":
                    return MutationMeddley_HasEvolution("abyssal_brine");
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

            if (choice.Id == "undertow_heart")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_UndertowUnlockedKey);
            }

            if (choice.Id == "abyssal_brine")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_AbyssalUnlockedKey);
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
            int mend = MutationMeddley_GetStateInt(MutationMeddley_MendKey, 0);
            int bastion = MutationMeddley_GetStateInt(MutationMeddley_BastionKey, 0);
            int wake = MutationMeddley_GetStateInt(MutationMeddley_WakeKey, 0);

            if (MutationMeddley_HasEvolution("wellspring_flesh"))
            {
                MutationMeddley_SetShift("Toughness", 1);
                MutationMeddley_SetShift("HeatResistance", 5 + (reserve * 2));
                MutationMeddley_SetShift("ColdResistance", 5 + (reserve * 2));
                MutationMeddley_SetShift("DV", mend > 0 ? 1 + mend : 0);

                if (MutationMeddley_HasEvolution("tidal_marrows"))
                {
                    MutationMeddley_SetShift("DV", (reserve / 2) + mend);
                    if (MutationMeddley_HasEvolution("sacred_reservoir"))
                    {
                        MutationMeddley_SetShift("AV", (reserve / 3) + (mend / 2));
                    }
                }
                else if (MutationMeddley_HasEvolution("cool_sump"))
                {
                    MutationMeddley_SetShift("DV", (saline ? 2 : 1) + (mend / 2));
                    if (MutationMeddley_HasEvolution("glacier_brine"))
                    {
                        MutationMeddley_SetShift("AV", (reserve / 3) + (mend / 2));
                    }
                }

                if (MutationMeddley_GetCurrentModeId() == "draw_brine" && reserve > 0)
                {
                    MutationMeddley_SetShift("DV", 1);
                }
                else if (MutationMeddley_GetCurrentModeId() == "cool_reserve" && reserve > 0)
                {
                    MutationMeddley_SetShift("ColdResistance", 5);
                }
            }
            else if (MutationMeddley_HasEvolution("saltglass_bloom"))
            {
                MutationMeddley_SetShift("Willpower", 1);
                MutationMeddley_SetShift("AV", bastion > 0 ? 1 + bastion : 0);

                if (MutationMeddley_HasEvolution("saltglass_bastion"))
                {
                    MutationMeddley_SetShift("AV", 1 + (reserve / 2) + bastion);
                    if (MutationMeddley_HasEvolution("cathedral_of_salt"))
                    {
                        MutationMeddley_SetShift("DV", (reserve / 3) + (bastion / 2));
                    }
                }
                else if (MutationMeddley_HasEvolution("knife_reef"))
                {
                    MutationMeddley_SetShift("DV", 1 + (reserve / 2) + (bastion / 2));
                    if (MutationMeddley_HasEvolution("reef_crown"))
                    {
                        MutationMeddley_SetShift("Quickness", (reserve / 2) + bastion);
                    }
                }
                else
                {
                    MutationMeddley_SetShift("DV", (reserve / 4) + (bastion / 2));
                }

                if (MutationMeddley_GetCurrentModeId() == "shell_up")
                {
                    MutationMeddley_SetShift("AV", reserve > 0 ? 1 : 0);
                    MutationMeddley_SetShift("DV", MutationMeddley_GetStateInt(MutationMeddley_ShellUpKey, 0) > 0 ? 1 : 0);
                }
                else if (MutationMeddley_GetCurrentModeId() == "knife_rind")
                {
                    MutationMeddley_SetShift("Quickness", reserve > 0 ? 1 : 0);
                    MutationMeddley_SetShift("DV", MutationMeddley_GetStateInt(MutationMeddley_KnifeRindKey, 0) > 0 ? 1 : 0);
                }
            }
            else if (MutationMeddley_HasEvolution("scouring_estuary"))
            {
                MutationMeddley_SetShift("Agility", 1);
                MutationMeddley_SetShift("Quickness", wake > 0 ? 1 + wake : 0);

                if (MutationMeddley_HasEvolution("desiccant_wake"))
                {
                    MutationMeddley_SetShift("Quickness", (reserve / 2) + wake);
                    if (MutationMeddley_HasEvolution("whitewater_predator"))
                    {
                        MutationMeddley_SetShift("DV", 1 + (reserve / 3) + (wake / 2));
                    }
                }
                else if (MutationMeddley_HasEvolution("brackish_jet"))
                {
                    MutationMeddley_SetShift("DV", (saline ? 2 + (reserve / 3) : reserve / 4) + (wake / 2));
                    if (MutationMeddley_HasEvolution("saltwind_hunter"))
                    {
                        MutationMeddley_SetShift("Quickness", (saline ? 2 + (reserve / 3) : 0) + wake);
                    }
                }
                else
                {
                    MutationMeddley_SetShift("DV", (reserve / 4) + (wake / 2));
                }

                if (MutationMeddley_GetCurrentModeId() == "dry_tide")
                {
                    MutationMeddley_SetShift("Quickness", !saline && reserve > 0 ? 1 : 0);
                    MutationMeddley_SetShift("DV", MutationMeddley_GetStateInt(MutationMeddley_DryTideKey, 0) > 0 ? 1 : 0);
                }
                else if (MutationMeddley_GetCurrentModeId() == "surge_tide")
                {
                    MutationMeddley_SetShift("Quickness", saline && reserve > 0 ? 1 : 0);
                    MutationMeddley_SetShift("DV", MutationMeddley_GetStateInt(MutationMeddley_SurgeTideKey, 0) > 0 ? 1 : 0);
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

            if (MutationMeddley_IsTriadActive("undertow_furnace") && wet)
            {
                MutationMeddley_SetShift("AV", 1);
                MutationMeddley_SetShift("HeatResistance", 5);
            }

            if (MutationMeddley_IsTriadActive("salt_eclipse") && !lit && saline)
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("resonant_undertow") && wet)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_IsTriadActive("smoke_reef") && MutationMeddley_IsCurrentCellSmoky())
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("whitewater_ossuary") && wet)
            {
                MutationMeddley_SetShift("AV", 2);
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

            if (MutationMeddley_HasEvolution("undertow_heart") && reserve > 0)
            {
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasEvolution("abyssal_brine") && wet)
            {
                MutationMeddley_SetShift("ColdResistance", 10);
                MutationMeddley_SetShift("AV", 1);
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
            bool usedTidalMarrowsRecovery = false;
            bool usedShellUp = false;
            bool usedKnifeRind = false;
            bool usedDryTide = false;
            bool usedSurgeTide = false;
            int mend = Math.Max(0, MutationMeddley_GetStateInt(MutationMeddley_MendKey, 0) - 1);
            int bastion = Math.Max(0, MutationMeddley_GetStateInt(MutationMeddley_BastionKey, 0) - 1);
            int wake = Math.Max(0, MutationMeddley_GetStateInt(MutationMeddley_WakeKey, 0) - 1);

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

            if (MutationMeddley_HasEvolution("wellspring_flesh")
                && MutationMeddley_GetCurrentModeId() == "draw_brine"
                && reserve > 0
                && ParentObject != null
                && ParentObject.hitpoints < ParentObject.baseHitpoints)
            {
                int healAmount = MutationMeddley_HasEvolution("tidal_marrows")
                    ? (MutationMeddley_HasMutation("Regeneration") ? 2 : 1)
                    : 1;
                ParentObject.Heal(healAmount);
                reserve -= 1;
                mend = Math.Min(3, mend + 1 + (MutationMeddley_HasEvolution("sacred_reservoir") ? 1 : 0));
                usedTidalMarrowsRecovery = MutationMeddley_HasEvolution("tidal_marrows");
            }

            if (MutationMeddley_HasEvolution("saltglass_bloom")
                && MutationMeddley_GetCurrentModeId() == "shell_up"
                && reserve > 0
                && moved == 0)
            {
                reserve -= 1;
                bastion = Math.Min(4, bastion + 1 + (MutationMeddley_HasEvolution("saltglass_bastion") ? 1 : 0));
                reserve = Math.Min(maxReserve, reserve + (MutationMeddley_HasEvolution("cathedral_of_salt") ? 1 : 0));
                usedShellUp = true;
            }

            if (MutationMeddley_HasEvolution("saltglass_bloom")
                && MutationMeddley_GetCurrentModeId() == "knife_rind"
                && reserve > 0
                && moved > 0)
            {
                reserve -= 1;
                bastion = Math.Min(4, bastion + 1 + (MutationMeddley_HasEvolution("reef_crown") ? 1 : 0));
                usedKnifeRind = true;
            }

            if (MutationMeddley_HasEvolution("scouring_estuary")
                && MutationMeddley_GetCurrentModeId() == "dry_tide"
                && reserve > 0
                && moved > 0
                && !saline)
            {
                reserve -= 1;
                wake = Math.Min(4, wake + 1 + (MutationMeddley_HasEvolution("desiccant_wake") ? 1 : 0));
                usedDryTide = true;
            }

            if (MutationMeddley_HasEvolution("scouring_estuary")
                && MutationMeddley_GetCurrentModeId() == "surge_tide"
                && reserve > 0
                && moved > 0
                && saline)
            {
                reserve -= 1;
                wake = Math.Min(4, wake + 1 + (MutationMeddley_HasEvolution("brackish_jet") ? 1 : 0));
                usedSurgeTide = true;
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
            MutationMeddley_TrackUndertowHeartDiscovery(usedTidalMarrowsRecovery);
            MutationMeddley_TrackAbyssalBrineDiscovery(saline);

            MutationMeddley_SetStateInt(MutationMeddley_ReserveKey, Math.Max(0, Math.Min(reserve, maxReserve)));
            MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 0);
            MutationMeddley_SetStateInt(MutationMeddley_SalineKey, saline ? 1 : 0);
            MutationMeddley_SetStateInt(MutationMeddley_ShellUpKey, usedShellUp ? 1 : 0);
            MutationMeddley_SetStateInt(MutationMeddley_KnifeRindKey, usedKnifeRind ? 1 : 0);
            MutationMeddley_SetStateInt(MutationMeddley_DryTideKey, usedDryTide ? 1 : 0);
            MutationMeddley_SetStateInt(MutationMeddley_SurgeTideKey, usedSurgeTide ? 1 : 0);
            MutationMeddley_SetStateInt(MutationMeddley_MendKey, Math.Max(0, Math.Min(mend, 3)));
            MutationMeddley_SetStateInt(MutationMeddley_BastionKey, Math.Max(0, Math.Min(bastion, 4)));
            MutationMeddley_SetStateInt(MutationMeddley_WakeKey, Math.Max(0, Math.Min(wake, 4)));
            MutationMeddley_RefreshPassiveEffects();
        }

        private void MutationMeddley_HandleBrinePressure()
        {
            if (ParentObject == null)
            {
                return;
            }

            if (MutationMeddley_HasEvolution("wellspring_flesh") && MutationMeddley_GetStateInt(MutationMeddley_MendKey, 0) > 0)
            {
                MutationMeddley_ConsumeStateInt(MutationMeddley_MendKey, 1);
                MutationMeddley_TryHeal(1);
                MutationMeddley_AddPlayerMessage("Stored brine closes over the fresh damage.");
                return;
            }

            if (MutationMeddley_HasEvolution("saltglass_bloom") && MutationMeddley_GetStateInt(MutationMeddley_BastionKey, 0) > 0)
            {
                MutationMeddley_ConsumeStateInt(MutationMeddley_BastionKey, 1);
                MutationMeddley_TryHeal(1);
                MutationMeddley_AddPlayerMessage("Saltglass bastion flakes away instead of your flesh.");
            }
        }

        private void MutationMeddley_HandleBrineStrike()
        {
            if (ParentObject == null)
            {
                return;
            }

            bool saline = MutationMeddley_IsSalineEnvironment();

            if (MutationMeddley_HasEvolution("saltglass_bloom") && MutationMeddley_GetStateInt(MutationMeddley_BastionKey, 0) > 0)
            {
                MutationMeddley_ConsumeStateInt(MutationMeddley_BastionKey, 1);
                MutationMeddley_SetStateInt(MutationMeddley_ReserveKey, Math.Min(MutationMeddley_GetReserve() + 1, MutationMeddley_GetMaxReserve()));
                MutationMeddley_AddPlayerMessage("Saltglass edges break loose on the hit and feed your reserve.");
                return;
            }

            if (MutationMeddley_HasEvolution("scouring_estuary") && MutationMeddley_GetStateInt(MutationMeddley_WakeKey, 0) > 0)
            {
                MutationMeddley_ConsumeStateInt(MutationMeddley_WakeKey, 1);
                MutationMeddley_TryHeal(1);
                if (saline && MutationMeddley_HasEvolution("brackish_jet"))
                {
                    MutationMeddley_SetStateInt(MutationMeddley_ReserveKey, Math.Min(MutationMeddley_GetReserve() + 1, MutationMeddley_GetMaxReserve()));
                }

                MutationMeddley_AddPlayerMessage("Your estuary wake crashes through the contact.");
            }
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
                || MutationMeddley_GetCurrentModeId() != "shell_up"
                || moved != 0)
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_ReliquaryProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_ReliquaryUnlockedKey);
            }
        }

        private void MutationMeddley_TrackUndertowHeartDiscovery(bool usedTidalMarrowsRecovery)
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_UndertowUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("tidal_marrows")
                || !MutationMeddley_HasMutation("Regeneration")
                || MutationMeddley_GetCurrentModeId() != "draw_brine"
                || !usedTidalMarrowsRecovery)
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_UndertowProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_UndertowUnlockedKey);
            }
        }

        private void MutationMeddley_TrackAbyssalBrineDiscovery(bool saline)
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_AbyssalUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("cool_sump")
                || !MutationMeddley_HasMutation("Freezing Ray")
                || MutationMeddley_GetCurrentModeId() != "cool_reserve"
                || (!MutationMeddley_IsCurrentCellWet() && !saline))
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_AbyssalProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_AbyssalUnlockedKey);
            }
        }

        private bool MutationMeddley_IsTriadActive(string id)
        {
            return MutationMeddley_IsSynergyActive(new MutationMeddley_SynergyDefinition(id, "", ""));
        }
    }
}
