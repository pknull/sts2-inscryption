using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class UrayuliCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Urayuli;
    public override CardModel Card => ModelDb.Card<Urayuli>();
}
