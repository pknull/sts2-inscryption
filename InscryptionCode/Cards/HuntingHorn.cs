using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>Ferocity for this turn, as the Defect's Hotfix is Focus for the turn.</summary>
public sealed class HuntingHorn() : InscryptionCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<HuntingHornPower>(2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HuntingHornPower>(choiceContext, Owner.Creature, DynamicVars["HuntingHornPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HuntingHornPower"].UpgradeValueBy(1m);
    }
}
