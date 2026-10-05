using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

public sealed class PorcupineCreature : BoardCreature
{
    public override CreatureStats Stats => Bestiary.Porcupine;
    public override CardModel Card => ModelDb.Card<Porcupine>();
}
