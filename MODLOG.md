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

- Mod id `Inscryption`, character class `Inscryption`; display name "The Challenger" until it became
  Luke Carder (see Art direction).
- Inscryption content comes from public wiki data (Inscryption is not installed here); art will be
  fal-generated lookalikes, never copied game files.
- Vertical slice (milestone 1): character selectable; starting deck of a few creature cards with
  placeholder art; blood sacrifice summons a creature that fights; verified in a real combat from
  godot.log plus a screenshot.

### Design mappings in the slice (Inscryption -> StS2)

- Creature card -> Skill, 0 energy, `TargetType.Self`. Blood is the only cost. Playing it sacrifices
  `Blood` creatures (the player picks them on a card grid; see Playtest round 2) and
  summons a pet. The card then goes to discard and cycles like any StS2 card.
- Stats: power x3 (`Balance.PowerScale`), health x5 (`Balance.HealthScale`; was a single x3 until the
  Keeper's playtest: StS2 enemies hit harder than Inscryption's, so x3 creatures died in one hit and
  could not hold a lane). Stoat 1/3 -> 3 dmg / 15 HP, Bullfrog 1/2 -> 3/10, Wolf 3/2 -> 9/10,
  Squirrel 0/1 -> 0/5. Max 4 creatures (4 lanes). Keeper: lanes stay strict (faithful); creatures
  "could scale better as the cards get better" (backlog: upgrades, Inscryption campfire +power/+health).
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
- The Challenger (Keeper: "a shadowy figure ... Only hands showing"): `challenger_body.png` (hooded figure,
  empty black hood, pale hands holding cards) and `challenger_select_bg.png` (16:9, 2K: candlelit cabin,
  cards in four lanes, figure on the right), both `um fal edit --ref stoat_flat_probe.png`. The splash has
  a third skeletal hand at the bottom right; Keeper approved it as is. `make_art.py character()` derives
  the 340 px combat body, select icon + locked silhouette, top-bar icon, map marker and the 1920x1080 splash.
  Wired in `Inscryption.cs`: `CreateCustomVisuals` (static PNG), icon paths, and `CustomCharacterSelectBg` ->
  `Inscryption/scenes/char_select_bg.tscn` (Control + full-rect TextureRect; export turns it into a
  binary .scn behind a .tscn.remap). Rest site, merchant and energy icons still use placeholders.
  Spend total: 9 nano-banana-2 images = $0.72, plus 6 birefnet cutouts.
- Superseded the same day: Keeper: "the lore says Luke Carder is who you play as" (Inscryption's
  found-footage card-pack YouTuber). Drawn as a stylized ordinary young man (messy brown hair, dark grey
  hoodie, jeans, cards in hand); no attempt at the real actor's likeness. Keeper: playing cards "look
  strange" -> `luke_body_v2.png` / `luke_select_bg_v2.png` replace them with parchment creature cards
  (edits of `luke_body.png` / `luke_select_bg.png`). The body is mirrored to face right. Charui outputs
  renamed to neutral `body.png`, `char_select_icon(_locked).png`, `character_icon.png`, `map_marker.png`.
  Title -> "Luke Carder", he/him, `CharacterGender.Masculine`. Total fal spend ~$1.52 (19 images).
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

### Card rewards (2026-10-04)

- FAIL after the first win: `CardFactory.CreateForReward` threw "couldn't generate a valid rarity".
  Cause: `RollForRarity` -> `GetNextAllowedRarity` only cycles Common/Uncommon/Rare present in the pool;
  ours held Basic + Token only. Each reward also needs 3 distinct eligible cards (blacklist per pick).
- Fix (Keeper chose "all six"): Bullfrog and Wolf -> Common (Bullfrog later back to Basic, see Large Capsule) (Inscryption choice-pool cards; Stoat stays
  Basic, it is a starter/talking card there). New sigil-free Blood cards from the reference: Grizzly 3/4/6,
  River Snapper 2/1/6, Ring Worm 1/0/1 (Common); Urayuli 4/7/7, Amalgam 2/3/3, Geck free 1/1 (Rare).
  Art: 6 more fal edits ($0.48) + cutouts. Bullfrog's Mighty Leap and Amalgam's all-tribes trait are
  inert until sigils/totems exist. Staged; in-game verification pending.

### Neow's Large Capsule (2026-10-04)
- FAIL (game stuck after picking Large Capsule at Neow): `LargeCapsule.GetStrikeForCharacter` /
  `GetDefendForCharacter` call `CardPool.AllCards.First(Basic && Tags.Contains(Strike|Defend))` with no
  fallback. `Fasten` likewise does `.First(Tags.Contains(Defend))` over unlocked pool cards.
