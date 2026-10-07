using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>Puts a creature on the board: from a card, from Corpse Eater, or out of a Frozen Away creature.</summary>
public static class Summoning
{
    /// <summary>
    /// Summon <typeparamref name="TCreature"/> into the lowest empty lane, or into <paramref name="lane"/> if that one
    /// is empty. Campfire bonuses come from the summoning card; <paramref name="source"/> is that card (an Unkillable
    /// creature returns a copy of it).
    /// </summary>
    public static async Task<Creature> Summon<TCreature>(PlayerChoiceContext choiceContext, Player owner,
        CreatureStats stats, int powerBonus = 0, int healthBonus = 0, CardModel? source = null, int lane = -1)
        where TCreature : BoardCreature
    {
        // A Fish Hook catch stands in its photograph, which its visuals load as the pet is made.
        if (source is HookedCard catchCard)
        {
            HookedCreature.PendingPhoto = catchCard.Inscryption_HookedPhoto;
        }
        var creature = await PlayerCmd.AddPet<TCreature>(owner);
        if (creature.Monster is HookedCreature hooked && source is HookedCard caught)
        {
            hooked.Catch(caught);
        }
        // Take the lowest empty lane now, in game logic, rather than whenever the UI first looks.
        Board.LaneOf(creature);
        if (lane >= 0 && Board.LaneOf(creature) != lane)
        {
            Board.MoveTo(creature, lane);
        }
        // The room laid the board out when the creature arrived, before it had a lane; lay it out again.
        BoardLayout.Refresh();
        var board = (BoardCreature)creature.Monster!;
        board.BonusPower = powerBonus;
        board.SourceCard = source;
        board.OwnSigils.UnionWith(stats.Sigils);
        int health = (stats.Health + healthBonus) * Balance.HealthScale;
        if (health != creature.MaxHp)
        {
            await CreatureCmd.SetMaxHp(creature, health);
            await CreatureCmd.SetCurrentHp(creature, health);
        }
        await PowerCmd.Apply<CreaturePower>(choiceContext, creature, 1m, owner.Creature, source);
        board.ShowIntent();
        Guardian.Reposition(owner);
        return creature;
    }
}
