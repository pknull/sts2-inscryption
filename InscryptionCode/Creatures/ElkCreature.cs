using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class ElkCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Elk;
    public override CardModel Card => ModelDb.Card<Elk>();
}
