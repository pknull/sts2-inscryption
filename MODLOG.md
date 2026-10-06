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
  `godmode`,`card <ID>`; screenshots via`import -window`. Console-jumped rooms are not saved on entry or by
  Save and Quit (`RunManager.EnterRoomDebug`), so tests left the Keeper's run as it was: Continue returns to
  the post-Thieving-Hopper loot (deck 27, 159 gold, floor 19). Correction from batch 1: WINNING a console
  fight does save (see below). Modded saves backed
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

### Rest site and shop scenes (2026-10-04)

- Luke at the rest site (new fal pose: sitting on a log with creature cards, mirrored to face the fire) and in
  the shop (standing body at 460 px). `CustomRestSiteAnimPath` / `CustomMerchantAnimPath` point at
  single-Sprite2D scenes (`Inscryption/scenes/rest_site.tscn`, `merchant.tscn`); BaseLib's
  NRestSiteCharacterFactory / NMerchantCharacterFactory build hitbox, flip point and thought bubbles.
  Energy counter and card trail are still Ironclad placeholders.

### Sigils and Totems, batch 1 of 3 (2026-10-04)

- Keeper's design calls: Totems are Power cards (not the Woodcarver event); Touch of Death kills normal enemies
  only (elites and bosses are "made of stone", Kaycee's Mod's immunity); Mighty Leap = flying enemies (their
  hits pass over blockers to Luke unless the blocker has Mighty Leap). Build in three batches, verify and
  commit after each.
- Not ported (enemy/boss-only in Act 1): Repulsive, Steel Trap, Tidal Lock, Omni Strike.
- Translations: Airborne = strikes ignore Block (ValueProp.Unblockable); Waterborne = unhittable on the enemy
  turn (`ShouldAllowHitting`) and doesn't block; Stinky = lane enemy -3 per hit (`ModifyDamageAdditive`, also
  lowers its intent); Sharp Quills = 3 back to an attacker (`AfterDamageReceived`, Unpowered); Burrower =
  moves into the empty lane being attacked inside `ModifyUnblockedDamageTarget`; Guardian = at the enemy turn's
  start, if its lane's enemy isn't attacking, moves to the leftmost empty lane facing an attacker;
  Sprinter = moves one lane after striking, flipping at edges/blocks; Bi/Trifurcated = lanes L-1/L+1 (+L),
  edge lanes dropped, empty target lanes fall back to the first enemy; Leader = +1 Power to creatures in
  adjacent lanes (live in `ScaledPower`; `BoardLayout.Refresh` re-shows intents).
- Flyers: Byrdonis, OwlMagistrate, ThievingHopper (decompile shows flight/hover/swoop states). One list in
  `Sigils.Flyers`; candidates to check in the bestiary: Ovicopter, Flyconid, Byrdpip, Parafright, Vantom.
- Engine: `Sigil` and `[Flags] Tribe` enums; sigils and tribes are BaseLib CustomEnum CardKeywords (keys drop the
  underscore: `INSCRYPTION-MIGHTYLEAP`), mapped lazily because BaseLib fills the fields at runtime. Creature
  sigils = card sigils copied to `BoardCreature.OwnSigils` at summon + Totems of its tribe (per-combat
  registry keyed by the combat state). Hovering a creature lists its sigils (`CreaturePower.ExtraHoverTips`).
- Totems: six Power cards (Canine, Hooved, Reptile, Avian, Insect, Squirrel), 1 energy, Uncommon. The sigil is
  rolled at `AfterCreated` from `new Rng(Owner, Id, copies-in-deck)` and saved as
  `[SavedProperty] Inscryption_TotemSigil`, whose setter swaps the card's keyword.
- 19 creatures (stats from the reference; rarity: Common = simple sigils and <=2 Blood/<=4 Bones; Uncommon =
  strong sigils or >=3 Blood or >=6 Bones; Rare = Inscryption rares): Sparrow, Raven, Bat, Turkey Vulture,
  Kingfisher, Mantis, Mantis God (R), Pronghorn, Elk, Long Elk (R), Alpha, Bloodhound, Skunk, Porcupine,
  Adder, Great White, River Otter, Mole, Mole Man (R). Bullfrog gains Mighty Leap; tribes on existing cards.

