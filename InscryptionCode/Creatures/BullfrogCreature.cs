using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class BullfrogCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Bullfrog;
    public override CardModel Card => ModelDb.Card<Bullfrog>();
}
