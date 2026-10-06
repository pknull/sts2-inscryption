using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// Lays the player's creatures out as Inscryption's board: four fixed lane slots in a row in front of the player,
/// lane 1 nearest. Each creature is fitted inside its slot, so the row has the same footprint whatever is on it and
/// never depends on where the enemies stand; the player never moves.
/// In co-op the players stand side by side, this machine's player nearest the enemies, so every Luke's row starts in
/// front of that player: lane 3 of each Luke lines up with enemy lane 3. This machine's Luke has the front row; each
/// other Luke's row stands further back (up) and a little smaller. When this machine's player is not a Luke, the
/// front row is left to him and the Lukes' rows all stand behind it.
/// <c>NCombatRoom.AddCreature</c> instead spreads pets across the owner's feet and makes them non-interactable,
/// which hides their HP bars; Osty is exempted from that the same way.
/// </summary>
internal static class BoardLayout
{
    public const float SlotWidth = 115f;
    public const float SlotHeight = 110f;
    private const float Gap = 10f;

    /// <summary>Lanes 2 and 4 stand this far back (up), so neighbouring health bars and statuses don't collide.</summary>
    public const float BackRowLift = 40f;

    /// <summary>Each other Luke's row stands this much further back, at <see cref="RowScale"/>.</summary>
    private const float RowLift = 190f;

    /// <summary>
    /// The back row stands at most this far back, so four rows (four Lukes, or three behind a non-Luke) fit.
    /// </summary>
    private const float MaxLift = 400f;

    private const float RowScale = 0.8f;

    private const string BaseScale = "inscryption_base_scale";

    /// <summary>
    /// Each Luke with his row, front to back: this machine's player first, then the others in player order. The front
    /// row (0) belongs to this machine's player, so when he is not a Luke the Lukes' rows start at 1.
    /// </summary>
    private static List<(Player Luke, int Row)> Rows(ICombatState? state)
    {
        var me = LocalContext.GetMe(state);
        int first = me != null && Board.UsesLanes(me) ? 0 : 1;
        return state?.Players.Where(Board.UsesLanes).OrderBy(p => p == me ? 0 : 1).Select((p, i) => (p, i + first))
            .ToList() ?? [];
    }

    private static void Arrange(NCombatRoom room, Player player, int row)
    {
        foreach (var creature in Board.Shown.Creatures(player))
        {
            var node = room.GetCreatureNode(creature);
            int lane = Board.Shown.LaneOf(creature);
            if (node == null || Slot(room, player, lane) is not { } slot)
            {
                continue;
            }
            // SetScaleAndHue overwrites DefaultScale, so fit from the scale the creature arrived with.
            if (!node.HasMeta(BaseScale))
            {
                node.SetMeta(BaseScale, node.Visuals.DefaultScale);
            }
            float baseScale = node.GetMeta(BaseScale).AsSingle();
            float width = node.Visuals.Bounds.Size.X * baseScale;
            float height = node.Visuals.Bounds.Size.Y * baseScale;
            float fit = Mathf.Min(1f, Mathf.Min(SlotWidth / width, SlotHeight / height)) * (row > 0 ? RowScale : 1f);

            // Each creature sits in its own lane's slot; empty lanes stay empty. Lanes 1 and 3 of the front row
            // stand in front.
            node.Position = slot;
            node.ZIndex = row == 0 && lane % 2 == 0 ? 1 : 0;
            // Unlike ScaleTo (a temporary visual scale), this also resizes the hitbox, reticle and health bar.
            node.SetScaleAndHue(baseScale * fit, 0f);
            node.ToggleIsInteractable(true);
        }
    }

    /// <summary>
    /// Where <paramref name="player"/>'s creature in <paramref name="lane"/> stands (its feet), in the coordinates of
    /// the creature nodes' parent; null if the player has no row or no node.
    /// </summary>
    public static Vector2? Slot(NCombatRoom room, Player player, int lane)
    {
        var state = player.Creature.CombatState;
        var rows = Rows(state);
        int row = rows.Where(r => r.Luke == player).Select(r => r.Row).DefaultIfEmpty(-1).First();
        var front = room.GetCreatureNode(LocalContext.GetMe(state)?.Creature ?? player.Creature);
        if (row < 0 || lane < 0 || front == null)
        {
            return null;
        }
        float start = front.Position.X + front.Visuals.Bounds.Size.X * 0.5f + Gap * 2;
        float slotLeft = start + lane * (SlotWidth + Gap);
        // Rows close up when there are more than three, to stay on screen.
        int backRow = rows[^1].Row;
        float rowLift = backRow > 0 ? Mathf.Min(RowLift, MaxLift / backRow) : RowLift;
        float lift = row * rowLift + (lane % 2 == 1 ? BackRowLift : 0f) * (row > 0 ? RowScale : 1f);
        return new Vector2(slotLeft + SlotWidth * 0.5f, front.Position.Y + 10f - lift);
    }

    /// <summary>The same point in canvas coordinates, to compare with the mouse; null if there is none.</summary>
    public static Vector2? SlotGlobal(NCombatRoom room, Player player, int lane) =>
        Slot(room, player, lane) is { } slot && room.GetCreatureNode(player.Creature)?.GetParent() is CanvasItem parent
            ? parent.GetGlobalTransform() * slot
            : null;

    /// <summary>Re-run the layout, lane badges and attack intents (Leader changes neighbours' power).</summary>
    public static void Refresh()
    {
        if (NCombatRoom.Instance is { } room)
        {
            Rearrange(room);
        }
    }

    /// <summary>Re-run the layout and lane badges whenever any creature comes or goes.</summary>
    private static void Rearrange(NCombatRoom room)
    {
        // A creature being removed may already have left its combat state, so read it from the live nodes.
        var state = room.CreatureNodes.Select(n => n.Entity.CombatState).FirstOrDefault(s => s != null);
        foreach (var (luke, row) in Rows(state))
        {
            Arrange(room, luke, row);
            foreach (var creature in Board.Shown.Creatures(luke))
            {
                ((BoardCreature)creature.Monster!).ShowIntent();
            }
        }
        LaneMarkers.Refresh(room);
    }

    [HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.AddCreature))]
    private static class OnAdd
    {
        [HarmonyPostfix]
        private static void Postfix(NCombatRoom __instance) => Rearrange(__instance);
    }

    [HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.RemoveCreatureNode))]
    private static class OnRemove
    {
        // The node leaves the room's list at once but stays on screen for its death animation, which the badge
        // refresh no longer reaches; take its lane badge off here.
        [HarmonyPostfix]
        private static void Postfix(NCombatRoom __instance, NCreature node)
        {
            LaneMarkers.Clear(node);
            Rearrange(__instance);
        }
    }
}
