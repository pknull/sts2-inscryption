using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class CoyoteCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Coyote;
    public override CardModel Card => ModelDb.Card<Coyote>();
}
