using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class RingWormCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.RingWorm;
    public override CardModel Card => ModelDb.Card<RingWorm>();
}