- Fix: Stoat tagged `CardTag.Strike`; Bullfrog back to Basic and tagged `CardTag.Defend` (its Mighty Leap
  is Inscryption's blocking sigil). Commons for rewards: Wolf, Grizzly, River Snapper, Ring Worm.
  Other Strike/Defend uses (StrikeDummy, NeowsTalisman, LeafyPoultice, NutritiousSoup, Tezcatara,
  Amalgamator, GhostSeed) filter with Where and are safe. Verified: Keeper granted it via the dev console
  (`relic add LARGE_CAPSULE`) and it worked.

### Showing creature power (2026-10-04)
- Keeper: cards say "Summon a 3/9" but the board shows "9/9". The bar under a creature is current/max HP;
  power was shown nowhere. Fix: `BoardCreature`'s idle move carries `SingleAttackIntent(ScaledPower)` when
  power > 0, so the creature shows an attack intent (sword + number) like an enemy. Pets never perform
  moves; `PowerCmd.Apply` refreshes the intent when CreaturePower lands. Intent previews are computed
  against the player as target (engine passes `combatState.Allies`), so target-side modifiers can skew
  the number.
- FAIL in game: no intent appeared. Cause: a pet's `MonsterModel.NextMove` stays the default empty
  `new MoveState()`; only enemies get `RollMove` (`Creature.PrepareForNextTurn`), and PowerCmd's intent
  refresh sits in the stack-change path. Fix: `BoardCreature.ShowIntent()` calls `SetMoveImmediate(idle,
  forceTransition: true)` (sets NextMove and refreshes the intent; avoids `RollMove`, which would consume
  the MonsterAi RNG); `CreatureCard.OnPlay` calls it after summoning. Staged; re-verify pending.
- Keeper: the purple placeholder blobs "should be bones or something". New flat icons (fal edit + rmbg,
  $0.24): Bones (skull and crossed bones), Creature (paw print), Side Deck relic (squirrel on a tied card
  stack). `make_art.py icons()` writes 64/256 power icons and the 94 px relic + white outline + 256.

### Playtest round 2 (2026-10-04, seed SX0XDR6WQ5)
- Verified on screen: attack intents (Stoat 3, Wolf 9), paw/skull/squirrel icons, health x5 (Stoat
  survived at 4/15), Luke's body and top-bar icon.
- Keeper: lanes behave as intended (enemy N hits creature N, empty lanes hit the player); keep it faithful.
- FAIL: three creatures overran the nearest enemy (third Stoat overlapped the bush). Fix in `BoardLayout`:
  measure the gap to the nearest living enemy's hitbox (converted into the ally container's space) and
  shrink the row evenly with `NCreature.ScaleTo` (relative to `Visuals.DefaultScale`, the Osty pattern),
  min 0.55. Staged.
- Keeper: "you should be able to pick which creature you sacrifice." Fix: `CreatureCard.ChooseSacrifices`
  shows each creature on the board as its card (`BoardCreature.Card`, created with
  `CombatState.CreateCard` and removed again with `RemoveCard`) in `CardSelectCmd.FromSimpleGrid`, pick
  exactly Blood; the grid skips itself when the board holds exactly enough. Picks map back by reference
  (identical cards stay distinct). Prompt: `<card>.selectionScreenPrompt` with
  `{Blood:plural:creature|creatures}`. Limitation: the grid shows card stats, not a creature's current HP.
  Staged.

### Legibility (2026-10-04)
- Keeper: reward pool felt good; playing several creatures felt bad (crowding/overlap -> row fit above);
  attack vs defend confusing: "who blocks whom" and "when things happen". Built all three proposals plus
  the Keeper's "an indicator, like how the reject works" (read as the selection reticle):
  - `LaneMarkers`: numbered badge (Kreon font if `res://themes/kreon_bold_shared.tres` loads as a Font) on
    each creature and enemy, refreshed on every `AddCreature`/`RemoveCreatureNode` and on `OnCombatSetUp`.
  - Hover link: postfixes on `NCreature.OnFocus`/`OnUnfocus` show `ShowSingleSelectReticle` on the lane
    partner (enemy <-> creature; an enemy in an empty lane highlights the player). Skipped while
    `NTargetManager` is in selection.
  - `InscryptionKeywords.Creature` (BaseLib `[CustomEnum, KeywordProperties(Before)]`) on every creature
    card; hover tip explains lanes and timing. CreaturePower text updated.
  - Strikes: a 40 px lunge tween + `VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_slash")` and a
    0.25 s `Cmd.Wait` per creature, left to right.
  Staged; verification pending.

