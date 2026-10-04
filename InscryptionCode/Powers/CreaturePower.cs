using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
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
        return Board.Enemies(Owner.CombatState).IndexOf(dealer) == Board.LaneOf(Owner) ? Owner : target;
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
        int lane = Board.LaneOf(Owner);
        Creature target = lane >= 0 && lane < enemies.Count ? enemies[lane] : enemies[0];
        await CreatureCmd.Damage(choiceContext, target, creature.ScaledPower, ValueProp.Move, Owner);
    }
}
