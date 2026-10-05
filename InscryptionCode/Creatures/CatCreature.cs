using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class CatCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Cat;
    public override CardModel Card => ModelDb.Card<Cat>();
}
