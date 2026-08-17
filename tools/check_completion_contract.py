#!/usr/bin/env python3
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
CODE = ROOT / "Code"


def read(name: str) -> str:
    path = CODE / name
    if not path.exists():
        raise SystemExit(f"completion contract FAILED: missing {path.relative_to(ROOT)}")
    return path.read_text(encoding="utf-8")


def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"completion contract FAILED: {message}")


adaptive = read("MutationMeddley_AdaptiveMutationBase.cs")
service = read("MutationMeddley_PrimaryActionService.cs")
catalog = read("MutationMeddley_PrimaryActionCatalog.cs")
actions = read("MutationMeddley_BiologySupport.ActionsA.cs")
core = read("MutationMeddley_BiologySupport.CoreA.cs")
installer = read("MutationMeddley_BiologySupport.Install.cs")
state_access = read("MutationMeddley_StateEnvelopeAccess.cs")

require("class MutationMeddley_PrimaryActionCatalog" in catalog,
        "primary action catalog is missing")
require("GetSignature(" in catalog and "GetName(" in catalog and "GetDescription(" in catalog,
        "primary action catalog is incomplete")

require("MutationMeddley_StateEnvelopeAccess.GetInt" in service,
        "primary action service must read mutation state through StateEnvelopeAccess")
require("MutationMeddley_StateEnvelopeAccess.SetInt" in service,
        "primary action service must write mutation state through StateEnvelopeAccess")
require("MutationMeddley_BiologySupport support," not in service,
        "primary action transaction must not require BiologySupport")
require("support.MutationMeddley_GetStateInt" not in service,
        "primary action service still reads gameplay state through BiologySupport")
require("support.MutationMeddley_SetStateInt" not in service,
        "primary action service still writes gameplay state through BiologySupport")
require('owner.UseEnergy(1000, "Physical Mutation")' in service,
        "successful primary actions must keep the normal Qud action cost")

require("MutationMeddley_PrimaryActionService.MutationMeddley_TryUse" in actions,
        "Biology action page must call the shared primary action service")
require("MutationMeddley_TryUse(\n                this," not in actions,
        "Biology is still being passed as a primary action transaction dependency")
require("UseEnergy(" not in actions,
        "Biology UI must not spend action energy itself")

require("MutationMeddley_PrimaryActionService.MutationMeddley_TryUse" in adaptive,
        "mutation command path must execute through the shared primary action service")
require("MutationMeddley_PrimaryActionCatalog.GetSignature(this)" in adaptive,
        "mutation lifecycle must derive action identity from the shared catalog")
require("MutationMeddley_PrimaryActionCatalog.GetName(this)" in adaptive,
        "mutation lifecycle must derive action name from the shared catalog")
require("support.MutationMeddley_InvokePrimaryAction(this)" not in adaptive,
        "mutation command path is still routed through Biology")
require('Object.GetPart("MutationMeddley_BiologySupport")' in adaptive,
        "early-new-game repair must recognize an already-installed Biology player marker")
require("if (Object == null || !Object.IsPlayer())" not in adaptive,
        "mutation-side Biology repair still rejects an early player before checking existing support")
require("MutationMeddley_SyncPrimaryActionAbility();" in adaptive,
        "mutation-owned primary action synchronization is missing")
require("MutationMeddley_SyncModeAbility();" in adaptive,
        "mutation-owned stance synchronization is missing")

require(core.count("RegisterPartEvent(this, BiologyCommand)") == 1,
        "Biology must own exactly one inspector command registration")
for command in ("CarapaceCommand", "CrystalCommand", "BrineCommand", "AshCommand", "ColonyCommand"):
    require(f"RegisterPartEvent(this, {command}" not in core,
            f"Biology must not register mutation action command {command}")

require(installer.count("[PlayerMutator]") == 1,
        "new-game Biology bootstrap must contain exactly one PlayerMutator")
require(installer.count("[CallAfterGameLoadedAttribute]") == 1,
        "loaded-save Biology bootstrap must contain exactly one after-load hook")
require(installer.count("trustedPlayerObject: true") >= 2,
        "new-game and load hooks must use the trusted player-object path")

require("metadata[\"statev\"] = \"1\"" in state_access,
        "state-envelope writes must preserve state version metadata")

print("Completion contract passed:")
print("  mutation command execution: mutation-owned")
print("  primary action transactions: independent of BiologySupport")
print("  primary action metadata: shared catalog")
print("  early-new-game player marker fallback: present")
print("  Biology: aggregate inspector only")
print("  new-game/load bootstrap: present")
print("  state writes: centralized and versioned")
