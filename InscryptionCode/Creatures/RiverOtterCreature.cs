using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class RiverOtterCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.RiverOtter;
    public override CardModel Card => ModelDb.Card<RiverOtter>();
}
