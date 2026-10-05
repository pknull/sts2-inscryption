using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Inscryption.InscryptionCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Relics;

namespace Inscryption.InscryptionCode.Character;

public class Inscryption : PlaceholderCharacterModel
{
    public const string CharacterId = "Inscryption";
    
    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 70;
    
    // Provisional. Squirrels come from the SideDeck relic, not the deck.
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<Stoat>(),
        ModelDb.Card<Stoat>(),
        ModelDb.Card<Stoat>(),
        ModelDb.Card<Stoat>(),
        ModelDb.Card<Bullfrog>(),
        ModelDb.Card<Bullfrog>(),
        ModelDb.Card<Bullfrog>(),
        ModelDb.Card<Wolf>(),
        ModelDb.Card<Wolf>(),
        ModelDb.Card<Wolf>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<SideDeck>()
    ];
    
    public override CardPoolModel CardPool => ModelDb.CardPool<InscryptionCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<InscryptionRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<InscryptionPotionPool>();
    
    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets. 
        These are just some of the simplest assets, given some placeholders to differentiate your character with. 
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    public override string CustomIconTexturePath => "character_icon.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_icon.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_icon_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker.png".CharacterUiPath();

    public override string CustomCharacterSelectBg => $"{MainFile.ResPath}/scenes/char_select_bg.tscn";

    // Single-Sprite2D scenes; BaseLib converts them into the rest site and merchant characters.
    public override string CustomRestSiteAnimPath => $"{MainFile.ResPath}/scenes/rest_site.tscn";
    public override string CustomMerchantAnimPath => $"{MainFile.ResPath}/scenes/merchant.tscn";

    // A static image instead of the placeholder's Ironclad skeleton. The energy counter and card trail are still
    // Ironclad's placeholders.
    public override NCreatureVisuals? CreateCustomVisuals() =>
        NodeFactory<NCreatureVisuals>.CreateFromResource("body.png".CharacterUiPath());
}