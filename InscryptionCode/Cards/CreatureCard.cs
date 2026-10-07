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
/// An Inscryption creature card. It is dragged to a lane (<see cref="LaneDrop"/>): it pays its Blood (sacrificing
/// creatures already on the board, see <see cref="Sacrifice"/>) or its Bones, then summons
/// <typeparamref name="TCreature"/> into that lane, or the lowest empty lane if that one is taken. Blood and Bones are
/// the costs, as in Inscryption, plus energy by rarity (<see cref="Balance.EnergyCost"/>) so summoning shares the
/// turn's energy with Luke's other cards. Upgrading (Smith) lowers the Blood or Bones by 1; the campfire raises the
/// creature's stats for the run. Every creature card is a Skill: summoning is not an attack.
/// </summary>
public abstract class CreatureCard<TCreature>(CreatureStats stats, CardRarity rarity, CardType type = CardType.Skill,
    int? energy = null)
    : InscryptionCard(energy ?? Balance.EnergyCost(rarity), type, rarity, TargetType.Self), ICreatureCard
    where TCreature : BoardCreature
{
    private CreatureStats _stats = stats;
    private readonly int _energy = energy ?? Balance.EnergyCost(rarity);
    private int _powerBonus;
    private int _healthBonus;

    // Corpse Eater's lane for the next automatic play; -1 when none is pending (combat only, not saved).
    private int _freeLane = -1;

    public CreatureStats Stats => _stats;

    /// <summary>A Fish Hook catch takes its stats from the enemy it was; set them on this card and its text.</summary>
    protected void SetStats(CreatureStats value)
    {
        AssertMutable();
        _stats = value;
        DynamicVars["Blood"].BaseValue = value.Blood;
        DynamicVars["Bones"].BaseValue = value.Bones;
        ((StatVar)DynamicVars["Power"]).Reprint(Power, Balance.Damage(value.Power, value, _energy));
        ((StatVar)DynamicVars["Health"]).Reprint(Health, Balance.Health(value.Health, _energy));
    }

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
    private int Power => Balance.Damage(_stats.Power + _powerBonus, _stats, _energy);
    private int Health => Balance.Health(_stats.Health + _healthBonus, _energy);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Blood", _stats.Blood),
        new DynamicVar("Bones", _stats.Bones),
        // Set only on the sacrifice screen's stand-in cards, so the text names the lane being given up.
        new DynamicVar("Lane", 0),
        new StatVar("Power", Power, Balance.Damage(_stats.Power, _stats, _energy)),
        new StatVar("Health", Health, Balance.Health(_stats.Health, _energy)),
    ];

    /// <summary>
    /// Power or Health, compared with the printed card when shown: the card text (<c>{Power:diff()}</c>) turns green
    /// when campfires or Ouroboros have raised it, the way the game shows an upgraded value.
    /// </summary>
    private sealed class StatVar(string name, decimal value, decimal printed) : DynamicVar(name, value)
    {
        private decimal _printed = printed;

        public void Reprint(decimal value, decimal printedValue)
        {
            BaseValue = value;
            _printed = printedValue;
        }

        public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
        {
            EnchantedValue = _printed;
            PreviewValue = BaseValue;
        }
    }

    // Corpse Eater acts from the hand, and creatures mostly die on the enemy's turn, after the hand is discarded; in
    // Inscryption the hand persists, so these cards Retain.
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        _stats.Sigils.Contains(Sigil.CorpseEater)
            ? [InscryptionKeywords.Creature, .. Sigils.Keywords(_stats), CardKeyword.Retain]
            : [InscryptionKeywords.Creature, .. Sigils.Keywords(_stats)];

    // Free creatures have no cost to lower, and Inscryption never upgrades Squirrels.
    public override int MaxUpgradeLevel => _stats.IsFree ? 0 : 1;

    protected override void OnUpgrade()
    {
        DynamicVars[_stats.Blood > 0 ? "Blood" : "Bones"].UpgradeValueBy(-1);
    }

    public void Warm(CampfireBuff buff)
    {
        if (buff == CampfireBuff.Power)
        {
            Grow(Balance.CampfirePower, 0);
        }
        else
        {
            Grow(0, Balance.CampfireHealth);
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
            await Summoning.Summon<TCreature>(choiceContext, Owner, _stats, _powerBonus, _healthBonus, this, lane);
            return;
        }
        // The lane it was dropped on (sent to the other players in co-op); sacrifices may free it.
        int chosenLane = await LaneDrop.Choose(choiceContext, this);
        await Sacrifice.Perform(await ChooseSacrifices(choiceContext));
        if (Bones > 0)
        {
            await PowerCmd.Apply<BonesPower>(choiceContext, Owner.Creature, -Bones, Owner.Creature, this);
        }
        // Campfire bonuses live on the card; carry them onto this summon.
        await Summoning.Summon<TCreature>(choiceContext, Owner, _stats, _powerBonus, _healthBonus, this, chosenLane);
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
            return await Sacrifice.Pick(choiceContext, Owner, options, Blood, SelectionScreenPrompt);
        }
        var picked = new List<Creature>();
        int paid = 0;
        while (paid < Blood)
        {
            var allowed = options.Where(c => !picked.Contains(c) && Sacrifice.Feasible(picked, c, options, Blood, empty))
                .ToList();
            var prompt = new LocString("cards", "INSCRYPTION-SACRIFICE.selectionScreenPrompt");
            prompt.Add("Blood", Blood - paid);
            var choice = (await Sacrifice.Pick(choiceContext, Owner, allowed, 1, prompt)).FirstOrDefault();
            if (choice == null)
            {
                break;
            }
            picked.Add(choice);
            paid += Sacrifice.Value(choice);
        }
        return picked;
    }
}
