using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class TurkeyVultureCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.TurkeyVulture;
    public override CardModel Card => ModelDb.Card<TurkeyVulture>();
}
