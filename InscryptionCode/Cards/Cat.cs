using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// The Cat has nine lives: sacrificing it pays its Blood and leaves it on the board (Many Lives). The ninth
/// sacrifice spends the last life: the card in the deck becomes the Undead Cat, and the Cat still on the board has
/// no lives left, so its next sacrifice kills it.
/// </summary>
public sealed class Cat() : CreatureCard<CatCreature>(Bestiary.Cat, CardRarity.Common)
{
    private const int Lives = 9;

    private int _livesLost;

    [SavedProperty]
    public int Inscryption_LivesLost
    {
        get => _livesLost;
        set
        {
            AssertMutable();
            _livesLost = value;
            DynamicVars["Lives"].BaseValue = LivesLeft;
        }
    }

    public int LivesLeft => Math.Max(0, Lives - _livesLost);

    protected override IEnumerable<DynamicVar> CanonicalVars => [.. base.CanonicalVars, new DynamicVar("Lives", Lives)];

    public override void CopyStateFrom(CardModel source)
    {
        base.CopyStateFrom(source);
        if (source is Cat cat)
        {
            Inscryption_LivesLost = cat._livesLost;
        }
    }

    /// <summary>Spend one life. On the deck's card, the last one turns it into the Undead Cat.</summary>
    public async Task LoseLife()
    {
        Inscryption_LivesLost += 1;
        if (_livesLost >= Lives && Pile?.Type == PileType.Deck)
        {
            await CardCmd.TransformTo<UndeadCat>(this);
        }
    }
}
