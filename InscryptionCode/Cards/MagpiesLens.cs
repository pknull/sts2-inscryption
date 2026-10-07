using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// Inscryption's Magpie's Lens picks a card out of the deck. Here: a creature card from the draw pile into the hand,
/// then a draw (the Silent's Secret Weapon does the same for Attacks).
/// </summary>
public sealed class MagpiesLens() : InscryptionCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var picked = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(Owner), Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1), c => c is ICreatureCard)).FirstOrDefault();
        if (picked != null)
        {
            await CardPileCmd.Add(picked, PileType.Hand);
        }
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
