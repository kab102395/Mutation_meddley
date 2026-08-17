from pathlib import Path

path = Path('Code/MutationMeddley_AdaptiveMutationBase.cs')
text = path.read_text(encoding='utf-8')

replacements = []

replacements.append((
'''            if (!string.IsNullOrEmpty(primaryActionCommand) && E.ID == primaryActionCommand)
            {
                global::XRL.World.Parts.MutationMeddley_BiologySupport support =
                    MutationMeddley_EnsureBiologySupport(ParentObject);
                if (support != null)
                {
                    support.MutationMeddley_InvokePrimaryAction(this);
                }
                return false;
            }
''',
'''            if (!string.IsNullOrEmpty(primaryActionCommand) && E.ID == primaryActionCommand)
            {
                // The mutation owns and executes its own command path. Biology is an
                // optional inspector/telemetry surface, never a gameplay dependency.
                global::XRL.World.Parts.MutationMeddley_BiologySupport support =
                    MutationMeddley_EnsureBiologySupport(ParentObject);
                string signature = global::XRL.World.Parts.MutationMeddley_PrimaryActionCatalog.GetSignature(this);
                string actionName = global::XRL.World.Parts.MutationMeddley_PrimaryActionCatalog.GetName(this);
                string actionDescription = support != null
                    ? support.MutationMeddley_GetPrimaryActionDescriptionForMutation(this)
                    : global::XRL.World.Parts.MutationMeddley_PrimaryActionCatalog.GetDescription(this);

                global::XRL.World.Parts.MutationMeddley_PrimaryActionService.MutationMeddley_TryUse(
                    this,
                    ParentObject,
                    signature,
                    actionName,
                    actionDescription);
                return false;
            }
'''))

replacements.append((
'''        private global::XRL.World.Parts.MutationMeddley_BiologySupport MutationMeddley_EnsureBiologySupport(GameObject Object)
        {
            if (Object == null || !Object.IsPlayer())
            {
                return null;
            }

            return global::XRL.World.Parts.MutationMeddley_BiologySupport.MutationMeddley_EnsureInstalled(Object);
        }
''',
'''        private global::XRL.World.Parts.MutationMeddley_BiologySupport MutationMeddley_EnsureBiologySupport(GameObject Object)
        {
            if (Object == null)
            {
                return null;
            }

            // PlayerMutator may install Biology before IsPlayer() becomes observable
            // during character construction. Existing support is therefore a trusted
            // player-UI marker and must be accepted before the ordinary NPC guard.
            global::XRL.World.Parts.MutationMeddley_BiologySupport existing =
                Object.GetPart("MutationMeddley_BiologySupport")
                    as global::XRL.World.Parts.MutationMeddley_BiologySupport;
            if (existing != null)
            {
                return existing;
            }

            if (!Object.IsPlayer())
            {
                return null;
            }

            return global::XRL.World.Parts.MutationMeddley_BiologySupport.MutationMeddley_EnsureInstalled(Object);
        }
'''))

replacements.append((
'''        private void MutationMeddley_SyncPrimaryActionAbility()
        {
            if (ParentObject == null || !ParentObject.IsPlayer())
            {
                return;
            }

            global::XRL.World.Parts.MutationMeddley_BiologySupport support =
                MutationMeddley_EnsureBiologySupport(ParentObject);
            if (support == null)
            {
                return;
            }

            string command = MutationMeddley_GetPrimaryActionCommand();
            string desiredSignature = support.MutationMeddley_GetPrimaryActionSignatureForMutation(this);
            string desiredName = support.MutationMeddley_GetPrimaryActionNameForMutation(this);
            string desiredDescription = support.MutationMeddley_GetPrimaryActionDescriptionForMutation(this);
''',
'''        private void MutationMeddley_SyncPrimaryActionAbility()
        {
            if (ParentObject == null)
            {
                return;
            }

            // Requiring the player-only Biology marker instead of re-checking
            // IsPlayer() closes early-new-game ordering gaps without giving NPC
            // mutations player UI abilities.
            global::XRL.World.Parts.MutationMeddley_BiologySupport support =
                MutationMeddley_EnsureBiologySupport(ParentObject);
            if (support == null)
            {
                return;
            }

            string command = MutationMeddley_GetPrimaryActionCommand();
            string desiredSignature = global::XRL.World.Parts.MutationMeddley_PrimaryActionCatalog.GetSignature(this);
            string desiredName = global::XRL.World.Parts.MutationMeddley_PrimaryActionCatalog.GetName(this);
            string desiredDescription = support.MutationMeddley_GetPrimaryActionDescriptionForMutation(this);
'''))

for old, new in replacements:
    count = text.count(old)
    if count != 1:
        raise SystemExit(f'Expected exactly one source match, found {count}. Refusing partial transform.')
    text = text.replace(old, new, 1)

path.write_text(text, encoding='utf-8')
