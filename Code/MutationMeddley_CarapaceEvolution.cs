using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_CarapaceEvolution : MutationMeddley_AdaptiveMutationBase
    {
        private const string MutationMeddley_MovedKey = "carapace_moved";
        private const string MutationMeddley_StationaryKey = "carapace_stationary";
        private const string MutationMeddley_BraceKey = "carapace_brace";
        private const string MutationMeddley_ImpactKey = "carapace_impact";
        private const string MutationMeddley_HeatAttuneKey = "carapace_attune_heat";
        private const string MutationMeddley_MireAttuneKey = "carapace_attune_mire";
        private const string MutationMeddley_RimeAttuneKey = "carapace_attune_rime";
        private const string MutationMeddley_PorcupineUnlockedKey = "carapace_hidden_porcupine";
        private const string MutationMeddley_PorcupineProgressKey = "carapace_hidden_porcupine_progress";
        private const string MutationMeddley_EstuaryUnlockedKey = "carapace_hidden_estuary";
        private const string MutationMeddley_EstuaryProgressKey = "carapace_hidden_estuary_progress";
        private const string MutationMeddley_SkitterUnlockedKey = "carapace_hidden_skitter";
        private const string MutationMeddley_SkitterProgressKey = "carapace_hidden_skitter_progress";
        private const string MutationMeddley_HookstormUnlockedKey = "carapace_hidden_hookstorm";
        private const string MutationMeddley_HookstormProgressKey = "carapace_hidden_hookstorm_progress";

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
                MutationMeddley_RefreshPassiveEffects();
            }
            else if (E.ID == "AttackerDealtDamage")
            {
                MutationMeddley_HandleShellStrike(E);
                MutationMeddley_RefreshPassiveEffects();
            }
            else if (E.ID == "TookDamage")
            {
                MutationMeddley_HandleShellPressure(E, false);
                MutationMeddley_RefreshPassiveEffects();
            }
            else if (E.ID == "TookEnvironmentalDamage")
            {
                MutationMeddley_HandleShellPressure(E, true);
                MutationMeddley_RefreshPassiveEffects();
            }
            else if (E.ID == "EndTurn")
            {
                MutationMeddley_SetStateInt(
                    MutationMeddley_StationaryKey,
                    MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) == 0 ? 1 : 0
                );

                MutationMeddley_TrackPorcupineDiscovery();
                MutationMeddley_TrackEstuaryHuskDiscovery();
                MutationMeddley_TrackSkitterBulwarkDiscovery();
                MutationMeddley_TrackHookstormFrameDiscovery();
                if (MutationMeddley_HasMutation("Regeneration")
                    && MutationMeddley_IsFunctionallyActive()
                    && MutationMeddley_HasEvolution("fortress")
                    && MutationMeddley_GetStateInt(MutationMeddley_StationaryKey, 0) > 0
                    && ParentObject != null)
                {
                    ParentObject.Heal(1);
                }

                MutationMeddley_ProcessShellTurn();
                MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 0);
                MutationMeddley_RefreshPassiveEffects();
            }

            return base.FireEvent(E);
        }

        public override string GetDescription()
        {
            return "A Mutation Meddley companion evolution intended to pair with vanilla Carapace.\n\n"
                + "This mutation does not replace vanilla Carapace. Instead, it offers a separate branching shell-specialization layer that can be taken alongside it, with explicit shell synergies that go dormant safely when vanilla Carapace is missing.";
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
                + MutationMeddley_GetUsageSummary()
                + "\n\n"
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState()
                + "\n"
                + MutationMeddley_GetCurrentMechanicsSummary()
                + "\n"
                + MutationMeddley_GetPassiveBonusSummary()
                + "\n"
                + MutationMeddley_GetSynergySummary();
        }

        protected override bool MutationMeddley_IsFunctionallyActive()
        {
            return MutationMeddley_HasVanillaCarapace();
        }

        protected override string MutationMeddley_GetInactiveReason()
        {
            return "Carapace Evolution is dormant.\n\nTake vanilla Carapace first, then evolve the shell through Mutation Meddley.";
        }

        protected override IEnumerable<string> MutationMeddley_GetCurrentMechanicNotes()
        {
            if (!MutationMeddley_IsFunctionallyActive())
            {
                yield return "Dormant until vanilla Carapace is present on the character.";
                yield break;
            }

            if (MutationMeddley_HasEvolution("fortress"))
            {
                yield return "Fortress builds brace when you hold ground, then spends brace into shell punishment or line-holding when pressure lands.";
                yield return "Current brace: " + MutationMeddley_GetStateInt(MutationMeddley_BraceKey, 0) + ".";
                if (MutationMeddley_HasEvolution("faceted_keep"))
                {
                    yield return "Faceted Keep turns spent brace into punishing close-shell follow-through.";
                }
                if (MutationMeddley_HasEvolution("entrenched_bastion"))
                {
                    yield return "Entrenched Bastion cushions pressure more efficiently and keeps brace while planted.";
                }
            }
            else if (MutationMeddley_HasEvolution("hunter_shell"))
            {
                yield return "Hunter Shell builds impact from movement, then spends impact into skirmish tempo or a heavier shell-slam after contact.";
                yield return "Current impact: " + MutationMeddley_GetStateInt(MutationMeddley_ImpactKey, 0) + ".";
                if (MutationMeddley_HasEvolution("ravager_joints"))
                {
                    yield return "Ravager Joints retains more impact after repeated pursuit hits.";
                }
                if (MutationMeddley_HasEvolution("spur_lattice"))
                {
                    yield return "Spur Lattice converts spent impact into heavier shell-slam follow-through.";
                }
            }
            else if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                yield return "Adaptive Carapace stores heat, mire, and rime from the environment, then spends them into local shell responses under pressure or on contact.";
                yield return "Current attunement: heat "
                    + MutationMeddley_GetStateInt(MutationMeddley_HeatAttuneKey, 0)
                    + ", mire "
                    + MutationMeddley_GetStateInt(MutationMeddley_MireAttuneKey, 0)
                    + ", rime "
                    + MutationMeddley_GetStateInt(MutationMeddley_RimeAttuneKey, 0)
                    + ".";
                if (MutationMeddley_HasEvolution("thermal_baffles"))
                {
                    yield return "Thermal Baffles routes stored heat or rime harder depending on veil stance.";
                }
                if (MutationMeddley_HasEvolution("mire_sheath"))
                {
                    yield return "Mire Sheath stockpiles wet and saline contact into a membrane shell instead of a dry wall.";
                }
            }
            else
            {
                yield return "Choose a rank-3 shell identity first to unlock the real mechanics.";
            }
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
                    "porcupine_redoubt",
                    "Porcupine Redoubt",
                    "Quilled shell growth turns your fortification into a patient thorn wall.",
                    9,
                    3,
                    "faceted_keep",
                    "UNUSUAL ADAPTATION. Requires repeated rooted shell behavior with Quills.",
                    true
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
                    "skitter_bulwark",
                    "Skitter Bulwark",
                    "Articulated pursuit stops being pure speed and becomes a chase-wall that closes angles around prey.",
                    9,
                    3,
                    "ravager_joints",
                    "UNUSUAL ADAPTATION. Requires repeated pursuit turns while carrying Multiple Legs.",
                    true
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
                    "hookstorm_frame",
                    "Hookstorm Frame",
                    "Committed shell contact becomes a hazard field instead of a simple impact frame.",
                    9,
                    3,
                    "spur_lattice",
                    "UNUSUAL ADAPTATION. Requires repeated committed contact turns while carrying Quills.",
                    true
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
                ),
                new MutationMeddley_EvolutionChoice(
                    "estuary_husk",
                    "Estuary Husk",
                    "Your shell stops behaving like dry armor and starts breathing as a saline membrane.",
                    9,
                    3,
                    "mire_sheath",
                    "UNUSUAL ADAPTATION. Requires prolonged amphibious or saline shell play with live Carapace.",
                    true
                )
            };
        }

        protected override IEnumerable<string> MutationMeddley_GetIntrinsicSemanticTags()
        {
            return new string[] { "BIOLOGICAL", "STRUCTURAL", "CHITINOUS", "BODY_PART_INTERACTION" };
        }

        protected override IEnumerable<string> MutationMeddley_GetEvolutionSemanticTags()
        {
            List<string> tags = new List<string>();
            if (MutationMeddley_HasEvolution("fortress"))
            {
                tags.Add("RETALIATORY");
            }

            if (MutationMeddley_HasEvolution("hunter_shell"))
            {
                tags.Add("PREDATORY");
                tags.Add("PURSUIT");
                tags.Add("MOBILE");
            }

            if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                tags.Add("ENVIRONMENTAL");
                tags.Add("TERRAIN_INTERACTION");
            }

            return tags;
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

        protected override List<MutationMeddley_SynergyDefinition> MutationMeddley_GetSynergyDefinitions()
        {
            return new List<MutationMeddley_SynergyDefinition>
            {
                new MutationMeddley_SynergyDefinition("vanilla_carapace", "Carapace", "The evolution layer is currently augmenting a live vanilla shell."),
                new MutationMeddley_SynergyDefinition("multiple_legs", "Multiple Legs", "Articulated shell segments reward pursuit or repositioning."),
                new MutationMeddley_SynergyDefinition("quills", "Quills", "Your shell can reinterpret quills as a wall, hook, or hazard."),
                new MutationMeddley_SynergyDefinition("regeneration", "Regeneration", "Still shell stances recover more cleanly when the shell can repair itself."),
                new MutationMeddley_SynergyDefinition("burrowing_claws", "Burrowing Claws", "Terrain and shell framing now cooperate instead of competing."),
                new MutationMeddley_SynergyDefinition("amphibious", "Amphibious", "Wet ground and shell tuning interact more naturally."),
                new MutationMeddley_SynergyDefinition("ash_pair", "Ash Metabolism", "A live shell gives your heat ecology somewhere structural to settle."),
                new MutationMeddley_SynergyDefinition("walking_colony_pair", "Walking Colony", "A living colony changes how the shell handles burden, stillness, and pursuit."),
                new MutationMeddley_SynergyDefinition("living_crystal_pair", "Living Crystal", "Crystalline shell integration changes how the shell carries force or light."),
                new MutationMeddley_SynergyDefinition("brineborn_pair", "Brineborn", "Mineral deposition turns saline biology into shell architecture."),
                new MutationMeddley_SynergyDefinition("cathedral_organism", "Cathedral Organism", "Shell, crystal, and saline crust have become one fortification.", isTriad: true),
                new MutationMeddley_SynergyDefinition("breakwater_predator", "Breakwater Predator", "Liquid movement now compounds shell pursuit, cadence, and reserve pressure.", isTriad: true),
                new MutationMeddley_SynergyDefinition("prism_estuary", "Prism Estuary", "Weather tuning, light, and saline metabolism now share one shell logic.", isTriad: true),
                new MutationMeddley_SynergyDefinition("glass_kiln_bastion", "Glass Kiln Bastion", "Heat-banked crystal and shell fortification harden into one bastion.", isTriad: true),
                new MutationMeddley_SynergyDefinition("ember_pursuit_engine", "Ember Pursuit Engine", "Hunter shell timing now compounds with heat-fed cadence.", isTriad: true),
                new MutationMeddley_SynergyDefinition("mirage_exuvium", "Mirage Exuvium", "Adaptive shell tuning now shares one logic with light and smoke.", isTriad: true),
                new MutationMeddley_SynergyDefinition("ossuary_rampart", "Ossuary Rampart", "Shell, marrow ecology, and crystal structure become one anti-burst wall.", isTriad: true),
                new MutationMeddley_SynergyDefinition("drift_parliament", "Drift Parliament", "Hunter shell routing now carries colonial pressure across shallow-liquid lines.", isTriad: true),
                new MutationMeddley_SynergyDefinition("salt_eclipse", "Salt Eclipse", "Saline refraction and adaptive shell weathering become one dim-space defense.", isTriad: true),
                new MutationMeddley_SynergyDefinition("bone_kiln_parliament", "Bone Kiln Parliament", "Shell structure, heat, and colonial burden become one heavy body-plan fortress.", isTriad: true),
                new MutationMeddley_SynergyDefinition("chorus_husk", "Chorus Husk", "Rhythm, membrane-shell logic, and distributed strain become one unstable body.", isTriad: true),
                new MutationMeddley_SynergyDefinition("whitewater_ossuary", "Whitewater Ossuary", "Harsh traversal and deep sustain reinforce a slow moving wall.", isTriad: true),
                new MutationMeddley_SynergyDefinition("porcupine_redoubt_state", "Porcupine Redoubt", "Your rooted shell now treats quills as part of its fortification.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("estuary_husk_state", "Estuary Husk", "Your shell now behaves like a saline membrane instead of dry armor.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("skitter_bulwark_state", "Skitter Bulwark", "Your articulated pursuit shell now closes space like a chase-wall.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("hookstorm_frame_state", "Hookstorm Frame", "Committed shell contact now creates a hazard field instead of a single hit.", isUnusual: true)
            };
        }

        protected override bool MutationMeddley_IsSynergyActive(MutationMeddley_SynergyDefinition synergy)
        {
            switch (synergy.Id)
            {
                case "vanilla_carapace":
                    return MutationMeddley_HasVanillaCarapace();
                case "multiple_legs":
                    return MutationMeddley_HasMutation("Multiple Legs")
                        && MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("hunter_shell");
                case "quills":
                    return MutationMeddley_HasMutation("Quills")
                        && MutationMeddley_IsFunctionallyActive()
                        && (MutationMeddley_HasEvolution("fortress") || MutationMeddley_HasEvolution("hunter_shell"));
                case "regeneration":
                    return MutationMeddley_HasMutation("Regeneration")
                        && MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("fortress");
                case "burrowing_claws":
                    return MutationMeddley_HasMutation("Burrowing Claws")
                        && MutationMeddley_IsFunctionallyActive()
                        && (MutationMeddley_HasEvolution("hunter_shell") || MutationMeddley_HasEvolution("adaptive_carapace"));
                case "amphibious":
                    return MutationMeddley_HasMutation("Amphibious")
                        && MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("adaptive_carapace");
                case "ash_pair":
                    return MutationMeddley_HasMutation("Ash Metabolism")
                        && MutationMeddley_IsFunctionallyActive()
                        && (MutationMeddley_HasEvolution("fortress")
                            || MutationMeddley_HasEvolution("hunter_shell")
                            || MutationMeddley_HasEvolution("adaptive_carapace"));
                case "walking_colony_pair":
                    return MutationMeddley_HasMutation("Walking Colony")
                        && MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasAnyEvolution();
                case "living_crystal_pair":
                    return MutationMeddley_HasMutation("Living Crystal")
                        && MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasAnyEvolution();
                case "brineborn_pair":
                    return MutationMeddley_HasMutation("Brineborn")
                        && MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasAnyEvolution();
                case "cathedral_organism":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("fortress")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "saltglass_bloom");
                case "breakwater_predator":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("hunter_shell")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary");
                case "prism_estuary":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("adaptive_carapace")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "wellspring_flesh");
                case "glass_kiln_bastion":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("fortress")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "furnace_skin");
                case "ember_pursuit_engine":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("hunter_shell")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "cinder_gut");
                case "mirage_exuvium":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("adaptive_carapace")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "smoke_organ");
                case "ossuary_rampart":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("fortress")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "marrow_hive")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice");
                case "drift_parliament":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("hunter_shell")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "surveyor_swarm")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary");
                case "salt_eclipse":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("adaptive_carapace")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "saltglass_bloom")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix");
                case "bone_kiln_parliament":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("fortress")
                        && MutationMeddley_MutationHasEvolution("Ash Metabolism", "furnace_skin")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "graft_parliament");
                case "chorus_husk":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("adaptive_carapace")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "graft_parliament");
                case "whitewater_ossuary":
                    return MutationMeddley_IsFunctionallyActive()
                        && MutationMeddley_HasEvolution("fortress")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary")
                        && MutationMeddley_MutationHasEvolution("Walking Colony", "marrow_hive");
                case "porcupine_redoubt_state":
                    return MutationMeddley_HasEvolution("porcupine_redoubt");
                case "estuary_husk_state":
                    return MutationMeddley_HasEvolution("estuary_husk");
                case "skitter_bulwark_state":
                    return MutationMeddley_HasEvolution("skitter_bulwark");
                case "hookstorm_frame_state":
                    return MutationMeddley_HasEvolution("hookstorm_frame");
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

            if (choice.Id == "porcupine_redoubt")
            {
                return MutationMeddley_GetStateInt(MutationMeddley_PorcupineUnlockedKey, 0) > 0;
            }

            if (choice.Id == "estuary_husk")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_EstuaryUnlockedKey);
            }

            if (choice.Id == "skitter_bulwark")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_SkitterUnlockedKey);
            }

            if (choice.Id == "hookstorm_frame")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_HookstormUnlockedKey);
            }

            return false;
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
            bool wetGround = MutationMeddley_IsCurrentCellWet();
            bool lit = MutationMeddley_IsCurrentCellLit();
            bool saline = MutationMeddley_IsCurrentCellSaline();
            int brace = MutationMeddley_GetStateInt(MutationMeddley_BraceKey, 0);
            int impact = MutationMeddley_GetStateInt(MutationMeddley_ImpactKey, 0);
            int heat = MutationMeddley_GetStateInt(MutationMeddley_HeatAttuneKey, 0);
            int mire = MutationMeddley_GetStateInt(MutationMeddley_MireAttuneKey, 0);
            int rime = MutationMeddley_GetStateInt(MutationMeddley_RimeAttuneKey, 0);

            if (MutationMeddley_HasEvolution("fortress"))
            {
                MutationMeddley_SetShift("AV", 1);
                MutationMeddley_SetShift("Toughness", 1);
                MutationMeddley_SetShift("AV", 1 + (brace / 2));
                MutationMeddley_SetShift("DV", brace / 3);

                if (MutationMeddley_HasEvolution("faceted_keep"))
                {
                    MutationMeddley_SetShift("AV", engaged ? 1 + (brace / 2) : 1);
                    MutationMeddley_SetShift("Strength", engaged ? 1 + (brace / 3) : 0);
                    MutationMeddley_SetShift("DV", MutationMeddley_HasEvolution("living_fortress") && engaged ? 1 : 0);
                }
                else if (MutationMeddley_HasEvolution("entrenched_bastion"))
                {
                    MutationMeddley_SetShift("AV", stationary ? 1 + (brace / 2) : 1);
                    MutationMeddley_SetShift("Willpower", stationary ? 1 + (brace / 4) : 0);
                    if (MutationMeddley_HasEvolution("redoubt_engine") && stationary)
                    {
                        MutationMeddley_SetShift("DV", 1 + (brace / 3));
                    }
                }
                else
                {
                    MutationMeddley_SetShift("AV", 1 + (brace / 2));
                }

                if (MutationMeddley_HasMutation("Quills") && brace > 0)
                {
                    MutationMeddley_SetShift("DV", 1);
                    MutationMeddley_SetShift("AV", 1);
                }

                if (MutationMeddley_HasMutation("Regeneration") && brace > 1)
                {
                    MutationMeddley_SetShift("DV", 1);
                }

                if (MutationMeddley_HasMutation("Ash Metabolism"))
                {
                    MutationMeddley_SetShift("HeatResistance", 5 + brace);
                }

                if (MutationMeddley_GetCurrentModeId() == "anchor_down")
                {
                    MutationMeddley_SetShift("AV", stationary ? 1 + (brace / 2) : 0);
                    MutationMeddley_SetShift("DV", stationary ? brace / 3 : 0);
                }
                else if (MutationMeddley_GetCurrentModeId() == "spiteful_wall")
                {
                    MutationMeddley_SetShift("Strength", engaged ? 1 + (brace / 3) : 0);
                    MutationMeddley_SetShift("DV", engaged ? 1 + (brace / 3) : 0);
                }
            }
            else if (MutationMeddley_HasEvolution("hunter_shell"))
            {
                MutationMeddley_SetShift("DV", 1);
                MutationMeddley_SetShift("Agility", 1);
                MutationMeddley_SetShift("Strength", impact / 3);

                if (MutationMeddley_HasEvolution("ravager_joints"))
                {
                    MutationMeddley_SetShift("Quickness", 1 + (impact / 2) + (MutationMeddley_HasEvolution("pursuit_predator") ? 1 : 0));
                    MutationMeddley_SetShift("DV", engaged ? 1 + (impact / 2) : 1 + (impact / 3));
                }
                else if (MutationMeddley_HasEvolution("spur_lattice"))
                {
                    MutationMeddley_SetShift("AV", engaged ? 1 + (impact / 2) : 1);
                    MutationMeddley_SetShift("Strength", engaged ? 1 + (impact / 2) : 0);
                    MutationMeddley_SetShift("Quickness", 1 + (impact / 3));
                }
                else
                {
                    MutationMeddley_SetShift("Quickness", 1 + (impact / 3));
                    MutationMeddley_SetShift("DV", 1 + (impact / 2));
                }

                if (MutationMeddley_HasMutation("Multiple Legs") && impact > 0)
                {
                    MutationMeddley_SetShift("Quickness", 2);
                    MutationMeddley_SetShift("DV", 1);
                }

                if (MutationMeddley_HasMutation("Quills") && engaged)
                {
                    MutationMeddley_SetShift("AV", 1);
                }

                if (MutationMeddley_HasMutation("Burrowing Claws"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }

                if (MutationMeddley_HasMutation("Ash Metabolism") && impact > 0)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }

                if (MutationMeddley_GetCurrentModeId() == "skirmish_gait")
                {
                    MutationMeddley_SetShift("DV", impact / 2);
                    MutationMeddley_SetShift("Quickness", 1 + (impact / 3));
                }
                else if (MutationMeddley_GetCurrentModeId() == "ramming_gait")
                {
                    MutationMeddley_SetShift("AV", engaged ? 1 + (impact / 3) : impact / 3);
                    MutationMeddley_SetShift("Strength", 1 + (impact / 2));
                }
            }
            else if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                MutationMeddley_SetShift("Willpower", 1);
                MutationMeddley_SetShift("HeatResistance", 2 * heat);
                MutationMeddley_SetShift("ColdResistance", 2 * rime);
                MutationMeddley_SetShift("DV", mire / 2);

                if (MutationMeddley_HasEvolution("thermal_baffles"))
                {
                    MutationMeddley_SetShift("HeatResistance", MutationMeddley_GetCurrentModeId() == "ember_veil" ? heat * 2 : 0);
                    MutationMeddley_SetShift("ColdResistance", MutationMeddley_GetCurrentModeId() == "rime_veil" ? rime * 2 : 0);
                    MutationMeddley_SetShift("Strength", MutationMeddley_GetCurrentModeId() == "ember_veil" ? heat / 3 : 0);
                    MutationMeddley_SetShift("DV", 1 + (MutationMeddley_HasEvolution("storm_carapace") ? 1 + (Math.Max(heat, rime) / 2) : 0));
                }
                else if (MutationMeddley_HasEvolution("mire_sheath"))
                {
                    MutationMeddley_SetShift("DV", 1 + (mire / 2));
                    MutationMeddley_SetShift("AV", wetGround && MutationMeddley_HasEvolution("bog_shell") ? 1 + (mire / 2) : mire / 3);
                    MutationMeddley_SetShift("ColdResistance", wetGround ? 5 + (mire * 2) : mire);
                }
                else
                {
                    MutationMeddley_SetShift("HeatResistance", MutationMeddley_GetCurrentModeId() == "ember_veil" ? 5 + (heat * 2) : heat);
                    MutationMeddley_SetShift("ColdResistance", MutationMeddley_GetCurrentModeId() == "rime_veil" ? 5 + (rime * 2) : rime);
                }

                if (MutationMeddley_HasMutation("Amphibious") && wetGround)
                {
                    MutationMeddley_SetShift("DV", 1);
                }

                if (MutationMeddley_HasMutation("Burrowing Claws"))
                {
                    MutationMeddley_SetShift("DV", wetGround ? 1 : 0);
                }

                if (MutationMeddley_HasMutation("Ash Metabolism") && lit)
                {
                    MutationMeddley_SetShift("HeatResistance", 5);
                }
            }

            if (MutationMeddley_HasMutation("Living Crystal"))
            {
                if (MutationMeddley_HasEvolution("fortress")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice")
                    && stationary)
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("hunter_shell")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("adaptive_carapace")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix")
                    && lit)
                {
                    MutationMeddley_SetShift("HeatResistance", 5);
                    MutationMeddley_SetShift("ColdResistance", 5);
                }
            }

            if (MutationMeddley_HasMutation("Brineborn"))
            {
                if (MutationMeddley_HasEvolution("fortress")
                    && MutationMeddley_MutationHasEvolution("Brineborn", "saltglass_bloom")
                    && saline)
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("hunter_shell")
                    && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary")
                    && wetGround)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("adaptive_carapace")
                    && MutationMeddley_MutationHasEvolution("Brineborn", "wellspring_flesh")
                    && saline)
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_HasMutation("Walking Colony"))
            {
                if (MutationMeddley_HasEvolution("fortress")
                    && MutationMeddley_MutationHasEvolution("Walking Colony", "marrow_hive")
                    && stationary)
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("hunter_shell")
                    && MutationMeddley_MutationHasEvolution("Walking Colony", "surveyor_swarm"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("adaptive_carapace")
                    && MutationMeddley_MutationHasEvolution("Walking Colony", "graft_parliament"))
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_IsTriadActive("cathedral_organism") && stationary && saline)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("breakwater_predator")
                && wetGround
                && impact > 0)
            {
                MutationMeddley_SetShift("Quickness", 2);
                MutationMeddley_SetShift("Strength", 1);
            }

            if (MutationMeddley_IsTriadActive("prism_estuary") && lit && saline)
            {
                MutationMeddley_SetShift("HeatResistance", 10);
                MutationMeddley_SetShift("ColdResistance", 10);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("glass_kiln_bastion") && stationary)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("HeatResistance", 5);
            }

            if (MutationMeddley_IsTriadActive("ember_pursuit_engine") && impact > 0)
            {
                MutationMeddley_SetShift("Quickness", 2);
                MutationMeddley_SetShift("Strength", 1);
            }

            if (MutationMeddley_IsTriadActive("mirage_exuvium") && lit && wetGround)
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("ossuary_rampart") && stationary)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("drift_parliament") && wetGround && impact > 0)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_IsTriadActive("salt_eclipse") && !lit && saline)
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("bone_kiln_parliament") && MutationMeddley_IsCurrentCellHot())
            {
                MutationMeddley_SetShift("AV", 2);
            }

            if (MutationMeddley_IsTriadActive("chorus_husk"))
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("whitewater_ossuary") && wetGround)
            {
                MutationMeddley_SetShift("AV", 2);
            }

            if (MutationMeddley_HasEvolution("porcupine_redoubt") && stationary)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasEvolution("estuary_husk") && wetGround)
            {
                MutationMeddley_SetShift("DV", 2);
                MutationMeddley_SetShift("ColdResistance", 10);
            }

            if (MutationMeddley_HasEvolution("skitter_bulwark") && impact > 0)
            {
                MutationMeddley_SetShift("Quickness", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasEvolution("hookstorm_frame") && engaged)
            {
                MutationMeddley_SetShift("AV", 1);
                MutationMeddley_SetShift("DV", 1);
                MutationMeddley_SetShift("Strength", 1);
            }
        }

        private bool MutationMeddley_HasVanillaCarapace()
        {
            return MutationMeddley_HasMutation("Carapace");
        }

        private void MutationMeddley_ProcessShellTurn()
        {
            if (!MutationMeddley_IsFunctionallyActive() || ParentObject == null)
            {
                return;
            }

            bool engaged = ParentObject.IsEngagedInMelee();
            bool stationary = MutationMeddley_GetStateInt(MutationMeddley_StationaryKey, 0) > 0;
            bool moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0;
            bool wetGround = MutationMeddley_IsCurrentCellWet();
            bool saline = MutationMeddley_IsCurrentCellSaline();
            bool hot = MutationMeddley_IsCurrentCellHot();
            bool lit = MutationMeddley_IsCurrentCellLit();
            int brace = MutationMeddley_GetStateInt(MutationMeddley_BraceKey, 0);
            int impact = MutationMeddley_GetStateInt(MutationMeddley_ImpactKey, 0);
            int heat = MutationMeddley_GetStateInt(MutationMeddley_HeatAttuneKey, 0);
            int mire = MutationMeddley_GetStateInt(MutationMeddley_MireAttuneKey, 0);
            int rime = MutationMeddley_GetStateInt(MutationMeddley_RimeAttuneKey, 0);

            if (MutationMeddley_HasEvolution("fortress"))
            {
                if (stationary)
                {
                    brace += MutationMeddley_GetCurrentModeId() == "anchor_down" ? 2 : 1;
                }
                else
                {
                    brace = Math.Max(0, brace - 1);
                }

                if (MutationMeddley_HasEvolution("entrenched_bastion") && stationary)
                {
                    brace += 1;
                }

                if (MutationMeddley_HasMutation("Quills") && stationary)
                {
                    brace += 1;
                }

                if (MutationMeddley_GetCurrentModeId() == "spiteful_wall" && engaged && brace > 0)
                {
                    brace = Math.Max(0, brace - 1);
                }

                if (MutationMeddley_HasEvolution("living_fortress") && stationary && engaged)
                {
                    MutationMeddley_TryHeal(1);
                }

                if (MutationMeddley_HasEvolution("redoubt_engine") && stationary && brace >= 3)
                {
                    MutationMeddley_TryHeal(1);
                }

                MutationMeddley_SetStateInt(MutationMeddley_BraceKey, Math.Min(brace, MutationMeddley_HasEvolution("living_fortress") ? 5 : 4));
                return;
            }

            if (MutationMeddley_HasEvolution("hunter_shell"))
            {
                if (moved && engaged)
                {
                    impact += MutationMeddley_GetCurrentModeId() == "ramming_gait" ? 2 : 1;
                }
                else if (moved)
                {
                    impact += MutationMeddley_GetCurrentModeId() == "skirmish_gait" ? 1 : 0;
                }
                else
                {
                    impact = Math.Max(0, impact - 1);
                }

                if (MutationMeddley_HasEvolution("ravager_joints") && moved)
                {
                    impact += 1;
                }

                if (MutationMeddley_HasMutation("Multiple Legs") && moved)
                {
                    impact += 1;
                }

                if (MutationMeddley_HasEvolution("pursuit_predator") && moved && engaged)
                {
                    MutationMeddley_TryHeal(1);
                }

                if (MutationMeddley_HasEvolution("hooked_pursuer") && engaged && impact >= 3)
                {
                    MutationMeddley_TryHeal(1);
                }

                MutationMeddley_SetStateInt(MutationMeddley_ImpactKey, Math.Min(impact, MutationMeddley_HasEvolution("pursuit_predator") ? 6 : 4));
                return;
            }

            if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                if (hot || (lit && MutationMeddley_GetCurrentModeId() == "ember_veil"))
                {
                    heat += 1;
                }
                else
                {
                    heat = Math.Max(0, heat - 1);
                }

                if (wetGround || saline)
                {
                    mire += 1;
                }
                else
                {
                    mire = Math.Max(0, mire - 1);
                }

                if (!lit || MutationMeddley_GetCurrentModeId() == "rime_veil")
                {
                    rime += 1;
                }
                else
                {
                    rime = Math.Max(0, rime - 1);
                }

                if (MutationMeddley_HasEvolution("bog_shell") && wetGround && stationary)
                {
                    MutationMeddley_TryHeal(1);
                }

                if (MutationMeddley_HasEvolution("estuary_husk") && saline)
                {
                    MutationMeddley_TryHeal(1);
                }

                MutationMeddley_SetStateInt(MutationMeddley_HeatAttuneKey, Math.Min(heat, MutationMeddley_HasEvolution("storm_carapace") ? 6 : 4));
                MutationMeddley_SetStateInt(MutationMeddley_MireAttuneKey, Math.Min(mire, MutationMeddley_HasEvolution("bog_shell") ? 6 : 4));
                MutationMeddley_SetStateInt(MutationMeddley_RimeAttuneKey, Math.Min(rime, MutationMeddley_HasEvolution("storm_carapace") ? 6 : 4));
            }
        }

        private void MutationMeddley_HandleShellPressure(Event E, bool environmental)
        {
            if (!MutationMeddley_IsFunctionallyActive() || ParentObject == null)
            {
                return;
            }

            if (MutationMeddley_IsBonusDamageDispatchActive())
            {
                MutationMeddley_TraceDamageProc("carapace.pressure", "suppressed recursive shell pressure handling");
                return;
            }

            bool engaged = ParentObject.IsEngagedInMelee();
            bool stationary = MutationMeddley_GetStateInt(MutationMeddley_StationaryKey, 0) > 0;
            bool wetGround = MutationMeddley_IsCurrentCellWet();
            bool saline = MutationMeddley_IsCurrentCellSaline();
            bool hot = MutationMeddley_IsCurrentCellHot();
            int brace = MutationMeddley_GetStateInt(MutationMeddley_BraceKey, 0);
            int heat = MutationMeddley_GetStateInt(MutationMeddley_HeatAttuneKey, 0);
            int mire = MutationMeddley_GetStateInt(MutationMeddley_MireAttuneKey, 0);
            int rime = MutationMeddley_GetStateInt(MutationMeddley_RimeAttuneKey, 0);
            GameObject source = MutationMeddley_GetIncomingDamageSource(E);

            if (MutationMeddley_HasEvolution("fortress") && brace > 0)
            {
                int spent = MutationMeddley_ConsumeStateInt(
                    MutationMeddley_BraceKey,
                    MutationMeddley_HasEvolution("entrenched_bastion") && stationary ? 1 : 2
                );
                if (spent > 0)
                {
                    int heal = 1 + (MutationMeddley_HasEvolution("living_fortress") ? 1 : 0);
                    bool spiteful = MutationMeddley_GetCurrentModeId() == "spiteful_wall";
                    bool struckBack = false;
                    if (spiteful && engaged)
                    {
                        MutationMeddley_BonusDamageResult result = MutationMeddley_TryBonusDamage(
                            source,
                            spent + (MutationMeddley_HasEvolution("faceted_keep") ? 1 : 0),
                            "spiteful wall",
                            "carapace.spiteful_wall");
                        struckBack = result.DamageDispatched;
                        if (!struckBack)
                        {
                            MutationMeddley_SetStateInt(
                                MutationMeddley_BraceKey,
                                Math.Min(MutationMeddley_GetStateInt(MutationMeddley_BraceKey, 0) + 1, 5)
                            );
                        }
                    }

                    if (MutationMeddley_HasEvolution("entrenched_bastion") && stationary)
                    {
                        MutationMeddley_SetStateInt(
                            MutationMeddley_BraceKey,
                            Math.Min(MutationMeddley_GetStateInt(MutationMeddley_BraceKey, 0) + 1, 5)
                        );
                    }

                    MutationMeddley_TryHeal(heal);
                    if (spiteful && struckBack)
                    {
                        MutationMeddley_AddPlayerMessage("Your shell answers the blow with a spiteful crack.");
                    }
                    else if (spiteful)
                    {
                        MutationMeddley_AddPlayerMessage("Your shell keeps a spiteful brace ready for the next exchange.");
                    }
                    else if (MutationMeddley_HasEvolution("entrenched_bastion") && stationary)
                    {
                        MutationMeddley_AddPlayerMessage("Your shell spends brace and holds the line.");
                    }
                    else
                    {
                        MutationMeddley_AddPlayerMessage("Your shell spends brace to absorb the pressure.");
                    }
                }

                return;
            }

            if (MutationMeddley_HasEvolution("adaptive_carapace") && environmental)
            {
                if (hot && heat > 0)
                {
                    MutationMeddley_ConsumeStateInt(MutationMeddley_HeatAttuneKey, 1);
                    MutationMeddley_TryHeal(1);
                    MutationMeddley_AddPlayerMessage("Stored heat vents across your shell.");
                }
                else if ((wetGround || saline) && mire > 0)
                {
                    MutationMeddley_ConsumeStateInt(MutationMeddley_MireAttuneKey, 1);
                    MutationMeddley_TryHeal(1);
                    MutationMeddley_AddPlayerMessage("Your mire sheath turns the ground's pressure aside.");
                }
                else if (rime > 0)
                {
                    MutationMeddley_ConsumeStateInt(MutationMeddley_RimeAttuneKey, 1);
                    MutationMeddley_TryHeal(1);
                    MutationMeddley_AddPlayerMessage("Rime flakes from your shell and deadens the shock.");
                }
            }
        }

        private void MutationMeddley_HandleShellStrike(Event E)
        {
            if (!MutationMeddley_IsFunctionallyActive() || ParentObject == null)
            {
                return;
            }

            if (MutationMeddley_IsBonusDamageDispatchActive())
            {
                MutationMeddley_TraceDamageProc("carapace.strike", "suppressed recursive shell strike handling");
                return;
            }

            bool moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0;
            bool wetGround = MutationMeddley_IsCurrentCellWet();
            int impact = MutationMeddley_GetStateInt(MutationMeddley_ImpactKey, 0);
            int heat = MutationMeddley_GetStateInt(MutationMeddley_HeatAttuneKey, 0);
            int mire = MutationMeddley_GetStateInt(MutationMeddley_MireAttuneKey, 0);
            int rime = MutationMeddley_GetStateInt(MutationMeddley_RimeAttuneKey, 0);
            GameObject defender = MutationMeddley_GetOutgoingDamageTarget(E);

            if (MutationMeddley_HasEvolution("hunter_shell") && impact > 0)
            {
                bool ramming = MutationMeddley_GetCurrentModeId() == "ramming_gait";
                int spent = MutationMeddley_ConsumeStateInt(MutationMeddley_ImpactKey, ramming ? 2 : 1);
                if (spent > 0)
                {
                    MutationMeddley_TryHeal(1);
                    if (!ramming)
                    {
                        int refund = MutationMeddley_HasEvolution("ravager_joints") ? 1 : 0;
                        if (MutationMeddley_HasEvolution("hooked_pursuer") && moved)
                        {
                            refund += 1;
                        }

                        if (refund > 0)
                        {
                            MutationMeddley_SetStateInt(
                                MutationMeddley_ImpactKey,
                                Math.Min(MutationMeddley_GetStateInt(MutationMeddley_ImpactKey, 0) + refund, 6)
                            );
                        }

                        MutationMeddley_AddPlayerMessage("Your shell slips through and spends stored impact.");
                    }
                    else
                    {
                        int shellDamage = spent + 1;
                        if (MutationMeddley_HasEvolution("spur_lattice"))
                        {
                            shellDamage += 1;
                        }
                        if (MutationMeddley_HasEvolution("hooked_pursuer") && moved)
                        {
                            shellDamage += 1;
                        }

                        MutationMeddley_BonusDamageResult result = MutationMeddley_TryBonusDamage(
                            defender,
                            shellDamage,
                            "ramming shell",
                            "carapace.ramming_shell");
                        bool struck = result.DamageDispatched;
                        if (struck)
                        {
                            MutationMeddley_AddPlayerMessage("Your shell-slam cashes out stored impact into the prey.");
                        }
                        else
                        {
                            MutationMeddley_TryHeal(1);
                            MutationMeddley_AddPlayerMessage("Your shell-slam cashes out stored impact and keeps you driving forward.");
                        }
                    }
                }

                return;
            }

            if (MutationMeddley_HasEvolution("adaptive_carapace"))
            {
                if (MutationMeddley_GetCurrentModeId() == "ember_veil" && heat > 0)
                {
                    MutationMeddley_ConsumeStateInt(MutationMeddley_HeatAttuneKey, 1);
                    MutationMeddley_TryHeal(1);
                    if (MutationMeddley_IsCurrentCellHot())
                    {
                        MutationMeddley_SetStateInt(MutationMeddley_HeatAttuneKey, Math.Min(MutationMeddley_GetStateInt(MutationMeddley_HeatAttuneKey, 0) + 1, 4));
                    }
                    MutationMeddley_AddPlayerMessage("Your shell vents stored heat on contact.");
                }
                else if (MutationMeddley_GetCurrentModeId() == "rime_veil" && rime > 0)
                {
                    MutationMeddley_ConsumeStateInt(MutationMeddley_RimeAttuneKey, 1);
                    MutationMeddley_TryHeal(1);
                    if (!moved)
                    {
                        MutationMeddley_SetStateInt(MutationMeddley_RimeAttuneKey, Math.Min(MutationMeddley_GetStateInt(MutationMeddley_RimeAttuneKey, 0) + 1, 4));
                    }
                    MutationMeddley_AddPlayerMessage("Your shell sheds rime into a guarding crust.");
                }
                else if (wetGround && mire > 0)
                {
                    MutationMeddley_ConsumeStateInt(MutationMeddley_MireAttuneKey, 1);
                    MutationMeddley_TryHeal(1);
                    if (MutationMeddley_IsCurrentCellSaline())
                    {
                        MutationMeddley_SetStateInt(MutationMeddley_MireAttuneKey, Math.Min(MutationMeddley_GetStateInt(MutationMeddley_MireAttuneKey, 0) + 1, 4));
                    }
                    MutationMeddley_AddPlayerMessage("Your membrane shell drinks momentum from the mire.");
                }
            }
        }

        private void MutationMeddley_TrackPorcupineDiscovery()
        {
            if (MutationMeddley_GetStateInt(MutationMeddley_PorcupineUnlockedKey, 0) > 0)
            {
                return;
            }

            if (MutationMeddley_HasSelectionAtTier(3))
            {
                return;
            }

            if (!MutationMeddley_HasMutation("Quills")
                || !MutationMeddley_IsFunctionallyActive()
                || !MutationMeddley_HasEvolution("faceted_keep")
                || MutationMeddley_GetStateInt(MutationMeddley_StationaryKey, 0) == 0)
            {
                return;
            }

            int progress = MutationMeddley_GetStateInt(MutationMeddley_PorcupineProgressKey, 0) + 1;
            MutationMeddley_SetStateInt(MutationMeddley_PorcupineProgressKey, progress);
            if (progress >= 5)
            {
                MutationMeddley_SetStateInt(MutationMeddley_PorcupineUnlockedKey, 1);
            }
        }

        private void MutationMeddley_TrackEstuaryHuskDiscovery()
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_EstuaryUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("mire_sheath")
                || !MutationMeddley_IsFunctionallyActive()
                || (!MutationMeddley_HasMutation("Amphibious") && !MutationMeddley_IsCurrentCellSaline()))
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_EstuaryProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_EstuaryUnlockedKey);
            }
        }

        private void MutationMeddley_TrackSkitterBulwarkDiscovery()
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_SkitterUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_IsFunctionallyActive()
                || !MutationMeddley_HasEvolution("ravager_joints")
                || !MutationMeddley_HasMutation("Multiple Legs")
                || MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) == 0)
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_SkitterProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_SkitterUnlockedKey);
            }
        }

        private void MutationMeddley_TrackHookstormFrameDiscovery()
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_HookstormUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_IsFunctionallyActive()
                || !MutationMeddley_HasEvolution("spur_lattice")
                || !MutationMeddley_HasMutation("Quills")
                || ParentObject == null
                || !ParentObject.IsEngagedInMelee())
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_HookstormProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_HookstormUnlockedKey);
            }
        }

        private bool MutationMeddley_IsTriadActive(string id)
        {
            return MutationMeddley_IsSynergyActive(new MutationMeddley_SynergyDefinition(id, "", ""));
        }
    }
}
