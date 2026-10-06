using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// What follows when one of the player's creatures dies, sacrificed or killed: Bones (four with Bone King), Ouroboros
/// growing, Unkillable returning to the hand, a Cockroach infesting the deck. Only a creature killed in combat (not
/// sacrificed) lets something into its lane: Frozen Away releases an Opossum, else a Corpse Eater in the hand takes
/// the lane for free.
/// </summary>
public static class Afterlife
{
    public static async Task AfterDeath(PlayerChoiceContext choiceContext, Player owner, Creature creature)
    {
        if (creature.PetOwner != owner || creature.Monster is not BoardCreature board)
        {
            return;
        }
        decimal bones = Sigils.Has(creature, Sigil.BoneKing) ? 4m : 1m;
        await PowerCmd.Apply<BonesPower>(choiceContext, owner.Creature, bones, owner.Creature, null);

        if (board is OuroborosCreature)
        {
            await Ouroboros.GrowAll(owner);
        }
        if (Sigils.Has(creature, Sigil.Unkillable))
        {
            await ReturnToHand(owner, board);
        }
        if (board is CockroachCreature)
        {
            await Infest(owner);
        }

        int lane = Board.LaneOf(creature);
        if (Sacrifice.WasSacrificed(creature) || lane < 0)
        {
            return;
        }
        if (Sigils.Has(creature, Sigil.FrozenAway))
        {
            await Summoning.Summon<OpossumCreature>(choiceContext, owner, Bestiary.Opossum, lane: lane);
            return;
        }
        if (Board.CreatureInLane(owner, lane) == null
            && owner.PlayerCombatState?.Hand.Cards.FirstOrDefault(c => Sigils.CardHas(c, Sigil.CorpseEater)) is
                ICreatureCard eater)
        {
            eater.PlayFreeInto(lane);
            await CardCmd.AutoPlay(choiceContext, (CardModel)eater, null);
        }
    }

    /// <summary>
    /// A dead Cockroach adds a curse Cockroach to the deck for good: a new card, never a copy of the one that died, so
    /// upgrades and buffs on that card don't breed.
    /// </summary>
    private static async Task Infest(Player owner)
    {
        var curse = owner.RunState.CreateCard(ModelDb.Card<CockroachCurse>(), owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(curse, PileType.Deck));
    }

    /// <summary>Unkillable: a copy of the summoning card (campfire bonuses and all) goes to the hand.</summary>
    private static async Task ReturnToHand(Player owner, BoardCreature board)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState == null)
        {
            return;
        }
        var source = board.SourceCard;
        var copy = source != null && source.Pile?.IsCombatPile != false
            ? source.CreateClone()
            : combatState.CreateCard(board.Card, owner);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, owner);
    }
}
