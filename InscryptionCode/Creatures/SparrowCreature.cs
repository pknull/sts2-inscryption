using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class SparrowCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Sparrow;
    public override CardModel Card => ModelDb.Card<Sparrow>();
}
