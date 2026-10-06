using BaseLib.Utils;
using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// The infestation: every Cockroach that dies adds one of these to the deck for good (<see cref="Afterlife"/>). It is
/// a new card each time, never a copy of the Cockroach that died, so nothing stacked on that card spreads. It summons
/// a Cockroach like the reward card does, and its Cockroaches breed too. It lives in the game's token pool, which no
/// reward or card generator draws from.
/// </summary>
[Pool(typeof(TokenCardPool))]
public sealed class CockroachCurse() : CreatureCard<CockroachCreature>(Bestiary.Cockroach, CardRarity.Curse, CardType.Curse)
{
    // The Cockroach's art; the curse frame comes from the card type.
    public override string CustomPortraitPath => "cockroach.png".BigCardImagePath();
    public override string PortraitPath => "cockroach.png".CardImagePath();
    public override string BetaPortraitPath => "beta/cockroach.png".CardImagePath();

    // A curse is not smithed.
    public override int MaxUpgradeLevel => 0;
}
