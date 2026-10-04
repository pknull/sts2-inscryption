using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class GrizzlyCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Grizzly;
    public override CardModel Card => ModelDb.Card<Grizzly>();
}
