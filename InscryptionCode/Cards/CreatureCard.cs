using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>Inscryption's campfire warms a creature for +1 Power or +2 Health; each campfire offers one.</summary>
public enum CampfireBuff
{
    Power,
    Health,
}

/// <summary>Non-generic handle on creature cards, for the campfire, death sigils and other deck-wide effects.</summary>
public interface ICreatureCard
{
    CreatureStats Stats { get; }

    void Warm(CampfireBuff buff);

    /// <summary>Raise the card's Inscryption Power and Health (campfire, Ouroboros).</summary>
    void Grow(int power, int health);

    /// <summary>Corpse Eater: the next automatic play of this card is free and summons into <paramref name="lane"/>.</summary>
    void PlayFreeInto(int lane);

    /// <summary>Show what <paramref name="source"/> carries (campfire bonuses, a Cat's lives) on this stand-in card.</summary>
    void CopyStateFrom(CardModel source);
}

/// <summary>
/// An Inscryption creature card. Playing it pays its Blood (sacrificing creatures already on the board, see
/// <see cref="Sacrifice"/>) or its Bones, then summons <typeparamref name="TCreature"/> into the lowest empty lane.
/// It costs no energy: Blood and
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

    // Corpse Eater's lane for the next automatic play; -1 when none is pending (combat only, not saved).
    private int _freeLane = -1;

    public CreatureStats Stats => stats;

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

    // Corpse Eater acts from the hand, and creatures mostly die on the enemy's turn, after the hand is discarded; in
    // Inscryption the hand persists, so these cards Retain.
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        stats.Sigils.Contains(Sigil.CorpseEater)
            ? [InscryptionKeywords.Creature, .. Sigils.Keywords(stats), CardKeyword.Retain]
            : [InscryptionKeywords.Creature, .. Sigils.Keywords(stats)];

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
            Grow(1, 0);
        }
        else
        {
            Grow(0, 2);
        }
    }

    public void Grow(int power, int health)
    {
        Inscryption_PowerBonus += power;
        Inscryption_HealthBonus += health;
    }

    public void PlayFreeInto(int lane) => _freeLane = lane;

    public virtual void CopyStateFrom(CardModel source)
    {
        if (source is CreatureCard<TCreature> card)
        {
            Inscryption_PowerBonus = card._powerBonus;
            Inscryption_HealthBonus = card._healthBonus;
        }
    }

    protected override bool IsPlayable =>
        Sacrifice.CanPay(Owner, Blood) && Owner.Creature.GetPowerAmount<BonesPower>() >= Bones;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Corpse Eater: played automatically into a fallen creature's lane, costing nothing.
        if (cardPlay.IsAutoPlay && _freeLane >= 0)
        {
            int lane = _freeLane;
            _freeLane = -1;
            await Summoning.Summon<TCreature>(choiceContext, Owner, stats, _powerBonus, _healthBonus, this, lane);
            return;
        }
        await Sacrifice.Perform(await ChooseSacrifices(choiceContext));
        if (Bones > 0)
        {
            await PowerCmd.Apply<BonesPower>(choiceContext, Owner.Creature, -Bones, Owner.Creature, this);
        }
        // Campfire bonuses live on the card; carry them onto this summon.
        await Summoning.Summon<TCreature>(choiceContext, Owner, stats, _powerBonus, _healthBonus, this);
    }

    /// <summary>
    /// The player picks which creatures pay the Blood, as in Inscryption. When every creature is worth 1 Blood and any
    /// pick leaves a lane free, one grid asks for exactly enough (and skips itself when there is no real choice).
    /// Otherwise (Worthy Sacrifice, or Many Lives on a full board) creatures are picked one at a time until the Blood
    /// is paid, and only picks that can still finish with a free lane are offered.
    /// </summary>
    private async Task<List<Creature>> ChooseSacrifices(PlayerChoiceContext choiceContext)
    {
        if (Blood == 0)
        {
            return [];
        }
        var options = Sacrifice.Candidates(Owner);
        int empty = Sacrifice.EmptyLanes(Owner);
        if (options.All(c => Sacrifice.Value(c) == 1) && (empty > 0 || !options.Any(Sacrifice.Stays)))
        {
            return await Pick(choiceContext, options, Blood, SelectionScreenPrompt);
        }
        var picked = new List<Creature>();
        int paid = 0;
        while (paid < Blood)
        {
            var allowed = options.Where(c => !picked.Contains(c) && Sacrifice.Feasible(picked, c, options, Blood, empty))
                .ToList();
            var prompt = new LocString("cards", "INSCRYPTION-SACRIFICE.selectionScreenPrompt");
            prompt.Add("Blood", Blood - paid);
            var choice = (await Pick(choiceContext, allowed, 1, prompt)).FirstOrDefault();
            if (choice == null)
            {
                break;
            }
            picked.Add(choice);
            paid += Sacrifice.Value(choice);
        }
        return picked;
    }

    /// <summary>The card shown for a creature on the sacrifice screen, carrying its summoning card's state.</summary>
    private CardModel StandIn(ICombatState combatState, BoardCreature creature)
    {
        var card = combatState.CreateCard(creature.Card, Owner);
        if (creature.SourceCard != null && card is ICreatureCard standIn)
        {
            standIn.CopyStateFrom(creature.SourceCard);
        }
        return card;
    }

    /// <summary>Show <paramref name="creatures"/> as their cards, in lane order, and return the ones picked.</summary>
    private async Task<List<Creature>> Pick(PlayerChoiceContext choiceContext, List<Creature> creatures, int count,
        LocString prompt)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null || creatures.Count == 0)
        {
            return [];
        }
        var cards = creatures.Select(c => StandIn(combatState, (BoardCreature)c.Monster!)).ToList();
        try
        {
            var picked = await CardSelectCmd.FromSimpleGrid(choiceContext, cards, Owner,
                new CardSelectorPrefs(prompt, Math.Min(count, cards.Count)));
            // By reference: two Stoats are distinct creatures even if their cards compare equal.
            return picked.Select(card => creatures[cards.FindIndex(o => ReferenceEquals(o, card))]).ToList();
        }
        finally
        {
            // The stand-in cards were only for display; keep them out of the combat's card list.
            foreach (var card in cards)
            {
                combatState.RemoveCard(card);
            }
        }
    }
}
