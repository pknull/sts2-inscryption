using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Inscryption lets you place a card in the slot you choose. A creature card is dragged up like any untargeted card;
/// where it is let go picks the lane: the nearest lane slot on your side, or the enemy facing a lane, whichever is
/// closest across the screen. Every lane can be chosen, with or without an enemy in it. Keyboard and controller plays
/// have no drop point and take the lowest empty lane.
/// </summary>
internal static class LaneDrop
{
    private static readonly ConditionalWeakTable<CardModel, StrongBox<int>> Dropped = new();

    /// <summary>The lane the card was dropped on (0-3), or -1; forgets it.</summary>
    public static int Take(CardModel card)
    {
        if (!Dropped.TryGetValue(card, out var lane))
        {
            return -1;
        }
        Dropped.Remove(card);
        return lane.Value;
    }

    // The creature card being dragged, while it is; its lane is lit up every frame (LaneMarkers.HighlightLane).
    private static NMouseCardPlay? _dragging;

    public static bool IsDragging => _dragging != null;

    private static void Tick()
    {
        if (_dragging == null || !GodotObject.IsInstanceValid(_dragging) || _dragging.Holder?.CardModel is not { } card
            || NCombatRoom.Instance is not { } room)
        {
            Stop();
            return;
        }
        // The card plays only once dragged up past the game's play line (three quarters down the screen).
        var viewport = room.GetViewport();
        bool inPlayZone = viewport.GetMousePosition().Y < viewport.GetVisibleRect().Size.Y * 0.75f;
        LaneMarkers.HighlightLane(inPlayZone ? LaneAt(room, card.Owner, room.GetGlobalMousePosition()) : -1);
    }

    private static void Stop()
    {
        if (_dragging != null && Engine.GetMainLoop() is SceneTree tree)
        {
            tree.ProcessFrame -= Tick;
        }
        _dragging = null;
        LaneMarkers.HighlightLane(-1);
    }

    [HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay.Start))]
    private static class OnDragStart
    {
        [HarmonyPostfix]
        private static void Postfix(NMouseCardPlay __instance)
        {
            if (__instance.Holder?.CardModel is not { } card || card is not ICreatureCard || !Board.UsesLanes(card.Owner))
            {
                return;
            }
            if (_dragging == null && Engine.GetMainLoop() is SceneTree tree)
            {
                tree.ProcessFrame += Tick;
            }
            _dragging = __instance;
        }
    }

    [HarmonyPatch(typeof(NMouseCardPlay), nameof(NMouseCardPlay._ExitTree))]
    private static class OnDragEnd
    {
        [HarmonyPostfix]
        private static void Postfix(NMouseCardPlay __instance)
        {
            if (__instance == _dragging)
            {
                Stop();
            }
        }
    }

    /// <summary>The lane whose slot or facing enemy is nearest to <paramref name="point"/> across the screen.</summary>
    private static int LaneAt(NCombatRoom room, Player player, Vector2 point)
    {
        int best = -1;
        float bestDistance = float.MaxValue;
        void Consider(int lane, Vector2? anchor)
        {
            if (anchor is { } at && Mathf.Abs(at.X - point.X) < bestDistance)
            {
                bestDistance = Mathf.Abs(at.X - point.X);
                best = lane;
            }
        }
        for (int lane = 0; lane < Board.LaneCount; lane++)
        {
            Consider(lane, BoardLayout.SlotGlobal(room, player, lane));
        }
        foreach (var enemy in Board.Enemies(player.Creature.CombatState))
        {
            int lane = Board.LaneOf(enemy);
            if (lane >= 0)
            {
                Consider(lane, room.GetCreatureNode(enemy)?.GlobalPosition);
            }
        }
        return best;
    }

    // The mouse is where the card was let go when the play is attempted; the play itself may wait in a queue.
    [HarmonyPatch(typeof(NCardPlay), "TryPlayCard")]
    private static class OnTryPlayCard
    {
        [HarmonyPrefix]
        private static void Prefix(NCardPlay __instance)
        {
            var card = __instance.Holder?.CardModel;
            if (__instance is not NMouseCardPlay || card is not ICreatureCard || NCombatRoom.Instance is not { } room
                || !Board.UsesLanes(card.Owner))
            {
                return;
            }
            int lane = LaneAt(room, card.Owner, room.GetGlobalMousePosition());
            if (lane >= 0)
            {
                Dropped.AddOrUpdate(card, new StrongBox<int>(lane));
            }
        }
    }
}
