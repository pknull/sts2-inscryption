using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class GeckCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Geck;
    public override CardModel Card => ModelDb.Card<Geck>();
}
