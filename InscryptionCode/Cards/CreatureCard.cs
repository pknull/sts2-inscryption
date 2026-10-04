using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// An Inscryption creature card. Playing it pays its Blood by sacrificing creatures already on the board, then
/// summons <typeparamref name="TCreature"/> into the next lane. It costs no energy: Blood is the cost, as in
/// Inscryption.
/// </summary>
public abstract class CreatureCard<TCreature>(CreatureStats stats, CardRarity rarity)
    : InscryptionCard(0, CardType.Skill, rarity, TargetType.Self) where TCreature : BoardCreature
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Blood", stats.Blood),
        new DynamicVar("Power", stats.Power * Balance.StatScale),
        new DynamicVar("Health", stats.Health * Balance.StatScale),
    ];

    protected override bool IsPlayable
    {
        get
        {
            int onBoard = Board.Creatures(Owner).Count;
            return onBoard >= stats.Blood && onBoard - stats.Blood < Balance.MaxCreatures;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Inscryption lets you choose the sacrifices; for now the weakest creatures go first.
        var sacrifices = Board.Creatures(Owner)
            .OrderBy(c => ((BoardCreature)c.Monster!).Stats.Power)
            .Take(stats.Blood)
            .ToList();
        foreach (var sacrifice in sacrifices)
        {
            await CreatureCmd.Kill(sacrifice);
        }
        var creature = await PlayerCmd.AddPet<TCreature>(Owner);
        await PowerCmd.Apply<CreaturePower>(choiceContext, creature, 1m, Owner.Creature, this);
    }
}
