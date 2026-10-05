using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class GreatWhiteCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.GreatWhite;
    public override CardModel Card => ModelDb.Card<GreatWhite>();
}
