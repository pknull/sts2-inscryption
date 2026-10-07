using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Rooms;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// The Angler's Fish Hook (Keeper, 2026-10-06: a Rare with Exhaust that makes a card of an enemy). In Inscryption it
/// pulls an opposing card onto your side of the board. Here it hooks a normal enemy at half HP or less (not a minion;
/// none in elite or boss fights): the enemy leaves the fight, stands in its lane on Luke's side, and joins the deck for
/// good as a creature card with its photograph. Power comes from the enemy's hit (before buffs, so every player's game
/// agrees), Health from its HP, both capped.
/// </summary>
public sealed class FishHook() : InscryptionCard(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool IsPlayable => Owner?.Creature.CombatState?.Enemies.Any(Hookable) == true;

    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public static bool Hookable(Creature enemy) =>
        enemy is { IsAlive: true, IsEnemy: true, IsPrimaryEnemy: true }
        && enemy.CombatState?.Encounter?.RoomType == RoomType.Monster
        && enemy.CurrentHp * 2 <= enemy.MaxHp;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { Monster: { } monster } target || !Hookable(target))
        {
            MainFile.Logger.Info($"Fish Hook: {cardPlay.Target?.Monster?.Id.Entry} slipped free");
            return;
        }
        // Inscryption Power 1-4 (2-8 damage per strike), Health 2-6 (4-12 HP).
        decimal hit = monster.NextMove?.Intents.OfType<AttackIntent>().FirstOrDefault()?.DamageCalc?.Invoke() ?? 4m;
        int power = Math.Clamp((int)Math.Ceiling(hit / 4m), 1, 4);
        int health = Math.Clamp((target.CurrentHp + 1) / 2, 2, 6);
        int caught = Owner.Deck.Cards.Count(c => c is HookedCard);
        string key = $"{Owner.RunState.Rng.StringSeed}-{Owner.RunState.TotalFloor}-{monster.Id.Entry.ToLowerInvariant()}-{caught}";

        await HookPhoto.Take(target, key);
        var card = (HookedCard)Owner.RunState.CreateCard(ModelDb.Card<HookedCard>(), Owner);
        card.Catch(monster.Id, power, health, key);
        MainFile.Logger.Info($"Fish Hook: caught {monster.Id.Entry} (hit {hit}, HP {target.CurrentHp}) as {power}/{health}");

        // Onto Luke's side now, in the enemy's lane if it is free, before the enemy goes (a last enemy ends the fight).
        int lane = Board.LaneOf(target);
        if (Sacrifice.EmptyLanes(Owner) > 0)
        {
            await Summoning.Summon<HookedCreature>(choiceContext, Owner, card.Stats, source: card, lane: lane);
        }
        await CreatureCmd.Kill(target);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
