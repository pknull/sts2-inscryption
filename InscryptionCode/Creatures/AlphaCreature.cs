using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class AlphaCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Alpha;
    public override CardModel Card => ModelDb.Card<Alpha>();
}
