using System;
using System.Collections.Generic;
using XRL;
using XRL.Core;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    [Serializable]
    public partial class MutationMeddley_BiologySupport : IPart
    {
        private const string BiologyCommand = "MutationMeddley_OpenBiology";
        internal const string CarapaceCommand = "MutationMeddley_Action_Carapace";
        internal const string CrystalCommand = "MutationMeddley_Action_LivingCrystal";
        internal const string BrineCommand = "MutationMeddley_Action_Brineborn";
        internal const string AshCommand = "MutationMeddley_Action_AshMetabolism";
        internal const string ColonyCommand = "MutationMeddley_Action_WalkingColony";

        // Keep every public field introduced by the first 0.7.1 build so a save made
        // with that build keeps the same serialized field layout. Mutation-specific
        // action ownership has moved back to the mutations; these five action fields
        // are migration-only and are cleaned up if they point to old abilities.
        public Guid MutationMeddley_BiologyAbilityID = Guid.Empty;
        public Guid MutationMeddley_CarapaceActionAbilityID = Guid.Empty;
        public Guid MutationMeddley_CrystalActionAbilityID = Guid.Empty;
        public Guid MutationMeddley_BrineActionAbilityID = Guid.Empty;
        public Guid MutationMeddley_AshActionAbilityID = Guid.Empty;
        public Guid MutationMeddley_ColonyActionAbilityID = Guid.Empty;

        public string MutationMeddley_CarapaceActionSignature = "";
        public string MutationMeddley_CrystalActionSignature = "";
        public string MutationMeddley_BrineActionSignature = "";
        public string MutationMeddley_AshActionSignature = "";
        public string MutationMeddley_ColonyActionSignature = "";

        internal static MutationMeddley_BiologySupport MutationMeddley_EnsureInstalled(GameObject Object)
        {
            if (Object == null || !Object.IsPlayer())
            {
                return null;
            }

            Object.RequirePart<ActivatedAbilities>();
            Object.RequirePart<MutationMeddley_BiologySupport>();

            MutationMeddley_BiologySupport support =
                Object.GetPart("MutationMeddley_BiologySupport") as MutationMeddley_BiologySupport;
            if (support != null)
            {
                support.MutationMeddley_RefreshAbilitySurface();
            }

            return support;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, BiologyCommand);
            Object.RegisterPartEvent(this, CarapaceCommand);
            Object.RegisterPartEvent(this, CrystalCommand);
            Object.RegisterPartEvent(this, BrineCommand);
            Object.RegisterPartEvent(this, AshCommand);
            Object.RegisterPartEvent(this, ColonyCommand);
            Object.RegisterPartEvent(this, "EndTurn");
            base.Register(Object);

            MutationMeddley_EnsureBiologyAbility();
            MutationMeddley_RefreshAbilitySurface();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == BiologyCommand)
            {
                MutationMeddley_RefreshAbilitySurface();
                MutationMeddley_ShowBiology();
                return false;
            }

            if (E.ID == CarapaceCommand)
            {
                MutationMeddley_UseCarapaceAction();
                return false;
            }

            if (E.ID == CrystalCommand)
            {
                MutationMeddley_UseCrystalAction();
                return false;
            }

            if (E.ID == BrineCommand)
            {
                MutationMeddley_UseBrineAction();
                return false;
            }

            if (E.ID == AshCommand)
            {
                MutationMeddley_UseAshAction();
                return false;
            }

            if (E.ID == ColonyCommand)
            {
                MutationMeddley_UseColonyAction();
                return false;
            }

            if (E.ID == "EndTurn")
            {
                // Biology owns only the aggregate inspector now. Mutation classes
                // synchronize their own primary action and stance abilities.
                MutationMeddley_RefreshAbilitySurface();
            }

            return base.FireEvent(E);
        }

        private MutationMeddley_AdaptiveMutationBase MutationMeddley_GetMutation(string name)
        {
            if (ParentObject == null)
            {
                return null;
            }

            global::XRL.World.Parts.Mutations mutations =
                ParentObject.GetPart("Mutations") as global::XRL.World.Parts.Mutations;
            if (mutations == null)
            {
                return null;
            }

            return mutations.GetMutationByName(name) as MutationMeddley_AdaptiveMutationBase;
        }

        private List<MutationMeddley_AdaptiveMutationBase> MutationMeddley_GetOwnedMutations()
        {
            List<MutationMeddley_AdaptiveMutationBase> result = new List<MutationMeddley_AdaptiveMutationBase>();
            string[] names =
            {
                "Carapace Evolution",
                "Living Crystal",
                "Brineborn",
                "Ash Metabolism",
                "Walking Colony"
            };

            for (int i = 0; i < names.Length; i++)
            {
                MutationMeddley_AdaptiveMutationBase mutation = MutationMeddley_GetMutation(names[i]);
                if (mutation != null)
                {
                    result.Add(mutation);
                }
            }

            return result;
        }

        private ActivatedAbilities MutationMeddley_GetActivatedAbilities()
        {
            if (ParentObject == null)
            {
                return null;
            }

            ActivatedAbilities abilities =
                ParentObject.GetPart("ActivatedAbilities") as ActivatedAbilities;
            if (abilities == null)
            {
                ParentObject.RequirePart<ActivatedAbilities>();
                abilities = ParentObject.GetPart("ActivatedAbilities") as ActivatedAbilities;
            }

            return abilities;
        }

        private bool MutationMeddley_AbilityExists(Guid abilityID)
        {
            ActivatedAbilities abilities = MutationMeddley_GetActivatedAbilities();
            return abilityID != Guid.Empty
                && abilities != null
                && abilities.AbilityByGuid != null
                && abilities.AbilityByGuid.ContainsKey(abilityID);
        }

        private Guid MutationMeddley_AddAbility(string name, string command, string description)
        {
            ActivatedAbilities abilities = MutationMeddley_GetActivatedAbilities();
            if (abilities == null)
            {
                return Guid.Empty;
            }

            return abilities.AddAbility(
                Name: name,
                Command: command,
                Class: "Physical Mutation",
                Description: description);
        }

        private void MutationMeddley_RefreshAbilitySurface()
        {
            // New-game PlayerMutator can run before mutations are populated. Biology
            // therefore stays installed even if the first scan is empty.
            MutationMeddley_EnsureBiologyAbility();

            // Clean up action buttons created by the first 0.7.1 implementation.
            // Their GUID fields remain serialized for compatibility, but mutations now
            // own their action buttons and persist those GUIDs in their state envelope.
            MutationMeddley_RemoveLegacyActionAbilities();

            List<MutationMeddley_AdaptiveMutationBase> owned = MutationMeddley_GetOwnedMutations();
            if (owned.Count == 0)
            {
                MutationMeddley_UpdateAbilityDescription(
                    MutationMeddley_BiologyAbilityID,
                    "Free inspection. No Mutation Meddley mutation is currently present on this body. Biology will resynchronize automatically when one becomes available.");
                return;
            }

            MutationMeddley_UpdateAbilityDescription(
                MutationMeddley_BiologyAbilityID,
                MutationMeddley_GetBiologyAbilityDescription());
        }

        private void MutationMeddley_EnsureBiologyAbility()
        {
            if (MutationMeddley_AbilityExists(MutationMeddley_BiologyAbilityID))
            {
                return;
            }

            MutationMeddley_BiologyAbilityID = MutationMeddley_AddAbility(
                "Mutation Meddley Biology",
                BiologyCommand,
                MutationMeddley_GetBiologyAbilityDescription());
        }

        private void MutationMeddley_UpdateAbilityDescription(Guid abilityID, string description)
        {
            ActivatedAbilities abilities = MutationMeddley_GetActivatedAbilities();
            if (abilityID == Guid.Empty
                || abilities == null
                || abilities.AbilityByGuid == null
                || !abilities.AbilityByGuid.ContainsKey(abilityID))
            {
                return;
            }

            abilities.AbilityByGuid[abilityID].Description = description;
        }

        private void MutationMeddley_RemoveAbility(ref Guid abilityID)
        {
            ActivatedAbilities abilities = MutationMeddley_GetActivatedAbilities();
            if (abilityID != Guid.Empty
                && abilities != null
                && abilities.AbilityByGuid != null
                && abilities.AbilityByGuid.ContainsKey(abilityID))
            {
                abilities.RemoveAbility(abilityID);
            }
            abilityID = Guid.Empty;
        }

        private void MutationMeddley_RemoveLegacyActionAbilities()
        {
            MutationMeddley_RemoveAbility(ref MutationMeddley_CarapaceActionAbilityID);
            MutationMeddley_RemoveAbility(ref MutationMeddley_CrystalActionAbilityID);
            MutationMeddley_RemoveAbility(ref MutationMeddley_BrineActionAbilityID);
            MutationMeddley_RemoveAbility(ref MutationMeddley_AshActionAbilityID);
            MutationMeddley_RemoveAbility(ref MutationMeddley_ColonyActionAbilityID);

            MutationMeddley_CarapaceActionSignature = "";
            MutationMeddley_CrystalActionSignature = "";
            MutationMeddley_BrineActionSignature = "";
            MutationMeddley_AshActionSignature = "";
            MutationMeddley_ColonyActionSignature = "";
        }

        internal string MutationMeddley_GetPrimaryActionCommandForMutation(MutationMeddley_AdaptiveMutationBase mutation)
        {
            if (mutation == null)
            {
                return "";
            }

            switch (mutation.MutationMeddley_EvolutionDisplayName)
            {
                case "Carapace Evolution": return CarapaceCommand;
                case "Living Crystal": return CrystalCommand;
                case "Brineborn": return BrineCommand;
                case "Ash Metabolism": return AshCommand;
                case "Walking Colony": return ColonyCommand;
                default: return "";
            }
        }

        internal string MutationMeddley_GetPrimaryActionSignatureForMutation(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_GetActionSignature(mutation);
        }

        internal string MutationMeddley_GetPrimaryActionNameForMutation(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_GetActionName(mutation);
        }

        internal string MutationMeddley_GetPrimaryActionDescriptionForMutation(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_GetActionDescription(mutation);
        }
    }
}
