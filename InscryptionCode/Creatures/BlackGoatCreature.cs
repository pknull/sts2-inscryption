using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class BlackGoatCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.BlackGoat;
    public override CardModel Card => ModelDb.Card<BlackGoat>();
}
