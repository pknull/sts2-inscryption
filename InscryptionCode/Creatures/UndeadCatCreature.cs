using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class UndeadCatCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.UndeadCat;
    public override CardModel Card => ModelDb.Card<UndeadCat>();
}
