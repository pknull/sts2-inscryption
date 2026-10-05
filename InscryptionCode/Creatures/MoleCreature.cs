using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class MoleCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Mole;
    public override CardModel Card => ModelDb.Card<Mole>();
}
