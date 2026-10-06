using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.Powers;
using Inscryption.InscryptionCode.RestSite;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Relics;

/// <summary>
/// Starting relic carrying Inscryption's economy: the Squirrel side deck (one Squirrel into your hand each turn,
/// ten per combat), the scale (<see cref="ScalePower"/>) and what follows a creature's death: Bones and the death
/// sigils (<see cref="Afterlife"/>).
/// </summary>
public sealed class SideDeck : InscryptionRelic
{
    private int _squirrelsLeft;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(Balance.SideDeckSize)];

    private int SquirrelsLeft
    {
        get => _squirrelsLeft;
        set
        {
            AssertMutable();
            _squirrelsLeft = value;
        }
    }

    public override async Task BeforeCombatStart()
    {
        SquirrelsLeft = Balance.SideDeckSize;
        HitLog.Reset();
        LaneEnemies(Owner.Creature.CombatState);
        // The scale: Luke heals half the damage he and his creatures deal.
        await PowerCmd.Apply<ScalePower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);
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

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || SquirrelsLeft <= 0 || Owner.Creature.CombatState == null)
        {
            return;
        }
        SquirrelsLeft--;
        Flash();
        var squirrel = Owner.Creature.CombatState.CreateCard<Squirrel>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(squirrel, PileType.Hand, Owner);
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

    // Reported for every result, killed targets included (AfterDamageReceived skips those).
    public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        HitLog.After(result);
        return Task.CompletedTask;
    }
}
