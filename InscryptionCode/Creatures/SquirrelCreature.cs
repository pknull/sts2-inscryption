using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class SquirrelCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Squirrel;
    public override CardModel Card => ModelDb.Card<Squirrel>();
}
