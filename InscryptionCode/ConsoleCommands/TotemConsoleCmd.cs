using Inscryption.InscryptionCode.Cards;
using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.ConsoleCommands;

/// <summary>
/// Dev console (debug builds of the console only): <c>totem reptile boneking</c> puts a Reptile Totem carrying Bone
/// King in the hand. A Totem's sigil is otherwise a seeded roll, so a test could not ask for a particular one.
/// </summary>
public sealed class TotemConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "totem";
    public override string Args => "<tribe:string> <sigil:string>";
    public override string Description => "Add a Luke Carder Totem of a tribe, carrying a sigil, to the hand ('totem reptile boneking').";
    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length != 2 || issuingPlayer == null)
        {
            return new CmdResult(success: false, "Usage: totem " + Args);
        }
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (!CombatManager.Instance.IsInProgress || combatState == null)
        {
            return new CmdResult(success: false, "Totems can only be added during combat.");
        }
        if (!TryParseEnum<Tribe>(args[0], out var tribe) || !Sigils.Tribes.Contains(tribe))
        {
            return new CmdResult(success: false, "Unknown tribe '" + args[0] + "'. Tribes: " + string.Join(", ", Sigils.Tribes));
        }
        if (!TryParseEnum<Sigil>(args[1], out var sigil) || sigil == Sigil.None)
        {
            return new CmdResult(success: false, "Unknown sigil '" + args[1] + "'. Sigils: " + string.Join(", ", Sigils.Modular));
        }
        CardModel canonical = tribe switch
        {
            Tribe.Canine => ModelDb.Card<CanineTotem>(),
            Tribe.Hooved => ModelDb.Card<HoovedTotem>(),
            Tribe.Reptile => ModelDb.Card<ReptileTotem>(),
            Tribe.Avian => ModelDb.Card<AvianTotem>(),
            Tribe.Insect => ModelDb.Card<InsectTotem>(),
            _ => ModelDb.Card<SquirrelTotem>(),
        };
        var card = (TotemCard)combatState.CreateCard(canonical, issuingPlayer);
        card.Inscryption_TotemSigil = (int)sigil;
        return new CmdResult(CardPileCmd.Add(card, PileType.Hand), success: true, $"Added a {tribe} Totem carrying {sigil}");
    }

    public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
    {
        if (args.Length <= 1)
        {
            return CompleteArgument(Sigils.Tribes.Select(t => t.ToString().ToLowerInvariant()), [], args.FirstOrDefault() ?? "");
        }
        if (args.Length == 2)
        {
            return CompleteArgument(Sigils.Modular.Select(s => s.ToString().ToLowerInvariant()), [args[0]], args[1]);
        }
        return new CompletionResult { Type = CompletionType.Argument, ArgumentContext = CmdName };
    }
}
