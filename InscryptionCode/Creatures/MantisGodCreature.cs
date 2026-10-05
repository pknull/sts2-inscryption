using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class MantisGodCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.MantisGod;
    public override CardModel Card => ModelDb.Card<MantisGod>();
}
