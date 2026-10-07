using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// The free creature, sacrifice fodder: one each turn from <see cref="Relics.SideDeck"/>, more from cards such as
/// Squirrel Bottle and Snare. It exhausts when played and, being Ethereal, when left in the hand at the end of the turn.
/// </summary>
public sealed class Squirrel() : CreatureCard<SquirrelCreature>(Bestiary.Squirrel, CardRarity.Token)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [.. base.CanonicalKeywords, CardKeyword.Exhaust, CardKeyword.Ethereal];

    /// <summary>Make a Squirrel in <paramref name="owner"/>'s hand (combat only).</summary>
    public static async Task AddToHand(Player owner)
    {
        if (owner.Creature.CombatState is not { } combatState)
        {
            return;
        }
        var squirrel = combatState.CreateCard<Squirrel>(owner);
        await CardPileCmd.AddGeneratedCardToCombat(squirrel, PileType.Hand, owner);
    }
}
