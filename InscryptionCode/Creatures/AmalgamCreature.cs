using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class AmalgamCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Amalgam;
    public override CardModel Card => ModelDb.Card<Amalgam>();
}
