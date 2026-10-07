using MegaCrit.Sts2.Core.Entities.Players;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Guardian, as in Inscryption: "When an opposing card is played opposite an empty space, this card moves to that
/// space." A Guardian facing no enemy moves into the lowest empty lane that faces one, whether or not that enemy is
/// about to attack (Keeper, 2026-10-06, #13). Enemies are on the board from the start of a Slay the Spire fight, so it
/// checks whenever the lanes change: a summon, an enemy arriving or dying, one of Luke's creatures dying or sprinting.
/// Runs from game logic only, so every player's game moves it at the same moment.
/// </summary>
public static class Guardian
{
    public static void Reposition(Player owner)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }
        foreach (var guardian in Board.Creatures(owner).Where(c => Sigils.Has(c, Sigil.Guardian)))
        {
            if (Board.EnemyInLane(combatState, Board.LaneOf(guardian)) != null)
            {
                continue;
            }
            int lane = Enumerable.Range(0, Board.LaneCount).FirstOrDefault(
                l => Board.EnemyInLane(combatState, l) != null && Board.CreatureInLane(owner, l) == null, -1);
            if (lane >= 0)
            {
                Board.MoveTo(guardian, lane);
            }
        }
    }
}
