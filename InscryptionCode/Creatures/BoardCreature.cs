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

    /// <summary>The card instance that summoned it, if any; Unkillable returns a copy of it.</summary>
    public CardModel? SourceCard { get; set; }

    /// <summary>The sigils printed on the card that summoned it (Totem sigils are added by <see cref="Sigils.Has"/>).</summary>
    public HashSet<Sigil> OwnSigils { get; } = [];

    /// <summary>Sprinter's direction: +1 moves towards higher lanes, -1 towards lower.</summary>
    public int SprintDirection { get; set; } = 1;

    /// <summary>Damage per strike: printed Power, campfire Power, +1 for each adjacent Leader; scaled.</summary>
    public int ScaledPower => (Stats.Power + BonusPower + AdjacentLeaders()) * Balance.PowerScale;

    private int AdjacentLeaders()
    {
        if (Creature?.PetOwner == null)
        {
            return 0;
        }
        int lane = Board.LaneOf(Creature);
        return lane < 0 ? 0 : new[] { lane - 1, lane + 1 }
            .Select(l => Board.CreatureInLane(Creature.PetOwner, l))
            .Count(c => c != null && Sigils.Has(c, Sigil.Leader));
    }

    public override int MinInitialHp => Stats.Health * Balance.HealthScale;
    public override int MaxInitialHp => Stats.Health * Balance.HealthScale;

    // Loads Inscryption/images/creatures/<creature_id>.png
    private string ArtPath => $"creatures/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".ImagePath();

    public override NCreatureVisuals? CreateCustomVisuals() => NodeFactory<NCreatureVisuals>.CreateFromResource(ArtPath);

    // The visuals come from a PNG rather than a scene, so only the PNG needs preloading.
    public override IEnumerable<string> AssetPaths => [ArtPath];

    // Never performed (pets take no turns). Its attack intent shows the creature's power above it, the way
    // enemies show what they will hit for; the HP bar alone would read as "9/9" for a 3/9 Stoat. A creature that
    // strikes several lanes (Bifurcated, Trifurcated) shows the count, as a multi-hit enemy does.
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        // The intents read ScaledPower when drawn, so campfire Power and Leaders show up.
        _strike = new MoveState("STRIKE_MOVE", _ => Task.CompletedTask, new SingleAttackIntent(() => ScaledPower));
        _multiStrike = new MoveState("MULTI_STRIKE_MOVE", _ => Task.CompletedTask,
            new StrikesIntent(() => ScaledPower, () => Strikes));
        _idle = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        _strike.FollowUpState = _strike;
        _multiStrike.FollowUpState = _multiStrike;
        _idle.FollowUpState = _idle;
        return new MonsterMoveStateMachine([_idle, _strike, _multiStrike], _idle);
    }

    private MoveState? _idle;
    private MoveState? _strike;
    private MoveState? _multiStrike;

    private int Strikes => Creature == null ? 1 : Sigils.StrikeLanes(Creature).Count();

    /// <summary>A multi-hit intent whose damage is read live (the base class only takes a fixed number).</summary>
    private sealed class StrikesIntent : MultiAttackIntent
    {
        public StrikesIntent(Func<decimal> damage, Func<int> strikes) : base(0, strikes)
        {
            DamageCalc = damage;
        }
    }

    /// <summary>
    /// Pets never roll a move, so NextMove stays empty and no intent shows. Set it directly (no RNG use,
    /// unlike RollMove); SetMoveImmediate also refreshes the creature's intent display.
    /// </summary>
    public void ShowIntent()
    {
        var move = ScaledPower <= 0 ? _idle : Strikes > 1 ? _multiStrike : _strike;
        if (move != null)
        {
            SetMoveImmediate(move, forceTransition: true);
        }
    }
}
