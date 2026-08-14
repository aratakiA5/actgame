using ActGame.Core;
using Microsoft.Xna.Framework;

namespace ActGame.Entities;

public sealed class Enemy
{
    public EntityBody Body { get; }
    public bool IsAlive { get; private set; } = true;
    public float MoveSpeed { get; }
    public bool IsMoving { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool FacingRight { get; private set; }
    public double AnimationTime { get; private set; }

    private const float AttackRange = 95f;

    public Enemy(Vector2 position, Vector2 visualSize, Vector2 collisionSize, float moveSpeed)
    {
        Body = new EntityBody(position, visualSize, collisionSize, Vector2.Zero);
        MoveSpeed = moveSpeed;
    }

    public void Update(GameTime gameTime, Vector2 playerPosition)
    {
        if (!IsAlive) return;

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        AnimationTime += gameTime.ElapsedGameTime.TotalSeconds;

        var dx = playerPosition.X - Body.Position.X;
        FacingRight = dx >= 0f;
        IsAttacking = MathF.Abs(dx) <= AttackRange;
        IsMoving = !IsAttacking && MathF.Abs(dx) > 2f;

        if (IsMoving)
        {
            var direction = MathF.Sign(dx);
            Body.Position += new Vector2(direction * MoveSpeed * dt, 0f);
        }
    }

    public void CheckAttack(Rectangle attackBounds)
    {
        if (IsAlive && !attackBounds.IsEmpty && Body.CollisionBounds.Intersects(attackBounds))
            IsAlive = false;
    }
}
