using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>Luke hits once per creature on his board, as the Defect's Barrage hits once per orb.</summary>
public sealed class PackHunt() : InscryptionCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) =>
            card.Owner == null ? 0 : Board.Shown.Creatures(card.Owner).Count),
    ];

    protected override bool ShouldGlowRedInternal => Owner != null && Board.Shown.Creatures(Owner).Count == 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        int hits = Board.Creatures(Owner).Count;
        if (hits == 0)
        {
            return;
        }
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
