using BaseLib.Extensions;
using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// An enemy the Fish Hook pulled out of a fight, kept in the deck as a creature card: its Power from the enemy's hit,
/// its Health from the enemy's HP, its portrait the photograph taken as it was hooked. A token, so it never appears as a
/// reward, but it costs 1 energy and 2 Blood like an Uncommon.
/// </summary>
public sealed class HookedCard() : CreatureCard<HookedCreature>(Bestiary.Hooked, CardRarity.Token, energy: 1)
{
    private ModelId? _monster;
    private int _power;
    private int _health;
    private string _photo = "";

    [SavedProperty]
    public ModelId? Inscryption_HookedMonster
    {
        get => _monster;
        set
        {
            AssertMutable();
            _monster = value;
        }
    }

    [SavedProperty]
    public int Inscryption_HookedPower
    {
        get => _power;
        set
        {
            AssertMutable();
            _power = value;
            Restat();
        }
    }

    [SavedProperty]
    public int Inscryption_HookedHealth
    {
        get => _health;
        set
        {
            AssertMutable();
            _health = value;
            Restat();
        }
    }

    [SavedProperty]
    public string Inscryption_HookedPhoto
    {
        get => _photo;
        set
        {
            AssertMutable();
            _photo = value ?? "";
        }
    }

    private void Restat()
    {
        if (_power > 0 && _health > 0)
        {
            SetStats(Bestiary.Hooked with { Power = _power, Health = _health });
        }
    }

    /// <summary>Fill in a fresh catch.</summary>
    public void Catch(ModelId monster, int power, int health, string photo)
    {
        Inscryption_HookedMonster = monster;
        Inscryption_HookedPower = power;
        Inscryption_HookedHealth = health;
        Inscryption_HookedPhoto = photo;
    }

    private string MonsterName =>
        _monster != null && ModelDb.GetByIdOrNull<MonsterModel>(_monster) is { } monster
            ? monster.Title.GetFormattedText()
            : new LocString("cards", "INSCRYPTION-HOOKED_CARD.unknownMonster").GetFormattedText();

    public override string Title
    {
        get
        {
            var title = new LocString("cards", "INSCRYPTION-HOOKED_CARD.title");
            title.Add("Monster", MonsterName);
            return IsUpgraded ? title.GetFormattedText() + "+" : title.GetFormattedText();
        }
    }

    private string? PhotoPortrait =>
        _photo != "" && HookPhoto.Load(HookPhoto.PortraitPath(_photo)) != null ? HookPhoto.PortraitPath(_photo) : null;

    public override string CustomPortraitPath => PhotoPortrait ?? "hooked_card.png".BigCardImagePath();
    public override string PortraitPath => PhotoPortrait ?? "hooked_card.png".CardImagePath();

    public override void CopyStateFrom(CardModel source)
    {
        base.CopyStateFrom(source);
        if (source is HookedCard catchCard)
        {
            Catch(catchCard._monster!, catchCard._power, catchCard._health, catchCard._photo);
        }
    }
}
