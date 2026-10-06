using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.RestSite;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Relics;

/// <summary>
/// Luke's starting relic, Inscryption's scale: health tips with the damage each side deals, so Luke heals half of the
/// HP that he and his creatures take from enemies (rounded down per hit; Block and overkill don't count). It also
/// carries the board's rules: lane blocking, enemy lanes, Bones and the death sigils (<see cref="Afterlife"/>), and
/// the Campfire rest option.
/// </summary>
public sealed class Scale : InscryptionRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override Task BeforeCombatStart()
    {
        HitLog.Reset();
        LaneEnemies(Owner.Creature.CombatState);
        return Task.CompletedTask;
    }

    // Enemy lanes are handed out here, at fixed points of the combat, so every player's game agrees on them: at the
    // start, when an enemy arrives, and when one dies (an enemy waiting without a lane steps into the freed one).
    public override Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.IsEnemy)
        {
            LaneEnemies(creature.CombatState);
        }
        return Task.CompletedTask;
    }

    private static void LaneEnemies(ICombatState? combatState)
    {
        Board.Enemies(combatState);
        BoardLayout.Refresh();
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
        {
            return false;
        }
        options.Add(new CampfireRestSiteOption(player));
        return true;
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature.IsEnemy)
        {
            LaneEnemies(creature.CombatState);
        }
        return Afterlife.AfterDeath(choiceContext, Owner, creature);
    }

    // Lane blocking: the creature in the attacker's lane takes the hit before Luke's Block does (see Blocking).
    public override Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        Blocking.BeforeHit(Owner, target, props, dealer);
        HitLog.Before(Owner, target, amount, props, dealer);
        return Task.CompletedTask;
    }

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource) =>
        target == Owner.Creature ? Blocking.AfterCreature(target, amount, props) : amount;

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        Blocking.Clear(target);
        return Task.CompletedTask;
    }

    // Reported for every result, killed targets included (AfterDamageReceived skips those). Every player's relic
    // hears every hit: the scale heals only for this Luke's own damage and his creatures'.
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        HitLog.After(result);
        var luke = Owner.Creature;
        if (dealer == null || (dealer != luke && dealer.PetOwner != Owner) || !result.Receiver.IsEnemy || luke.IsDead)
        {
            return;
        }
        int heal = result.UnblockedDamage / 2;
        if (heal <= 0)
        {
            return;
        }
        int before = luke.CurrentHp;
        if (before < luke.MaxHp)
        {
            Flash();
        }
        await CreatureCmd.Heal(luke, heal);
        MainFile.Logger.Info($"Scale: {dealer.Monster?.Id.Entry ?? "Luke"} took {result.UnblockedDamage} HP from {result.Receiver.Monster?.Id.Entry}; Luke heals {heal}, HP {before} -> {luke.CurrentHp}");
    }
}

