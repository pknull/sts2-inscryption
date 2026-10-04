# MODLOG: Inscryption character for Slay the Spire 2

Goal (Keeper, 2026-10-04): put Inscryption cards and effects into Slay the Spire 2, as a **new playable
character** (chosen over a shared card set or a Necrobinder card set).

## Versions and paths (verified 2026-10-04)

- Game: Slay the Spire 2 v0.107.1 (commit 59260271, 2026-06-18), Steam app 2868840, native Linux build.
  - Install: `~/.steam/debian-installation/steamapps/common/Slay the Spire 2`
  - Engine: Godot 4.5.1 (pck format 3) + .NET (`data_sts2_linuxbsd_x86_64/sts2.dll`). No anti-cheat.
  - User data: `~/.local/share/SlayTheSpire2/` (saves under `steam/<steamid>/`, logs `logs/godot.log`).
  - Modded runs use separate save files; `-nomods` launches vanilla.
- Built-in mod loader (`MegaCrit.Sts2.Core.Modding`): loads `<install>/mods/<Id>/{Id.json, Id.dll, Id.pck}`
  and Workshop mods; enable under Settings -> Mod Settings.
- BaseLib v3.4.7 (github.com/Alchyr/BaseLib-StS2, MIT), `min_game_version` 0.107.1. Installed from the GitHub
  release zip (sha256 d28c649a...e4bd8, matches the release digest) into `<install>/mods/BaseLib/`.
- Toolchain (user-local):
  - .NET SDK 9.0.318 in `~/.dotnet` (use `DOTNET_ROOT=$HOME/.dotnet PATH=$HOME/.dotnet:$HOME/.dotnet/tools:$PATH`).
    The apt `/bin/dotnet` is a 7.0 runtime only; do not use it.
  - Godot 4.5.1 .NET: `~/opt/Godot_v4.5.1-stable_mono_linux_x86_64/` (SHA-512 verified). For .pck export.
    `~/bin/godot` (4.5 non-.NET, dotfiles symlink) is NOT used.
  - ilspycmd 9.1.0.7988 (needs `DOTNET_ROLL_FORWARD=Major`; 10.x/11.x fail to install on SDK 9).
  - Templates: Alchyr.Sts2.Templates 2.5.2 (`dotnet new alchyrsts2charmod`).
- Decompiled game: `~/sts2-decomp/sts2/` (3424 files). Never copy into this repo.
- Save backup: `~/.universal-modder/backups/sts2-saves/20261004-130325.zip` (68 files).
  Restore: `bin/um backup restore sts2-saves --snapshot <zip> --yes` from the universal-modder clone.

## Project

- Generated with `dotnet new alchyrsts2charmod --ModAuthor PKnull -n Inscryption`.
- `Directory.Build.props` (gitignored, machine-local): `GodotPath` = the Godot 4.5.1 .NET binary above;
  `Sts2Path` set explicitly because the template's Linux discovery looks in `~/.local/share/Steam`.
- Build: `dotnet build` copies dll/json/pdb to `<install>/mods/Inscryption/`. Text and art changes need
  `dotnet publish` (exports the .pck through Godot).
- Baseline build of the untouched template: fails only on missing character localization (STS001), which
  the wiki says is the expected state.

## Engine facts (from the decompile)

- Pets: `PlayerCombatState.Pets` is a list; several pets, even of one type, are supported
  (`PlayerCmd.AddPet<T>`). `NCombatRoom.PositionPlayersAndPets` spreads pets around the player's feet.
- Pets do not act on their own: `CombatManager.ExecuteEnemyTurn` only runs `TakeTurn` for enemies. Osty's
  move state machine is a single NOTHING_MOVE. Creature attacks must come from a hook.
- Damage interception: `DieForYouPower.ModifyUnblockedDamageTarget` redirects unblocked powered attacks
  from the owner to the pet (Osty). `Hook.ModifyUnblockedDamageTarget` chains all models in order, so the
  first redirect wins. `dealer` is passed in, which allows lane matching.

