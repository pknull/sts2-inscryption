using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class FrozenOpossumCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.FrozenOpossum;
    public override CardModel Card => ModelDb.Card<FrozenOpossum>();
}
