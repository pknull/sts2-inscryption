using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class KingfisherCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Kingfisher;
    public override CardModel Card => ModelDb.Card<Kingfisher>();
}
