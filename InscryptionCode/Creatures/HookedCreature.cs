using BaseLib.Extensions;
using BaseLib.Utils.NodeFactories;
using Godot;
using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Extensions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// An enemy hooked by the Fish Hook, fighting for Luke: its stats, name and photograph come from the
/// <see cref="HookedCard"/> that summons it.
/// </summary>
public sealed class HookedCreature : BoardCreature
{
    /// <summary>The photo key of the catch being summoned; <see cref="Summoning"/> sets it just before the pet is made.</summary>
    internal static string? PendingPhoto;

    private CreatureStats _stats = Bestiary.Hooked;
    private ModelId? _monster;

    public override CreatureStats Stats => _stats;
    public override CardModel Card => ModelDb.Card<HookedCard>();

    public override LocString Title =>
        _monster != null && ModelDb.GetByIdOrNull<MonsterModel>(_monster) is { } monster ? monster.Title : base.Title;

    private static string StockArt => "creatures/hooked_creature.png".ImagePath();

    public override IEnumerable<string> AssetPaths => [StockArt];

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        var photo = PendingPhoto != null ? HookPhoto.Load(HookPhoto.SpritePath(PendingPhoto)) : null;
        PendingPhoto = null;
        return photo != null
            ? NodeFactory<NCreatureVisuals>.CreateFromResource(photo)
            : NodeFactory<NCreatureVisuals>.CreateFromResource(StockArt);
    }

    /// <summary>Take on the catch's stats and name (called right after the pet is made).</summary>
    public void Catch(HookedCard card)
    {
        _stats = card.Stats;
        _monster = card.Inscryption_HookedMonster;
    }
}
