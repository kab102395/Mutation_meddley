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
        private static readonly string[] MutationMeddley_CommonStatNames = new string[]
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

        private readonly Dictionary<string, int> MutationMeddley_PendingStatShifts =
            new Dictionary<string, int>(StringComparer.Ordinal);

        [NonSerialized]
        private Dictionary<string, int> MutationMeddley_AppliedStatShifts =
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
                // once here as the final pass keeps shared baseline/capstone additions,
                // continuous rank scaling, and movement-sensitive passives in sync.
                MutationMeddley_RefreshPassiveEffects();
                MutationMeddley_ApplyStaticFreezePassiveEffects();
                MutationMeddley_CommitCommonStatShifts();
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
            MutationMeddley_CommitCommonStatShifts();
            RemoveMyActivatedAbility(ref MutationMeddley_ModeAbilityID);
            return base.Unmutate(GO);
        }

        protected override void MutationMeddley_OnEvolutionStateChanged()
        {
            MutationMeddley_AddModeAbility();
            MutationMeddley_EnsureValidMode();
            MutationMeddley_RefreshPassiveEffects();
            MutationMeddley_ApplyStaticFreezePassiveEffects();
            MutationMeddley_CommitCommonStatShifts();
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
            // 0.7.1: build a desired stat snapshot without touching live Qud stats.
            // The previous clear-to-zero/reapply pattern briefly changed Toughness and
            // therefore max HP on every refresh, which could retrigger vanilla
            // low-health warnings during movement and Tighten Carapace turns.
            MutationMeddley_PendingStatShifts.Clear();
        }

        protected void MutationMeddley_SetShift(string stat, int amount)
        {
            int currentAmount;
            if (!MutationMeddley_PendingStatShifts.TryGetValue(stat, out currentAmount))
            {
                currentAmount = 0;
            }

            MutationMeddley_PendingStatShifts[stat] = currentAmount + amount;
        }

        private void MutationMeddley_EnsureAppliedStatShiftCache()
        {
            if (MutationMeddley_AppliedStatShifts == null)
            {
                MutationMeddley_AppliedStatShifts =
                    new Dictionary<string, int>(StringComparer.Ordinal);
            }
        }

        private void MutationMeddley_CommitCommonStatShifts()
        {
            MutationMeddley_EnsureAppliedStatShiftCache();

            for (int i = 0; i < MutationMeddley_CommonStatNames.Length; i++)
            {
                string stat = MutationMeddley_CommonStatNames[i];
                int desired;
                int applied;

                if (!MutationMeddley_PendingStatShifts.TryGetValue(stat, out desired))
                {
                    desired = 0;
                }

                if (!MutationMeddley_AppliedStatShifts.TryGetValue(stat, out applied))
                {
                    applied = 0;
                }

                if (desired == applied)
                {
                    continue;
                }

                StatShifter.SetStatShift(stat, desired, true);

                if (desired == 0)
                {
                    MutationMeddley_AppliedStatShifts.Remove(stat);
                }
                else
                {
                    MutationMeddley_AppliedStatShifts[stat] = desired;
                }
            }
        }

        internal void MutationMeddley_RefreshForBiology()
        {
            MutationMeddley_RefreshPassiveEffects();
            MutationMeddley_ApplyStaticFreezePassiveEffects();
            MutationMeddley_CommitCommonStatShifts();
        }

        internal bool MutationMeddley_TryBiologyHeal(int amount)
        {
            return MutationMeddley_TryHeal(amount);
        }

        internal string MutationMeddley_PeekCurrentModeName()
        {
            return MutationMeddley_GetCurrentModeName();
        }

        internal string MutationMeddley_PeekEvolutionSummary()
        {
            return MutationMeddley_GetEvolutionSummary();
        }

        internal string MutationMeddley_PeekPassiveBonusSummary()
        {
            return MutationMeddley_GetPassiveBonusSummary();
        }

        internal string MutationMeddley_PeekSynergySummary()
        {
            return MutationMeddley_GetSynergySummary();
        }

        internal string MutationMeddley_PeekCurrentMechanicsSummary()
        {
            return MutationMeddley_GetCurrentMechanicsSummary();
        }

        protected int MutationMeddley_GetContinuousProgressionMaturity()
        {
            int level = Math.Max(1, Level);
            return level <= 1 ? 0 : (level + 1) / 3;
        }

        protected int MutationMeddley_GetContinuousProgressionVerbGrowth()
        {
            int level = Math.Max(1, Level);
            return Math.Max(0, (level - 1) / 3);
        }

        protected int MutationMeddley_GetContinuousProgressionBaselineCap(int baseCap)
        {
            return Math.Max(0, baseCap) + (Math.Max(1, Level) >= 2 ? 1 : 0);
        }

        private int MutationMeddley_ScaleContinuousVerbAmount(int amount)
        {
            if (amount <= 0)
            {
                return amount;
            }

            return amount + MutationMeddley_GetContinuousProgressionVerbGrowth();
        }

        protected new bool MutationMeddley_TryHeal(int amount)
        {
            return base.MutationMeddley_TryHeal(MutationMeddley_ScaleContinuousVerbAmount(amount));
        }

        protected new MutationMeddley_BonusDamageResult MutationMeddley_TryBonusDamage(
            GameObject target,
            int amount,
            string label,
            string context)
        {
            return base.MutationMeddley_TryBonusDamage(
                target,
                MutationMeddley_ScaleContinuousVerbAmount(amount),
                label,
                context
            );
        }

        private string MutationMeddley_GetContinuousProgressionSummary()
        {
            int level = Math.Max(1, Level);
            int maturity = MutationMeddley_GetContinuousProgressionMaturity();
            int verbGrowth = MutationMeddley_GetContinuousProgressionVerbGrowth();
            string next;

            if (level >= 10)
            {
                next = "normal mutation-point progression is complete at rank 10; physical rapid-advancement levels continue these formulas.";
            }
            else
            {
                int nextLevel = level + 1;
                if (nextLevel == 3 || nextLevel == 6 || nextLevel == 9)
                {
                    next = "rank " + nextLevel + " adds the next evolution milestone while retaining all prior scaling.";
                }
                else if (nextLevel == 2 || nextLevel == 5 || nextLevel == 8)
                {
                    next = "rank " + nextLevel + " increases the branch maturity passive.";
                }
                else
                {
                    next = "rank " + nextLevel + " increases Mutation Meddley healing/bonus-damage output and the branch's verb-support passive.";
                }
            }

            return "Continuous growth: rank "
                + level
                + ", maturity tier "
                + maturity
                + ", verb output +"
                + verbGrowth
                + ". Next: "
                + next;
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
            return "Every mutation rank strengthens the biology you already have.\n"
                + "Ranks 3, 6, and 9 additionally unlock identity, specialization, and capstone choices.\n"
                + "Use Evolve "
                + MutationMeddley_EvolutionDisplayName
                + " at those milestones.\n"
                + "Use "
                + MutationMeddley_ModeAbilityName
                + " to switch stance after you have a path.\n"
                + MutationMeddley_GetContinuousProgressionSummary();
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

            if (modes[selection].Id == MutationMeddley_GetCurrentModeId())
            {
                return;
            }

            MutationMeddley_SetCurrentModeId(modes[selection].Id);
            MutationMeddley_RefreshPassiveEffects();
            MutationMeddley_ApplyStaticFreezePassiveEffects();
            MutationMeddley_CommitCommonStatShifts();
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
                int baselineCap = MutationMeddley_GetContinuousProgressionBaselineCap(2);
                switch (MutationMeddley_EvolutionDisplayName)
                {
                    case "Carapace Evolution":
                        yield return "Baseline shell reflex: holding ground stores up to " + baselineCap + " brace; incoming damage spends 1 brace to close a wound.";
                        break;
                    case "Living Crystal":
                        yield return "Baseline lattice reflex: stillness or close pressure stores up to " + baselineCap + " stress; incoming damage spends 1 stress to close a fracture.";
                        break;
                    case "Brineborn":
                        yield return "Baseline brine reflex: saline reserve grants weather resistance; incoming damage can spend 1 reserve to close a wound.";
                        break;
                    case "Ash Metabolism":
                        yield return "Baseline ember reflex: embers grant heat resistance; incoming damage can spend 1 ember to cauterize a wound.";
                        break;
                    case "Walking Colony":
                        yield return "Baseline colony reflex: pressure firms the body; incoming damage can spend 1 pressure to close a wound.";
                        break;
                }

                yield break;
            }

            yield return "Offensive stored-state spends require adjacent, engaged melee contact. Ranged or other non-contact damage preserves the stored state.";

            if (MutationMeddley_EvolutionDisplayName == "Carapace Evolution"
                && MutationMeddley_HasEvolution("fortress")
                && MutationMeddley_GetCurrentModeId() == "spiteful_wall")
            {
                yield return "Spiteful Wall keeps stationary brace for actual incoming pressure instead of discarding it merely for remaining engaged.";
            }

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

            int baselineCap = MutationMeddley_GetContinuousProgressionBaselineCap(2);

            if (MutationMeddley_EvolutionDisplayName == "Carapace Evolution")
            {
                int brace = MutationMeddley_GetStateInt("carapace_brace", 0);
                brace = MutationMeddley_MovedSinceLastEndTurn
                    ? Math.Max(0, brace - 1)
                    : Math.Min(baselineCap, brace + 1);
                MutationMeddley_SetStateInt("carapace_brace", brace);
            }
            else if (MutationMeddley_EvolutionDisplayName == "Living Crystal")
            {
                int stress = MutationMeddley_GetStateInt("lc_stress", 0);
                stress = (!MutationMeddley_MovedSinceLastEndTurn || ParentObject.IsEngagedInMelee())
                    ? Math.Min(baselineCap, stress + 1)
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
            if (MutationMeddley_EvolutionDisplayName == "Carapace Evolution"
                && MutationMeddley_HasEvolution("fortress")
                && MutationMeddley_GetCurrentModeId() == "spiteful_wall"
                && MutationMeddley_GetStateInt("carapace_stationary", 0) > 0
                && ParentObject != null
                && ParentObject.IsEngagedInMelee())
            {
                int braceCap = MutationMeddley_HasEvolution("living_fortress") ? 5 : 4;
                MutationMeddley_SetStateInt(
                    "carapace_brace",
                    Math.Min(braceCap, MutationMeddley_GetStateInt("carapace_brace", 0) + 1)
                );
            }

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
                    && MutationMeddley_GetStateInt("carapace_stationary", 0) > 0
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
                    && MutationMeddley_GetStateInt("colony_stride_streak", 0) == 0)
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

        private void MutationMeddley_ApplyContinuousProgressionPassiveEffects()
        {
            int maturity = MutationMeddley_GetContinuousProgressionMaturity();
            int verbGrowth = MutationMeddley_GetContinuousProgressionVerbGrowth();

            if (maturity <= 0 && verbGrowth <= 0)
            {
                return;
            }

            switch (MutationMeddley_EvolutionDisplayName)
            {
                case "Carapace Evolution":
                    if (MutationMeddley_HasEvolution("hunter_shell"))
                    {
                        MutationMeddley_SetShift("Agility", maturity);
                        MutationMeddley_SetShift("Quickness", verbGrowth * 2);
                    }
                    else if (MutationMeddley_HasEvolution("adaptive_carapace"))
                    {
                        MutationMeddley_SetShift("Willpower", maturity);
                        MutationMeddley_SetShift("HeatResistance", verbGrowth * 3);
                        MutationMeddley_SetShift("ColdResistance", verbGrowth * 3);
                    }
                    else
                    {
                        MutationMeddley_SetShift("Toughness", maturity);
                        MutationMeddley_SetShift("AV", verbGrowth);
                    }
                    break;
                case "Living Crystal":
                    if (MutationMeddley_HasEvolution("prismatic_matrix"))
                    {
                        MutationMeddley_SetShift("Agility", maturity);
                        MutationMeddley_SetShift("DV", verbGrowth);
                    }
                    else if (MutationMeddley_HasEvolution("resonant_crystal"))
                    {
                        MutationMeddley_SetShift("Ego", maturity);
                        MutationMeddley_SetShift("Quickness", verbGrowth * 2);
                    }
                    else
                    {
                        MutationMeddley_SetShift("Toughness", maturity);
                        MutationMeddley_SetShift("AV", verbGrowth);
                    }
                    break;
                case "Brineborn":
                    if (MutationMeddley_HasEvolution("saltglass_bloom"))
                    {
                        MutationMeddley_SetShift("Toughness", maturity);
                        MutationMeddley_SetShift("AV", verbGrowth);
                    }
                    else if (MutationMeddley_HasEvolution("scouring_estuary"))
                    {
                        MutationMeddley_SetShift("Agility", maturity);
                        MutationMeddley_SetShift("Quickness", verbGrowth * 2);
                    }
                    else
                    {
                        MutationMeddley_SetShift("Toughness", maturity);
                        MutationMeddley_SetShift("HeatResistance", verbGrowth * 2);
                        MutationMeddley_SetShift("ColdResistance", verbGrowth * 2);
                    }
                    break;
                case "Ash Metabolism":
                    if (MutationMeddley_HasEvolution("cinder_gut"))
                    {
                        MutationMeddley_SetShift("Agility", maturity);
                        MutationMeddley_SetShift("Quickness", verbGrowth * 2);
                        MutationMeddley_SetShift("HeatResistance", verbGrowth * 2);
                    }
                    else if (MutationMeddley_HasEvolution("smoke_organ"))
                    {
                        MutationMeddley_SetShift("Ego", maturity);
                        MutationMeddley_SetShift("DV", verbGrowth);
                        MutationMeddley_SetShift("HeatResistance", verbGrowth * 2);
                    }
                    else
                    {
                        MutationMeddley_SetShift("Toughness", maturity);
                        MutationMeddley_SetShift("AV", verbGrowth);
                        MutationMeddley_SetShift("HeatResistance", verbGrowth * 3);
                    }
                    break;
                case "Walking Colony":
                    if (MutationMeddley_HasEvolution("surveyor_swarm"))
                    {
                        MutationMeddley_SetShift("Agility", maturity);
                        MutationMeddley_SetShift("Quickness", verbGrowth * 2);
                    }
                    else if (MutationMeddley_HasEvolution("graft_parliament"))
                    {
                        MutationMeddley_SetShift("Intelligence", maturity);
                        MutationMeddley_SetShift("Willpower", verbGrowth);
                    }
                    else
                    {
                        MutationMeddley_SetShift("Toughness", maturity);
                        MutationMeddley_SetShift("AV", verbGrowth);
                    }
                    break;
            }
        }

        private void MutationMeddley_ApplyStaticFreezePassiveEffects()
        {
            if (!MutationMeddley_IsFunctionallyActive())
            {
                return;
            }

            MutationMeddley_ApplyContinuousProgressionPassiveEffects();

            if (MutationMeddley_HasAnyEvolution())
            {
                return;
            }

            switch (MutationMeddley_EvolutionDisplayName)
            {
                case "Carapace Evolution":
                    MutationMeddley_SetShift("AV", 1);
                    if (MutationMeddley_GetStateInt("carapace_brace", 0) > 0)
                    {
                        // Brace is a fast-changing combat resource. Do not toggle
                        // Toughness/max HP from it; use shell defense instead.
                        MutationMeddley_SetShift("DV", 1);
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
