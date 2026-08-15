using System;
using System.Collections.Generic;
using System.Text;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_ModeChoice
    {
        public string Id;
        public string Name;
        public string Description;

        public MutationMeddley_ModeChoice(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }

    [Serializable]
    public abstract class MutationMeddley_AdaptiveMutationBase : MutationMeddley_EvolvingMutationBase
    {
        private readonly Dictionary<string, int> MutationMeddley_PendingStatShifts =
            new Dictionary<string, int>(StringComparer.Ordinal);

        [NonSerialized]
        private bool MutationMeddley_MovedSinceLastEndTurn;

        public Guid MutationMeddley_ModeAbilityID = Guid.Empty;

        protected virtual string MutationMeddley_ModeAbilityClass
        {
            get { return "Physical Mutation"; }
        }

        protected abstract string MutationMeddley_ModeAbilityName { get; }
        protected abstract string MutationMeddley_ModeAbilityDescription { get; }

        protected abstract List<MutationMeddley_ModeChoice> MutationMeddley_GetModeChoices();

        protected abstract void MutationMeddley_RefreshPassiveEffects();

        protected virtual string MutationMeddley_GetModeCommand()
        {
            return "MutationMeddley_Mode_" + GetType().Name;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, MutationMeddley_GetModeCommand());
            base.Register(Object);
            MutationMeddley_OnEvolutionStateChanged();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == MutationMeddley_GetModeCommand())
            {
                MutationMeddley_ShowModePicker();
                return false;
            }

            if (E.ID == "EnteredCell")
            {
                MutationMeddley_MovedSinceLastEndTurn = true;
            }

            MutationMeddley_HandleStaticFreezeEvent(E);

            if (E.ID == "EnteredCell"
                || E.ID == "EndTurn"
                || E.ID == "AttackerDealtDamage"
                || E.ID == "TookDamage"
                || E.ID == "TookEnvironmentalDamage")
            {
                // Concrete mutations already refresh in most of these paths. Doing it
                // once here as the final pass keeps shared baseline/capstone additions
                // and Brineborn movement-sensitive passives in sync.
                MutationMeddley_RefreshPassiveEffects();
                MutationMeddley_ApplyStaticFreezePassiveEffects();
            }

            if (E.ID == "EndTurn")
            {
                MutationMeddley_MovedSinceLastEndTurn = false;
            }

            return base.FireEvent(E);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            MutationMeddley_AddModeAbility();
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            MutationMeddley_ClearCommonStatShifts();
            RemoveMyActivatedAbility(ref MutationMeddley_ModeAbilityID);
            return base.Unmutate(GO);
        }

        protected override void MutationMeddley_OnEvolutionStateChanged()
        {
            MutationMeddley_AddModeAbility();
            MutationMeddley_EnsureValidMode();
            MutationMeddley_RefreshPassiveEffects();
            MutationMeddley_ApplyStaticFreezePassiveEffects();
        }

        protected string MutationMeddley_GetCurrentModeName()
        {
            List<MutationMeddley_ModeChoice> modes = MutationMeddley_GetModeChoices();
            for (int i = 0; i < modes.Count; i++)
            {
                if (modes[i].Id == MutationMeddley_GetCurrentModeId())
                {
                    return modes[i].Name;
                }
            }

            return "Unaligned";
        }

        protected bool MutationMeddley_HasAnyEvolution()
        {
            return MutationMeddley_GetSelectedEvolutionIds().Count > 0;
        }

        protected int MutationMeddley_GetPathDepth(string tier1Id, string tier2Id, string tier3Id)
        {
            int result = 0;

            if (MutationMeddley_HasEvolution(tier1Id))
            {
                result += 1;
            }

            if (MutationMeddley_HasEvolution(tier2Id))
            {
                result += 1;
            }

            if (MutationMeddley_HasEvolution(tier3Id))
            {
                result += 1;
            }

            return result;
        }

        protected void MutationMeddley_ClearCommonStatShifts()
        {
            string[] stats = new string[]
            {
                "AV",
                "DV",
                "MoveSpeed",
                "HeatResistance",
                "ColdResistance",
                "Quickness",
                "Strength",
                "Agility",
                "Toughness",
                "Willpower",
                "Intelligence",
                "Ego"
            };

            MutationMeddley_PendingStatShifts.Clear();

            for (int i = 0; i < stats.Length; i++)
            {
                StatShifter.SetStatShift(stats[i], 0, true);
            }
        }

        protected void MutationMeddley_SetShift(string stat, int amount)
        {
            int currentAmount;
            if (!MutationMeddley_PendingStatShifts.TryGetValue(stat, out currentAmount))
            {
                currentAmount = 0;
            }

            currentAmount += amount;
            MutationMeddley_PendingStatShifts[stat] = currentAmount;
            StatShifter.SetStatShift(stat, currentAmount, true);
        }

        protected string MutationMeddley_DescribeModeState()
        {
            if (!MutationMeddley_HasAnyEvolution())
            {
                return "No path chosen yet; the baseline mutation loop remains active.";
            }

            return "Current stance: " + MutationMeddley_GetCurrentModeName() + ".";
        }

        protected virtual IEnumerable<string> MutationMeddley_GetCurrentMechanicNotes()
        {
            return new string[0];
        }

        protected string MutationMeddley_GetUsageSummary()
        {
            return "Ranks 1-2 retain a baseline passive and reactive loop.\n"
                + "Use Evolve "
                + MutationMeddley_EvolutionDisplayName
                + " at ranks 3, 6, and 9 to choose branches.\n"
                + "Use "
                + MutationMeddley_ModeAbilityName
                + " to switch stance after you have a path.";
        }

        protected string MutationMeddley_GetCurrentMechanicsSummary()
        {
            StringBuilder result = new StringBuilder();
            bool wroteAny = false;

            foreach (string note in MutationMeddley_GetCurrentMechanicNotes())
            {
                if (string.IsNullOrEmpty(note))
                {
                    continue;
                }

                if (!wroteAny)
                {
                    result.Append("Current mechanics:");
                    wroteAny = true;
                }

                result.Append("\n- ");
                result.Append(note);
            }

            foreach (string note in MutationMeddley_GetStaticFreezeMechanicNotes())
            {
                if (string.IsNullOrEmpty(note))
                {
                    continue;
                }

                if (!wroteAny)
                {
                    result.Append("Current mechanics:");
                    wroteAny = true;
                }

                result.Append("\n- ");
                result.Append(note);
            }

            if (!wroteAny)
            {
                return "Current mechanics:\n- Baseline mutation behavior is active.";
            }

            return result.ToString();
        }

        protected string MutationMeddley_GetPassiveBonusSummary()
        {
            if (MutationMeddley_PendingStatShifts.Count == 0)
            {
                return "Current bonuses:\n- No passive stat bonuses active.";
            }

            string[] statOrder = new string[]
            {
                "AV",
                "DV",
                "Quickness",
                "MoveSpeed",
                "Strength",
                "Agility",
                "Toughness",
                "Willpower",
                "Intelligence",
                "Ego",
                "HeatResistance",
                "ColdResistance"
            };

            StringBuilder result = new StringBuilder("Current bonuses:");
            bool wroteAny = false;

            for (int i = 0; i < statOrder.Length; i++)
            {
                int amount;
                if (!MutationMeddley_PendingStatShifts.TryGetValue(statOrder[i], out amount) || amount == 0)
                {
                    continue;
                }

                result.Append("\n- ");
                result.Append(amount > 0 ? "+" : "");
                result.Append(amount);
                result.Append(" ");
                result.Append(MutationMeddley_GetStatDisplayName(statOrder[i]));
                wroteAny = true;
            }

            if (!wroteAny)
            {
                result.Append("\n- No passive stat bonuses active.");
            }

            return result.ToString();
        }

        private string MutationMeddley_GetStatDisplayName(string stat)
        {
            switch (stat)
            {
                case "HeatResistance":
                    return "Heat Resist";
                case "ColdResistance":
                    return "Cold Resist";
                case "MoveSpeed":
                    return "Move Speed";
                default:
                    return stat;
            }
        }

        protected string MutationMeddley_GetCurrentModeId()
        {
            return MutationMeddley_GetStateValue("mode");
        }

        internal string MutationMeddley_PeekCurrentModeId()
        {
            return MutationMeddley_GetCurrentModeId();
        }

        protected void MutationMeddley_SetCurrentModeId(string id)
        {
            MutationMeddley_SetStateValue("mode", id);
        }

        private void MutationMeddley_AddModeAbility()
        {
            if (MutationMeddley_ModeAbilityID != Guid.Empty)
            {
                return;
            }

            MutationMeddley_ModeAbilityID = AddMyActivatedAbility(
                Name: MutationMeddley_ModeAbilityName,
                Command: MutationMeddley_GetModeCommand(),
                Class: MutationMeddley_ModeAbilityClass,
                Description: MutationMeddley_ModeAbilityDescription
            );
        }

        private void MutationMeddley_EnsureValidMode()
        {
            List<MutationMeddley_ModeChoice> modes = MutationMeddley_GetModeChoices();
            if (modes.Count == 0)
            {
                MutationMeddley_SetCurrentModeId("");
                return;
            }

            for (int i = 0; i < modes.Count; i++)
            {
                if (modes[i].Id == MutationMeddley_GetCurrentModeId())
                {
                    return;
                }
            }

            MutationMeddley_SetCurrentModeId(modes[0].Id);
        }

        private void MutationMeddley_ShowModePicker()
        {
            if (!MutationMeddley_IsFunctionallyActive())
            {
                Popup.Show(MutationMeddley_GetInactiveReason());
                return;
            }

            List<MutationMeddley_ModeChoice> modes = MutationMeddley_GetModeChoices();
            if (modes.Count == 0)
            {
                Popup.Show(
                    "No stance is available for "
                    + MutationMeddley_EvolutionDisplayName
                    + " yet.\n\n"
                    + "Choose an evolution path first."
                );
                return;
            }

            string[] options = new string[modes.Count];
            char[] hotkeys = new char[modes.Count];

            for (int i = 0; i < modes.Count; i++)
            {
                StringBuilder option = new StringBuilder();
                option.Append(modes[i].Name);
                option.Append("\n");
                option.Append(modes[i].Description);

                if (modes[i].Id == MutationMeddley_GetCurrentModeId())
                {
                    option.Append("\n(Current)");
                }

                options[i] = option.ToString();
                hotkeys[i] = (char)('A' + i);
            }

            int selection = Popup.ShowOptionList(
                MutationMeddley_ModeAbilityName,
                Options: options,
                Hotkeys: hotkeys,
                AllowEscape: true
            );

            if (selection < 0 || selection >= modes.Count)
            {
                return;
            }

            MutationMeddley_SetCurrentModeId(modes[selection].Id);
            MutationMeddley_RefreshPassiveEffects();
            MutationMeddley_ApplyStaticFreezePassiveEffects();
            UseEnergy(1000, "Physical Mutation");
        }

        private IEnumerable<string> MutationMeddley_GetStaticFreezeMechanicNotes()
        {
            if (!MutationMeddley_IsFunctionallyActive())
            {
                yield break;
            }

            if (MutationMeddley_HasMutation("Evolution Seed [DEV]"))
            {
                yield return "DEV environment: lit="
                    + MutationMeddley_IsCurrentCellLit()
                    + ", wet="
                    + MutationMeddley_IsCurrentCellWet()
                    + ", saline="
                    + MutationMeddley_IsCurrentCellSaline()
                    + ", hot="
                    + MutationMeddley_IsCurrentCellHot()
                    + ", smoky="
                    + MutationMeddley_IsCurrentCellSmoky()
                    + ".";
            }

            if (!MutationMeddley_HasAnyEvolution())
            {
                switch (MutationMeddley_EvolutionDisplayName)
                {
                    case "Carapace Evolution":
                        yield return "Baseline shell reflex: holding ground stores up to 2 brace; incoming damage spends 1 brace to close 1 wound.";
                        break;
                    case "Living Crystal":
                        yield return "Baseline lattice reflex: stillness or close pressure stores up to 2 stress; incoming damage spends 1 stress to close 1 wound.";
                        break;
                    case "Brineborn":
                        yield return "Baseline brine reflex: saline reserve already grants weather resistance; incoming damage can spend 1 reserve to close 1 wound.";
                        break;
                    case "Ash Metabolism":
                        yield return "Baseline ember reflex: embers already grant heat resistance; incoming damage can spend 1 ember to cauterize 1 wound.";
                        break;
                    case "Walking Colony":
                        yield return "Baseline colony reflex: pressure already firms the body; incoming damage can spend 1 pressure to close 1 wound.";
                        break;
                }

                yield break;
            }

            yield return "Offensive stored-state spends require adjacent, engaged melee contact. Ranged or other non-contact damage preserves the stored state.";

            if (MutationMeddley_EvolutionDisplayName == "Brineborn"
                && MutationMeddley_HasEvolution("wellspring_flesh")
                && MutationMeddley_GetCurrentModeId() == "cool_reserve")
            {
                yield return "Cool Reserve converts reserve into persistent mend buffering instead of waiting for the wounded Draw Brine trigger.";
            }

            if (MutationMeddley_EvolutionDisplayName == "Walking Colony"
                && MutationMeddley_HasEvolution("marrow_hive")
                && MutationMeddley_GetCurrentModeId() == "bank_scars"
                && !MutationMeddley_HasEvolution("scar_feeders"))
            {
                yield return "Bank Scars converts stationary colony pressure into stitch reserve even before Scar Feeders is selected.";
            }

            if (MutationMeddley_EvolutionDisplayName == "Walking Colony"
                && MutationMeddley_HasEvolution("graft_parliament"))
            {
                yield return "Graft Parliament has a self-contained fallback: Delegate Load can bank calm load and Override Frame can spend movement pressure even without a second structural/body-plan mutation.";
            }

            if (MutationMeddley_HasEvolution("thermal_baffles"))
            {
                yield return "Thermal Baffles now preserve the stance-matched heat or rime attunement when environmental pressure lands.";
            }
            if (MutationMeddley_HasEvolution("mire_sheath"))
            {
                yield return "Mire Sheath now builds mire from wet pressure and can spend mire into an extra close-contact membrane strike.";
            }
            if (MutationMeddley_HasEvolution("cool_sump"))
            {
                yield return "Cool Sump now recovers reserve when hostile wet, saline, or hot pressure reaches a Cool Reserve body.";
            }
            if (MutationMeddley_HasEvolution("porcupine_redoubt"))
            {
                yield return "Porcupine Redoubt now adds a direct quill-backed retaliation while you hold ground.";
            }
            if (MutationMeddley_HasEvolution("skitter_bulwark"))
            {
                yield return "Skitter Bulwark refunds impact after successful moving melee contact with Multiple Legs.";
            }
            if (MutationMeddley_HasEvolution("hookstorm_frame"))
            {
                yield return "Hookstorm Frame adds a quill-backed follow-up to committed melee contact.";
            }
            if (MutationMeddley_HasEvolution("estuary_husk"))
            {
                yield return "Estuary Husk preserves extra mire attunement when wet or saline pressure reaches the shell.";
            }
            if (MutationMeddley_HasEvolution("storm_carapace"))
            {
                yield return "Storm Carapace rebalances environmental pressure into whichever attunement is currently weakest.";
            }
            if (MutationMeddley_HasEvolution("abyssal_brine"))
            {
                yield return "Abyssal Brine turns wet or saline pressure into reserve recovery.";
            }
            if (MutationMeddley_HasEvolution("whitewater_predator"))
            {
                yield return "Whitewater Predator refunds wake after dry-ground melee contact.";
            }
            if (MutationMeddley_HasEvolution("glasshouse_carapace"))
            {
                yield return "Glasshouse Carapace preserves a kiln layer when hot pressure lands.";
            }
            if (MutationMeddley_HasEvolution("ember_halo"))
            {
                yield return "Ember Halo preserves a kiln layer when pressure lands in lit space.";
            }
            if (MutationMeddley_HasEvolution("wakefeast"))
            {
                yield return "Wakefeast refunds rush after successful moving melee contact.";
            }
            if (MutationMeddley_HasEvolution("overdraft_heart"))
            {
                yield return "Overdraft Heart refunds rush after successful hot-ground melee contact.";
            }
            if (MutationMeddley_HasEvolution("crematory_mirage"))
            {
                yield return "Crematory Mirage preserves haze when smoky pressure reaches you.";
            }
            if (MutationMeddley_HasEvolution("blackdraft_engine"))
            {
                yield return "Blackdraft Engine preserves haze through moving smoky melee contact.";
            }
            if (MutationMeddley_HasEvolution("cinder_jet"))
            {
                yield return "Cinder Jet now lashes adjacent prey with a direct draft follow-up during smoky movement.";
            }
            if (MutationMeddley_HasEvolution("ossuary_bloom"))
            {
                yield return "Ossuary Bloom regrows stitch from colony pressure when fresh damage arrives.";
            }
            if (MutationMeddley_HasEvolution("burrowed_nursery"))
            {
                yield return "Burrowed Nursery regrows extra stitch from rooted Burrowing Claws pressure.";
            }
            if (MutationMeddley_HasEvolution("pack_pursuit"))
            {
                yield return "Pack Pursuit refunds scout pressure after successful moving melee contact.";
            }
            if (MutationMeddley_HasEvolution("distributed_verdict"))
            {
                yield return "Distributed Verdict throws an extra point of delegated strain back at an attacker while colonial pressure remains.";
            }
            if (MutationMeddley_HasEvolution("colony_interface"))
            {
                yield return "Colony Interface regenerates delegated load from other body-part-interaction anatomy under pressure.";
            }
            if (MutationMeddley_HasEvolution("choir_of_tendons"))
            {
                yield return "Choir of Tendons regenerates delegated load from resonance-friendly anatomy under pressure.";
            }
        }

        private void MutationMeddley_HandleStaticFreezeEvent(Event E)
        {
            if (E == null || !MutationMeddley_IsFunctionallyActive() || MutationMeddley_IsBonusDamageDispatchActive())
            {
                return;
            }

            if (E.ID == "EndTurn")
            {
                MutationMeddley_HandleBaselineEndTurn();
                MutationMeddley_HandleStanceCompletionEndTurn();
                return;
            }

            if (E.ID == "TookDamage" || E.ID == "TookEnvironmentalDamage")
            {
                MutationMeddley_HandleBaselineIncomingDamage();
                MutationMeddley_HandleCapstoneIncomingDamage(E);
                return;
            }

            if (E.ID == "AttackerDealtDamage")
            {
                MutationMeddley_HandleCapstoneOutgoingDamage(E);
            }
        }

        private void MutationMeddley_HandleBaselineEndTurn()
        {
            if (MutationMeddley_HasAnyEvolution() || ParentObject == null)
            {
                return;
            }

            if (MutationMeddley_EvolutionDisplayName == "Carapace Evolution")
            {
                int brace = MutationMeddley_GetStateInt("carapace_brace", 0);
                brace = MutationMeddley_MovedSinceLastEndTurn
                    ? Math.Max(0, brace - 1)
                    : Math.Min(2, brace + 1);
                MutationMeddley_SetStateInt("carapace_brace", brace);
            }
            else if (MutationMeddley_EvolutionDisplayName == "Living Crystal")
            {
                int stress = MutationMeddley_GetStateInt("lc_stress", 0);
                stress = (!MutationMeddley_MovedSinceLastEndTurn || ParentObject.IsEngagedInMelee())
                    ? Math.Min(2, stress + 1)
                    : Math.Max(0, stress - 1);
                MutationMeddley_SetStateInt("lc_stress", stress);
            }
        }

        private void MutationMeddley_HandleBaselineIncomingDamage()
        {
            if (MutationMeddley_HasAnyEvolution()
                || ParentObject == null
                || ParentObject.hitpoints >= ParentObject.baseHitpoints)
            {
                return;
            }

            string stateKey = "";
            string message = "";

            switch (MutationMeddley_EvolutionDisplayName)
            {
                case "Carapace Evolution":
                    stateKey = "carapace_brace";
                    message = "Your unevolved shell spends brace to close around the wound.";
                    break;
                case "Living Crystal":
                    stateKey = "lc_stress";
                    message = "Your unevolved lattice spends stress to seal a fracture.";
                    break;
                case "Brineborn":
                    stateKey = "brine_reserve";
                    message = "Stored brine closes over the wound.";
                    break;
                case "Ash Metabolism":
                    stateKey = "ash_embers";
                    message = "An ember cauterizes the fresh wound.";
                    break;
                case "Walking Colony":
                    stateKey = "colony_charge";
                    message = "The unevolved colony spends pressure to close the wound.";
                    break;
            }

            if (!string.IsNullOrEmpty(stateKey) && MutationMeddley_ConsumeStateInt(stateKey, 1) > 0)
            {
                if (MutationMeddley_TryHeal(1))
                {
                    MutationMeddley_AddPlayerMessage(message);
                }
            }
        }

        private void MutationMeddley_HandleStanceCompletionEndTurn()
        {
            if (MutationMeddley_EvolutionDisplayName == "Brineborn"
                && MutationMeddley_HasEvolution("wellspring_flesh")
                && MutationMeddley_GetCurrentModeId() == "cool_reserve")
            {
                int reserve = MutationMeddley_GetStateInt("brine_reserve", 0);
                int mend = MutationMeddley_GetStateInt("brine_mend", 0);
                bool bankable = !MutationMeddley_MovedSinceLastEndTurn
                    || MutationMeddley_IsCurrentCellWet()
                    || MutationMeddley_IsCurrentCellSaline()
                    || MutationMeddley_IsCurrentCellHot();

                if (reserve > 0 && mend < 3 && bankable)
                {
                    MutationMeddley_SetStateInt("brine_reserve", reserve - 1);
                    MutationMeddley_SetStateInt("brine_mend", Math.Min(3, mend + 2));
                    MutationMeddley_AddPlayerMessage("You settle reserve into a cool mend buffer.");
                }
            }

            if (MutationMeddley_EvolutionDisplayName == "Walking Colony"
                && MutationMeddley_HasEvolution("marrow_hive")
                && MutationMeddley_GetCurrentModeId() == "bank_scars"
                && !MutationMeddley_HasEvolution("scar_feeders")
                && !MutationMeddley_MovedSinceLastEndTurn)
            {
                int pressure = MutationMeddley_GetStateInt("colony_charge", 0);
                int stitch = MutationMeddley_GetStateInt("colony_stitch", 0);
                if (pressure > 0 && stitch < 4)
                {
                    MutationMeddley_SetStateInt("colony_charge", pressure - 1);
                    MutationMeddley_SetStateInt("colony_stitch", Math.Min(4, stitch + 2));
                    MutationMeddley_AddPlayerMessage("The colony banks pressure as scar tissue and stitch reserve.");
                }
            }

            if (MutationMeddley_EvolutionDisplayName == "Walking Colony"
                && MutationMeddley_HasEvolution("graft_parliament"))
            {
                int pressure = MutationMeddley_GetStateInt("colony_charge", 0);
                int parliament = MutationMeddley_GetStateInt("colony_parliament", 0);

                if (MutationMeddley_GetCurrentModeId() == "delegate_load"
                    && !MutationMeddley_MovedSinceLastEndTurn
                    && !MutationMeddley_HasOtherMutationWithTag("BODY_PART_INTERACTION"))
                {
                    MutationMeddley_SetStateInt("colony_parliament", Math.Min(4, parliament + 2));
                }
                else if (MutationMeddley_GetCurrentModeId() == "override_frame"
                    && MutationMeddley_MovedSinceLastEndTurn
                    && !MutationMeddley_HasOtherMutationWithTag("STRUCTURAL")
                    && pressure > 0)
                {
                    MutationMeddley_SetStateInt("colony_charge", pressure - 1);
                    MutationMeddley_SetStateInt("colony_parliament", Math.Min(4, parliament + 2));
                }
            }
        }

        private void MutationMeddley_HandleCapstoneIncomingDamage(Event E)
        {
            if (ParentObject == null)
            {
                return;
            }

            if (MutationMeddley_EvolutionDisplayName == "Carapace Evolution")
            {
                if (MutationMeddley_HasEvolution("thermal_baffles") && E.ID == "TookEnvironmentalDamage")
                {
                    if (MutationMeddley_GetCurrentModeId() == "ember_veil" && MutationMeddley_IsCurrentCellHot())
                    {
                        MutationMeddley_SetStateInt(
                            "carapace_attune_heat",
                            Math.Min(6, MutationMeddley_GetStateInt("carapace_attune_heat", 0) + 1)
                        );
                    }
                    else if (MutationMeddley_GetCurrentModeId() == "rime_veil" && !MutationMeddley_IsCurrentCellHot())
                    {
                        MutationMeddley_SetStateInt(
                            "carapace_attune_rime",
                            Math.Min(6, MutationMeddley_GetStateInt("carapace_attune_rime", 0) + 1)
                        );
                    }
                }

                if (MutationMeddley_HasEvolution("mire_sheath")
                    && (MutationMeddley_IsCurrentCellWet() || MutationMeddley_IsCurrentCellSaline()))
                {
                    MutationMeddley_SetStateInt(
                        "carapace_attune_mire",
                        Math.Min(6, MutationMeddley_GetStateInt("carapace_attune_mire", 0) + 1)
                    );
                }

                if (MutationMeddley_HasEvolution("porcupine_redoubt")
                    && MutationMeddley_HasMutation("Quills")
                    && !MutationMeddley_MovedSinceLastEndTurn
                    && ParentObject.IsEngagedInMelee()
                    && E.ID == "TookDamage")
                {
                    GameObject source = MutationMeddley_GetIncomingDamageSource(E);
                    MutationMeddley_TryBonusDamage(source, 1, "porcupine redoubt", "carapace.porcupine_redoubt");
                }

                if (MutationMeddley_HasEvolution("estuary_husk")
                    && (MutationMeddley_IsCurrentCellWet() || MutationMeddley_IsCurrentCellSaline()))
                {
                    MutationMeddley_SetStateInt(
                        "carapace_attune_mire",
                        Math.Min(6, MutationMeddley_GetStateInt("carapace_attune_mire", 0) + 1)
                    );
                }

                if (MutationMeddley_HasEvolution("storm_carapace") && E.ID == "TookEnvironmentalDamage")
                {
                    MutationMeddley_RebalanceCarapaceAttunement();
                }
            }
            else if (MutationMeddley_EvolutionDisplayName == "Brineborn")
            {
                if (MutationMeddley_HasEvolution("cool_sump")
                    && MutationMeddley_GetCurrentModeId() == "cool_reserve"
                    && (MutationMeddley_IsCurrentCellWet()
                        || MutationMeddley_IsCurrentCellSaline()
                        || MutationMeddley_IsCurrentCellHot()))
                {
                    int maxReserve = MutationMeddley_GetSharedBrineMaxReserve();
                    MutationMeddley_SetStateInt(
                        "brine_reserve",
                        Math.Min(maxReserve, MutationMeddley_GetStateInt("brine_reserve", 0) + 1)
                    );
                }

                if (MutationMeddley_HasEvolution("abyssal_brine")
                    && (MutationMeddley_IsCurrentCellWet() || MutationMeddley_IsCurrentCellSaline()))
                {
                    int maxReserve = MutationMeddley_GetSharedBrineMaxReserve();
                    MutationMeddley_SetStateInt(
                        "brine_reserve",
                        Math.Min(maxReserve, MutationMeddley_GetStateInt("brine_reserve", 0) + 1)
                    );
                }
            }
            else if (MutationMeddley_EvolutionDisplayName == "Ash Metabolism")
            {
                if (MutationMeddley_HasEvolution("glasshouse_carapace") && MutationMeddley_IsCurrentCellHot())
                {
                    MutationMeddley_SetStateInt("ash_kiln", Math.Min(4, MutationMeddley_GetStateInt("ash_kiln", 0) + 1));
                }

                if (MutationMeddley_HasEvolution("ember_halo") && MutationMeddley_IsCurrentCellLit())
                {
                    MutationMeddley_SetStateInt("ash_kiln", Math.Min(4, MutationMeddley_GetStateInt("ash_kiln", 0) + 1));
                }

                if (MutationMeddley_HasEvolution("crematory_mirage") && MutationMeddley_IsCurrentCellSmoky())
                {
                    MutationMeddley_SetStateInt("ash_haze", Math.Min(4, MutationMeddley_GetStateInt("ash_haze", 0) + 1));
                }
            }
            else if (MutationMeddley_EvolutionDisplayName == "Walking Colony")
            {
                if (MutationMeddley_HasEvolution("ossuary_bloom")
                    && MutationMeddley_GetStateInt("colony_charge", 0) > 0)
                {
                    MutationMeddley_SetStateInt("colony_stitch", Math.Min(4, MutationMeddley_GetStateInt("colony_stitch", 0) + 1));
                }

                if (MutationMeddley_HasEvolution("burrowed_nursery")
                    && MutationMeddley_HasMutation("Burrowing Claws")
                    && !MutationMeddley_MovedSinceLastEndTurn)
                {
                    MutationMeddley_SetStateInt("colony_stitch", Math.Min(4, MutationMeddley_GetStateInt("colony_stitch", 0) + 1));
                }

                if (MutationMeddley_HasEvolution("distributed_verdict")
                    && MutationMeddley_GetStateInt("colony_charge", 0) > 0
                    && E.ID == "TookDamage")
                {
                    GameObject source = MutationMeddley_GetIncomingDamageSource(E);
                    MutationMeddley_TryBonusDamage(source, 1, "distributed verdict", "colony.distributed_verdict");
                }

                if (MutationMeddley_HasEvolution("colony_interface")
                    && MutationMeddley_HasOtherMutationWithTag("BODY_PART_INTERACTION"))
                {
                    MutationMeddley_SetStateInt(
                        "colony_parliament",
                        Math.Min(4, MutationMeddley_GetStateInt("colony_parliament", 0) + 1)
                    );
                }

                if (MutationMeddley_HasEvolution("choir_of_tendons")
                    && (MutationMeddley_HasMutation("Heightened Hearing")
                        || MutationMeddley_MutationHasEvolution("Living Crystal", "resonant_crystal")))
                {
                    MutationMeddley_SetStateInt(
                        "colony_parliament",
                        Math.Min(4, MutationMeddley_GetStateInt("colony_parliament", 0) + 1)
                    );
                }
            }
        }

        private void MutationMeddley_HandleCapstoneOutgoingDamage(Event E)
        {
            GameObject defender = MutationMeddley_GetOutgoingDamageTarget(E);
            if (!MutationMeddley_IsOutgoingMeleeContactContext())
            {
                return;
            }

            if (MutationMeddley_EvolutionDisplayName == "Carapace Evolution")
            {
                if (MutationMeddley_HasEvolution("mire_sheath")
                    && (MutationMeddley_IsCurrentCellWet() || MutationMeddley_IsCurrentCellSaline())
                    && MutationMeddley_ConsumeStateInt("carapace_attune_mire", 1) > 0)
                {
                    MutationMeddley_TryBonusDamage(defender, 1, "mire sheath", "carapace.mire_sheath");
                }

                if (MutationMeddley_HasEvolution("skitter_bulwark")
                    && MutationMeddley_MovedSinceLastEndTurn
                    && MutationMeddley_HasMutation("Multiple Legs"))
                {
                    MutationMeddley_SetStateInt("carapace_impact", Math.Min(6, MutationMeddley_GetStateInt("carapace_impact", 0) + 1));
                }

                if (MutationMeddley_HasEvolution("hookstorm_frame") && MutationMeddley_HasMutation("Quills"))
                {
                    MutationMeddley_TryBonusDamage(defender, 1, "hookstorm frame", "carapace.hookstorm_frame");
                }
            }
            else if (MutationMeddley_EvolutionDisplayName == "Brineborn")
            {
                if (MutationMeddley_HasEvolution("whitewater_predator") && !MutationMeddley_IsCurrentCellSaline())
                {
                    MutationMeddley_SetStateInt("brine_wake", Math.Min(4, MutationMeddley_GetStateInt("brine_wake", 0) + 1));
                }
            }
            else if (MutationMeddley_EvolutionDisplayName == "Ash Metabolism")
            {
                if (MutationMeddley_HasEvolution("wakefeast") && MutationMeddley_MovedSinceLastEndTurn)
                {
                    MutationMeddley_SetStateInt("ash_rush", Math.Min(4, MutationMeddley_GetStateInt("ash_rush", 0) + 1));
                }

                if (MutationMeddley_HasEvolution("overdraft_heart") && MutationMeddley_IsCurrentCellHot())
                {
                    MutationMeddley_SetStateInt("ash_rush", Math.Min(4, MutationMeddley_GetStateInt("ash_rush", 0) + 1));
                }

                if (MutationMeddley_HasEvolution("blackdraft_engine")
                    && MutationMeddley_MovedSinceLastEndTurn
                    && MutationMeddley_IsCurrentCellSmoky())
                {
                    MutationMeddley_SetStateInt("ash_haze", Math.Min(4, MutationMeddley_GetStateInt("ash_haze", 0) + 1));
                }

                if (MutationMeddley_HasEvolution("cinder_jet")
                    && MutationMeddley_MovedSinceLastEndTurn
                    && MutationMeddley_IsCurrentCellSmoky())
                {
                    MutationMeddley_TryBonusDamage(defender, 1, "cinder jet", "ash.cinder_jet");
                    MutationMeddley_SetStateInt("ash_haze", Math.Min(4, MutationMeddley_GetStateInt("ash_haze", 0) + 1));
                }
            }
            else if (MutationMeddley_EvolutionDisplayName == "Walking Colony")
            {
                if (MutationMeddley_HasEvolution("pack_pursuit") && MutationMeddley_MovedSinceLastEndTurn)
                {
                    MutationMeddley_SetStateInt("colony_scout", Math.Min(4, MutationMeddley_GetStateInt("colony_scout", 0) + 1));
                }
            }
        }

        private void MutationMeddley_RebalanceCarapaceAttunement()
        {
            int heat = MutationMeddley_GetStateInt("carapace_attune_heat", 0);
            int mire = MutationMeddley_GetStateInt("carapace_attune_mire", 0);
            int rime = MutationMeddley_GetStateInt("carapace_attune_rime", 0);

            if (heat <= mire && heat <= rime)
            {
                MutationMeddley_SetStateInt("carapace_attune_heat", Math.Min(6, heat + 1));
            }
            else if (mire <= rime)
            {
                MutationMeddley_SetStateInt("carapace_attune_mire", Math.Min(6, mire + 1));
            }
            else
            {
                MutationMeddley_SetStateInt("carapace_attune_rime", Math.Min(6, rime + 1));
            }
        }

        private int MutationMeddley_GetSharedBrineMaxReserve()
        {
            int maxReserve = 6;
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

        private void MutationMeddley_ApplyStaticFreezePassiveEffects()
        {
            if (!MutationMeddley_IsFunctionallyActive() || MutationMeddley_HasAnyEvolution())
            {
                return;
            }

            switch (MutationMeddley_EvolutionDisplayName)
            {
                case "Carapace Evolution":
                    MutationMeddley_SetShift("AV", 1);
                    if (MutationMeddley_GetStateInt("carapace_brace", 0) > 0)
                    {
                        MutationMeddley_SetShift("Toughness", 1);
                    }
                    break;
                case "Living Crystal":
                    MutationMeddley_SetShift("AV", 1);
                    MutationMeddley_SetShift("HeatResistance", 5);
                    MutationMeddley_SetShift("ColdResistance", 5);
                    if (MutationMeddley_GetStateInt("lc_stress", 0) > 0)
                    {
                        MutationMeddley_SetShift("DV", 1);
                    }
                    break;
                case "Brineborn":
                    int reserve = MutationMeddley_GetStateInt("brine_reserve", 0);
                    MutationMeddley_SetShift("HeatResistance", 5 + (reserve * 2));
                    MutationMeddley_SetShift("ColdResistance", 5 + (reserve * 2));
                    if (MutationMeddley_IsCurrentCellSaline())
                    {
                        MutationMeddley_SetShift("DV", 1);
                    }
                    break;
                case "Ash Metabolism":
                    int embers = MutationMeddley_GetStateInt("ash_embers", 0);
                    MutationMeddley_SetShift("HeatResistance", 5 + (embers * 2));
                    if (MutationMeddley_IsCurrentCellSmoky())
                    {
                        MutationMeddley_SetShift("DV", 1);
                    }
                    break;
                case "Walking Colony":
                    int pressure = MutationMeddley_GetStateInt("colony_charge", 0);
                    MutationMeddley_SetShift("Toughness", 1);
                    if (pressure >= 2)
                    {
                        MutationMeddley_SetShift("DV", 1);
                    }
                    break;
            }
        }
    }
}
