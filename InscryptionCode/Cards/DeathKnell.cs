using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// The bell rung for the last time: the creatures strike X times (X+1 upgraded), then perish, as the Defect's
/// Multicast evokes an orb X times and spends it. Their deaths give Bones and death sigils as usual.
/// </summary>
public sealed class DeathKnell() : InscryptionCard(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override bool HasEnergyCostX => true;

    protected override bool ShouldGlowRedInternal => Owner != null && !Command.AnyStriker(Owner);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int times = ResolveEnergyXValue() + (IsUpgraded ? 1 : 0);
        var creatures = Board.Creatures(Owner);
        if (times > 0)
        {
            await Command.StrikeAll(choiceContext, Owner, times);
        }
        foreach (var creature in creatures.Where(c => c.IsAlive))
        {
            await CreatureCmd.Kill(creature);
        }
    }
}
