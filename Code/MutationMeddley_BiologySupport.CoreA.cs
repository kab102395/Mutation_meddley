using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using XRL;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    public partial class MutationMeddley_BiologySupport : IPart
    {
        private const string BiologyCommand = "MutationMeddley_OpenBiology";
        private const string CarapaceCommand = "MutationMeddley_Action_Carapace";
        private const string CrystalCommand = "MutationMeddley_Action_LivingCrystal";
        private const string BrineCommand = "MutationMeddley_Action_Brineborn";
        private const string AshCommand = "MutationMeddley_Action_AshMetabolism";
        private const string ColonyCommand = "MutationMeddley_Action_WalkingColony";

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
            MutationMeddley_RefreshAbilitySurface();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == BiologyCommand)
            {
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
            return ParentObject == null
                ? null
                : ParentObject.GetPart("ActivatedAbilities") as ActivatedAbilities;
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
            List<MutationMeddley_AdaptiveMutationBase> owned = MutationMeddley_GetOwnedMutations();
            if (owned.Count == 0)
            {
                MutationMeddley_RemoveAbility(ref MutationMeddley_BiologyAbilityID);
                MutationMeddley_RemoveActionAbilities();
                return;
            }

            MutationMeddley_EnsureBiologyAbility();
            MutationMeddley_EnsureActionAbility(
                "Carapace Evolution",
                CarapaceCommand,
                ref MutationMeddley_CarapaceActionAbilityID,
                ref MutationMeddley_CarapaceActionSignature);
            MutationMeddley_EnsureActionAbility(
                "Living Crystal",
                CrystalCommand,
                ref MutationMeddley_CrystalActionAbilityID,
                ref MutationMeddley_CrystalActionSignature);
            MutationMeddley_EnsureActionAbility(
                "Brineborn",
                BrineCommand,
                ref MutationMeddley_BrineActionAbilityID,
                ref MutationMeddley_BrineActionSignature);
            MutationMeddley_EnsureActionAbility(
                "Ash Metabolism",
                AshCommand,
                ref MutationMeddley_AshActionAbilityID,
                ref MutationMeddley_AshActionSignature);
            MutationMeddley_EnsureActionAbility(
                "Walking Colony",
                ColonyCommand,
                ref MutationMeddley_ColonyActionAbilityID,
                ref MutationMeddley_ColonyActionSignature);

            MutationMeddley_UpdateAbilityDescription(
                MutationMeddley_BiologyAbilityID,
                MutationMeddley_GetBiologyAbilityDescription());

            MutationMeddley_UpdateActionDescription("Carapace Evolution", MutationMeddley_CarapaceActionAbilityID);
            MutationMeddley_UpdateActionDescription("Living Crystal", MutationMeddley_CrystalActionAbilityID);
            MutationMeddley_UpdateActionDescription("Brineborn", MutationMeddley_BrineActionAbilityID);
            MutationMeddley_UpdateActionDescription("Ash Metabolism", MutationMeddley_AshActionAbilityID);
            MutationMeddley_UpdateActionDescription("Walking Colony", MutationMeddley_ColonyActionAbilityID);
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

        private void MutationMeddley_EnsureActionAbility(
            string mutationName,
            string command,
            ref Guid abilityID,
            ref string signature)
        {
            MutationMeddley_AdaptiveMutationBase mutation = MutationMeddley_GetMutation(mutationName);
            if (mutation == null || !mutation.MutationMeddley_PeekIsFunctionallyActive())
            {
                MutationMeddley_RemoveAbility(ref abilityID);
                signature = "";
                return;
            }

            string desiredSignature = MutationMeddley_GetActionSignature(mutation);
            string desiredName = MutationMeddley_GetActionName(mutation);

            bool missing = !MutationMeddley_AbilityExists(abilityID);
            if (!missing && signature == desiredSignature)
            {
                return;
            }

            if (!missing)
            {
                MutationMeddley_RemoveAbility(ref abilityID);
            }

            abilityID = MutationMeddley_AddAbility(
                desiredName,
                command,
                MutationMeddley_GetActionDescription(mutation));
            signature = desiredSignature;
        }

        private void MutationMeddley_UpdateActionDescription(string mutationName, Guid abilityID)
        {
            if (abilityID == Guid.Empty)
            {
                return;
            }

            MutationMeddley_AdaptiveMutationBase mutation = MutationMeddley_GetMutation(mutationName);
            if (mutation == null)
            {
                return;
            }

            MutationMeddley_UpdateAbilityDescription(abilityID, MutationMeddley_GetActionDescription(mutation));
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

        private void MutationMeddley_RemoveActionAbilities()
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

    }
}