### Upgrades and the campfire (2026-10-04)
- Found: creature cards had no `OnUpgrade`, but `MaxUpgradeLevel` defaults to 1, so Smith offered them and
  the upgrade did nothing. Keeper's design: Smith gives the StS-style upgrade (Blood -1), and a new
  Campfire rest option gives Inscryption's warmth (+1 Power or +2 Health) for the run.
- `CreatureCard`: Blood is a DynamicVar (`OnUpgrade` -> -1); `MaxUpgradeLevel` 0 for free cards (Squirrel,
  Geck; Inscryption never upgrades Squirrels). Card text hides "Sacrifice" at 0 Blood via
  `{Blood:cond:>0?Sacrifice {Blood}.\n|}`. Campfire bonuses are `[SavedProperty] Inscryption_PowerBonus`
  / `Inscryption_HealthBonus` (inherited public properties are found: `SavedPropertiesTypeCache` uses
  `GetProperties` without DeclaredOnly). Summon copies them: `BoardCreature.BonusPower`, and
  `CreatureCmd.SetMaxHp`/`SetCurrentHp` when health differs. The strike intent reads power live
  (`SingleAttackIntent(() => ScaledPower)`); 0-power creatures use a separate no-intent move.
- `CampfireRestSiteOption` (BaseLib `CustomRestSiteOption`, id INSCRYPTION_CAMPFIRE, icon
  `images/ui/rest_site_campfire.png`) added by `SideDeck.TryModifyRestSiteOptions`. Each campfire's buff
  alternates by `RunState.TotalFloor` (even = Power), deterministic across reloads. Picks one creature card
  from the deck (`CardSelectCmd.FromDeckGeneric`, cancelable). Strings in `rest_site_ui.json`.
- Not ported: Inscryption's repeat warming and the survivors eating the card.
  Staged. Note: BaseLib's "Added N new properties to SerializableCard" counts its own SavedSpireFields,
  not [SavedProperty]; mod types' [SavedProperty]s are cached in `PostModInitPatch.LatePostInit` via
  `BetaMainCompatibility.CacheSavedProperties`, which warns on unsupported ones (none logged).
  Real check: warm a card, save and quit, continue, inspect the card.

### Playtest round 3 (2026-10-04, floor 9)
- Verified on screen: lane badges 1-2-3 on creatures and enemies, attack intents (warmed River Snapper
  shows 6), paw/skull icons, Campfire present.
- FAIL: the row still ran under enemy 1 when three enemies stood close; the shrink hit its 0.55 floor.
  Fix: step the player back first (up to 150 px, as the game does for the Necrobinder with Osty; home X
  kept in node meta `inscryption_home_x`), then shrink (floor 0.45, gap 16). Re-arranges on any creature
  add/remove, enemies included; the player is read from live nodes because a removed creature may have
  left its combat state. Staged.

### Layout rethink (2026-10-04, Phrog Parasite elite)
- Keeper: "pretty broken at the moment visually ... a terrible jumble". Causes: (1) the step-back build
  recorded "home" during combat setup, before the room positions the player, so Luke snapped to mid-room;
  (2) fitting the row to the nearest enemy collapses when an elite splits into four parasites near the
  player: every limit clamps and creatures, bars, intents and badges pile onto enemies.
- Replaced with Inscryption's model: four fixed lane slots (115 x 110, gap 10) in front of the player; each
  creature is scaled to fit its slot (never above 1), the player never moves, and the row's footprint no
  longer depends on enemy positions. Own-creature lane badges sit at a fixed spot above the slot.
- Keeper: "why not just make the four lanes and let each summon fill accordingly, we can't ever have more
  than 4." Lanes are now fixed (Inscryption's slots): `Board` keeps a ConditionalWeakTable creature->lane;
  each creature takes the lowest lane no living creature on its side holds and keeps it until death (the
  lane then stays empty until refilled). Enemies laned the same way in `combatState.Enemies` order; extra
  enemies have no lane (they hit the player) until one frees. Blocking, attacks, badges, hover partner and
  slot placement all read `Board.LaneOf`. Summons take their lane in `CreatureCard.OnPlay` (game logic).
  Limitation: enemy lanes are assigned lazily on lookup; fine single-player, but a UI-only lookup between an
  enemy death and a spawn could assign differently on a UI-less co-op peer. Staged; verification pending.
