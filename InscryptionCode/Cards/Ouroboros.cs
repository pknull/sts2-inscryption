using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// The serpent that eats its tail: Unkillable, and each time an Ouroboros dies every Ouroboros the player has grows
/// +1/+1 for the rest of the run (the deck's cards, this combat's copies, and any on the board).
/// </summary>
public sealed class Ouroboros() : CreatureCard<OuroborosCreature>(Bestiary.Ouroboros, CardRarity.Rare)
{
    public static async Task GrowAll(Player owner)
    {
        var cards = owner.Deck.Cards.Concat(owner.PlayerCombatState?.AllCards ?? []).OfType<Ouroboros>().ToList();
        foreach (var card in cards)
        {
            card.Grow(1, 1);
        }
        foreach (var creature in Board.Creatures(owner).Where(c => c.Monster is OuroborosCreature))
        {
            ((BoardCreature)creature.Monster!).BonusPower += 1;
            await CreatureCmd.SetMaxHp(creature, creature.MaxHp + Balance.HealthScale);
            await CreatureCmd.SetCurrentHp(creature, creature.CurrentHp + Balance.HealthScale);
        }
        BoardLayout.Refresh();
    }
}
