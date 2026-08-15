using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_LivingCrystal : MutationMeddley_AdaptiveMutationBase
    {
        private const string MutationMeddley_MovedKey = "lc_moved";
        private const string MutationMeddley_StationaryKey = "lc_stationary";
        private const string MutationMeddley_CadenceKey = "lc_cadence";
        private const string MutationMeddley_FracturedChoirUnlockedKey = "lc_hidden_choir";
        private const string MutationMeddley_FracturedChoirProgressKey = "lc_hidden_choir_progress";
        private const string MutationMeddley_HeatSinkUnlockedKey = "lc_hidden_heatsink";
        private const string MutationMeddley_HeatSinkProgressKey = "lc_hidden_heatsink_progress";
        private const string MutationMeddley_SolarWakeUnlockedKey = "lc_hidden_solar";
        private const string MutationMeddley_SolarWakeProgressKey = "lc_hidden_solar_progress";
        private const string MutationMeddley_NullPrismUnlockedKey = "lc_hidden_null";
        private const string MutationMeddley_NullPrismProgressKey = "lc_hidden_null_progress";

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
                if (MutationMeddley_HasEvolution("resonant_crystal"))
                {
                    MutationMeddley_SetStateInt(
                        MutationMeddley_CadenceKey,
                        Math.Min(MutationMeddley_GetStateInt(MutationMeddley_CadenceKey, 0) + 1, 6)
                    );
                }

                MutationMeddley_RefreshPassiveEffects();
            }
            else if (E.ID == "EndTurn")
            {
                int moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0);
                MutationMeddley_SetStateInt(MutationMeddley_StationaryKey, moved == 0 ? 1 : 0);

                if (MutationMeddley_HasEvolution("resonant_crystal") && moved == 0)
                {
                    MutationMeddley_SetStateInt(
                        MutationMeddley_CadenceKey,
                        Math.Max(MutationMeddley_GetStateInt(MutationMeddley_CadenceKey, 0) - 1, 0)
                    );
                }
                else if (!MutationMeddley_HasEvolution("resonant_crystal"))
                {
                    MutationMeddley_SetStateInt(MutationMeddley_CadenceKey, 0);
                }

                MutationMeddley_TrackFracturedChoirDiscovery();
                MutationMeddley_TrackHeatSinkChoirDiscovery();
                MutationMeddley_TrackSolarWakeDiscovery();
                MutationMeddley_TrackNullPrismDiscovery();
                MutationMeddley_ProcessCrystalTurn();
                MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 0);
                MutationMeddley_RefreshPassiveEffects();
            }

            return base.FireEvent(E);
        }

        public override string GetDescription()
        {
            return "Your body is slowly replacing pliant tissue with living crystal.\n\n"
                + "Living Crystal is a build-defining mutation focused on branch identity, posture changes, hard tradeoffs, and visible interactions with other mutations.";
        }

        public override string GetLevelText(int Level)
        {
            string cadenceText = MutationMeddley_HasEvolution("resonant_crystal")
                ? "Cadence: " + MutationMeddley_GetEffectiveCadence() + "\n"
                : string.Empty;

            return "Rank 3: choose a crystalline identity.\n"
                + "Rank 6: specialize that identity.\n"
                + "Rank 9: secure its capstone.\n\n"
                + MutationMeddley_GetUsageSummary()
                + "\n\n"
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState()
                + "\n"
                + cadenceText
                + MutationMeddley_GetCurrentMechanicsSummary()
                + "\n"
                + MutationMeddley_GetPassiveBonusSummary()
                + "\n"
                + MutationMeddley_GetSynergySummary();
        }

        protected override IEnumerable<string> MutationMeddley_GetCurrentMechanicNotes()
        {
            if (MutationMeddley_HasEvolution("diamond_lattice"))
            {
                yield return "Diamond Lattice rewards bracing and close pressure. End turns engaged or stationary to convert crystal stress into recovery.";
                if (MutationMeddley_HasEvolution("heat_sink_choir"))
                {
                    yield return "Heat Sink Choir only pays out in genuinely hot cells, not from passive loadout ownership.";
                }
            }
            else if (MutationMeddley_HasEvolution("prismatic_matrix"))
            {
                yield return "Prismatic Matrix changes with lighting. Dawn paths want lit cells; dusk paths want dim cells.";
                if (MutationMeddley_HasEvolution("solar_wake"))
                {
                    yield return "Solar Wake turns lit movement into a stronger refractive payoff.";
                }
                if (MutationMeddley_HasEvolution("null_prism"))
                {
                    yield return "Null Prism rewards staying in dim cells and leaning into phase-style evasive play.";
                }
            }
            else if (MutationMeddley_HasEvolution("resonant_crystal"))
            {
                yield return "Resonant Crystal builds cadence from movement. Higher cadence feeds both defense and end-turn recovery.";
                if (MutationMeddley_HasEvolution("tuning_fork_frame"))
                {
                    yield return "Tuning Fork Frame prefers stillness after buildup; Choral Spines prefers continued movement.";
                }
            }
            else
            {
                yield return "Choose a crystalline identity first to unlock the active crystal loop.";
            }
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
                    detailText: "Structural identity. Rewards pressure, bracing, and contact."
                ),
                new MutationMeddley_EvolutionChoice(
                    "prismatic_matrix",
                    "Prismatic Matrix",
                    "Split light and threat through refractive geometry.",
                    3,
                    1,
                    detailText: "Light identity. Your shell changes behavior in lit and unlit spaces."
                ),
                new MutationMeddley_EvolutionChoice(
                    "resonant_crystal",
                    "Resonant Crystal",
                    "Turn your body into a humming instrument of stress and motion.",
                    3,
                    1,
                    detailText: "Rhythm identity. Movement builds cadence that branches spend differently."
                ),
                new MutationMeddley_EvolutionChoice(
                    "faceted_bulwark",
                    "Faceted Bulwark",
                    "Your facets spread force across broad defensive planes.",
                    6,
                    2,
                    "diamond_lattice",
                    "Best when enemies stay on you and you answer pressure with certainty."
                ),
                new MutationMeddley_EvolutionChoice(
                    "dense_core",
                    "Dense Core",
                    "Your crystal mass condenses into a brutal, rooted center.",
                    6,
                    2,
                    "diamond_lattice",
                    "Best when you hold ground and let the shell take the load."
                ),
                new MutationMeddley_EvolutionChoice(
                    "sunlens_array",
                    "Sunlens Array",
                    "Facet geometry drinks in bright exposure and redirects it.",
                    6,
                    2,
                    "prismatic_matrix",
                    "Lit cells become your strongest operating space."
                ),
                new MutationMeddley_EvolutionChoice(
                    "shade_reflector",
                    "Shade Reflector",
                    "Your shell refracts threat through dimness and angled cover.",
                    6,
                    2,
                    "prismatic_matrix",
                    "Dark cells and low light become part of your defense."
                ),
                new MutationMeddley_EvolutionChoice(
                    "choral_spines",
                    "Choral Spines",
                    "Movement turns your body into a predatory crystalline chorus.",
                    6,
                    2,
                    "resonant_crystal",
                    "Cadence becomes aggression and tempo."
                ),
                new MutationMeddley_EvolutionChoice(
                    "tuning_fork_frame",
                    "Tuning Fork Frame",
                    "Your crystal frame stores rhythm and releases it as guarded precision.",
                    6,
                    2,
                    "resonant_crystal",
                    "Cadence becomes steadiness and timing."
                ),
                new MutationMeddley_EvolutionChoice(
                    "impact_cathedral",
                    "Impact Cathedral",
                    "Your shell becomes a sanctuary for crushing contact and held lines.",
                    9,
                    3,
                    "faceted_bulwark",
                    "Capstone contact-defense line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "heat_sink_choir",
                    "Heat Sink Choir",
                    "Thermal shock teaches your lattice to turn impact and glare into dangerous stored rhythm.",
                    9,
                    3,
                    "faceted_bulwark",
                    "UNUSUAL ADAPTATION. Requires repeated thermal shock while carrying Ash Metabolism.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "anchor_maze",
                    "Anchor Maze",
                    "Your rooted shell becomes a nearly immovable labyrinth of facets.",
                    9,
                    3,
                    "dense_core",
                    "Capstone bracing line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "mirrorshard_halo",
                    "Mirrorshard Halo",
                    "Bright light blooms into a hard corona of refracted defense.",
                    9,
                    3,
                    "sunlens_array",
                    "Capstone bright-space line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "solar_wake",
                    "Solar Wake",
                    "Bright refraction stops being shelter alone and starts dragging enemies through your glare lines.",
                    9,
                    3,
                    "sunlens_array",
                    "UNUSUAL ADAPTATION. Requires repeated lit-space refraction while carrying Light Manipulation.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "eclipse_veil",
                    "Eclipse Veil",
                    "Dimness gathers around your shell as layered concealment and refraction.",
                    9,
                    3,
                    "shade_reflector",
                    "Capstone dark-space line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "null_prism",
                    "Null Prism",
                    "Darkness and phase-state stop behaving like cover and start behaving like brittle absence.",
                    9,
                    3,
                    "shade_reflector",
                    "UNUSUAL ADAPTATION. Requires prolonged dim-space play while carrying Phasing.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "song_of_fracture",
                    "Song of Fracture",
                    "Your stride becomes a sharp, escalating crystalline attack rhythm.",
                    9,
                    3,
                    "choral_spines",
                    "Capstone mobile cadence line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "fractured_choir",
                    "Fractured Choir",
                    "Sonic trauma teaches your lattice to sing through its own fractures.",
                    9,
                    3,
                    "choral_spines",
                    "UNUSUAL ADAPTATION. Requires sustained high cadence with a resonance-friendly build.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "stilltone_engine",
                    "Stilltone Engine",
                    "Stored rhythm resolves into an uncanny, poised defensive engine.",
                    9,
                    3,
                    "tuning_fork_frame",
                    "Capstone guarded cadence line."
                )
            };
        }

        protected override IEnumerable<string> MutationMeddley_GetIntrinsicSemanticTags()
        {
            return new string[] { "KEYSTONE", "CRYSTALLINE", "STRUCTURAL", "BIOLOGICAL" };
        }

        protected override IEnumerable<string> MutationMeddley_GetEvolutionSemanticTags()
        {
            List<string> tags = new List<string>();
            if (MutationMeddley_HasEvolution("diamond_lattice"))
            {
                tags.Add("KINETIC");
            }

            if (MutationMeddley_HasEvolution("prismatic_matrix"))
            {
                tags.Add("RADIANT");
                tags.Add("LIGHT_INTERACTION");
            }

            if (MutationMeddley_HasEvolution("resonant_crystal"))
            {
                tags.Add("RESONANT");
                tags.Add("SOUND_INTERACTION");
            }

            return tags;
        }

        protected override List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices()
        {
            if (MutationMeddley_HasEvolution("diamond_lattice"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("facet_lock", "Facet Lock", "Compress into a dense, anchored shell."),
                    new MutationMeddley_ModeChoice("saw_edges", "Saw Edges", "Open sharp seams for more active contact defense.")
                };
            }

            if (MutationMeddley_HasEvolution("prismatic_matrix"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("dawn_glare", "Dawn Glare", "Favor bright-space refraction."),
                    new MutationMeddley_ModeChoice("dusk_glare", "Dusk Glare", "Favor dim-space refraction.")
                };
            }

            if (MutationMeddley_HasEvolution("resonant_crystal"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("pulse_step", "Pulse Step", "Turn cadence into motion and attack rhythm."),
                    new MutationMeddley_ModeChoice("humming_guard", "Humming Guard", "Turn cadence into poised defense.")
                };
            }

            return new List<MutationMeddley_ModeChoice>();
        }

        protected override List<MutationMeddley_SynergyDefinition> MutationMeddley_GetSynergyDefinitions()
        {
            return new List<MutationMeddley_SynergyDefinition>
            {
                new MutationMeddley_SynergyDefinition("electrical_generation", "Electrical Generation", "Piezoelectric lattice hardens or quickens under stored charge."),
                new MutationMeddley_SynergyDefinition("light_manipulation", "Light Manipulation", "Refracted geometry becomes easier to weaponize and defend with."),
                new MutationMeddley_SynergyDefinition("flaming_ray", "Flaming Ray", "Heat-loaded facets reward bright, aggressive crystalline play."),
                new MutationMeddley_SynergyDefinition("freezing_ray", "Freezing Ray", "Thermal shock deepens cold-space and stillness patterns."),
                new MutationMeddley_SynergyDefinition("phasing", "Phasing", "Your lattice slips half a beat out of phase and becomes harder to pin down."),
                new MutationMeddley_SynergyDefinition("heightened_hearing", "Heightened Hearing", "Fine resonance awareness stabilizes cadence and sonic timing."),
                new MutationMeddley_SynergyDefinition("ash_pair", "Ash Metabolism", "Thermal and crystalline stress start feeding each other by branch."),
                new MutationMeddley_SynergyDefinition("walking_colony_pair", "Walking Colony", "A colony-aware body changes how your lattice carries strain and tempo."),
                new MutationMeddley_SynergyDefinition("brineborn_pair", "Brineborn", "Saltglass physiology mineralizes the lattice differently by branch."),
                new MutationMeddley_SynergyDefinition("carapace_pair", "Carapace Evolution", "Crystalline shell integration changes how your body carries armor and rhythm."),
                new MutationMeddley_SynergyDefinition("cathedral_organism", "Cathedral Organism", "Your shell, crystal, and saltglass defenses behave like one organ system.", isTriad: true),
                new MutationMeddley_SynergyDefinition("breakwater_predator", "Breakwater Predator", "Wet movement compounds cadence, pursuit, and saline violence.", isTriad: true),
                new MutationMeddley_SynergyDefinition("prism_estuary", "Prism Estuary", "Light, weather, and saline metabolism fold into one refractive ecology.", isTriad: true),
                new MutationMeddley_SynergyDefinition("glass_kiln_bastion", "Glass Kiln Bastion", "Heat-banked shell and crystal structure now punish impact together.", isTriad: true),
                new MutationMeddley_SynergyDefinition("ember_pursuit_engine", "Ember Pursuit Engine", "Cadence and heat-fed pursuit reinforce one another.", isTriad: true),
                new MutationMeddley_SynergyDefinition("mirage_exuvium", "Mirage Exuvium", "Light, smoke, and weathered shell become one evasive ecology.", isTriad: true),
                new MutationMeddley_SynergyDefinition("salt_kiln_reliquary", "Salt Kiln Reliquary", "Thermal mineralization hardens your salt-crystal defense.", isTriad: true),
                new MutationMeddley_SynergyDefinition("steam_choir", "Steam Choir", "Resonance now rides steam, smoke, and wet pursuit pressure.", isTriad: true),
                new MutationMeddley_SynergyDefinition("ossuary_rampart", "Ossuary Rampart", "Crystal density, shell fortification, and marrow ecology reinforce one wall.", isTriad: true),
                new MutationMeddley_SynergyDefinition("salt_eclipse", "Salt Eclipse", "Saline refraction and weathered shell tuning become one dim-space defense.", isTriad: true),
                new MutationMeddley_SynergyDefinition("resonant_undertow", "Resonant Undertow", "Cadence and reserve now feed motion and recovery together.", isTriad: true),
                new MutationMeddley_SynergyDefinition("smoke_reef", "Smoke Reef", "Smoke, mineral edges, and prismatic refraction mislead the whole battlefield.", isTriad: true),
                new MutationMeddley_SynergyDefinition("chorus_husk", "Chorus Husk", "Rhythm, membrane-shell logic, and distributed strain become one unstable body.", isTriad: true),
                new MutationMeddley_SynergyDefinition("blackglass_pursuit", "Blackglass Pursuit", "Pursuit, impact, and route memory start hunting as one frame.", isTriad: true),
                new MutationMeddley_SynergyDefinition("fractured_choir_state", "Fractured Choir", "Your lattice now answers motion with dangerous harmonic fracture.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("heat_sink_choir_state", "Heat Sink Choir", "Thermal shock now feeds a harsher impact rhythm through your lattice.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("solar_wake_state", "Solar Wake", "Bright refraction now drags enemies through your glare geometry.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("null_prism_state", "Null Prism", "Darkness and phase-state now create a brittle evasive absence around you.", isUnusual: true)
            };
        }

        protected override bool MutationMeddley_IsSynergyActive(MutationMeddley_SynergyDefinition synergy)
        {
            switch (synergy.Id)
            {
                case "electrical_generation":
                    return MutationMeddley_HasMutation("Electrical Generation")
                        && (MutationMeddley_HasEvolution("diamond_lattice") || MutationMeddley_HasEvolution("resonant_crystal"));
                case "light_manipulation":
                    return MutationMeddley_HasMutation("Light Manipulation") && MutationMeddley_HasEvolution("prismatic_matrix");
                case "flaming_ray":
                    return MutationMeddley_HasMutation("Flaming Ray")
                        && (MutationMeddley_HasEvolution("diamond_lattice") || MutationMeddley_HasEvolution("prismatic_matrix"));
                case "freezing_ray":
                    return MutationMeddley_HasMutation("Freezing Ray")
                        && (MutationMeddley_HasEvolution("diamond_lattice") || MutationMeddley_HasEvolution("prismatic_matrix"));
                case "phasing":
                    return MutationMeddley_HasMutation("Phasing")
                        && (MutationMeddley_HasEvolution("prismatic_matrix") || MutationMeddley_HasEvolution("resonant_crystal"));
                case "heightened_hearing":
                    return MutationMeddley_HasMutation("Heightened Hearing") && MutationMeddley_HasEvolution("resonant_crystal");
                case "ash_pair":
                    return MutationMeddley_HasMutation("Ash Metabolism")
                        && (MutationMeddley_HasEvolution("diamond_lattice")
                            || MutationMeddley_HasEvolution("prismatic_matrix")
                            || MutationMeddley_HasEvolution("resonant_crystal"));
                case "walking_colony_pair":
                    return MutationMeddley_HasMutation("Walking Colony") && MutationMeddley_HasAnyEvolution();
                case "brineborn_pair":
                    return MutationMeddley_HasMutation("Brineborn") && MutationMeddley_HasAnyEvolution();
                case "carapace_pair":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && (
                            MutationMeddley_HasEvolution("diamond_lattice")
                            || MutationMeddley_HasEvolution("prismatic_matrix")
                            || MutationMeddley_HasEvolution("resonant_crystal")
                        );
                case "cathedral_organism":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress")
                        && MutationMeddley_HasEvolution("diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "saltglass_bloom");
                case "breakwater_predator":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell")
                        && MutationMeddley_HasEvolution("resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary");
                case "prism_estuary":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace")
                        && MutationMeddley_HasEvolution("prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "wellspring_flesh");
                case "glass_kiln_bastion":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "furnace_skin");
                case "ember_pursuit_engine":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "cinder_gut");
                case "mirage_exuvium":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "smoke_organ");
                case "salt_kiln_reliquary":
                    return MutationMeddley_HasEvolution("diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "saltglass_bloom")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "furnace_skin");
                case "steam_choir":
                    return MutationMeddley_HasEvolution("resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "smoke_organ");
                case "ossuary_rampart":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "marrow_hive");
                case "salt_eclipse":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "saltglass_bloom")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace");
                case "resonant_undertow":
                    return MutationMeddley_HasEvolution("resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "wellspring_flesh")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "surveyor_swarm");
                case "smoke_reef":
                    return MutationMeddley_HasEvolution("prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "smoke_organ")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "saltglass_bloom");
                case "chorus_husk":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "graft_parliament")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace");
                case "blackglass_pursuit":
                    return MutationMeddley_HasEvolution("diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "cinder_gut")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "surveyor_swarm");
                case "fractured_choir_state":
                    return MutationMeddley_HasEvolution("fractured_choir");
                case "heat_sink_choir_state":
                    return MutationMeddley_HasEvolution("heat_sink_choir");
                case "solar_wake_state":
                    return MutationMeddley_HasEvolution("solar_wake");
                case "null_prism_state":
                    return MutationMeddley_HasEvolution("null_prism");
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

            if (choice.Id == "fractured_choir")
            {
                return MutationMeddley_GetStateInt(MutationMeddley_FracturedChoirUnlockedKey, 0) > 0;
            }

            if (choice.Id == "heat_sink_choir")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_HeatSinkUnlockedKey);
            }

            if (choice.Id == "solar_wake")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_SolarWakeUnlockedKey);
            }

            if (choice.Id == "null_prism")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_NullPrismUnlockedKey);
            }

            return false;
        }

        protected override void MutationMeddley_RefreshPassiveEffects()
        {
            MutationMeddley_ClearCommonStatShifts();

            bool engaged = ParentObject != null && ParentObject.IsEngagedInMelee();
            bool stationary = MutationMeddley_GetStateInt(MutationMeddley_StationaryKey, 0) > 0;
            bool lit = MutationMeddley_IsCurrentCellLit();
            bool saline = MutationMeddley_IsCurrentCellSaline();
            int cadence = MutationMeddley_GetEffectiveCadence();

            if (MutationMeddley_HasEvolution("diamond_lattice"))
            {
                MutationMeddley_SetShift("AV", 1);
                MutationMeddley_SetShift("Toughness", 1);
                if (MutationMeddley_HasEvolution("faceted_bulwark"))
                {
                    if (engaged)
                    {
                        MutationMeddley_SetShift("AV", MutationMeddley_HasEvolution("impact_cathedral") ? 5 : 4);
                        MutationMeddley_SetShift("DV", MutationMeddley_GetCurrentModeId() == "saw_edges" ? 2 : 1);
                    }
                    else
                    {
                        MutationMeddley_SetShift("AV", 2);
                    }
                }
                else if (MutationMeddley_HasEvolution("dense_core"))
                {
                    if (stationary)
                    {
                        MutationMeddley_SetShift("AV", MutationMeddley_HasEvolution("anchor_maze") ? 6 : 4);
                        MutationMeddley_SetShift("DV", MutationMeddley_GetCurrentModeId() == "saw_edges" ? 1 : 0);
                    }
                    else
                    {
                        MutationMeddley_SetShift("AV", 2);
                    }
                }
                else
                {
                    MutationMeddley_SetShift("AV", engaged ? 3 : 2);
                }

                if (MutationMeddley_HasMutation("Electrical Generation"))
                {
                    MutationMeddley_SetShift("AV", stationary ? 1 : 0);
                    MutationMeddley_SetShift("DV", engaged ? 1 : 0);
                }

                if (MutationMeddley_HasMutation("Ash Metabolism"))
                {
                    MutationMeddley_SetShift("HeatResistance", 5);
                    if (MutationMeddley_HasEvolution("heat_sink_choir"))
                    {
                        MutationMeddley_SetShift("Quickness", engaged ? 1 : 0);
                    }
                }

                if (MutationMeddley_HasMutation("Flaming Ray"))
                {
                    MutationMeddley_SetShift("HeatResistance", 10 + (lit ? 5 : 0));
                }

                if (MutationMeddley_HasMutation("Freezing Ray"))
                {
                    MutationMeddley_SetShift("ColdResistance", 10 + (stationary ? 5 : 0));
                }
            }
            else if (MutationMeddley_HasEvolution("prismatic_matrix"))
            {
                MutationMeddley_SetShift("Ego", 1);

                if (MutationMeddley_HasEvolution("sunlens_array"))
                {
                    if (lit)
                    {
                        MutationMeddley_SetShift(
                            "DV",
                            (MutationMeddley_HasEvolution("mirrorshard_halo") ? 4 : 3)
                            + (MutationMeddley_GetCurrentModeId() == "dawn_glare" ? 1 : 0)
                        );
                        MutationMeddley_SetShift("HeatResistance", MutationMeddley_HasEvolution("mirrorshard_halo") ? 35 : 20);
                    }
                    else
                    {
                        MutationMeddley_SetShift("DV", 1);
                        MutationMeddley_SetShift("HeatResistance", 10);
                    }
                }
                else if (MutationMeddley_HasEvolution("shade_reflector"))
                {
                    if (!lit)
                    {
                        MutationMeddley_SetShift(
                            "DV",
                            (MutationMeddley_HasEvolution("eclipse_veil") ? 5 : 3)
                            + (MutationMeddley_GetCurrentModeId() == "dusk_glare" ? 1 : 0)
                        );
                        MutationMeddley_SetShift("ColdResistance", MutationMeddley_HasEvolution("eclipse_veil") ? 35 : 20);
                    }
                    else
                    {
                        MutationMeddley_SetShift("DV", 1);
                        MutationMeddley_SetShift("ColdResistance", 10);
                    }
                }
                else
                {
                    MutationMeddley_SetShift("DV", lit ? 2 : 1);
                    MutationMeddley_SetShift("HeatResistance", MutationMeddley_GetCurrentModeId() == "dawn_glare" ? 15 : 5);
                    MutationMeddley_SetShift("ColdResistance", MutationMeddley_GetCurrentModeId() == "dusk_glare" ? 15 : 5);
                }

                if (MutationMeddley_HasMutation("Light Manipulation"))
                {
                    MutationMeddley_SetShift("DV", lit ? 1 : 0);
                }

                if (MutationMeddley_HasMutation("Flaming Ray"))
                {
                    MutationMeddley_SetShift("HeatResistance", 10);
                    if (lit)
                    {
                        MutationMeddley_SetShift("DV", 1);
                    }
                }

                if (MutationMeddley_HasMutation("Freezing Ray"))
                {
                    MutationMeddley_SetShift("ColdResistance", 10);
                    if (!lit)
                    {
                        MutationMeddley_SetShift("DV", 1);
                    }
                }

                if (MutationMeddley_HasMutation("Phasing"))
                {
                    MutationMeddley_SetShift("DV", 1);
                    if (!lit)
                    {
                        MutationMeddley_SetShift("Quickness", 1);
                    }
                }

                if (MutationMeddley_HasMutation("Ash Metabolism"))
                {
                    MutationMeddley_SetShift("HeatResistance", lit ? 10 : 5);
                }
            }
            else if (MutationMeddley_HasEvolution("resonant_crystal"))
            {
                MutationMeddley_SetShift("Agility", 1);

                if (MutationMeddley_HasEvolution("choral_spines"))
                {
                    MutationMeddley_SetShift("Quickness", cadence + (MutationMeddley_HasEvolution("song_of_fracture") ? 2 : 0));
                    MutationMeddley_SetShift(
                        "DV",
                        MutationMeddley_GetCurrentModeId() == "pulse_step" ? Math.Max(cadence - 1, 0) : cadence / 2
                    );
                }
                else if (MutationMeddley_HasEvolution("tuning_fork_frame"))
                {
                    MutationMeddley_SetShift("DV", cadence + (MutationMeddley_HasEvolution("stilltone_engine") ? 2 : 0));
                    MutationMeddley_SetShift("AV", MutationMeddley_GetCurrentModeId() == "humming_guard" ? cadence / 2 : 0);
                }
                else
                {
                    MutationMeddley_SetShift("Quickness", cadence / 2);
                    MutationMeddley_SetShift("DV", cadence / 2);
                }

                if (MutationMeddley_HasEvolution("fractured_choir"))
                {
                    MutationMeddley_SetShift("Quickness", 2);
                    MutationMeddley_SetShift("DV", 1);
                }

                if (MutationMeddley_HasMutation("Phasing"))
                {
                    MutationMeddley_SetShift("DV", 1);
                }

                if (MutationMeddley_HasMutation("Walking Colony"))
                {
                    MutationMeddley_SetShift("Quickness", cadence >= 3 ? 1 : 0);
                }
            }

            if (MutationMeddley_HasMutation("Heightened Hearing") && MutationMeddley_HasEvolution("resonant_crystal"))
            {
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasMutation("Electrical Generation") && MutationMeddley_HasEvolution("resonant_crystal"))
            {
                MutationMeddley_SetShift("Quickness", 1);
            }

            if (MutationMeddley_HasMutation("Brineborn"))
            {
                if (MutationMeddley_HasEvolution("diamond_lattice") && saline)
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("prismatic_matrix") && saline && lit)
                {
                    MutationMeddley_SetShift("DV", 1);
                }
                else if (MutationMeddley_HasEvolution("resonant_crystal") && saline)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }

            if (MutationMeddley_HasMutation("Walking Colony"))
            {
                if (MutationMeddley_HasEvolution("diamond_lattice")
                    && MutationMeddley_MutationHasEvolution("Walking Colony", "marrow_hive"))
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("resonant_crystal")
                    && MutationMeddley_MutationHasEvolution("Walking Colony", "surveyor_swarm"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("prismatic_matrix")
                    && MutationMeddley_MutationHasEvolution("Walking Colony", "graft_parliament"))
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution"))
            {
                if (MutationMeddley_HasEvolution("diamond_lattice")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress")
                    && stationary)
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("prismatic_matrix")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace")
                    && lit)
                {
                    MutationMeddley_SetShift("HeatResistance", 5);
                    MutationMeddley_SetShift("ColdResistance", 5);
                }
                else if (MutationMeddley_HasEvolution("resonant_crystal")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }

            if (MutationMeddley_IsTriadActive("cathedral_organism") && stationary && saline)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("breakwater_predator")
                && MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0
                && MutationMeddley_IsCurrentCellWet())
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_IsTriadActive("prism_estuary") && lit && saline)
            {
                MutationMeddley_SetShift("HeatResistance", 10);
                MutationMeddley_SetShift("ColdResistance", 10);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("glass_kiln_bastion") && engaged)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("HeatResistance", 5);
            }

            if (MutationMeddley_IsTriadActive("ember_pursuit_engine") && MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_IsTriadActive("mirage_exuvium") && lit)
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("salt_kiln_reliquary") && saline)
            {
                MutationMeddley_SetShift("AV", 1);
                MutationMeddley_SetShift("HeatResistance", 5);
            }

            if (MutationMeddley_IsTriadActive("steam_choir") && MutationMeddley_IsCurrentCellWet())
            {
                MutationMeddley_SetShift("Quickness", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("ossuary_rampart") && stationary)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("salt_eclipse") && !lit && saline)
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("resonant_undertow") && MutationMeddley_IsCurrentCellWet())
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_IsTriadActive("smoke_reef") && MutationMeddley_IsCurrentCellSmoky())
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("chorus_husk"))
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("blackglass_pursuit") && MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_HasEvolution("heat_sink_choir"))
            {
                MutationMeddley_SetShift("HeatResistance", 10);
                MutationMeddley_SetShift("AV", engaged ? 1 : 0);
            }

            if (MutationMeddley_HasEvolution("solar_wake") && lit)
            {
                MutationMeddley_SetShift("DV", 2);
                MutationMeddley_SetShift("Quickness", 1);
            }

            if (MutationMeddley_HasEvolution("null_prism") && !lit)
            {
                MutationMeddley_SetShift("DV", 2);
                MutationMeddley_SetShift("Quickness", 1);
            }
        }

        private int MutationMeddley_GetEffectiveCadence()
        {
            int cadence = MutationMeddley_GetStateInt(MutationMeddley_CadenceKey, 0);
            if (MutationMeddley_HasMutation("Heightened Hearing"))
            {
                cadence += 1;
            }

            if (MutationMeddley_HasMutation("Electrical Generation") && MutationMeddley_HasEvolution("diamond_lattice"))
            {
                cadence += 1;
            }

            if (MutationMeddley_HasMutation("Brineborn") && MutationMeddley_IsCurrentCellSaline())
            {
                cadence += 1;
            }

            if (MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell")
                && MutationMeddley_HasEvolution("resonant_crystal"))
            {
                cadence += 1;
            }

            if (MutationMeddley_HasEvolution("fractured_choir"))
            {
                cadence += 1;
            }

            if (MutationMeddley_HasEvolution("heat_sink_choir"))
            {
                cadence += 1;
            }

            return Math.Min(cadence, 8);
        }

        private void MutationMeddley_ProcessCrystalTurn()
        {
            if (ParentObject == null)
            {
                return;
            }

            bool engaged = ParentObject.IsEngagedInMelee();
            bool stationary = MutationMeddley_GetStateInt(MutationMeddley_StationaryKey, 0) > 0;
            bool moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0;
            bool lit = MutationMeddley_IsCurrentCellLit();
            bool hot = MutationMeddley_IsCurrentCellHot();
            int cadence = MutationMeddley_GetEffectiveCadence();

            if (MutationMeddley_HasEvolution("diamond_lattice"))
            {
                int healing = 0;

                if (MutationMeddley_HasEvolution("faceted_bulwark") && engaged)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("dense_core") && stationary)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("impact_cathedral") && engaged)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("anchor_maze") && stationary)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("heat_sink_choir") && hot)
                {
                    healing += 1;
                }

                MutationMeddley_TryHeal(healing);
                return;
            }

            if (MutationMeddley_HasEvolution("prismatic_matrix"))
            {
                int healing = 0;

                if (MutationMeddley_HasEvolution("sunlens_array")
                    && lit
                    && MutationMeddley_GetCurrentModeId() == "dawn_glare")
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("shade_reflector")
                    && !lit
                    && MutationMeddley_GetCurrentModeId() == "dusk_glare")
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("mirrorshard_halo") && lit)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("eclipse_veil") && !lit)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("solar_wake") && lit && moved)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("null_prism") && !lit && stationary)
                {
                    healing += 1;
                }

                MutationMeddley_TryHeal(healing);
                return;
            }

            if (MutationMeddley_HasEvolution("resonant_crystal"))
            {
                int healing = 0;

                if (cadence >= 3)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("choral_spines") && moved)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("tuning_fork_frame") && stationary)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("song_of_fracture") && cadence >= 4)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("stilltone_engine") && cadence >= 4 && stationary)
                {
                    healing += 1;
                }

                if (MutationMeddley_HasEvolution("fractured_choir") && cadence >= 5)
                {
                    healing += 1;
                }

                MutationMeddley_TryHeal(healing);
            }
        }

        private void MutationMeddley_TrackFracturedChoirDiscovery()
        {
            if (MutationMeddley_GetStateInt(MutationMeddley_FracturedChoirUnlockedKey, 0) > 0)
            {
                return;
            }

            if (MutationMeddley_HasSelectionAtTier(3))
            {
                return;
            }

            if (!MutationMeddley_HasEvolution("choral_spines") || !MutationMeddley_HasMutation("Heightened Hearing"))
            {
                return;
            }

            if (MutationMeddley_GetEffectiveCadence() >= 4)
            {
                int progress = MutationMeddley_GetStateInt(MutationMeddley_FracturedChoirProgressKey, 0) + 1;
                MutationMeddley_SetStateInt(MutationMeddley_FracturedChoirProgressKey, progress);
                if (progress >= 4)
                {
                    MutationMeddley_SetStateInt(MutationMeddley_FracturedChoirUnlockedKey, 1);
                }
            }
        }

        private void MutationMeddley_TrackHeatSinkChoirDiscovery()
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_HeatSinkUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("faceted_bulwark")
                || !MutationMeddley_HasMutation("Ash Metabolism")
                || !MutationMeddley_HasMutation("Flaming Ray")
                || !MutationMeddley_IsCurrentCellHot())
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_HeatSinkProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_HeatSinkUnlockedKey);
            }
        }

        private void MutationMeddley_TrackSolarWakeDiscovery()
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_SolarWakeUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("sunlens_array")
                || !MutationMeddley_HasMutation("Light Manipulation")
                || !MutationMeddley_IsCurrentCellLit())
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_SolarWakeProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_SolarWakeUnlockedKey);
            }
        }

        private void MutationMeddley_TrackNullPrismDiscovery()
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_NullPrismUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("shade_reflector")
                || !MutationMeddley_HasMutation("Phasing")
                || MutationMeddley_IsCurrentCellLit())
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_NullPrismProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_NullPrismUnlockedKey);
            }
        }

        private bool MutationMeddley_IsTriadActive(string id)
        {
            return MutationMeddley_IsSynergyActive(new MutationMeddley_SynergyDefinition(id, "", ""));
        }
    }
}