## Decisions (logged choices; the Keeper can override)

- Mod id `Inscryption`, character class `Inscryption`, placeholder display name "The Challenger".
- Inscryption content comes from public wiki data (Inscryption is not installed here); art will be
  fal-generated lookalikes, never copied game files.
- Vertical slice (milestone 1): character selectable; starting deck of a few creature cards with
  placeholder art; blood sacrifice summons a creature that fights; verified in a real combat from
  godot.log plus a screenshot.

### Design mappings in the slice (Inscryption -> StS2)

- Creature card -> Skill, 0 energy, `TargetType.Self`. Blood is the only cost. Playing it sacrifices
  `Blood` creatures (weakest power first; Inscryption lets you choose, a choice UI is later work) and
  summons a pet. The card then goes to discard and cycles like any StS2 card.
- Stats x3 (`Balance.StatScale`): Stoat 1/3 -> 3 dmg / 9 HP, Bullfrog 1/2 -> 3/6, Wolf 3/2 -> 9/6,
  Squirrel 0/1 -> 0/3. Max 4 creatures (4 lanes).
- Lanes: creature lane = index among living creatures in summon order; enemy lane = index among living
  enemies. Lanes shift when a creature dies (Inscryption's slots are fixed).
- Blocking: `CreaturePower.ModifyUnblockedDamageTarget` sends an enemy's powered attack to the creature in
  that enemy's lane; empty lane -> the player takes it (the scale).
- Attacking: `CreaturePower.BeforeSideTurnEnd` (player side) deals `ScaledPower` via
  `CreatureCmd.Damage(ctx, target, amount, ValueProp.Move, dealer: creature)` to the enemy in its lane;
  empty lane -> first living enemy (stands in for damage to the scale).
- Side deck: starting relic `SideDeck` adds a Squirrel (Token, Exhaust) to hand each turn, 10 per combat.
- Bones: `SideDeck.AfterDeath` gives `BonesPower` +1 when one of your creatures dies. Nothing spends
  Bones yet.
- Starting deck: Stoat x4, Bullfrog x3, Wolf x3 (Basic). StartingHp 70 (template default).
- Placeholder art: creature PNGs drawn with Pillow (180x240, name + scaled stats); card portraits, relic
  and power icons fall back to the template's placeholders.

## Build log

- 2026-10-04: slice compiles (0 CS errors). STS001 analyzer errors cleared by writing characters,
  ancients (Architect minimum: 0-0r.char/.next, 0-1r.ancient, 0-attack), cards, monsters, powers, relics.
- Gotcha: `dotnet publish` export failed (Godot exit -1, "C# file but no solution file exists") because
  the CLI template made no `.sln`. Fix: `dotnet new sln -n Inscryption --format sln` +
  `dotnet sln add Inscryption.csproj`. Publish is now clean; pck 204 KB holds images and localization.
- Gotcha (toolkit): `um scan` ranks ModTheSpire/BaseMod (StS1's Java loader) as route 1 for StS2.

## Art direction (2026-10-04)

- Keeper first chose an Inscryption look, then switched after the probes to **Slay the Spire 2's cartoony
  look**: hand-painted 2D, bold dark outlines, chunky shapes, saturated painterly colour. Described in
  the prompt only; no game art is uploaded as a reference.
- Pipeline per creature: one `um fal image` (nano-banana-2, 4:3, 1K, side view facing right, simple painted
  background) -> card portrait; `um fal rmbg` (birefnet v2) -> transparent board sprite. Manifest:
  `assets/gen/fal_manifest.jsonl`.
- Probes: `stoat_probe.png` (woodcut, rejected), `stoat_px96.png` (pixelated, rejected),
  `stoat_sts_probe.png` (painterly with black outlines: Keeper: "still a little too detailed").
- Accepted: `stoat_flat_probe.png`, flat SVG-like style matching the game's monsters (flat fills, 1-2 flat
  shadow shapes, one flat highlight, no outlines, no texture, small simple eye, plain flat dark olive
  background). Keeper: "Perfect actually."
- Squirrel/Bullfrog/Wolf made with `um fal edit --ref stoat_flat_probe.png` (same style/background), then
  `um fal rmbg`. `tools/make_art.py` writes card portraits (1000x760, 250x190) and board sprites (trimmed,
  heights 90/110/120/180). Spend: 7 nano-banana-2 images = $0.56, plus 5 birefnet cutouts (fractions of a cent).
- Gotcha: the export preset's `all_resources` packed `assets/gen/` (pck 10 MB). Fixed with `.gdignore` in
  `assets/` and `tools/` plus `exclude_filter`; pck back to 2.2 MB with 0 `res://assets/` entries.

## In-game verification (2026-10-04, seed 9WZY0PJTUK, encounter NIBBITS_WEAK)

Method: Keeper clicks; agent reads godot.log and takes X11 window screenshots (`import -window`, no input).

- Mods load: `RUNNING MODDED! --- Loaded 2 mods`; BaseLib 282 patches, 0 failed; Inscryption init ok.
- Model registration: ModelIdSerializationCache 1622 -> 1637 entries (+15 = our models).
- Turn 1: Squirrel added to hand by SideDeck; creature cards correctly unplayable with an empty board.
- Squirrel -> Stoat: log `playing card INSCRYPTION-SQUIRREL`, `INSCRYPTION-STOAT`; Squirrel sacrificed and
  exhausted; Stoat 3/9 on board; Bones 1; other Stoats become playable, Wolves stay unplayable.
- End turn: Stoat hit the Nibbit 46 -> 43; Nibbit's 12 killed the Stoat; Bones 2; Squirrel on turn 2.
- FAIL: player 70 -> 67. Cause (confirmed in `CreatureCmd.Damage`, decompile line ~283): after a
  `ModifyUnblockedDamageTarget` redirect, the redirected target's `OverkillDamage` is applied to the original
  target. Fix: `CreaturePower.ModifyHpLostAfterOsty` caps the creature's HP loss at its `CurrentHp`, so
  overkill is 0 (Inscryption: a blocker soaks the whole hit).
- Re-verify (same seed and encounter, new build): Stoat in a lane row in front of the player with a 9/9 HP
  bar; new card portraits; Nibbit 46 -> 43; Nibbit's 12 killed the Stoat; Bones 2; player **70/70** (was
  67). No exceptions in godot.log. **Milestone 1 (vertical slice) done.**

