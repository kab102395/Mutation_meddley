using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_AshMetabolism : MutationMeddley_AdaptiveMutationBase
    {
        private const string MutationMeddley_EmbersKey = "ash_embers";
        private const string MutationMeddley_MovedKey = "ash_moved";
        private const string MutationMeddley_HotKey = "ash_hot";
        private const string MutationMeddley_SmokeKey = "ash_smoke";
        private const string MutationMeddley_VolcanicUnlockedKey = "ash_hidden_volcanic";
        private const string MutationMeddley_VolcanicProgressKey = "ash_hidden_volcanic_progress";
        private const string MutationMeddley_WakeEaterUnlockedKey = "ash_hidden_wake";
        private const string MutationMeddley_WakeEaterProgressKey = "ash_hidden_wake_progress";
        private const int MutationMeddley_MaxEmbers = 6;

        public override string MutationMeddley_EvolutionDisplayName
        {
            get { return "Ash Metabolism"; }
        }

        protected override string MutationMeddley_ModeAbilityName
        {
            get { return "Retune Ash Metabolism"; }
        }

        protected override string MutationMeddley_ModeAbilityDescription
        {
            get { return "Shift your ash ecology between banking heat, spending it, and riding smoke."; }
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
                MutationMeddley_ProcessAshTurn();
            }

            return base.FireEvent(E);
        }

        public override string GetDescription()
        {
            return "Your metabolism has learned to live on heat, ash, and atmospheres that should choke ordinary flesh.\n\n"
                + "Ash Metabolism is an environmental keystone about heat routing, smoke-reading, aggressive ash spend, and climate-dependent identities.";
        }

        public override string GetLevelText(int Level)
        {
            return "Rank 3: choose how your heat ecology expresses itself.\n"
                + "Rank 6: specialize the furnace, maw, or organ.\n"
                + "Rank 9: secure the ash capstone.\n\n"
                + MutationMeddley_GetEvolutionSummary()
                + "\n"
                + MutationMeddley_DescribeModeState()
                + "\n"
                + "Embers: "
                + MutationMeddley_GetEmbers()
                + "/"
                + MutationMeddley_GetMaxEmbers()
                + (MutationMeddley_IsHotEnvironment() ? " (hot ground)" : " (temperate ground)")
                + (MutationMeddley_IsSmokyEnvironment() ? ", smoke present" : ", clear air")
                + "\n"
                + MutationMeddley_GetSynergySummary();
        }

        protected override List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices()
        {
            return new List<MutationMeddley_EvolutionChoice>
            {
                new MutationMeddley_EvolutionChoice(
                    "furnace_skin",
                    "Furnace Skin",
                    "Bank punishing heat inside defensive tissue and shell-like ash layers.",
                    3,
                    1,
                    detailText: "Defensive heat identity. Wants exposure, contact, and controlled pressure."
                ),
                new MutationMeddley_EvolutionChoice(
                    "cinder_gut",
                    "Cinder Gut",
                    "Turn combustion and aftermath into a predatory metabolic engine.",
                    3,
                    1,
                    detailText: "Predatory heat identity. Wants momentum, movement, and aggressive ember spending."
                ),
                new MutationMeddley_EvolutionChoice(
                    "smoke_organ",
                    "Smoke Organ",
                    "Treat ash and atmosphere as tactical cover, pressure, and route control.",
                    3,
                    1,
                    detailText: "Atmosphere identity. Wants smoke, line-of-sight ambiguity, and positional play."
                ),
                new MutationMeddley_EvolutionChoice(
                    "kiln_plating",
                    "Kiln Plating",
                    "Your heat banks into hard plated layers that answer pressure with patient force.",
                    6,
                    2,
                    "furnace_skin",
                    "Best when you stay in hot spaces and let the shell keep the score."
                ),
                new MutationMeddley_EvolutionChoice(
                    "radiant_soot",
                    "Radiant Soot",
                    "Your ash bloom catches light and turns it into a moving defensive corona.",
                    6,
                    2,
                    "furnace_skin",
                    "Best when the air is bright or visibly burning."
                ),
                new MutationMeddley_EvolutionChoice(
                    "coal_maw",
                    "Coal Maw",
                    "Your core burns hotter the more you keep moving through hostile ground.",
                    6,
                    2,
                    "cinder_gut",
                    "Best when you keep a hot pursuit loop running."
                ),
                new MutationMeddley_EvolutionChoice(
                    "pyre_circulation",
                    "Pyre Circulation",
                    "Heat runs the body like a second bloodstream and pushes you toward overdrive.",
                    6,
                    2,
                    "cinder_gut",
                    "Best when you convert embers directly into action tempo."
                ),
                new MutationMeddley_EvolutionChoice(
                    "ash_veil",
                    "Ash Veil",
                    "Your smoke thickens into concealment and mobile defensive blur.",
                    6,
                    2,
                    "smoke_organ",
                    "Best in smoke, ash, or dim moving lines."
                ),
                new MutationMeddley_EvolutionChoice(
                    "chimney_lungs",
                    "Chimney Lungs",
                    "You turn combustion air into a pressure draft that carries movement and tempo.",
                    6,
                    2,
                    "smoke_organ",
                    "Best when you keep changing cells and reading atmosphere."
                ),
                new MutationMeddley_EvolutionChoice(
                    "glasshouse_carapace",
                    "Glasshouse Carapace",
                    "Stored heat hardens into a brittle but imposing ash-glass fortress.",
                    9,
                    3,
                    "kiln_plating",
                    "Capstone rooted heat-bank line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "volcanic_memory",
                    "Volcanic Memory",
                    "Repeated thermal punishment teaches your body to remember structure as eruption.",
                    9,
                    3,
                    "kiln_plating",
                    "UNUSUAL ADAPTATION. Requires repeated high-heat exposure while carrying a structural profile.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "ember_halo",
                    "Ember Halo",
                    "Lit air blooms into a harsh, mobile ash corona.",
                    9,
                    3,
                    "radiant_soot",
                    "Capstone bright ash-defense line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "wakefeast",
                    "Wakefeast",
                    "Your passing heat leaves you stronger and more dangerous to pursue.",
                    9,
                    3,
                    "coal_maw",
                    "Capstone pursuit combustion line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "wake_eater",
                    "Wake Eater",
                    "Your body begins hunting the wake of heat itself rather than waiting for aftermath.",
                    9,
                    3,
                    "coal_maw",
                    "UNUSUAL ADAPTATION. Requires repeated hot-ground pursuit while feeding the maw.",
                    true
                ),
                new MutationMeddley_EvolutionChoice(
                    "overdraft_heart",
                    "Overdraft Heart",
                    "Heat debt turns into a dangerous rhythm of acceleration and spend.",
                    9,
                    3,
                    "pyre_circulation",
                    "Capstone overdrive line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "crematory_mirage",
                    "Crematory Mirage",
                    "Smoke and glare fold around you as a moving false body.",
                    9,
                    3,
                    "ash_veil",
                    "Capstone concealment line."
                ),
                new MutationMeddley_EvolutionChoice(
                    "blackdraft_engine",
                    "Blackdraft Engine",
                    "Your lungs become a draft machine that turns atmospheric turmoil into motion.",
                    9,
                    3,
                    "chimney_lungs",
                    "Capstone motion-pressure line."
                )
            };
        }

        protected override IEnumerable<string> MutationMeddley_GetIntrinsicSemanticTags()
        {
            return new string[] { "KEYSTONE", "ENVIRONMENTAL", "THERMAL", "GAS_INTERACTION", "METABOLIC", "BIOLOGICAL" };
        }

        protected override IEnumerable<string> MutationMeddley_GetEvolutionSemanticTags()
        {
            List<string> tags = new List<string>();

            if (MutationMeddley_HasEvolution("furnace_skin"))
            {
                tags.Add("STRUCTURAL");
                tags.Add("CONTROL");
            }

            if (MutationMeddley_HasEvolution("cinder_gut"))
            {
                tags.Add("PREDATORY");
                tags.Add("PURSUIT");
                tags.Add("MOBILE");
            }

            if (MutationMeddley_HasEvolution("smoke_organ"))
            {
                tags.Add("STEALTH");
                tags.Add("AREA_DENIAL");
                tags.Add("GAS_INTERACTION");
            }

            return tags;
        }

        protected override List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices()
        {
            if (MutationMeddley_HasEvolution("furnace_skin"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("bank_heat", "Bank Heat", "Hold embers for heavier structure and resistance."),
                    new MutationMeddley_ModeChoice("flare_heat", "Flare Heat", "Spend embers more readily for active pressure.")
                };
            }

            if (MutationMeddley_HasEvolution("cinder_gut"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("feast_ash", "Feast Ash", "Consume embers aggressively to maintain momentum."),
                    new MutationMeddley_ModeChoice("stoke_ash", "Stoke Ash", "Hold heat for steadier pursuit and longer runs.")
                };
            }

            if (MutationMeddley_HasEvolution("smoke_organ"))
            {
                return new List<MutationMeddley_ModeChoice>
                {
                    new MutationMeddley_ModeChoice("veil_smoke", "Veil Smoke", "Favor concealment and smoke-fed defense."),
                    new MutationMeddley_ModeChoice("draft_smoke", "Draft Smoke", "Favor movement and atmospheric throughput.")
                };
            }

            return new List<MutationMeddley_ModeChoice>();
        }

        protected override List<MutationMeddley_SynergyDefinition> MutationMeddley_GetSynergyDefinitions()
        {
            return new List<MutationMeddley_SynergyDefinition>
            {
                new MutationMeddley_SynergyDefinition("flaming_ray", "Flaming Ray", "Combustive output feeds your ember loop more cleanly."),
                new MutationMeddley_SynergyDefinition("freezing_ray", "Freezing Ray", "Thermal violence sharpens how you spend stored heat."),
                new MutationMeddley_SynergyDefinition("photosynthetic_skin", "Photosynthetic Skin", "Lit ash and living tissue reinforce each other."),
                new MutationMeddley_SynergyDefinition("phasing", "Phasing", "Smoke and phase-state combine into harder-to-read positioning."),
                new MutationMeddley_SynergyDefinition("living_crystal_pair", "Living Crystal", "Crystalline bodies reinterpret heat as stress, glare, or cadence."),
                new MutationMeddley_SynergyDefinition("brineborn_pair", "Brineborn", "Steam, salt, and stored heat start sharing one ecology."),
                new MutationMeddley_SynergyDefinition("carapace_pair", "Carapace Evolution", "A live shell gives your heat banks somewhere structural to go."),
                new MutationMeddley_SynergyDefinition("glass_kiln_bastion", "Glass Kiln Bastion", "Shell, crystal, and kiln heat harden into one punishing redoubt."),
                new MutationMeddley_SynergyDefinition("ember_pursuit_engine", "Ember Pursuit Engine", "Heat-fed pursuit compounds cadence and shell aggression."),
                new MutationMeddley_SynergyDefinition("mirage_exuvium", "Mirage Exuvium", "Light, shell weathering, and smoke begin behaving as one moving mirage."),
                new MutationMeddley_SynergyDefinition("salt_kiln_reliquary", "Salt Kiln Reliquary", "Thermal mineralization turns saline defense into a kiln-kept bastion."),
                new MutationMeddley_SynergyDefinition("steam_choir", "Steam Choir", "Wet pursuit, smoke, and resonance produce an unstable pressure chorus."),
                new MutationMeddley_SynergyDefinition("volcanic_memory_state", "Volcanic Memory", "Stored heat now answers structural stress like a remembered eruption.", isUnusual: true),
                new MutationMeddley_SynergyDefinition("wake_eater_state", "Wake Eater", "Your body begins feeding on the wake of heat and motion itself.", isUnusual: true)
            };
        }

        protected override bool MutationMeddley_IsSynergyActive(MutationMeddley_SynergyDefinition synergy)
        {
            switch (synergy.Id)
            {
                case "flaming_ray":
                    return MutationMeddley_HasMutation("Flaming Ray") && MutationMeddley_HasAnyEvolution();
                case "freezing_ray":
                    return MutationMeddley_HasMutation("Freezing Ray")
                        && (MutationMeddley_HasEvolution("furnace_skin") || MutationMeddley_HasEvolution("cinder_gut"));
                case "photosynthetic_skin":
                    return MutationMeddley_HasMutation("Photosynthetic Skin")
                        && (MutationMeddley_HasEvolution("furnace_skin") || MutationMeddley_HasEvolution("smoke_organ"));
                case "phasing":
                    return MutationMeddley_HasMutation("Phasing") && MutationMeddley_HasEvolution("smoke_organ");
                case "living_crystal_pair":
                    return MutationMeddley_HasMutation("Living Crystal") && MutationMeddley_HasAnyEvolution();
                case "brineborn_pair":
                    return MutationMeddley_HasMutation("Brineborn") && MutationMeddley_HasAnyEvolution();
                case "carapace_pair":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution") && MutationMeddley_HasAnyEvolution();
                case "glass_kiln_bastion":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("furnace_skin")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress");
                case "ember_pursuit_engine":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("cinder_gut")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell");
                case "mirage_exuvium":
                    return MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution")
                        && MutationMeddley_HasEvolution("smoke_organ")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix")
                        && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace");
                case "salt_kiln_reliquary":
                    return MutationMeddley_HasEvolution("furnace_skin")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "saltglass_bloom")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice");
                case "steam_choir":
                    return MutationMeddley_HasEvolution("smoke_organ")
                        && MutationMeddley_MutationHasEvolution("Brineborn", "scouring_estuary")
                        && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal");
                case "volcanic_memory_state":
                    return MutationMeddley_HasEvolution("volcanic_memory");
                case "wake_eater_state":
                    return MutationMeddley_HasEvolution("wake_eater");
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

            if (choice.Id == "volcanic_memory")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_VolcanicUnlockedKey);
            }

            if (choice.Id == "wake_eater")
            {
                return MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_WakeEaterUnlockedKey);
            }

            return false;
        }

        protected override void MutationMeddley_RefreshPassiveEffects()
        {
            MutationMeddley_ClearCommonStatShifts();

            int embers = MutationMeddley_GetEmbers();
            bool hot = MutationMeddley_IsHotEnvironment();
            bool smoky = MutationMeddley_IsSmokyEnvironment();
            bool lit = MutationMeddley_IsCurrentCellLit();
            bool moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0;

            if (MutationMeddley_HasEvolution("furnace_skin"))
            {
                MutationMeddley_SetShift("HeatResistance", 10 + (embers * 3));
                MutationMeddley_SetShift("AV", 1 + (embers / 3));

                if (MutationMeddley_HasEvolution("kiln_plating"))
                {
                    MutationMeddley_SetShift("AV", hot ? 2 + (embers / 2) : 1 + (embers / 4));
                    MutationMeddley_SetShift("DV", MutationMeddley_GetCurrentModeId() == "flare_heat" ? 1 : 0);
                }
                else if (MutationMeddley_HasEvolution("radiant_soot"))
                {
                    MutationMeddley_SetShift("DV", lit ? 2 + (embers / 3) : 1);
                    MutationMeddley_SetShift("HeatResistance", lit ? 10 : 0);
                }
                else
                {
                    MutationMeddley_SetShift("DV", embers / 4);
                }

                if (MutationMeddley_HasEvolution("glasshouse_carapace") && hot)
                {
                    MutationMeddley_SetShift("AV", 2);
                }

                if (MutationMeddley_HasEvolution("ember_halo") && lit)
                {
                    MutationMeddley_SetShift("DV", 2);
                }
            }
            else if (MutationMeddley_HasEvolution("cinder_gut"))
            {
                MutationMeddley_SetShift("Quickness", 1 + (embers / 3));

                if (MutationMeddley_HasEvolution("coal_maw"))
                {
                    MutationMeddley_SetShift("Quickness", (moved ? 2 : 1) + (embers / 2));
                    MutationMeddley_SetShift("DV", hot ? 1 + (embers / 4) : embers / 5);
                }
                else if (MutationMeddley_HasEvolution("pyre_circulation"))
                {
                    MutationMeddley_SetShift("Quickness", 1 + (embers / 2));
                    MutationMeddley_SetShift("HeatResistance", 5 + (embers * 2));
                }

                if (MutationMeddley_HasEvolution("wakefeast") && moved)
                {
                    MutationMeddley_SetShift("DV", 1);
                }

                if (MutationMeddley_HasEvolution("overdraft_heart") && hot)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }
            else if (MutationMeddley_HasEvolution("smoke_organ"))
            {
                MutationMeddley_SetShift("DV", smoky ? 2 + (embers / 3) : 1);

                if (MutationMeddley_HasEvolution("ash_veil"))
                {
                    MutationMeddley_SetShift("DV", smoky ? 3 + (embers / 3) : 1);
                    MutationMeddley_SetShift("Quickness", smoky && MutationMeddley_GetCurrentModeId() == "veil_smoke" ? 1 : 0);
                }
                else if (MutationMeddley_HasEvolution("chimney_lungs"))
                {
                    MutationMeddley_SetShift("Quickness", moved ? 2 + (embers / 3) : embers / 4);
                    MutationMeddley_SetShift("DV", smoky ? 1 : 0);
                }
                else
                {
                    MutationMeddley_SetShift("Quickness", smoky ? 1 : 0);
                }

                if (MutationMeddley_HasEvolution("crematory_mirage") && smoky)
                {
                    MutationMeddley_SetShift("DV", 2);
                }

                if (MutationMeddley_HasEvolution("blackdraft_engine") && moved)
                {
                    MutationMeddley_SetShift("Quickness", 2);
                }
            }

            if (MutationMeddley_HasMutation("Flaming Ray"))
            {
                MutationMeddley_SetShift("HeatResistance", 5);
                if (hot)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }

            if (MutationMeddley_HasMutation("Freezing Ray") && MutationMeddley_HasEvolution("furnace_skin"))
            {
                MutationMeddley_SetShift("ColdResistance", 10);
            }

            if (MutationMeddley_HasMutation("Photosynthetic Skin") && lit)
            {
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasMutation("Phasing") && MutationMeddley_HasEvolution("smoke_organ"))
            {
                MutationMeddley_SetShift("DV", 1);
                if (smoky)
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }

            if (MutationMeddley_HasMutation("Living Crystal"))
            {
                if (MutationMeddley_HasEvolution("furnace_skin")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "diamond_lattice")
                    && hot)
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("cinder_gut")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("smoke_organ")
                    && MutationMeddley_MutationHasEvolution("Living Crystal", "prismatic_matrix")
                    && lit)
                {
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_HasMutation("Brineborn"))
            {
                if (MutationMeddley_HasEvolution("furnace_skin") && MutationMeddley_IsCurrentCellSaline())
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("smoke_organ") && MutationMeddley_IsCurrentCellWet())
                {
                    MutationMeddley_SetShift("DV", 1);
                }
                else if (MutationMeddley_HasEvolution("cinder_gut") && MutationMeddley_IsCurrentCellWet())
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
            }

            if (MutationMeddley_MutationIsFunctionallyActive("Carapace Evolution"))
            {
                if (MutationMeddley_HasEvolution("furnace_skin")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "fortress"))
                {
                    MutationMeddley_SetShift("AV", 1);
                }
                else if (MutationMeddley_HasEvolution("cinder_gut")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "hunter_shell"))
                {
                    MutationMeddley_SetShift("Quickness", 1);
                }
                else if (MutationMeddley_HasEvolution("smoke_organ")
                    && MutationMeddley_MutationHasEvolution("Carapace Evolution", "adaptive_carapace")
                    && lit)
                {
                    MutationMeddley_SetShift("HeatResistance", 5);
                    MutationMeddley_SetShift("DV", 1);
                }
            }

            if (MutationMeddley_IsTriadActive("glass_kiln_bastion") && hot)
            {
                MutationMeddley_SetShift("AV", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_IsTriadActive("ember_pursuit_engine") && moved)
            {
                MutationMeddley_SetShift("Quickness", 2);
            }

            if (MutationMeddley_IsTriadActive("mirage_exuvium") && lit && smoky)
            {
                MutationMeddley_SetShift("DV", 2);
            }

            if (MutationMeddley_IsTriadActive("salt_kiln_reliquary") && hot && MutationMeddley_IsCurrentCellSaline())
            {
                MutationMeddley_SetShift("AV", 2);
            }

            if (MutationMeddley_IsTriadActive("steam_choir") && MutationMeddley_IsCurrentCellWet() && smoky)
            {
                MutationMeddley_SetShift("Quickness", 2);
                MutationMeddley_SetShift("DV", 1);
            }

            if (MutationMeddley_HasEvolution("volcanic_memory"))
            {
                MutationMeddley_SetShift("AV", hot ? 2 : 1);
                MutationMeddley_SetShift("DV", hot ? 0 : -1);
            }

            if (MutationMeddley_HasEvolution("wake_eater"))
            {
                MutationMeddley_SetShift("Quickness", moved ? 2 : 1);
            }
        }

        private int MutationMeddley_GetEmbers()
        {
            return MutationMeddley_GetStateInt(MutationMeddley_EmbersKey, 0);
        }

        private int MutationMeddley_GetMaxEmbers()
        {
            int maxEmbers = MutationMeddley_MaxEmbers;
            if (MutationMeddley_HasMutation("Flaming Ray"))
            {
                maxEmbers += 1;
            }

            if (MutationMeddley_HasMutation("Photosynthetic Skin") && MutationMeddley_IsCurrentCellLit())
            {
                maxEmbers += 1;
            }

            return maxEmbers;
        }

        private void MutationMeddley_ProcessAshTurn()
        {
            bool hot = MutationMeddley_IsHotEnvironment();
            bool smoky = MutationMeddley_IsSmokyEnvironment();
            bool moved = MutationMeddley_GetStateInt(MutationMeddley_MovedKey, 0) > 0;
            int embers = MutationMeddley_GetEmbers();
            int maxEmbers = MutationMeddley_GetMaxEmbers();

            if (hot)
            {
                embers = Math.Min(maxEmbers, embers + 1 + (smoky ? 1 : 0));
            }
            else if (smoky)
            {
                embers = Math.Min(maxEmbers, embers + 1);
            }
            else
            {
                embers = Math.Max(0, embers - 1);
            }

            if (MutationMeddley_HasEvolution("kiln_plating")
                && MutationMeddley_GetCurrentModeId() == "bank_heat"
                && !hot)
            {
                embers = Math.Min(maxEmbers, embers + 1);
            }

            if (MutationMeddley_HasEvolution("radiant_soot")
                && MutationMeddley_GetCurrentModeId() == "flare_heat"
                && MutationMeddley_IsCurrentCellLit()
                && embers > 0)
            {
                embers -= 1;
            }

            if (MutationMeddley_HasEvolution("coal_maw")
                && MutationMeddley_GetCurrentModeId() == "feast_ash"
                && moved
                && embers > 0)
            {
                embers -= 1;
            }

            if (MutationMeddley_HasEvolution("pyre_circulation")
                && MutationMeddley_GetCurrentModeId() == "stoke_ash"
                && hot)
            {
                embers = Math.Min(maxEmbers, embers + 1);
            }

            if (MutationMeddley_HasEvolution("ash_veil")
                && MutationMeddley_GetCurrentModeId() == "veil_smoke"
                && smoky
                && embers > 0)
            {
                embers -= 1;
            }

            if (MutationMeddley_HasEvolution("chimney_lungs")
                && MutationMeddley_GetCurrentModeId() == "draft_smoke"
                && moved
                && smoky)
            {
                embers = Math.Min(maxEmbers, embers + 1);
            }

            MutationMeddley_TrackVolcanicMemoryDiscovery(hot);
            MutationMeddley_TrackWakeEaterDiscovery(hot, moved);

            MutationMeddley_SetStateInt(MutationMeddley_EmbersKey, Math.Max(0, Math.Min(embers, maxEmbers)));
            MutationMeddley_SetStateInt(MutationMeddley_MovedKey, 0);
            MutationMeddley_SetStateInt(MutationMeddley_HotKey, hot ? 1 : 0);
            MutationMeddley_SetStateInt(MutationMeddley_SmokeKey, smoky ? 1 : 0);
            MutationMeddley_RefreshPassiveEffects();
        }

        private void MutationMeddley_TrackVolcanicMemoryDiscovery(bool hot)
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_VolcanicUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("kiln_plating")
                || !hot
                || !MutationMeddley_HasOtherMutationWithTag("STRUCTURAL"))
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_VolcanicProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_VolcanicUnlockedKey);
            }
        }

        private void MutationMeddley_TrackWakeEaterDiscovery(bool hot, bool moved)
        {
            if (MutationMeddley_IsHiddenChoiceUnlocked(MutationMeddley_WakeEaterUnlockedKey)
                || !MutationMeddley_HasUnspentTier(3)
                || !MutationMeddley_HasEvolution("coal_maw")
                || MutationMeddley_GetCurrentModeId() != "feast_ash"
                || !hot
                || !moved)
            {
                return;
            }

            if (MutationMeddley_AdvanceHiddenProgress(MutationMeddley_WakeEaterProgressKey, 1, 5) >= 5)
            {
                MutationMeddley_UnlockHiddenChoice(MutationMeddley_WakeEaterUnlockedKey);
            }
        }

        private bool MutationMeddley_IsHotEnvironment()
        {
            if (ParentObject == null || ParentObject.CurrentCell == null)
            {
                return false;
            }

            string description = ParentObject.CurrentCell.ToString();
            if (string.IsNullOrEmpty(description))
            {
                return false;
            }

            string lowered = description.ToLowerInvariant();
            return lowered.Contains("fire")
                || lowered.Contains("burn")
                || lowered.Contains("ash")
                || lowered.Contains("cinder")
                || lowered.Contains("lava")
                || lowered.Contains("magma")
                || lowered.Contains("furnace");
        }

        private bool MutationMeddley_IsSmokyEnvironment()
        {
            if (ParentObject == null || ParentObject.CurrentCell == null)
            {
                return false;
            }

            string description = ParentObject.CurrentCell.ToString();
            if (string.IsNullOrEmpty(description))
            {
                return false;
            }

            string lowered = description.ToLowerInvariant();
            return lowered.Contains("smoke")
                || lowered.Contains("ash")
                || lowered.Contains("gas")
                || lowered.Contains("steam")
                || lowered.Contains("soot");
        }

        private bool MutationMeddley_IsTriadActive(string id)
        {
            return MutationMeddley_IsSynergyActive(new MutationMeddley_SynergyDefinition(id, "", ""));
        }
    }
}
