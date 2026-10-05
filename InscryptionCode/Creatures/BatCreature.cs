using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class BatCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Bat;
    public override CardModel Card => ModelDb.Card<Bat>();
}
