using ActGame.Core;
using Microsoft.Xna.Framework;

namespace ActGame.Entities;

public sealed class Enemy
{
    public EntityBody Body { get; }
    public bool IsAlive { get; private set; } = true;
    public float MoveSpeed { get; }

    public Enemy(Vector2 position, Vector2 visualSize, Vector2 collisionSize, float moveSpeed)
    {
        Body = new EntityBody(position, visualSize, collisionSize, Vector2.Zero);
        MoveSpeed = moveSpeed;
    }

    public void Update(GameTime gameTime, Vector2 playerPosition)
    {
        if (!IsAlive) return;

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var direction = MathF.Sign(playerPosition.X - Body.Position.X);
        Body.Position += new Vector2(direction * MoveSpeed * dt, 0f);
    }

    public void CheckKick(Rectangle kickBounds)
    {
        if (IsAlive && !kickBounds.IsEmpty && Body.CollisionBounds.Intersects(kickBounds))
            IsAlive = false;
    }
}
