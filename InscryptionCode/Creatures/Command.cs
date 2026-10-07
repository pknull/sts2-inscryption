using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Cards that make Luke's creatures strike now (Ring the Bell, Death Knell), as the Defect's Dualcast and Multicast
/// evoke orbs on demand. Each creature strikes its usual lanes, in lane order.
/// </summary>
public static class Command
{
    public static async Task StrikeAll(PlayerChoiceContext choiceContext, Player owner, int times = 1)
    {
        for (int i = 0; i < times; i++)
        {
            foreach (var creature in Board.Creatures(owner))
            {
                if (creature.GetPower<CreaturePower>() is { } power)
                {
                    await power.StrikeNow(choiceContext);
                }
            }
        }
    }

    /// <summary>Does <paramref name="owner"/> have a creature that would strike? (UI: red glow on the card.)</summary>
    public static bool AnyStriker(Player owner) =>
        Board.Shown.Creatures(owner).Any(c => c.Monster is BoardCreature { ScaledPower: > 0 });
}
