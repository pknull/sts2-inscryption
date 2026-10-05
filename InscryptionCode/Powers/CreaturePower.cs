using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>
/// Carried by every creature on the board. It does what Inscryption's board does: the creature takes attacks
/// aimed down its lane, and strikes across its lane when the player ends the turn. Its sigils (from its card or a
/// Totem) change how it blocks, strikes and moves.
/// </summary>
public sealed class CreaturePower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldPlayVfx => false;

    // Hovering the creature also explains each of its sigils.
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        Sigils.All(Owner).Select(s => HoverTipFactory.FromKeyword(Sigils.Keyword(s)));

    private bool Has(Sigil sigil) => Sigils.Has(Owner, sigil);

    // The creature Blocking chose for this hit (the one in the attacker's lane, or a Burrower) takes it. Same
    // mechanism as Osty's DieForYouPower; Blocking holds the player's Block back until the creature has taken it.
    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer) =>
        Blocking.PendingBlocker(target) == Owner && Owner.IsAlive ? Owner : target;

    // The engine passes a redirected hit's overkill on to the original target (Osty's overflow reaches the
    // Necrobinder). In Inscryption a blocker soaks the whole hit; Balance.BlockersSoakOverkill chooses which.
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return Balance.BlockersSoakOverkill && target == Owner && Owner.IsAlive ? Math.Min(amount, Owner.CurrentHp) : amount;
    }

    // Waterborne: submerged during the enemy turn, so nothing can hit it.
    public override bool ShouldAllowHitting(Creature creature) =>
        creature != Owner || !Has(Sigil.Waterborne) || Owner.CombatState?.CurrentSide != CombatSide.Enemy;

    // Stinky: the enemy in its lane has -1 Power.
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!Has(Sigil.Stinky) || Owner.IsDead || dealer == null || !dealer.IsEnemy || !props.IsPoweredAttack())
        {
            return 0m;
        }
        int lane = Board.LaneOf(Owner);
        return lane >= 0 && Board.LaneOf(dealer) == lane ? -Balance.PowerScale : 0m;
    }

    // Sharp Quills: whatever strikes it takes 1 Power of damage back.
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && Has(Sigil.SharpQuills) && dealer is { IsEnemy: true, IsAlive: true } && props.IsPoweredAttack())
        {
            await CreatureCmd.Damage(choiceContext, dealer, Balance.PowerScale, ValueProp.Unpowered, Owner);
        }
    }

    // Guardian: when the enemy turn starts, moves to block an attacking enemy whose lane is empty.
    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy || Owner.IsDead || !Has(Sigil.Guardian) || Owner.PetOwner == null)
        {
            return Task.CompletedTask;
        }
        if (IsAttacking(Board.EnemyInLane(combatState, Board.LaneOf(Owner))))
        {
            return Task.CompletedTask;
        }
        foreach (var enemy in Board.Enemies(combatState).Where(IsAttacking).OrderBy(Board.LaneOf))
        {
            if (Board.MoveTo(Owner, Board.LaneOf(enemy)))
            {
                break;
            }
        }
        return Task.CompletedTask;
    }

    private static bool IsAttacking(Creature? enemy) => enemy?.Monster?.NextMove.Intents.OfType<AttackIntent>().Any() == true;

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner.IsDead || Owner.Monster is not BoardCreature creature)
        {
            return;
        }
        if (creature.ScaledPower > 0)
        {
            foreach (int lane in Sigils.StrikeLanes(Owner).ToList())
            {
                await Strike(choiceContext, creature, lane);
            }
        }
        // Sprinter: after attacking, moves one lane, turning back at the edge or when blocked.
        if (Has(Sigil.Sprinter) && !Owner.IsDead
            && !Board.MoveTo(Owner, Board.LaneOf(Owner) + creature.SprintDirection))
        {
            creature.SprintDirection = -creature.SprintDirection;
            Board.MoveTo(Owner, Board.LaneOf(Owner) + creature.SprintDirection);
        }
    }

    private async Task Strike(PlayerChoiceContext choiceContext, BoardCreature creature, int lane)
    {
        var enemies = Board.Enemies(Owner.CombatState);
        if (enemies.Count == 0)
        {
            return;
        }
        // In Inscryption an empty lane lets the hit through to the scale; with no scale here, it hits the first enemy.
        Creature target = Board.EnemyInLane(Owner.CombatState, lane) ?? enemies[0];
        // Airborne: flies over the blocker, so the hit ignores Block.
        var props = Has(Sigil.Airborne) ? ValueProp.Move | ValueProp.Unblockable : ValueProp.Move;
        Lunge();
        VfxCmd.PlayOnCreatureCenter(target, "vfx/vfx_attack_slash");
        var results = await CreatureCmd.Damage(choiceContext, target, creature.ScaledPower, props, Owner);
        // Touch of Death: destroys what it damages; elites and bosses are made of stone.
        if (Has(Sigil.TouchOfDeath) && target.IsAlive && Owner.CombatState?.Encounter?.RoomType == RoomType.Monster
            && results.Any(r => r.Receiver == target && r.UnblockedDamage > 0))
        {
            await CreatureCmd.Kill(target);
        }
        // Pace the strikes so each creature's attack reads on its own.
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
