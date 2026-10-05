# Inscryption: Luke Carder for Slay the Spire 2

A fan-made character mod for Slay the Spire 2. Luke Carder fights with Inscryption's creatures: he summons them into
four lanes, sacrifices them to pay for bigger ones, and collects their Bones.

This is an early playtest build. Balance is still moving, and bug reports are welcome:
[open an issue](https://github.com/pknull/sts2-inscryption/issues).

## Requirements

- Slay the Spire 2 **v0.107.1** (Steam). A game update can break the mod until it is rebuilt.
- [BaseLib](https://github.com/Alchyr/BaseLib-StS2/releases) **v3.4.7** or later, the shared modding library.

## Install

1. Download `Inscryption-<version>.zip` from [Releases](https://github.com/pknull/sts2-inscryption/releases) and the
   BaseLib release zip.
2. Unzip both into the game's `mods` folder (create it if missing), so you have:

   ```
   Slay the Spire 2/mods/BaseLib/
   Slay the Spire 2/mods/Inscryption/
   ```

   In Steam: right-click Slay the Spire 2, Manage, Browse local files.
3. Launch the game and accept loading mods. The game closes once after you accept; launch it again.
4. Pick **Luke Carder** on the character select screen.

Modded play uses separate save profiles, so your normal progress is untouched. A fresh modded profile has only
Ironclad and Luke unlocked.

## How Luke plays

- **Starting deck:** 4 Strike, 4 Defend, 2 Stoat. More creatures join through card rewards, like any other
  character's cards.
- **Side Deck (starting relic):** a Squirrel arrives in your hand each turn, 10 per combat.
- **Creature cards cost no energy.** They cost a sacrifice (creatures already on your board) or Bones (gained
  whenever one of your creatures dies).
- **Lanes:** drag a creature card to a lane (an empty slot, or the enemy facing it) to summon it there. If that
  lane is taken, it goes to the lowest empty lane.
- **Blocking:** a creature takes the hits of the enemy in its lane; damage beyond its HP goes through to Luke, then
  his Block. An enemy facing an empty lane hits Luke directly. At the end of your turn, each creature strikes the
  enemy in its lane.
- **Sigils** from Inscryption (Airborne, Bifurcated Strike, Many Lives, Unkillable and more) are on the cards and
  explained in their tooltips. **Totems** grant a sigil to a whole tribe for the combat.
- **Rest sites:** Luke's Campfire option makes one creature card stronger (+3 damage per strike or +10 HP).

## Reporting a bug

Open an [issue](https://github.com/pknull/sts2-inscryption/issues) with what happened and, if you can, the game log
from that session:

- Windows: `%APPDATA%\SlayTheSpire2\logs\godot.log`
- Linux: `~/.local/share/SlayTheSpire2/logs/godot.log`

The log records each hit on Luke and his creatures, which helps with balance reports.

## Building from source

Needs the .NET 9 SDK, Godot 4.5.1 (.NET build) and the game installed (the build references its `sts2.dll`). Set
your paths in a `Directory.Build.props` next to `Inscryption.csproj` (it is gitignored):

```xml
<Project>
  <PropertyGroup>
    <Sts2Path>/path/to/Slay the Spire 2</Sts2Path>
    <GodotPath>/path/to/Godot_v4.5.1-stable_mono</GodotPath>
  </PropertyGroup>
</Project>
```

`Sts2Path` is found automatically for default Steam installs. Then:

- `dotnet build` builds `Inscryption.dll` and copies it, with `Inscryption.json`, to `<game>/mods/Inscryption/`.
- `dotnet publish` also exports `Inscryption.pck` (art and text) with Godot.

Add `-p:ModsPath=<folder>/` to put the files elsewhere. Don't build into the game folder while the game is running.

`MODLOG.md` is the development journal: engine findings, design decisions and playtest notes.

## Credits

- [Inscryption](https://www.inscryption.com/) by Daniel Mullins Games. Creature, sigil and character names and stats
  come from the game (via the Inscryption wiki). This mod is not affiliated with or endorsed by Daniel Mullins Games.
- [Slay the Spire 2](https://www.megacrit.com/) by Mega Crit. Not affiliated with or endorsed by Mega Crit.
- [BaseLib](https://github.com/Alchyr/BaseLib-StS2) and the `Alchyr.Sts2.Templates` character template by Alchyr.
- Art generated with [fal](https://fal.ai) (Nano Banana 2), with background removal by BiRefNet; provenance in
  `assets/gen/fal_manifest.jsonl`. No art or files from either game are included.

## License

The code is released under the [MIT License](LICENSE). Inscryption and Slay the Spire 2 names and content belong to
their owners.
