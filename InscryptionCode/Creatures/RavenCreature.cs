using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class RavenCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Raven;
    public override CardModel Card => ModelDb.Card<Raven>();
}
