using System.Runtime.CompilerServices;
using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Paying Blood. Each creature is worth 1 Blood (3 with Worthy Sacrifice; terrain is worth nothing and cannot be
/// chosen). A sacrifice dies, unless it has Many Lives: then it pays and stays. The summon still needs an empty lane,
/// so a choice that would leave none is never offered.
/// </summary>
public static class Sacrifice
{
    // Creatures killed as a sacrifice, so death sigils that only fire "in combat" (Corpse Eater, Frozen Away) skip them.
    private static readonly ConditionalWeakTable<Creature, object> Sacrificed = new();

    public static bool WasSacrificed(Creature creature) => Sacrificed.TryGetValue(creature, out _);

    public static int Value(Creature creature)
    {
        if (Sigils.Has(creature, Sigil.WorthySacrifice))
        {
            return 3;
        }
        return creature.Monster is BoardCreature { Stats.Terrain: true } ? 0 : 1;
    }

    /// <summary>Many Lives: pays its Blood without dying (a Cat that has spent all nine lives dies).</summary>
    public static bool Stays(Creature creature) =>
        Sigils.Has(creature, Sigil.ManyLives) && creature.Monster is not BoardCreature { SourceCard: Cat { LivesLeft: 0 } };

    /// <summary>The player's creatures that can be sacrificed, in lane order.</summary>
    public static List<Creature> Candidates(Player owner) => Board.Creatures(owner).Where(c => Value(c) > 0).ToList();

    public static int EmptyLanes(Player owner) => Board.LaneCount - Board.Creatures(owner).Count;

    /// <summary>Can the player pay <paramref name="blood"/> and still have a lane for the summon?</summary>
    public static bool CanPay(Player owner, int blood)
    {
        int empty = EmptyLanes(owner);
        if (blood <= 0)
        {
            return empty > 0;
        }
        var options = Candidates(owner);
        return options.Sum(Value) >= blood && empty + options.Count(c => !Stays(c)) > 0;
    }

    /// <summary>
    /// After picking <paramref name="next"/> on top of <paramref name="picked"/>, can the payment still be completed
    /// with a lane left over for the summon?
    /// </summary>
    public static bool Feasible(IReadOnlyCollection<Creature> picked, Creature next, IReadOnlyList<Creature> options,
        int blood, int emptyLanes)
    {
        var after = picked.Append(next).ToList();
        int paid = after.Sum(Value);
        int freed = after.Count(c => !Stays(c));
        if (paid >= blood)
        {
            return emptyLanes + freed > 0;
        }
        var rest = options.Except(after).ToList();
        return paid + rest.Sum(Value) >= blood && emptyLanes + freed + rest.Count(c => !Stays(c)) > 0;
    }

    /// <summary>
    /// Kill the chosen creatures, except those with Many Lives (the Cat counts the life it spent). Each one counts as
    /// sacrificed for The Altar, Many Lives included.
    /// </summary>
    public static async Task Perform(IEnumerable<Creature> victims)
    {
        foreach (var victim in victims)
        {
            if (victim.PetOwner?.Creature.GetPower<AltarPower>() is { } altar)
            {
                await altar.AfterSacrifice();
            }
            if (Stays(victim))
            {
                // The Cat's lives: shown on this combat's card, kept (and spent for good) on the deck's.
                if (victim.Monster is BoardCreature { SourceCard: Cat cat })
                {
                    await cat.LoseLife();
                    if (cat.DeckVersion is Cat deckCat)
                    {
                        await deckCat.LoseLife();
                    }
                }
                continue;
            }
            Sacrificed.AddOrUpdate(victim, true);
            await CreatureCmd.Kill(victim);
        }
    }

    /// <summary>The player picks one creature to sacrifice (Ritual Knife); null if there is none.</summary>
    public static async Task<Creature?> ChooseOne(PlayerChoiceContext choiceContext, Player owner, LocString prompt) =>
        (await Pick(choiceContext, owner, Candidates(owner), 1, prompt)).FirstOrDefault();

    /// <summary>
    /// The card shown for a creature on the sacrifice screen: it carries its summoning card's state and names its lane,
    /// so two Stoats can be told apart.
    /// </summary>
    private static CardModel StandIn(ICombatState combatState, Player owner, Creature creature)
    {
        var board = (BoardCreature)creature.Monster!;
        var card = combatState.CreateCard(board.Card, owner);
        if (board.SourceCard != null && card is ICreatureCard standIn)
        {
            standIn.CopyStateFrom(board.SourceCard);
        }
        card.DynamicVars["Lane"].BaseValue = Board.LaneOf(creature) + 1;
        return card;
    }

    /// <summary>Show <paramref name="creatures"/> as their cards, in lane order, and return the ones picked.</summary>
    public static async Task<List<Creature>> Pick(PlayerChoiceContext choiceContext, Player owner, List<Creature> creatures,
        int count, LocString prompt)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState == null || creatures.Count == 0)
        {
            return [];
        }
        var cards = creatures.Select(c => StandIn(combatState, owner, c)).ToList();
        try
        {
            var picked = await CardSelectCmd.FromSimpleGrid(choiceContext, cards, owner,
                new CardSelectorPrefs(prompt, Math.Min(count, cards.Count)));
            // By reference: two Stoats are distinct creatures even if their cards compare equal.
            return picked.Select(card => creatures[cards.FindIndex(o => ReferenceEquals(o, card))]).ToList();
        }
        finally
        {
            // The stand-in cards were only for display; keep them out of the combat's card list.
            foreach (var card in cards)
            {
                combatState.RemoveCard(card);
            }
        }
    }
}
