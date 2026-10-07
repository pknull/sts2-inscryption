using Inscryption.InscryptionCode.Creatures;
using Inscryption.InscryptionCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Inscryption.InscryptionCode.Cards;

/// <summary>
/// Inscryption's Woodcarver totem as a Power card: a head (the tribe) and a body (a sigil). For the rest of the
/// combat every creature of the tribe, on the board or summoned later, has the sigil. The body is rolled when the
/// card is created, as the Woodcarver offers a random body, and saved with the card.
/// </summary>
public abstract class TotemCard(Tribe tribe) : InscryptionCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    private int _sigil;

    [SavedProperty]
    public int Inscryption_TotemSigil
    {
        get => _sigil;
        set
        {
            AssertMutable();
            if (_sigil != 0)
            {
                RemoveKeyword(Sigils.Keyword((Sigil)_sigil));
            }
            _sigil = value;
            if (value != 0)
            {
                AddKeyword(Sigils.Keyword((Sigil)value));
            }
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [Sigils.Keyword(tribe)];

    public override void AfterCreated()
    {
        base.AfterCreated();
        if (_sigil == 0 && Owner != null)
        {
            // Seeded per player, card and how many of this totem the deck already holds, so it is stable on reload.
            int copies = Owner.Deck.Cards.Count(c => c.Id == Id);
            Inscryption_TotemSigil = (int)new Rng(Owner, Id, (uint)copies).NextItem(Sigils.ModularFor(tribe));
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_sigil == 0 || Owner.Creature.CombatState == null)
        {
            return;
        }
        Sigils.AddTotem(Owner.Creature.CombatState, Owner, tribe, (Sigil)_sigil);
        // A Corpse Eater must survive the end-of-turn discard to act, as printed Corpse Eaters Retain. Cards already
        // in combat get Retain now; TotemPower gives it to the tribe's cards made later.
        foreach (var card in Owner.PlayerCombatState?.AllCards.ToList() ?? [])
        {
            TotemPower.RetainIfCorpseEater(card);
        }
        await PowerCmd.Apply<TotemPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        BoardLayout.Refresh();
    }
}

public sealed class CanineTotem() : TotemCard(Tribe.Canine);

public sealed class HoovedTotem() : TotemCard(Tribe.Hooved);

public sealed class ReptileTotem() : TotemCard(Tribe.Reptile);

public sealed class AvianTotem() : TotemCard(Tribe.Avian);

public sealed class InsectTotem() : TotemCard(Tribe.Insect);

public sealed class SquirrelTotem() : TotemCard(Tribe.Squirrel);
