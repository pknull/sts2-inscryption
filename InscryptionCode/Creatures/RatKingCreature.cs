using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class RatKingCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.RatKing;
    public override CardModel Card => ModelDb.Card<RatKing>();
}
