using Microsoft.Xna.Framework;

namespace ActGame.Core;

/// <summary>
/// Separates an entity's world position from its visual and collision sizes.
/// Position always represents the bottom-center (feet/ground anchor).
/// This lets sprites of very different sizes share the same movement code.
/// </summary>
public sealed class EntityBody
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public Vector2 VisualSize { get; set; }
    public Vector2 CollisionSize { get; set; }
    public Vector2 CollisionOffset { get; set; }

    public EntityBody(Vector2 position, Vector2 visualSize, Vector2 collisionSize, Vector2 collisionOffset)
    {
        Position = position;
        VisualSize = visualSize;
        CollisionSize = collisionSize;
        CollisionOffset = collisionOffset;
    }

    public Rectangle CollisionBounds
    {
        get
        {
            var center = Position + CollisionOffset;
            return new Rectangle(
                (int)(center.X - CollisionSize.X / 2f),
                (int)(center.Y - CollisionSize.Y),
                (int)CollisionSize.X,
                (int)CollisionSize.Y);
        }
    }

    public Rectangle VisualBounds => new(
        (int)(Position.X - VisualSize.X / 2f),
        (int)(Position.Y - VisualSize.Y),
        (int)VisualSize.X,
        (int)VisualSize.Y);
}
