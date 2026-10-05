using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class CockroachCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Cockroach;
    public override CardModel Card => ModelDb.Card<Cockroach>();
}
