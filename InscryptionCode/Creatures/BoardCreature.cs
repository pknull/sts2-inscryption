using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils.NodeFactories;
using Inscryption.InscryptionCode.Extensions;
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

    public int ScaledPower => Stats.Power * Balance.StatScale;

    public override int MinInitialHp => Stats.Health * Balance.StatScale;
    public override int MaxInitialHp => Stats.Health * Balance.StatScale;

    // Loads Inscryption/images/creatures/<creature_id>.png
    private string ArtPath => $"creatures/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".ImagePath();

    public override NCreatureVisuals? CreateCustomVisuals() => NodeFactory<NCreatureVisuals>.CreateFromResource(ArtPath);

    // The visuals come from a PNG rather than a scene, so only the PNG needs preloading.
    public override IEnumerable<string> AssetPaths => [ArtPath];

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var idle = new MoveState("NOTHING_MOVE", _ => Task.CompletedTask);
        idle.FollowUpState = idle;
        return new MonsterMoveStateMachine([idle], idle);
    }
}
