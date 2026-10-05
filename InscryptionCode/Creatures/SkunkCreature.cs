using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class SkunkCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Skunk;
    public override CardModel Card => ModelDb.Card<Skunk>();
}
