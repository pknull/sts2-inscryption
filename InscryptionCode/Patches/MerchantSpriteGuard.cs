using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Inscryption.InscryptionCode.Patches;

/// <summary>
/// Luke's merchant scene is a plain Sprite2D, which BaseLib converts to an <see cref="NMerchantCharacter"/> when the
/// shop instantiates it. That auto-conversion does not mark the node as factory-made, so BaseLib's own guard lets
/// the game's <c>_Ready</c> run, and it throws trying to play a Spine animation on the sprite. Skip the animation
/// calls whenever the merchant character's visual is not a Spine sprite; Spine characters are untouched.
/// </summary>
[HarmonyPatch(typeof(NMerchantCharacter))]
internal static class MerchantSpriteGuard
{
    [HarmonyPatch(nameof(NMerchantCharacter._Ready))]
    [HarmonyPrefix]
    private static bool Ready(NMerchantCharacter __instance) => IsSpine(__instance);

    [HarmonyPatch(nameof(NMerchantCharacter.PlayAnimation))]
    [HarmonyPrefix]
    private static bool PlayAnimation(NMerchantCharacter __instance) => IsSpine(__instance);

    private static bool IsSpine(Node node) =>
        node.GetChildCount() > 0 && node.GetChild(0).GetClass() == MegaSprite.spineClassName;
}
