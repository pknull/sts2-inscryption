using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Inscryption.InscryptionCode.Creatures;

/// <summary>
/// The Fish Hook's catch keeps a photograph of the enemy it was, the way Leshy's camera turns things into cards: the
/// enemy's part of the screen at the moment it is hooked, saved under the user folder as a card portrait (1000x760)
/// and a board sprite (the same photo in a white print border). Every player's game takes its own picture under the
/// same key; a missing picture falls back to the stock art.
/// </summary>
public static class HookPhoto
{
    private const string Folder = "user://inscryption/hooked";
    private const int SpriteHeight = 150;

    public static string PortraitPath(string key) => $"{Folder}/{key}.res";

    public static string SpritePath(string key) => $"{Folder}/{key}_sprite.res";

    public static Texture2D? Load(string path) =>
        ResourceLoader.Exists(path) ? ResourceLoader.Load<Texture2D>(path) : null;

    /// <summary>
    /// Photograph <paramref name="target"/> (call before it leaves the fight). Its HP bar, intents and targeting
    /// reticle are hidden for the picture, and two frames pass so the card's targeting arrow is gone too.
    /// </summary>
    public static async Task Take(Creature target, string key)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null)
        {
            return;
        }
        var hidden = new List<CanvasItem>();
        try
        {
            node.HideSingleSelectReticle();
            foreach (var item in node.FindChildren("*", "", true, false).OfType<CanvasItem>()
                         .Where(c => c is NCreatureStateDisplay || c == node.IntentContainer)
                         .Where(c => c.Visible))
            {
                item.Visible = false;
                hidden.Add(item);
            }
            var tree = node.GetTree();
            await node.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            await node.ToSignal(tree, SceneTree.SignalName.ProcessFrame);

            var viewport = node.GetViewport();
            var screen = viewport.GetTexture().GetImage();
            var hitbox = node.Hitbox;
            var box = viewport.GetFinalTransform() * hitbox.GetGlobalTransformWithCanvas() * new Rect2(Vector2.Zero, hitbox.Size);
            var frame = Frame(box, screen.GetSize());
            var photo = screen.GetRegion(frame);
            photo.Convert(Image.Format.Rgba8);
            photo.Resize(1000, 760, Image.Interpolation.Lanczos);
            DirAccess.MakeDirRecursiveAbsolute(Folder);
            ResourceSaver.Save(ImageTexture.CreateFromImage(photo), PortraitPath(key));
            ResourceSaver.Save(ImageTexture.CreateFromImage(Print(photo)), SpritePath(key));
            MainFile.Logger.Info($"Fish Hook: photographed {target.Monster?.Id.Entry} as {key} (screen {frame})");
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Fish Hook: no photograph of {target.Monster?.Id.Entry}: {e.Message}");
        }
        finally
        {
            foreach (var item in hidden.Where(GodotObject.IsInstanceValid))
            {
                item.Visible = true;
            }
        }
    }

    /// <summary>The enemy's box, grown by a third and widened or heightened to the card's 1000:760, kept on screen.</summary>
    private static Rect2I Frame(Rect2 box, Vector2I screen)
    {
        var size = box.Size * 1.35f;
        const float aspect = 1000f / 760f;
        if (size.X / size.Y < aspect)
        {
            size.X = size.Y * aspect;
        }
        else
        {
            size.Y = size.X / aspect;
        }
        size = new Vector2(Math.Min(size.X, screen.X), Math.Min(size.Y, screen.Y));
        var centre = box.GetCenter();
        var origin = new Vector2(
            Math.Clamp(centre.X - size.X / 2, 0, screen.X - size.X),
            Math.Clamp(centre.Y - size.Y / 2, 0, screen.Y - size.Y));
        return new Rect2I((Vector2I)origin.Round(), (Vector2I)size.Round());
    }

    /// <summary>The photo as a small print with a white border, to stand in a lane.</summary>
    private static Image Print(Image photo)
    {
        int h = SpriteHeight, w = (int)Math.Round(h * 1000f / 760f), border = 8, foot = 22;
        var small = (Image)photo.Duplicate();
        small.Resize(w, h, Image.Interpolation.Lanczos);
        var print = Image.CreateEmpty(w + 2 * border, h + border + foot, false, Image.Format.Rgba8);
        print.Fill(new Color(0.95f, 0.93f, 0.88f));
        print.BlitRect(small, new Rect2I(0, 0, w, h), new Vector2I(border, border));
        return print;
    }
}
