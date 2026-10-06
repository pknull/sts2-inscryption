using BaseLib.Abstracts;
using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;

namespace Inscryption.InscryptionCode.RestSite;

/// <summary>
/// Inscryption's campfire at Slay the Spire 2's rest sites: warm one creature card for +1 Power or +2 Health (shown
/// as damage per strike and HP) for
/// the rest of the run. As in Inscryption each campfire offers one kind of warmth; here it alternates by floor so
/// it is the same on a reload. Added by the Scale relic; uses up the rest like Smith does.
/// </summary>
public sealed class CampfireRestSiteOption(Player owner) : CustomRestSiteOption(owner)
{
    public override string OptionId => "INSCRYPTION_CAMPFIRE";

    public override string? CustomIconPath => "ui/rest_site_campfire.png".ImagePath();

    private CampfireBuff Buff => Owner.RunState.TotalFloor % 2 == 0 ? CampfireBuff.Power : CampfireBuff.Health;

    public override LocString Description =>
        WithAmounts(new("rest_site_ui", $"OPTION_{OptionId}.description{(IsEnabled ? Buff.ToString() : "Disabled")}"));

    /// <summary>The text speaks in this game's numbers: damage per strike and HP, not Inscryption's Power and Health.</summary>
    private static LocString WithAmounts(LocString text)
    {
        text.Add("Damage", Balance.CampfirePower * Balance.PowerScale);
        text.Add("Hp", Balance.CampfireHealth * Balance.HealthScale);
        return text;
    }

    public override bool IsEnabled => Owner.Deck.Cards.Any(c => c is ICreatureCard);

    public override async Task<bool> OnSelect()
    {
        var prefs = new CardSelectorPrefs(WithAmounts(new LocString("rest_site_ui", $"OPTION_{OptionId}.prompt{Buff}")), 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true,
        };
        var picked = (await CardSelectCmd.FromDeckGeneric(Owner, prefs, c => c is ICreatureCard)).FirstOrDefault();
        if (picked is not ICreatureCard creature)
        {
            return false;
        }
        creature.Warm(Buff);
        return true;
    }
}
