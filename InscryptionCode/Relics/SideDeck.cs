using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.RestSite;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Inscryption.InscryptionCode.Relics;

/// <summary>
/// Starting relic carrying Inscryption's economy: the Squirrel side deck (one Squirrel into your hand each turn,
/// ten per combat) and what follows a creature's death: Bones and the death sigils (<see cref="Afterlife"/>).
/// </summary>
public sealed class SideDeck : InscryptionRelic
{
    private int _squirrelsLeft;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(Balance.SideDeckSize)];

    private int SquirrelsLeft
    {
        get => _squirrelsLeft;
        set
        {
            AssertMutable();
            _squirrelsLeft = value;
        }
    }

    public override Task BeforeCombatStart()
    {
        SquirrelsLeft = Balance.SideDeckSize;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || SquirrelsLeft <= 0 || Owner.Creature.CombatState == null)
        {
            return;
        }
        SquirrelsLeft--;
        Flash();
        var squirrel = Owner.Creature.CombatState.CreateCard<Squirrel>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(squirrel, PileType.Hand, Owner);
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
        {
            return false;
        }
        options.Add(new CampfireRestSiteOption(player));
        return true;
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength) =>
        Afterlife.AfterDeath(choiceContext, Owner, creature);
}
