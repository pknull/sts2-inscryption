using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>
/// Carried by every creature on the board. It does what Inscryption's board does: the creature takes attacks
/// aimed down its lane, and strikes across its lane when the player ends the turn.
/// </summary>
public sealed class CreaturePower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldPlayVfx => false;

    // Same mechanism as Osty's DieForYouPower, limited to the enemy in this creature's lane.
    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (dealer == null || Owner.IsDead || !props.IsPoweredAttack())
        {
            return target;
        }
        if (Owner.PetOwner == null || target != Owner.PetOwner.Creature)
        {
            return target;
        }
        int lane = Board.LaneOf(Owner);
        return lane >= 0 && Board.LaneOf(dealer) == lane ? Owner : target;
    }

    // The engine passes a redirected hit's overkill on to the original target (Osty's overflow reaches the
    // Necrobinder). In Inscryption a blocker soaks the whole hit, so cap the loss at what this creature has left.
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return target == Owner && Owner.IsAlive ? Math.Min(amount, Owner.CurrentHp) : amount;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner.IsDead || Owner.Monster is not BoardCreature creature || creature.ScaledPower <= 0)
        {
            return;
        }
        var enemies = Board.Enemies(Owner.CombatState);
        if (enemies.Count == 0)
        {
            return;
        }
        // In Inscryption an empty lane lets the hit through to the scale; with no scale here, it hits the first enemy.
        Creature target = Board.EnemyInLane(Owner.CombatState, Board.LaneOf(Owner)) ?? enemies[0];
        Lunge();
        VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_slash");
        await CreatureCmd.Damage(choiceContext, target, creature.ScaledPower, ValueProp.Move, Owner);
        // Pace the strikes so each creature's attack reads on its own, left to right.
        await Cmd.Wait(0.25f);
    }

    /// <summary>A short hop toward the enemies; the art is a static image, so there is no attack animation.</summary>
    private void Lunge()
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(Owner);
        if (node == null)
        {
            return;
        }
        float x = node.Position.X;
        var tween = node.CreateTween();
        tween.TweenProperty(node, "position:x", x + 40f, 0.08);
        tween.TweenProperty(node, "position:x", x, 0.14);
    }
}
