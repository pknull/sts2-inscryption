using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class BloodhoundCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Bloodhound;
    public override CardModel Card => ModelDb.Card<Bloodhound>();
}
