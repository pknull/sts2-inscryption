using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>
/// Ferocity for this turn only (Hunting Horn), like the Defect's Hotfix. Creatures strike before the turn ends
/// (<see cref="CreaturePower"/>, BeforeSideTurnEnd), so the end-of-turn strikes still have it; it goes after them.
/// </summary>
public sealed class HuntingHornPower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // The creatures' intents show their damage per strike; show the new amount.
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            Board.RefreshIntents(Owner.Player);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
            Board.RefreshIntents(Owner.Player);
        }
    }
}