### Gotchas found in game

1. Mod-loading popup: choosing "load mods" saves `PlayerAgreedToModLoading` and calls `NGame.Quit()`
   (`NConfirmModLoadingPopup.OnYesButtonPressed`); it looks like a crash. Relaunch.
2. Modded saves are separate (`steam/<id>/modded/profileN`): a fresh modded profile has only Ironclad plus
   modded characters unlocked (Silent/Regent/Necrobinder/Defect need their Timeline epochs).
3. The template's character-select icon is a "?", indistinguishable from the random-character button;
   the random button itself is hidden on a fresh profile. Replace the icon.
4. Pets render overlapping the player's body and show no HP bar. Cause: `NCombatRoom.AddCreature`
   spreads non-Osty pets across the owner's width at foot level and calls `ToggleIsInteractable(false)`,
   which hides `_stateDisplay` (the HP bar). Fix: `BoardLayout` postfixes on `AddCreature` and
   `RemoveCreatureNode` place our creatures in a lane row in front of the player and re-enable them.
   Verified in game.
5. Do not let MSBuild overwrite `mods/Inscryption/Inscryption.dll` while the game runs: build with
   `-p:ModsPath=<staging>/` and copy after the game exits.

## Next

- Verify in game: mod list shows Inscryption + BaseLib; character selectable; Squirrel arrives turn 1;
  Squirrel -> sacrifice for Stoat; Stoat attacks at turn end; enemy in lane 0 hits the Stoat;
  Bones +1 on sacrifice. Evidence: `~/.local/share/SlayTheSpire2/logs/godot.log` + screenshots.
