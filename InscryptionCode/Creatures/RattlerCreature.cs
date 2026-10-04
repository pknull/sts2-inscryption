using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class RattlerCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Rattler;
    public override CardModel Card => ModelDb.Card<Rattler>();
}
