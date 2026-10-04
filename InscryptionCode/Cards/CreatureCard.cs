using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>Inscryption's campfire warms a creature for +1 Power or +2 Health; each campfire offers one.</summary>
public enum CampfireBuff
{
    Power,
    Health,
}

/// <summary>Non-generic handle on creature cards, for the campfire and other deck-wide effects.</summary>
public interface ICreatureCard
{
    void Warm(CampfireBuff buff);
}

/// <summary>
/// An Inscryption creature card. Playing it pays its Blood (sacrificing creatures already on the board) or its
/// Bones, then summons <typeparamref name="TCreature"/> into the lowest empty lane. It costs no energy: Blood and
/// Bones are the costs, as in Inscryption. Upgrading (Smith) lowers that cost by 1; the campfire raises the
/// creature's stats for the run. Attackers (Power at least Health) are Attack cards; defenders and free creatures
/// are Skills.
/// </summary>
public abstract class CreatureCard<TCreature>(CreatureStats stats, CardRarity rarity)
    : InscryptionCard(0, TypeFor(stats), rarity, TargetType.Self), ICreatureCard where TCreature : BoardCreature
{
    private static CardType TypeFor(CreatureStats s) =>
        s.IsFree || s.Power < s.Health ? CardType.Skill : CardType.Attack;

    private int _powerBonus;
    private int _healthBonus;

    /// <summary>Inscryption Power added at campfires (before <see cref="Balance.PowerScale"/>).</summary>
    [SavedProperty]
    public int Inscryption_PowerBonus
    {
        get => _powerBonus;
        set
        {
            AssertMutable();
            _powerBonus = value;
            DynamicVars["Power"].BaseValue = Power;
        }
    }

    /// <summary>Inscryption Health added at campfires (before <see cref="Balance.HealthScale"/>).</summary>
    [SavedProperty]
    public int Inscryption_HealthBonus
    {
        get => _healthBonus;
        set
        {
            AssertMutable();
            _healthBonus = value;
            DynamicVars["Health"].BaseValue = Health;
        }
    }

    private int Blood => DynamicVars["Blood"].IntValue;
    private int Bones => DynamicVars["Bones"].IntValue;
    private int Power => (stats.Power + _powerBonus) * Balance.PowerScale;
    private int Health => (stats.Health + _healthBonus) * Balance.HealthScale;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Blood", stats.Blood),
        new DynamicVar("Bones", stats.Bones),
        new DynamicVar("Power", Power),
        new DynamicVar("Health", Health),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [InscryptionKeywords.Creature];

    // Free creatures have no cost to lower, and Inscryption never upgrades Squirrels.
    public override int MaxUpgradeLevel => stats.IsFree ? 0 : 1;

    protected override void OnUpgrade()
    {
        DynamicVars[stats.Blood > 0 ? "Blood" : "Bones"].UpgradeValueBy(-1);
    }

    public void Warm(CampfireBuff buff)
    {
        if (buff == CampfireBuff.Power)
        {
            Inscryption_PowerBonus += 1;
        }
        else
        {
            Inscryption_HealthBonus += 2;
        }
    }

    protected override bool IsPlayable
    {
        get
        {
            int onBoard = Board.Creatures(Owner).Count;
            return onBoard >= Blood && onBoard - Blood < Balance.MaxCreatures
                && Owner.Creature.GetPowerAmount<BonesPower>() >= Bones;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var sacrifice in await ChooseSacrifices(choiceContext))
        {
            await CreatureCmd.Kill(sacrifice);
        }
        if (Bones > 0)
        {
            await PowerCmd.Apply<BonesPower>(choiceContext, Owner.Creature, -Bones, Owner.Creature, this);
        }
        var creature = await PlayerCmd.AddPet<TCreature>(Owner);
        // Take the lowest empty lane now, in game logic, rather than whenever the UI first looks.
        Board.LaneOf(creature);
        var board = (BoardCreature)creature.Monster!;
        // Campfire bonuses live on the card; carry them onto this summon.
        board.BonusPower = _powerBonus;
        if (Health != creature.MaxHp)
        {
            await CreatureCmd.SetMaxHp(creature, Health);
            await CreatureCmd.SetCurrentHp(creature, Health);
        }
        await PowerCmd.Apply<CreaturePower>(choiceContext, creature, 1m, Owner.Creature, this);
        board.ShowIntent();
    }

    /// <summary>
    /// The player picks which creatures pay the Blood, as in Inscryption. Each creature on the board is shown as
    /// its card, in lane order; with no real choice (exactly enough creatures) the grid skips itself.
    /// </summary>
    private async Task<List<Creature>> ChooseSacrifices(PlayerChoiceContext choiceContext)
    {
        var onBoard = Board.Creatures(Owner);
        var combatState = Owner.Creature.CombatState;
        if (Blood == 0 || combatState == null)
        {
            return [];
        }
        var options = onBoard.Select(c => combatState.CreateCard(((BoardCreature)c.Monster!).Card, Owner)).ToList();
        try
        {
            var picked = await CardSelectCmd.FromSimpleGrid(choiceContext, options, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, Blood));
            // By reference: two Stoats are distinct creatures even if their cards compare equal.
            return picked.Select(card => onBoard[options.FindIndex(o => ReferenceEquals(o, card))]).ToList();
        }
        finally
        {
            // The stand-in cards were only for display; keep them out of the combat's card list.
            foreach (var option in options)
            {
                combatState.RemoveCard(option);
            }
        }
    }
}
