using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class StoatCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Stoat;
    public override CardModel Card => ModelDb.Card<Stoat>();
}
