using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>
/// Marks on the player how many Totems stand this combat; the sigils themselves are in <see cref="Sigils"/>. A Corpse
/// Eater acts from the hand, so cards that gain it from a Totem Retain, including cards made after the Totem.
/// </summary>
public sealed class TotemPower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Owner == Owner.Player)
        {
            RetainIfCorpseEater(card);
        }
        return Task.CompletedTask;
    }

    public static void RetainIfCorpseEater(CardModel card)
    {
        if (Sigils.CardHas(card, Sigil.CorpseEater) && !card.Keywords.Contains(CardKeyword.Retain))
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
        }
    }
}
