using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// Inscryption's combat bell: ringing it sends the creatures in. Here the creatures strike at once, and again at the
/// end of the turn. An Attack, so "when you play an Attack" effects see it (the Defect's Dualcast, but a card type
/// Luke's pool lacked).
/// </summary>
public sealed class RingTheBell() : InscryptionCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override bool ShouldGlowRedInternal => Owner != null && !Command.AnyStriker(Owner);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Command.StrikeAll(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