- Superseded the earlier mapping "Lanes shift when a creature dies".

### Card types, Bones cards, shop (2026-10-04)
- FAIL: shop crashed: `CardFactory.CreateForMerchant` stocks types Attack x2, Skill x2, Power x1
  (`MerchantInventory._coloredCardTypes`) from non-Basic pool cards and throws when a type is missing; all
  creatures were Skills. Type also drives Attack/Skill/Power Potions and ~30 relics (Shuriken, Kunai, Pen Nib,
  Letter Opener, Mummified Hand, Whetstone, War Paint...).
- Keeper's call (paused to design it): creatures typed by stats: Attack when Power >= Health, Skill when Health
  is greater, free creatures (Squirrel, Geck) always Skills; Powers later from Totems/sigils (Necrobinder
  precedent: summons are Skills). Strike/Defend tags moved to match: Wolf (Attack) = Basic Strike, Stoat
  (Skill) = Basic Defend; Bullfrog back to Common.
- Bones cards: `CreatureStats` gains `Bones`; cards check `GetPowerAmount<BonesPower>()` and spend with a
  negative `PowerCmd.Apply` (BonesPower has no AllowNegative; removed at 0). Smith lowers Bones by 1 for Bones
  cards. Opossum 2 Bones 1/1, Coyote 4 Bones 2/1, Rattler 6 Bones 3/1 (Common, Attacks). Art: 3 fal edits +
  cutouts ($0.24).
- Shop stopgap: `Patches/MerchantCardTypes` (Harmony prefix on `CreateForMerchant`): for Luke only, a type the
  non-Basic pool lacks (Power, for now) falls back to Skill. Power Potion with no Powers shows an empty choice.
  Staged; verification pending.

### Stagger and true scale (2026-10-04, floor 15)
- Verified on screen: four fixed slots with lane badges 1-4; Opossum (Bones card) summoned; Rattler card
  shows the Attack frame.
- FAIL: creature health bars and status rows overlapped. Cause: `NCreature.ScaleTo` is a temporary visual
  scale (`DoScaleTween` sets only `Visuals.Scale`); hitbox, reticle, intent position and health bar come
  from `UpdateBounds`, which ScaleTo never calls and which divides out `_tempScale`. Fix: public
  `SetScaleAndHue(scale, 0)` (sets DefaultScale + Scale and calls UpdateBounds; hue 0 leaves materials).
  Because it overwrites DefaultScale, each node's arrival scale is kept in meta `inscryption_base_scale`.
  Keeper's stagger: lanes 2 and 4 stand 40 px back (up) with ZIndex 0; lanes 1 and 3 in front (ZIndex 1).

### Agent-driven verification (2026-10-04, Keeper away and authorized driving the game)
- Method: xdotool on the game window (2560x1440), dev console (` key) for `room shop|restsite|monster`,
  `godmode`, `card <ID>`; screenshots via `import -window`. Console-jumped rooms are NOT saved
  (`RunManager.EnterRoomDebug`; save file unchanged by Save and Quit), so tests left the Keeper's run as it
  was: Continue returns to the post-Thieving-Hopper loot (deck 27, 159 gold, floor 19). Modded saves backed
  up first (`~/.universal-modder/backups/sts2-modded-saves/`). Gotcha: typing while the console is closed
  sends hotkeys to the game (opened the deck view, toggled View Upgrades); open the console, screenshot to
  confirm, then type.
- Verified: card reward with rares and an upgraded Rattler+ ("Spend 5 Bones"); Creature keyword and its
  tooltip; Attack/Skill frames; shop opens (Opossum, Rattler as Attacks; River Snapper, Bullfrog as Skills;
  Geck fills the Power slot); Campfire option, icon, Health description, creature-only grid, prompt, card
  updates to 3/25, rest consumed; campfire persistence across reload (Keeper's River Snapper Power +1 is in
  the save and reloads as "6/30"); Urayuli+ "Sacrifice 3" (Smith); four lanes staggered with readable HP bars
  and statuses; sacrifice grid ("Choose 1 creature to sacrifice", lane order); the summon takes the freed
  lane; lanes stay put when a creature dies; strikes (empty-lane Geck hits enemy 1, Stoat hits enemy 2);
  blocking with overkill cap (15-damage hit shows 5 on a 5-HP Squirrel); Bones +1 per death; Opossum spends
  2 Bones, takes empty lane 1, Bones power removed at 0; hover reticle (enemy 1 brackets the lane-1 creature).
- Not visually confirmed: the strike lunge tween (frames caught the death, not the hop).

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
