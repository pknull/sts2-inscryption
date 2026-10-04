using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils.NodeFactories;
using Inscryption.InscryptionCode.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// A creature on the player's side of the board, summoned as a pet. Pets never take turns, so it has a single idle
/// move (like Osty); <see cref="Powers.CreaturePower"/> makes it attack and guard its lane.
/// </summary>
public abstract class BoardCreature : CustomMonsterModel
{
    public abstract CreatureStats Stats { get; }

    /// <summary>The card that summons this creature; stands in for it on the sacrifice screen.</summary>
    public abstract CardModel Card { get; }

    /// <summary>Inscryption Power from campfires, copied from the card at summon (combat only).</summary>
    public int BonusPower { get; set; }

    public int ScaledPower => (Stats.Power + BonusPower) * Balance.PowerScale;

    public override int MinInitialHp => Stats.Health * Balance.HealthScale;
    public override int MaxInitialHp => Stats.Health * Balance.HealthScale;

    // Loads Inscryption/images/creatures/<creature_id>.png
    private string ArtPath => $"creatures/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".ImagePath();

    public override NCreatureVisuals? CreateCustomVisuals() => NodeFactory<NCreatureVisuals>.CreateFromResource(ArtPath);

    // The visuals come from a PNG rather than a scene, so only the PNG needs preloading.
    public override IEnumerable<string> AssetPaths => [ArtPath];

    // Never performed (pets take no turns). Its attack intent shows the creature's power above it, the way
    // enemies show what they will hit for; the HP bar alone would read as "9/9" for a 3/9 Stoat.
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // The intent reads ScaledPower when drawn, so campfire Power shows up.
        _strike = new MoveState("STRIKE_MOVE", _ => Task.CompletedTask, new SingleAttackIntent(() => ScaledPower));
        _idle = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        _strike.FollowUpState = _strike;
        _idle.FollowUpState = _idle;
        return new MonsterMoveStateMachine([_idle, _strike], _idle);
    }

    private MoveState? _idle;
    private MoveState? _strike;

    /// <summary>
    /// Pets never roll a move, so NextMove stays empty and no intent shows. Set it directly (no RNG use,
    /// unlike RollMove); SetMoveImmediate also refreshes the creature's intent display.
    /// </summary>
    public void ShowIntent()
    {
        var move = ScaledPower > 0 ? _strike : _idle;
        if (move != null)
        {
            SetMoveImmediate(move, forceTransition: true);
        }
    }
}
