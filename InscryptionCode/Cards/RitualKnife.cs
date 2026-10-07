using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// A sacrifice for its own sake: one creature for two cards, as the Ironclad's Burning Pact exhausts a card for two.
/// It is a sacrifice like any other (The Altar, Many Lives, Bones).
/// </summary>
public sealed class RitualKnife() : InscryptionCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override bool IsPlayable => Owner != null && Sacrifice.Candidates(Owner).Count > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var victim = await Sacrifice.ChooseOne(choiceContext, Owner, new LocString("cards", "INSCRYPTION-RITUAL_KNIFE.selectionScreenPrompt"));
        if (victim == null)
        {
            return;
        }
        await Sacrifice.Perform([victim]);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
