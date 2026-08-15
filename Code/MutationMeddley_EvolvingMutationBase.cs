using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MutationMeddley_EvolutionChoice
    {
        public string Id;
        public string Name;
        public string Description;
        public string DetailText;
        public int RequiredLevel;
        public int Tier;
        public string PrerequisiteId;
        public bool IsUnusual;

        public MutationMeddley_EvolutionChoice(
            string id,
            string name,
            string description,
            int requiredLevel,
            int tier,
            string prerequisiteId = "",
            string detailText = "",
            bool isUnusual = false)
        {
            Id = id;
            Name = name;
            Description = description;
            DetailText = detailText ?? "";
            RequiredLevel = requiredLevel;
            Tier = tier;
            PrerequisiteId = prerequisiteId ?? "";
            IsUnusual = isUnusual;
        }
    }

    [Serializable]
    public abstract class MutationMeddley_EvolvingMutationBase : BaseMutation
    {
        protected struct MutationMeddley_BonusDamageResult
        {
            public bool TargetResolved;
            public bool DamageDispatched;
            public bool EventContinued;
            public bool HitPointLossObserved;
            public bool RecursionSuppressed;
        }

        private enum MutationMeddley_DamageEventContext
        {
            None,
            Incoming,
            OutgoingMeleeContact,
            OutgoingNonMelee
        }

        private const string MutationMeddley_StateVersionKey = "statev";
        private const string MutationMeddley_CurrentStateVersion = "1";
        private static int MutationMeddley_BonusDamageDispatchDepth;
        internal static bool MutationMeddley_DebugDamageTracingEnabled;

        [NonSerialized]
        private MutationMeddley_DamageEventContext MutationMeddley_CurrentDamageEventContext =
            MutationMeddley_DamageEventContext.None;

        // Keep these public serialized fields stable. Future framework state should
        // preferably be encoded inside EvolutionState unless an explicit save
        // migration is introduced.
        public string MutationMeddley_EvolutionState = "";
        public Guid MutationMeddley_EvolveAbilityID = Guid.Empty;

        public abstract string MutationMeddley_EvolutionDisplayName { get; }

        protected abstract List<MutationMeddley_EvolutionChoice> MutationMeddley_GetEvolutionChoices();

        protected virtual string MutationMeddley_GetEvolutionCommand()
        {
            return "MutationMeddley_Evolve_" + GetType().Name;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, MutationMeddley_GetEvolutionCommand());
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == MutationMeddley_GetEvolutionCommand())
            {
                MutationMeddley_CurrentDamageEventContext = MutationMeddley_DamageEventContext.None;
                MutationMeddley_ShowEvolutionPicker();
                return false;
            }

            bool result = base.FireEvent(E);
            MutationMeddley_CurrentDamageEventContext = MutationMeddley_DamageEventContext.None;
            return result;
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            MutationMeddley_AddEvolutionAbility();
            MutationMeddley_OnEvolutionStateChanged();
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMyActivatedAbility(ref MutationMeddley_EvolveAbilityID);
            return base.Unmutate(GO);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            bool result = base.ChangeLevel(NewLevel);
            MutationMeddley_OnEvolutionStateChanged();
            return result;
        }

        protected virtual void MutationMeddley_OnEvolutionChosen(MutationMeddley_EvolutionChoice choice)
        {
        }

        protected virtual void MutationMeddley_OnEvolutionStateChanged()
        {
        }

        protected virtual bool MutationMeddley_IsFunctionallyActive()
        {
            return true;
        }

        protected virtual string MutationMeddley_GetInactiveReason()
        {
            return MutationMeddley_EvolutionDisplayName + " is currently inactive.";
        }

        protected virtual IEnumerable<string> MutationMeddley_GetIntrinsicSemanticTags()
        {
            return new string[0];
        }

        protected virtual IEnumerable<string> MutationMeddley_GetEvolutionSemanticTags()
        {
            return new string[0];
        }

        protected virtual List<MutationMeddley_SynergyDefinition> MutationMeddley_GetSynergyDefinitions()
        {
            return new List<MutationMeddley_SynergyDefinition>();
        }

        protected virtual bool MutationMeddley_IsSynergyActive(MutationMeddley_SynergyDefinition synergy)
        {
            return false;
        }

        protected virtual bool MutationMeddley_IsChoiceUnlocked(MutationMeddley_EvolutionChoice choice)
        {
            return !choice.IsUnusual;
        }

        protected bool MutationMeddley_HasEvolution(string id)
        {
            List<string> selected = MutationMeddley_GetSelectedEvolutionIds();
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        protected string MutationMeddley_GetEvolutionSummary()
        {
            List<string> selected = MutationMeddley_GetSelectedEvolutionIds();
            if (selected.Count == 0)
            {
                return "No evolutions chosen.";
            }

            List<MutationMeddley_EvolutionChoice> choices = MutationMeddley_GetEvolutionChoices();
            StringBuilder result = new StringBuilder("Evolutions: ");
            bool wroteAny = false;

            for (int i = 0; i < choices.Count; i++)
            {
                if (!MutationMeddley_HasEvolution(choices[i].Id))
                {
                    continue;
                }

                if (wroteAny)
                {
                    result.Append(" -> ");
                }

                result.Append(choices[i].Name);
                wroteAny = true;
            }

            return result.ToString();
        }

        protected string MutationMeddley_GetSynergySummary()
        {
            List<MutationMeddley_SynergyDefinition> active = MutationMeddley_GetActiveSynergies();
            if (active.Count == 0)
            {
                return "Pair Synergies: none active.\nTriads: none active.";
            }

            StringBuilder pairs = new StringBuilder("Pair Synergies:");
            StringBuilder triads = new StringBuilder("Triads:");
            StringBuilder unusual = new StringBuilder("UNUSUAL ADAPTATION:");
            bool wrotePairs = false;
            bool wroteTriads = false;
            bool wroteUnusual = false;

            for (int i = 0; i < active.Count; i++)
            {
                StringBuilder target = active[i].IsUnusual
                    ? unusual
                    : (active[i].IsTriad ? triads : pairs);
                target.Append("\n- ");
                target.Append(active[i].Title);
                target.Append(": ");
                target.Append(active[i].Summary);

                if (active[i].IsUnusual)
                {
                    wroteUnusual = true;
                }
                else if (active[i].IsTriad)
                {
                    wroteTriads = true;
                }
                else
                {
                    wrotePairs = true;
                }
            }

            if (!wrotePairs)
            {
                pairs.Append("\n- none active");
            }

            if (!wroteTriads)
            {
                triads.Append("\n- none active");
            }

            StringBuilder result = new StringBuilder();
            result.Append(pairs.ToString());
            result.Append("\n");
            result.Append(triads.ToString());

            if (wroteUnusual)
            {
                result.Append("\n");
                result.Append(unusual.ToString());
            }

            return result.ToString();
        }

        protected List<MutationMeddley_SynergyDefinition> MutationMeddley_GetActiveSynergies()
        {
            List<MutationMeddley_SynergyDefinition> result = new List<MutationMeddley_SynergyDefinition>();
            List<MutationMeddley_SynergyDefinition> definitions = MutationMeddley_GetSynergyDefinitions();

            for (int i = 0; i < definitions.Count; i++)
            {
                if (MutationMeddley_IsSynergyActive(definitions[i]))
                {
                    result.Add(definitions[i]);
                }
            }

            return result;
        }

        protected bool MutationMeddley_HasMutation(string mutationName)
        {
            return MutationMeddley_GetMutationByName(mutationName) != null;
        }

        protected bool MutationMeddley_MutationHasEvolution(string mutationName, string evolutionId)
        {
            MutationMeddley_EvolvingMutationBase mutation = MutationMeddley_GetMutationByName(mutationName)
                as MutationMeddley_EvolvingMutationBase;

            return mutation != null && mutation.MutationMeddley_HasEvolution(evolutionId);
        }

        protected bool MutationMeddley_MutationIsFunctionallyActive(string mutationName)
        {
            MutationMeddley_EvolvingMutationBase mutation = MutationMeddley_GetMutationByName(mutationName)
                as MutationMeddley_EvolvingMutationBase;

            return mutation != null && mutation.MutationMeddley_PeekIsFunctionallyActive();
        }

        protected bool MutationMeddley_MutationHasMode(string mutationName, string modeId)
        {
            MutationMeddley_AdaptiveMutationBase mutation = MutationMeddley_GetMutationByName(mutationName)
                as MutationMeddley_AdaptiveMutationBase;

            return mutation != null && mutation.MutationMeddley_PeekCurrentModeId() == modeId;
        }

        protected bool MutationMeddley_MutationHasSemanticTag(string mutationName, string tag)
        {
            BaseMutation liveMutation = MutationMeddley_GetMutationByName(mutationName);
            if (liveMutation == null)
            {
                return false;
            }

            MutationMeddley_EvolvingMutationBase mutation = liveMutation
                as MutationMeddley_EvolvingMutationBase;

            if (mutation != null)
            {
                if (!mutation.MutationMeddley_PeekIsFunctionallyActive())
                {
                    return false;
                }

                return mutation.MutationMeddley_GetCurrentSemanticTags().Contains(tag);
            }

            foreach (string mutationTag in MutationMeddley_TagRegistry.MutationMeddley_GetTagsForVanillaMutation(mutationName))
            {
                if (mutationTag == tag)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool MutationMeddley_PeekIsFunctionallyActive()
        {
            return MutationMeddley_IsFunctionallyActive();
        }

        protected BaseMutation MutationMeddley_GetMutationByName(string mutationName)
        {
            if (ParentObject == null)
            {
                return null;
            }

            global::XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations")
                as global::XRL.World.Parts.Mutations;

            if (mutations == null)
            {
                return null;
            }

            return mutations.GetMutationByName(mutationName);
        }

        protected bool MutationMeddley_HasSemanticTag(string tag)
        {
            return MutationMeddley_GetCurrentSemanticTags().Contains(tag);
        }

        protected bool MutationMeddley_HasOtherMutationWithTag(string tag)
        {
            if (ParentObject == null)
            {
                return false;
            }

            global::XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations")
                as global::XRL.World.Parts.Mutations;

            if (mutations == null)
            {
                return false;
            }

            foreach (BaseMutation mutation in mutations.MutationList)
            {
                if (mutation == null || mutation == this)
                {
                    continue;
                }

                MutationMeddley_EvolvingMutationBase evolving = mutation as MutationMeddley_EvolvingMutationBase;
                if (evolving != null)
                {
                    if (!evolving.MutationMeddley_PeekIsFunctionallyActive())
                    {
                        continue;
                    }

                    if (evolving.MutationMeddley_GetCurrentSemanticTags().Contains(tag))
                    {
                        return true;
                    }

                    continue;
                }

                foreach (string mutationTag in MutationMeddley_TagRegistry.MutationMeddley_GetTagsForVanillaMutation(mutation.Name))
                {
                    if (mutationTag == tag)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool MutationMeddley_HasOtherMutationWithTagExcept(string tag, string mutationName)
        {
            if (ParentObject == null)
            {
                return false;
            }

            global::XRL.World.Parts.Mutations mutations = ParentObject.GetPart("Mutations")
                as global::XRL.World.Parts.Mutations;

            if (mutations == null)
            {
                return false;
            }

            foreach (BaseMutation mutation in mutations.MutationList)
            {
                if (mutation == null || mutation == this || mutation.Name == mutationName)
                {
                    continue;
                }

                MutationMeddley_EvolvingMutationBase evolving = mutation as MutationMeddley_EvolvingMutationBase;
                if (evolving != null)
                {
                    if (!evolving.MutationMeddley_PeekIsFunctionallyActive())
                    {
                        continue;
                    }

                    if (evolving.MutationMeddley_GetCurrentSemanticTags().Contains(tag))
                    {
                        return true;
                    }

                    continue;
                }

                foreach (string mutationTag in MutationMeddley_TagRegistry.MutationMeddley_GetTagsForVanillaMutation(mutation.Name))
                {
                    if (mutationTag == tag)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected HashSet<string> MutationMeddley_GetCurrentSemanticTags()
        {
            HashSet<string> tags = new HashSet<string>(StringComparer.Ordinal);

            foreach (string tag in MutationMeddley_GetIntrinsicSemanticTags())
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    tags.Add(tag);
                }
            }

            foreach (string tag in MutationMeddley_GetEvolutionSemanticTags())
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    tags.Add(tag);
                }
            }

            return tags;
        }

        protected bool MutationMeddley_IsCurrentCellLit()
        {
            return ParentObject != null
                && ParentObject.CurrentCell != null
                && ParentObject.CurrentCell.IsLit();
        }

        protected bool MutationMeddley_IsCurrentCellWet()
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
                || loweredTerrain.Contains("marsh")
                || loweredTerrain.Contains("brine")
                || loweredTerrain.Contains("salt");
        }

        protected bool MutationMeddley_IsCurrentCellSaline()
        {
            if (ParentObject == null || ParentObject.CurrentCell == null)
            {
                return false;
            }

            string cellDescription = ParentObject.CurrentCell.ToString();
            if (!string.IsNullOrEmpty(cellDescription))
            {
                string loweredTerrain = cellDescription.ToLowerInvariant();
                if (loweredTerrain.Contains("salt") || loweredTerrain.Contains("brine"))
                {
                    return true;
                }
            }

            object liquid = ParentObject.CurrentCell.GetOpenLiquidVolume();
            if (liquid != null)
            {
                string liquidName = liquid.ToString().ToLowerInvariant();
                if (liquidName.Contains("salt") || liquidName.Contains("brine"))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool MutationMeddley_IsCurrentCellHot()
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

        protected bool MutationMeddley_IsCurrentCellSmoky()
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
                || lowered.Contains("soot")
                || lowered.Contains("haze");
        }

        protected bool MutationMeddley_IsCurrentCellHostileTraversal()
        {
            return MutationMeddley_IsCurrentCellWet()
                || MutationMeddley_IsCurrentCellSaline()
                || MutationMeddley_IsCurrentCellHot()
                || MutationMeddley_IsCurrentCellSmoky();
        }

        protected bool MutationMeddley_TryHeal(int amount)
        {
            if (amount <= 0 || ParentObject == null || ParentObject.hitpoints >= ParentObject.baseHitpoints)
            {
                return false;
            }

            ParentObject.Heal(amount);
            return true;
        }

        protected void MutationMeddley_AddPlayerMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || ParentObject == null || !ParentObject.IsPlayer())
            {
                return;
            }

            XRL.Messages.MessageQueue.AddPlayerMessage(message);
        }

        protected void MutationMeddley_SetDamageTraceEnabled(bool enabled)
        {
            MutationMeddley_DebugDamageTracingEnabled = enabled;
        }

        protected bool MutationMeddley_IsDamageTraceEnabled()
        {
            return MutationMeddley_DebugDamageTracingEnabled;
        }

        protected bool MutationMeddley_IsBonusDamageDispatchActive()
        {
            return MutationMeddley_BonusDamageDispatchDepth > 0;
        }

        protected GameObject MutationMeddley_GetIncomingDamageSource(Event E)
        {
            MutationMeddley_CurrentDamageEventContext = MutationMeddley_DamageEventContext.Incoming;
            return MutationMeddley_GetEventGameObject(E, "Source", "Attacker", "Actor");
        }

        protected GameObject MutationMeddley_GetOutgoingDamageTarget(Event E)
        {
            GameObject target = MutationMeddley_GetEventGameObject(E, "Defender", "Target", "Object");
            bool meleeContact = MutationMeddley_IsLikelyMeleeContact(target);
            MutationMeddley_CurrentDamageEventContext = meleeContact
                ? MutationMeddley_DamageEventContext.OutgoingMeleeContact
                : MutationMeddley_DamageEventContext.OutgoingNonMelee;

            if (!meleeContact)
            {
                MutationMeddley_TraceDamageProc(
                    "contact.gate",
                    "rejected outgoing damage as non-contact; targetResolved=" + (target != null)
                );
            }

            return target;
        }

        protected bool MutationMeddley_IsLikelyMeleeContact(GameObject target)
        {
            if (ParentObject == null
                || target == null
                || ParentObject.CurrentCell == null
                || target.CurrentCell == null
                || !ParentObject.IsEngagedInMelee())
            {
                return false;
            }

            int dx = Math.Abs(ParentObject.CurrentCell.X - target.CurrentCell.X);
            int dy = Math.Abs(ParentObject.CurrentCell.Y - target.CurrentCell.Y);
            return dx <= 1 && dy <= 1 && (dx > 0 || dy > 0);
        }

        protected bool MutationMeddley_IsOutgoingMeleeContactContext()
        {
            return MutationMeddley_CurrentDamageEventContext == MutationMeddley_DamageEventContext.OutgoingMeleeContact;
        }

        protected void MutationMeddley_TraceDamageProc(string context, string detail)
        {
            if (!MutationMeddley_DebugDamageTracingEnabled)
            {
                return;
            }

            MutationMeddley_AddPlayerMessage("[MM TRACE] " + context + ": " + detail);
        }

        private GameObject MutationMeddley_GetEventGameObject(Event E, params string[] names)
        {
            if (E == null || names == null)
            {
                return null;
            }

            for (int i = 0; i < names.Length; i++)
            {
                GameObject parameterObject = E.GetGameObjectParameter(names[i]);
                if (parameterObject != null)
                {
                    return parameterObject;
                }

                object parameterValue = E.GetParameter(names[i]);
                if (parameterValue is GameObject)
                {
                    return parameterValue as GameObject;
                }
            }

            return null;
        }

        protected MutationMeddley_BonusDamageResult MutationMeddley_TryBonusDamage(
            GameObject target,
            int amount,
            string label,
            string context)
        {
            MutationMeddley_BonusDamageResult result = new MutationMeddley_BonusDamageResult
            {
                TargetResolved = target != null,
                DamageDispatched = false,
                EventContinued = false,
                HitPointLossObserved = false,
                RecursionSuppressed = false
            };

            if (!result.TargetResolved || amount <= 0)
            {
                MutationMeddley_TraceDamageProc(
                    context,
                    "targetResolved=" + result.TargetResolved + ", amount=" + amount + ", dispatched=false"
                );
                return result;
            }

            if (MutationMeddley_BonusDamageDispatchDepth > 0)
            {
                result.RecursionSuppressed = true;
                MutationMeddley_TraceDamageProc(context, "targetResolved=true, amount=" + amount + ", recursionSuppressed=true");
                return result;
            }

            int hitPointsBefore = target.hitpoints;
            MutationMeddley_BonusDamageDispatchDepth += 1;
            try
            {
                Damage damage = new Damage(amount);
                Event takeDamage = Event.New("TakeDamage");
                takeDamage.SetParameter("Damage", damage);
                takeDamage.SetParameter("Owner", ParentObject);
                takeDamage.SetParameter("Attacker", ParentObject);
                takeDamage.SetParameter("Message", label);
                result.DamageDispatched = true;
                result.EventContinued = target.FireEvent(takeDamage);
                result.HitPointLossObserved = target.hitpoints < hitPointsBefore;
            }
            finally
            {
                MutationMeddley_BonusDamageDispatchDepth -= 1;
            }

            MutationMeddley_TraceDamageProc(
                context,
                "targetResolved=true, amount="
                    + amount
                    + ", dispatched="
                    + result.DamageDispatched
                    + ", eventContinued="
                    + result.EventContinued
                    + ", hpLossObserved="
                    + result.HitPointLossObserved
                    + ", recursionSuppressed=false"
            );
            return result;
        }

        protected int MutationMeddley_ConsumeStateInt(string key, int amount = 1)
        {
            if (MutationMeddley_CurrentDamageEventContext == MutationMeddley_DamageEventContext.OutgoingNonMelee)
            {
                MutationMeddley_TraceDamageProc(
                    "contact.consume",
                    "preserved state '" + key + "' because outgoing damage was not adjacent melee contact"
                );
                return 0;
            }

            if (amount <= 0)
            {
                return 0;
            }

            int current = MutationMeddley_GetStateInt(key, 0);
            if (current <= 0)
            {
                return 0;
            }

            int consumed = Math.Min(current, amount);
            MutationMeddley_SetStateInt(key, current - consumed);
            return consumed;
        }

        protected string MutationMeddley_GetStateValue(string key)
        {
            Dictionary<string, string> metadata = MutationMeddley_GetStateMetadata();
            string value;
            if (metadata.TryGetValue(key, out value))
            {
                return value;
            }

            return "";
        }

        protected int MutationMeddley_GetStateInt(string key, int defaultValue = 0)
        {
            int value;
            if (int.TryParse(MutationMeddley_GetStateValue(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return defaultValue;
        }

        protected void MutationMeddley_SetStateValue(string key, string value)
        {
            Dictionary<string, string> metadata = MutationMeddley_GetStateMetadata();
            if (string.IsNullOrEmpty(value))
            {
                metadata.Remove(key);
            }
            else
            {
                metadata[key] = value;
            }

            MutationMeddley_SetStateEnvelope(
                MutationMeddley_GetSelectedEvolutionIds(),
                metadata
            );
        }

        protected void MutationMeddley_SetStateInt(string key, int value)
        {
            MutationMeddley_SetStateValue(
                key,
                value.ToString(CultureInfo.InvariantCulture)
            );
        }

        private void MutationMeddley_AddEvolutionAbility()
        {
            if (MutationMeddley_EvolveAbilityID != Guid.Empty)
            {
                return;
            }

            MutationMeddley_EvolveAbilityID = AddMyActivatedAbility(
                Name: "Evolve " + MutationMeddley_EvolutionDisplayName,
                Command: MutationMeddley_GetEvolutionCommand(),
                Class: "Physical Mutation",
                Description: "Choose an available Mutation Meddley evolution. New tiers unlock at mutation rank milestones."
            );
        }

        protected List<string> MutationMeddley_GetSelectedEvolutionIds()
        {
            string encodedIds = MutationMeddley_GetEvolutionSegment();
            List<string> selected = new List<string>();
            if (string.IsNullOrEmpty(encodedIds))
            {
                return selected;
            }

            string[] ids = encodedIds.Split(
                new char[] { ';' },
                StringSplitOptions.RemoveEmptyEntries
            );

            for (int i = 0; i < ids.Length; i++)
            {
                selected.Add(ids[i]);
            }

            return selected;
        }

        protected void MutationMeddley_SetSelectedEvolutionIds(List<string> selected)
        {
            MutationMeddley_SetStateEnvelope(selected, MutationMeddley_GetStateMetadata());
        }

        protected bool MutationMeddley_HasSelectionAtTier(int tier)
        {
            List<MutationMeddley_EvolutionChoice> choices = MutationMeddley_GetEvolutionChoices();
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].Tier == tier && MutationMeddley_HasEvolution(choices[i].Id))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool MutationMeddley_HasUnspentTier(int tier)
        {
            return !MutationMeddley_HasSelectionAtTier(tier);
        }

        protected bool MutationMeddley_IsHiddenChoiceUnlocked(string key)
        {
            return MutationMeddley_GetStateInt(key, 0) > 0;
        }

        protected void MutationMeddley_UnlockHiddenChoice(string key)
        {
            MutationMeddley_SetStateInt(key, 1);
        }

        protected int MutationMeddley_AdvanceHiddenProgress(string key, int amount, int maxValue = int.MaxValue)
        {
            int progress = MutationMeddley_GetStateInt(key, 0);
            progress = Math.Min(progress + amount, maxValue);
            MutationMeddley_SetStateInt(key, progress);
            return progress;
        }

        protected bool MutationMeddley_IsEvolutionAvailable(MutationMeddley_EvolutionChoice choice)
        {
            if (Level < choice.RequiredLevel)
            {
                return false;
            }

            if (MutationMeddley_HasEvolution(choice.Id))
            {
                return false;
            }

            if (MutationMeddley_HasSelectionAtTier(choice.Tier))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(choice.PrerequisiteId)
                && !MutationMeddley_HasEvolution(choice.PrerequisiteId))
            {
                return false;
            }

            if (!MutationMeddley_IsChoiceUnlocked(choice))
            {
                return false;
            }

            return true;
        }

        protected List<MutationMeddley_EvolutionChoice> MutationMeddley_GetAvailableEvolutions()
        {
            List<MutationMeddley_EvolutionChoice> result = new List<MutationMeddley_EvolutionChoice>();
            List<MutationMeddley_EvolutionChoice> choices = MutationMeddley_GetEvolutionChoices();

            for (int i = 0; i < choices.Count; i++)
            {
                if (MutationMeddley_IsEvolutionAvailable(choices[i]))
                {
                    result.Add(choices[i]);
                }
            }

            return result;
        }

        private void MutationMeddley_ShowEvolutionPicker()
        {
            if (!MutationMeddley_IsFunctionallyActive())
            {
                Popup.Show(MutationMeddley_GetInactiveReason());
                return;
            }

            List<MutationMeddley_EvolutionChoice> available = MutationMeddley_GetAvailableEvolutions();
            if (available.Count == 0)
            {
                Popup.Show(
                    "No evolution is currently available for "
                    + MutationMeddley_EvolutionDisplayName
                    + ".\n\n"
                    + MutationMeddley_GetEvolutionSummary()
                );
                return;
            }

            string[] options = new string[available.Count];
            char[] hotkeys = new char[available.Count];
            for (int i = 0; i < available.Count; i++)
            {
                StringBuilder option = new StringBuilder();
                if (available[i].IsUnusual)
                {
                    option.Append("UNUSUAL ADAPTATION");
                    option.Append("\n");
                }

                option.Append(available[i].Name);
                option.Append("\n");
                option.Append(available[i].Description);

                if (!string.IsNullOrEmpty(available[i].DetailText))
                {
                    option.Append("\n");
                    option.Append(available[i].DetailText);
                }

                options[i] = option.ToString();
                hotkeys[i] = (char)('A' + i);
            }

            int selection = Popup.ShowOptionList(
                "Evolve: " + MutationMeddley_EvolutionDisplayName + " - rank " + Level,
                Options: options,
                Hotkeys: hotkeys,
                AllowEscape: true
            );

            if (selection < 0 || selection >= available.Count)
            {
                return;
            }

            MutationMeddley_EvolutionChoice chosen = available[selection];
            MutationMeddley_SelectEvolution(chosen);
        }

        private void MutationMeddley_SelectEvolution(MutationMeddley_EvolutionChoice choice)
        {
            if (!MutationMeddley_IsEvolutionAvailable(choice))
            {
                Popup.ShowFail("That evolution is not currently available.");
                return;
            }

            if (string.IsNullOrEmpty(MutationMeddley_EvolutionState))
            {
                MutationMeddley_SetSelectedEvolutionIds(
                    new List<string> { choice.Id }
                );
            }
            else
            {
                List<string> selected = MutationMeddley_GetSelectedEvolutionIds();
                selected.Add(choice.Id);
                MutationMeddley_SetSelectedEvolutionIds(selected);
            }

            MutationMeddley_OnEvolutionChosen(choice);
            MutationMeddley_OnEvolutionStateChanged();
            Popup.Show(
                MutationMeddley_EvolutionDisplayName
                + " evolved: "
                + choice.Name
                + "\n\n"
                + choice.Description
            );
        }

        private string MutationMeddley_GetEvolutionSegment()
        {
            if (string.IsNullOrEmpty(MutationMeddley_EvolutionState))
            {
                return "";
            }

            int metadataIndex = MutationMeddley_EvolutionState.IndexOf('|');
            if (metadataIndex < 0)
            {
                return MutationMeddley_EvolutionState;
            }

            return MutationMeddley_EvolutionState.Substring(0, metadataIndex);
        }

        private Dictionary<string, string> MutationMeddley_GetStateMetadata()
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(MutationMeddley_EvolutionState))
            {
                return metadata;
            }

            string[] segments = MutationMeddley_EvolutionState.Split('|');
            for (int i = 1; i < segments.Length; i++)
            {
                if (string.IsNullOrEmpty(segments[i]))
                {
                    continue;
                }

                int separatorIndex = segments[i].IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = segments[i].Substring(0, separatorIndex);
                string value = segments[i].Substring(separatorIndex + 1);
                metadata[key] = value;
            }

            return metadata;
        }

        private void MutationMeddley_SetStateEnvelope(List<string> selected, Dictionary<string, string> metadata)
        {
            if (metadata == null)
            {
                metadata = new Dictionary<string, string>();
            }

            if ((selected != null && selected.Count > 0) || metadata.Count > 0)
            {
                metadata[MutationMeddley_StateVersionKey] = MutationMeddley_CurrentStateVersion;
            }

            StringBuilder state = new StringBuilder();
            if (selected != null && selected.Count > 0)
            {
                for (int i = 0; i < selected.Count; i++)
                {
                    if (i > 0)
                    {
                        state.Append(';');
                    }

                    state.Append(selected[i]);
                }
            }

            if (metadata != null && metadata.Count > 0)
            {
                List<string> keys = new List<string>(metadata.Keys);
                keys.Sort(StringComparer.Ordinal);

                for (int i = 0; i < keys.Count; i++)
                {
                    if (string.IsNullOrEmpty(metadata[keys[i]]))
                    {
                        continue;
                    }

                    state.Append('|');
                    state.Append(keys[i]);
                    state.Append('=');
                    state.Append(metadata[keys[i]]);
                }
            }

            MutationMeddley_EvolutionState = state.ToString();
        }
    }
}
