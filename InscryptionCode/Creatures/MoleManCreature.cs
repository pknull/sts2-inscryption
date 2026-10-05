using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class MoleManCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.MoleMan;
    public override CardModel Card => ModelDb.Card<MoleMan>();
}
