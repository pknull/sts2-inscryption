using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class PronghornCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Pronghorn;
    public override CardModel Card => ModelDb.Card<Pronghorn>();
}
