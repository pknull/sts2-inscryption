using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class LongElkCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.LongElk;
    public override CardModel Card => ModelDb.Card<LongElk>();
}
