using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class OuroborosCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Ouroboros;
    public override CardModel Card => ModelDb.Card<Ouroboros>();
}