### Batch 1 verified in game (2026-10-04, agent driving; Byrdonis elite, then Nibbits normal)

- Airborne enemy: Byrdonis's hits passed over a Squirrel and an Adder in its lane (both unhurt).
- Mighty Leap: a Bullfrog in Byrdonis's lane took its three hits and died; tooltip on hover.
- Touch of Death: 3 damage left the Byrdonis elite alive (78 -> 75); against a normal Nibbit at 26/44 the
  Adder's 3 damage killed it outright.
- Stinky: the lane Nibbit's intent dropped 6 -> 3 when the Skunk was summoned; the Skunk took 3.
- Trifurcated: Mantis God in lane 2 hit Nibbit 1, Nibbit 2, and lane 3 (empty -> first enemy); totals matched
  (42 -> 33, 44 -> 41). Bifurcated: Mantis in lane 3 hit Nibbit 2 and lane 4 (empty -> Nibbit 1); Nibbit 2
  ended 40/46 (43 if it were not bifurcated).
- Guardian: Bloodhound left lane 1 (enemy buffing) for empty lane 2 and took the 14.
- Waterborne: River Otter in lane 1 untouched by the 14; the hit went past it to Luke.
- Sharp Quills: Porcupine took 8 (10 -> 2) and the Nibbit took 3 back (38 -> 35).
- Sprinter: Elk struck from lane 1, then moved to lane 2; boxed in on both sides, it stayed put.
- Burrower: with lane 1 emptied, the Mole dug from lane 3 into lane 1 and took the 8 (20 -> 12).
- Leader: the Elk's intent rose 6 -> 9 when the Alpha was summoned beside it.
- Totem: a Hooved Totem (rolled Trifurcated) gave the Elk Trifurcated Strike (hover lists it with Sprinter);
  the totem power shows on Luke. The shop's Power slot now stocks a Totem (Canine, rolled Sprinter).
- Card text and tooltips: tribe and sigil keywords render on cards, the sacrifice grid and creature hovers.
- Rest site: Luke on his log by the fire, Rest / Smith / Campfire. Shop: Luke standing, left of the merchant.
- Fixed after the first pass, re-verified in a second session:
  - Merchant scene threw `Expected BoundObject to be a SpineSprite, but it is a Sprite2D` (caught, logged).
    BaseLib's scene auto-conversion (`NodeFactory.TryAutoConvert`) never marks the node as factory-made, so its
    `MerchantCharacterAnimPatch` guard lets `NMerchantCharacter._Ready` play a Spine animation on our sprite.
    `Patches/MerchantSpriteGuard` skips `_Ready`/`PlayAnimation` when child 0 is not a SpineSprite. Zero
    exceptions after. (Worth reporting upstream to BaseLib.)
  - Stale lane badge: `RemoveCreatureNode` drops the node from the room's list but leaves it on screen for its
    death animation, so the badge refresh never reached it. `LaneMarkers.Clear` on removal; a double
    sacrifice and a creature killed in combat now leave no badge behind.
  - Multi-lane strikers showed a single number. They now show "3x3" (Trifurcated) / "3x2" (Bifurcated) via a
    live `MultiAttackIntent`; `Sigils.StrikeLanes` is shared by the intent and the strike.
