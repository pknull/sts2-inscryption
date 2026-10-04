using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class WolfCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Wolf;
    public override CardModel Card => ModelDb.Card<Wolf>();
}