- Not observed: Airborne strike ignoring Block (the Elk broke the block first; code is `ValueProp.Unblockable`).
- Polish noted: keyword order on cards varies (Mole: "Creature. Burrower."; Adder: "Touch of Death. Reptile.
  Creature."); a Totem's rolled sigil is seeded per player/card/copies owned, so offers of the same Totem repeat
  their sigil until one is bought.
- Test-harness gotchas:
  - `fight <ENC>` issued mid-combat with creatures on the board killed the game silently (the old pets were
    carried into the new room). Start console fights from a non-combat room (loot screen, shop).
  - WINNING a console fight writes the run save (`pre_finished_room`, map history, RNG counters, progress
    stats); it replaced the Keeper's post-Thieving-Hopper loot. Pre-fight files kept in
    `~/.universal-modder/backups/sts2-modded-saves/pre-nibbits-20261004-1853/`. Test without winning, or
    back up and restore.
  - The console only opens when the game window has focus: `xdotool windowactivate --sync` first, and check
    the console overlay is up before typing (otherwise letters are hotkeys; "E" ends the turn).
- Keeper approved restoring the pre-fight save. The first restore (plain copy) did NOT hold: see the save-sync
  gotcha under batch 2.

### Sigils, batch 2 of 3: sacrifice and death (2026-10-04)

- Sigils: Many Lives (a sacrifice that pays and stays; no death, so no Bones), Worthy Sacrifice (worth 3 Blood),
  Bone King (4 Bones on death instead of 1), Unkillable (on any death, sacrifice included, a copy of the
  summoning card returns to the hand: `CardModel.CreateClone` of the played card, so campfire bonuses and the
  Smith upgrade carry over), Corpse Eater (in hand: when a creature is killed, not sacrificed, the card is
  auto-played free into that lane), Frozen Away (killed, not sacrificed: an Opossum takes the lane; a Totem's
  Frozen Away also releases an Opossum). New card property Terrain: holds a lane, cannot be sacrificed (unless
  Worthy Sacrifice), shown as a keyword.
- Creatures: Black Goat 1Bl 0/1 Hooved Worthy Sacrifice (C); Cat 1Bl 0/1 Many Lives (C); Cockroach 4Bo 1/1 Insect
  Unkillable (C); Rat King 2Bl 2/1 Bone King (C); Corpse Maggots 5Bo 1/2 Insect Corpse Eater (U); Frozen Opossum
  free 0/5 Terrain Frozen Away (U; Inscryption's bottle item made a card); Ouroboros 2Bl 1/1 Reptile Unkillable
  (R); Undead Cat 1Bl 3/6 (Token, never offered).
- Cat: nine lives. Each sacrifice counts on the combat card (its text shows lives left) and on the deck card
  (`DeckVersion`, `[SavedProperty] Inscryption_LivesLost`); the ninth turns the deck card into the Undead Cat
  (`CardCmd.TransformTo`). Ouroboros: each death grows every Ouroboros +1/+1 for the run (deck cards, combat
  copies, and any on the board), then Unkillable returns the grown copy.
- Engine: `Sacrifice` (Blood values, Many Lives, what can still be paid with a lane left for the summon),
  `Summoning` (one summon path for cards, Corpse Eater and Frozen Away), `Afterlife` (called from
  `SideDeck.AfterDeath`: Bones, Ouroboros, Unkillable, then Frozen Away or Corpse Eater). Sacrificed creatures
  are marked so "killed" sigils skip them. When every candidate is worth 1 Blood and any pick leaves a lane, the
  old exact-count grid stays; otherwise the player picks one creature at a time ("Blood still owed") and only
  picks that can still finish with a free lane are offered. `ICreatureCard` gains `Stats`, `Grow`,
  `PlayFreeInto`; `Sigils.CardHas` checks a card's sigils (printed or Totem) for Corpse Eater in hand.
- Art: 8 fal edits + cutouts (about $0.70), same prompt template off `stoat_flat_probe.png`.
- Build: Godot's headless export needs `DOTNET_ROOT=~/.dotnet` (and `DOTNET_ROLL_FORWARD=Major`); without it the
  export crashed and its crash handler raised a zenity alert on the desktop. Run exports with
  `env -u DISPLAY -u WAYLAND_DISPLAY` so a failure stays in the log.

### Batch 2 verified in game (2026-10-04, agent driving; Nibbits normal fights)
- Worthy Sacrifice: with only the Black Goat on the board the 3-Blood Grizzly was playable, and the Goat alone
  paid for it.
- Many Lives: the Cat paid and stayed (card text 9 -> 8 lives); no Bone for it. With the Cat on a full board the
  one-at-a-time picker ran ("2 Blood still owed", then "1 Blood still owed") and the Cat was not offered twice.
- Unkillable: a sacrificed Cockroach and a killed Ouroboros each returned to the hand.
- Bone King: the Rat King's death took Bones 13 -> 17 (Luke's power tooltip).
- Corpse Eater: killing the Rat King auto-played the Maggots free into its lane; sacrifices with Maggots in hand
  did nothing; on the enemy turn, after the hand was discarded, the Nibbit's hit killed the Cat and the
  retained Maggots took lane 1.
- Frozen Away: killing the Frozen Opossum put an Opossum (3/5) in its lane, ahead of the Maggots in hand.
  Terrain: the Frozen Opossum was never offered as a sacrifice.
- Ouroboros: its death grew the hand copy to 6/10, and the deck card (added with `card ... Deck`) to 6/10.
- Cat: nine sacrifices of the deck Cat turned the deck card into the Undead Cat (9/30; deck count unchanged).
  A combat-only Cat sacrificed nine times showed "0 lives left" and its tenth sacrifice killed it.
- Zero exceptions in the log across three sessions.
- Fixed during the test, re-verified:
  - Corpse Eater cards were discarded at end of turn, and creatures mostly die on the enemy turn, so the sigil
    almost never fired. Corpse Eater cards now Retain (Inscryption's hand persists); a Corpse Eater Totem adds
    Retain to its tribe's cards in combat.
  - The sacrifice grid built stand-in cards from the base card, so the Cat always read "9 lives" there. Stand-ins
    now copy the summoning card's campfire bonuses and lives (`ICreatureCard.CopyStateFrom`).
  - A Cat with no lives left kept Many Lives. `Sacrifice.Stays` now excludes it, so its next sacrifice kills it.
- Not tested: an upgraded Unkillable copy keeping its upgrade (code clones the played card).

### Totems carrying batch-2 sigils (2026-10-05, agent driving)
- Fix first: Retain for Corpse Eater was stamped only on cards present when the Totem was played, so a Squirrel
  made later (side deck, Unkillable copy) was discarded at end of turn. `TotemPower.AfterCardEnteredCombat` now
  gives Retain to any card entering combat that has Corpse Eater (printed or from a Totem), the pattern of the
  game's `PhantomBladesPower`.
- Debug console command `totem <tribe> <sigil>` (`ConsoleCommands/TotemConsoleCmd.cs`, DebugOnly) puts a Totem with
  a chosen sigil in the hand; the game finds console commands in mods by reflection. Needed because a Totem's
  sigil is a seeded roll.
- Verified, one Totem each: Worthy Sacrifice (Squirrel Totem: one Squirrel paid the Grizzly's 3 Blood); Many Lives
  (Reptile Totem: the Geck's hover lists it; sacrificed, it stayed); Bone King (Canine Totem: sacrificed Coyote,
  Bones 7 -> 11); Unkillable (Insect Totem: killed Mantis returned to hand); Corpse Eater (Squirrel Totem: the
  Squirrel already in hand and one added afterwards both show Retain; the retained Squirrel played itself into
  the killed Mantis's lane); Frozen Away (Hooved Totem: killed Pronghorn left an Opossum in lane 1). The grid
  left out a Totem-Many-Lives Geck on a full board, as it should. No exceptions from the mod.
- Harness: a session crashed on `fight` after `room shop` was jumped to from inside a fight with creatures out
  (console only: creatures carried out of an unfinished combat). Steam twice stopped launching the game (cloud
  sync waiting on a dialog while Steam was logged out); the Keeper cleared it. The driver now finds the game
  window by name and sends no key unless the game window has focus; kills by index are made only after a
  `damage 0 <i>` probe names the target.
- Test-harness gotchas:
  - Steam Cloud sync undid the first save restore. At launch the game copies cloud -> local for any save whose
    local modified time differs from the cloud's (`CloudSaveStore.SyncCloudToLocalInternal`); a plain `cp`
    gives a new mtime, so the cloud's post-test files came back. A restore must copy the backup in and then set
    each file's mtime to the cloud time, which Steam records as `remotetime` in
    `~/.steam/.../userdata/<id>/2868840/remotecache.vdf`. Done that way, the launch skipped the sync, Continue
    returned to the Thieving Hopper loot (floor 19, 159 gold, 27 cards), and the quit uploaded it to the cloud.
  - `damage <n> <index>` targets `CombatState.Creatures` (player, every pet ever summoned this combat including
    dead ones, then enemies). One mistyped console command (xdotool typed "INSCRPYTION") left a pet unsummoned,
    the indexes landed on the enemies, the fight was won, and the run saved again. Do not kill by index in
    loops; test sacrifices with multi-Blood cards on a full board instead, and check the log after each step.

### Waterborne out of the Totem pool (2026-10-05, Keeper's call)
- Waterborne played poorly on a Totem. In Inscryption it was rated 1 (lowest) on the wiki's sigil table and
  worked as a racing piece: a submerged creature's lane hits the scale, its own strikes win the weight back, and
  nothing carries between fights. Here every hit through it is lasting HP, and a Totem opens a whole tribe's
  lanes. `Sigils.Modular` (the Totem roll pool) now leaves out Waterborne; `Sigils.Implemented` (every sigil)
  drives creature hover tips and the `totem` console command. Great White, Kingfisher and River Otter keep it.
  Totems already rolled keep their saved sigil.

### Playtest 2026-10-05 (Keeper) and the defense question
- Run 5T4W55CJ1H (standard, A0): died to the Decimillipede elite on floor 25. Damage per fight from the run
  history: Act 1 boss Ceremonial Beast 0 over 12 turns; Nibbits, Fuzzy Wurm, Fogmog, Cubex, Bowlbugs weak,
  Slumbering Beetle, The Obscura 0 each; Slithering Strangler 18; Slimes normal 14; Ruby Raiders 7; Vine
  Shambler 6; Inklets 9; Exoskeletons weak 8; Mytes 10; Bowlbugs normal 21; Decimillipede 31 (death). Fights ran
  3-12 turns. Zero exceptions across 385 card plays.
- Keeper: whole-hit soaking is too cheap (a free Squirrel cancels any hit; single targets cost nothing), yet
  groups hurt while lanes are open, and Luke has no defense but bodies. Biggest balance issue.
- Change for testing (Keeper's call): `Balance.BlockersSoakOverkill = false`. A blocker soaks up to its HP and the
  rest reaches Luke (the engine's Osty rule); the cap in `CreaturePower.ModifyHpLostAfterOsty` only applies when
  the switch is on. Creature text says so. Still one Squirrel per turn (turn 1 included), by the Keeper's call.
- Held until this is tested: Inscryption's scale as a power on Luke (absorbs damage that reaches him, half of
  what his creatures dealt that turn; not Block, which the engine applies before a blocker and would pad the
  blockers too). Not touched yet: fight length (offense, Power x3).
- Also staged in this build, uncommitted until the Keeper has seen them: rest-site sprite without its own log
  (bottom padded so his seat sits on BaseLib's 60% seat line; `luke_rest_nolog*.png`); buffed Power/Health shown
  green (`{X:diff()}` against the printed stats via `CreatureCard.StatVar`); keyword, campfire and character
  text translated out of Inscryption units (no Blood or Power wording; campfire shows +3 damage / +10 HP from
  `Balance`); a log line per Burrower decision (the Keeper saw a Mole not cover a lane; no cause found yet).
- Lane choice (Keeper's design): drag a creature card to a lane. First built as enemy targeting
  (`TargetType.AnyEnemy`), but that left lanes without an enemy unchoosable, and position matters (Trifurcated
  loses a strike on an edge lane; Leader, Bifurcated, Sprinter). Now cards stay untargeted; a Harmony prefix on
  `NCardPlay.TryPlayCard` (mouse plays only) records where the card was let go, and `LaneDrop` maps it to the
  nearest lane slot or lane-facing enemy across the screen. The summon goes there after the sacrifices (which may
  free it), else the lowest empty lane; keyboard/controller plays take the lowest empty lane. Empty slots show a
  dim lane number (`LaneMarkers.MarkEmptySlots`). Sacrifice-screen stand-ins say "In lane N" (a `Lane` var, 0 and
  hidden elsewhere). Built to staging; deploys when the game next closes.

### Second playtest and the standard kit (2026-10-05)
- Run D9ZK9M0PPR (overkill pass-through live): hallway fights 0-12 HP each (37 over eight); died to the Waterfall
  Giant (Act 1 boss) with 62 HP over 13 turns. Killed, the Giant stays, stuns itself ("About to Blow") and erupts
  next turn for its stored Steam Eruption: vanilla characters Block it; Luke could only put a creature in its lane,
  and everything above that creature's HP went through. No exceptions; no Mole played.
- Keeper's call: give Luke a normal kit, like the other characters, and let creatures grow over the run, adding
  cards that ease the StS mechanics even if they feel less Inscryption. Starting deck: 4 Strike (1 energy, 6
  damage, +3 upgraded), 4 Defend (1 energy, 5 Block, +3), 2 Stoat; Squirrels still from the Side Deck. Strike and
  Defend carry the Strike/Defend tags (Neow's Large Capsule and others look for them); Stoat stays Basic without a
  tag; Wolf is now Common (reward pool). Overkill pass-through stays (Osty's rule); the Scale idea is dropped.
- Art: `strike_flat.png` (a punch, mirrored to face right) and `defend_flat.png` (crossed forearms), fal edits with
  the stoat probe for style and Luke's body for the hoodie (about $0.16); `make_art.basics()` writes portraits.
- Found in the log: the game's enemy stats count Luke's creatures as opponents ("has died to a
  MONSTER.INSCRYPTION-SQUIRREL_CREATURE. That's 5 losses", from `EnemyStats`). Harmless; not fixed yet.

### Third playtest: Block order (2026-10-05)
- Keeper: with Block up, an enemy's hit took his Block instead of hitting the creature in its lane. Cause, in
  `CreatureCmd.Damage`: the target's Block (a pet's owner's, for pets) is spent before
  `ModifyUnblockedDamageTarget` redirects, so Luke's Block shielded his blockers and was gone for open lanes.
  Fix (`Creatures/Blocking.cs`): on an enemy's powered attack on Luke, `SideDeck.BeforeDamageReceived` picks the
  blocker (creature in the attacker's lane if it can block, else a Burrower that digs in); a Harmony prefix on
  `Creature.DamageBlockInternal` holds Luke's Block back for that hit; the creature takes it
  (`CreaturePower.ModifyUnblockedDamageTarget`); the overkill then meets Luke's Block in
  `SideDeck.ModifyHpLostAfterOsty`. Hits nobody can block use Luke's Block as normal. Built and deployed; not yet
  seen in game.
- Run J9FXFURTXH (standard kit, the Block bug live): died on floor 7 to the Phrog Parasite elite with 13 HP. HP
  went to Neow (16, Precarious Shears), the cheese event (14) and Ruby Raiders (30 over 8 turns, despite about 16
  Defends: the Block went to hits the creatures were blocking; the Tracker's Hounds hit 8 times a turn). Deck of
  12 with 2 Strikes; fights ran 5-8 turns. The Cat reached nine sacrifices in natural play and became the Undead
  Cat. Session log: no mod exceptions.
- Also built this round: a lane highlight while dragging a creature card (empty slot number brightens; the
  facing enemy and any occupant get the targeting reticle), from a per-frame tick during `NMouseCardPlay`.

### Fourth playtest: the damage curve (2026-10-05)
- Run 9RW2WGXP9W (all fixes live): died on floor 28 (Act 2) to the Infested Prisms elite, 53 HP in 5 turns.
  Per fight: 15 of 20 cost 0-1 HP; Inklets 9, Bygone Effigy (elite) 8, Chompers 16; The Kin (Act 1 boss) 32 over
  12 turns. Hallway fights ran 2-4 turns. Deck of 30 (two Great Whites, Grizzly, Amalgam, Ouroboros, 4 Strike,
  4 Defend). No mod exceptions; no Burrower lines (no Mole played).
- Keeper: the damage distribution still feels odd. Reading: creature HP is fixed (Health x5) while enemy hits
  grow by act and within a fight (Prism Jab 15 plus Pulsate buffs; the Kin Priest's Weak and Frail). A hit below
  the blocker's HP costs nothing; above it, the overkill lands in full, so damage switches from none to lethal
  instead of ramping. The run history records HP lost per fight, not per hit, so it cannot tell blockers too
  small from a Block-order misfire in the elite.
- Added `Creatures/HitLog.cs`: a godot.log line per hit on Luke or his creatures, e.g. `Hit (turn 3):
  INFESTED_PRISM in lane 1, 15 damage; SQUIRREL_CREATURE took 5 of its 5 HP, died; Luke: Block 6 -> 0, HP 53 ->
  49`, or why nobody blocked (lane empty, occupant could not block). Fed from `SideDeck.BeforeDamageReceived`
  (after `Blocking` picks the blocker) and `AfterDamageGiven` (unlike `AfterDamageReceived`, it also reports
  killed targets). Built and deployed; not yet seen in game.
- Levers once a run has the log: creature HP that grows by act, or cheaper ways to buy creature HP; a Block-order
  fix instead if the split points there.

### Co-op and the Keeper's rule changes (2026-10-06, issues #1-#11, agent driving)

- The Keeper's two co-op runs on 2026-10-05 (host and one client, both Luke) diverged four times. Cause: the
  drag-to-lane lane was read from the local mouse and never sent, so the dropping machine summoned into that lane
  and the others into the lowest empty one; from then on sacrifices and blocks differed. Second, `Board` handed out
  lanes on any lookup, and the UI's lookups run on each machine's own schedule.
- Fixes: `LaneDrop.Choose` sends the lane through `PlayerChoiceSynchronizer` as an index choice (as `CardSelectCmd`
  does); UI reads lanes through `Board.Shown`, which never assigns; enemy lanes are assigned at combat start, enemy
  arrival and enemy death (`SideDeck`). Every relic hears every hit, so `Blocking` and `HitLog` now return early
  for another Luke's hits (#2), and Totems record their owner (#3). `BoardLayout` gives every Luke a lane row in
  front of the local player; others' rows stand back at 0.8 scale; a non-Luke local player keeps the front row (#4).
  `Summoning` re-runs the layout after a summon takes its lane (the room laid the node out before it had one).
- Co-op test harness: two or three local instances, no Steam, own saves under `default/<id>`:
  `SlayTheSpire2 --force-steam off --fastmp host_standard --log-file host.log` and
  `--force-steam off --fastmp join --clientId 1000 --log-file client.log` (another `--clientId` for a third). Seed
  each account's `settings.save` with `mods_enabled: true` (else the mod popup quits) and windowed size; i3 tiles
  new windows, so float and size them with `i3-msg`. `xdotool search --pid P --name N` ORs the two; add `--all`.
  Clicks into an unfocused window are dropped: activate first.
- Verified: baseline on 71f3278 diverged at "After enemy turn end" (host's Squirrel blocked, client saw an empty
  lane). With the fix: two Lukes, three Lukes, and Ironclad host with a Luke client all logged identical hits on
  every machine and no divergence; the client's lane arrives as `PlayerChoiceResult indexes N`; a Totem buffed only
  its owner's Gecks; co-op Campfire and shop work. The partner Luke's creatures show no HP bars, as the game hides
  remote players' bars.
- Gameplay (#5-#11), verified in game: starting deck 4 Strike, 4 Defend, 2 Squirrel; Strike 5; ScalePower heals
  half the HP each hit takes from an enemy (Strike 5 heals 2, a 3-damage strike heals 1, capped at max HP), logged
  as `Scale: ...`; a Mantis God in lane 4 struck only the lane-3 slime, once; Mole Man needs two sacrifices; Geck
  shows blue; every creature card is a Skill; a dead Cockroach adds a curse `CockroachCurse` to the deck, which plays
  and breeds again.
- Scale icon (Keeper's go-ahead): one `um fal edit --ref stoat_flat_probe.png` (1:1, 1K, $0.08) of a brass balance
  scale tipped by teeth in its left pan, `um fal rmbg`, then `make_art.py icons()` writes `scale_power.png` (64 and
  256); `ScalePower` uses the default icon path. Re-running `icons()` left the other icons byte-identical.
- Release v0.2.0 (pre-release, `Inscryption-v0.2.0.zip`, same four-file layout as v0.1.0). Gotcha found before
  shipping: Godot's `all_resources` export packs any `.json` under the project, and it had picked up
  `Work/session-state/*.json`. `export_presets.cfg` now also excludes `Work/*` and `Memory/*`; checked that the
  `.pck` holds only `Inscryption/` and `.godot/`. `um publish check` flags the build's own files because the
  same files are deployed in `<game>/mods/Inscryption/`; those FAILs are expected.

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

- Play a run with HitLog live and split each elite and boss fight's damage: blocker HP, Luke's Block, open lanes.
- Then a lever for Act 2 hits (creature HP by act, or cheaper creature HP); offense if fights drag.
- Luke has no non-Basic Attacks now: Inscryption-flavoured non-creature cards; sigil batch 3.
- Enemy stats count Luke's creatures as opponents (harmless, unfixed); field note via `um kb` (Keeper's OK first).
