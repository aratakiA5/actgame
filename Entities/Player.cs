using ActGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ActGame.Entities;

public enum PlayerCombatStyle
{
    MartialArtist,
    Swordswoman
}

public sealed class Player
{
    private const float MoveSpeed = 260f;
    private const float JumpSpeed = 560f;
    private const float Gravity = 1500f;
    private const float AttackDuration = 0.32f;

    private float _attackTimer;
    private KeyboardState _previousKeyboard;

    public EntityBody Body { get; } = new(
        new Vector2(180, 620),
        visualSize: new Vector2(128, 128),
        collisionSize: new Vector2(46, 112),
        collisionOffset: Vector2.Zero);

    public PlayerCombatStyle CombatStyle { get; private set; } = PlayerCombatStyle.MartialArtist;
    public bool FacingRight { get; private set; } = true;
    public bool IsAttacking => _attackTimer > 0f;
    public bool IsGrounded { get; private set; }
    public bool IsMoving => Math.Abs(Body.Velocity.X) > 1f;
    public int HitPoints { get; private set; } = 5;

    public Rectangle AttackBounds
    {
        get
        {
            if (!IsAttacking)
                return Rectangle.Empty;

            var body = Body.CollisionBounds;

            if (CombatStyle == PlayerCombatStyle.Swordswoman)
            {
                // Rapier thrust: longer and slightly narrower than the fighter's kick.
                const int width = 92;
                const int height = 34;
                var x = FacingRight ? body.Right - 4 : body.Left - width + 4;
                var y = body.Bottom - 82;
                return new Rectangle(x, y, width, height);
            }

            // Martial artist kick.
            const int kickWidth = 64;
            const int kickHeight = 42;
            var kickX = FacingRight ? body.Right : body.Left - kickWidth;
            var kickY = body.Bottom - 70;
            return new Rectangle(kickX, kickY, kickWidth, kickHeight);
        }
    }

    public void ConfigureCharacter(PlayerCombatStyle style)
    {
        CombatStyle = style;
        _attackTimer = 0f;

        Body.VisualSize = style == PlayerCombatStyle.Swordswoman
            ? new Vector2(150, 150)
            : new Vector2(128, 128);

        Body.CollisionSize = style == PlayerCombatStyle.Swordswoman
            ? new Vector2(48, 112)
            : new Vector2(46, 112);
    }

    public void Update(GameTime gameTime, float groundY)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyboard = Keyboard.GetState();

        var direction = 0f;
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) direction -= 1f;
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) direction += 1f;

        if (direction != 0f)
            FacingRight = direction > 0f;

        Body.Velocity = new Vector2(direction * MoveSpeed, Body.Velocity.Y + Gravity * dt);

        IsGrounded = Body.Position.Y >= groundY - 0.5f;
        if (IsGrounded)
        {
            Body.Position = new Vector2(Body.Position.X, groundY);
            Body.Velocity = new Vector2(Body.Velocity.X, 0f);
        }

        if (IsGrounded && Pressed(keyboard, Keys.Space))
        {
            Body.Velocity = new Vector2(Body.Velocity.X, -JumpSpeed);
            IsGrounded = false;
        }

        if (Pressed(keyboard, Keys.J))
            _attackTimer = AttackDuration;

        _attackTimer = Math.Max(0f, _attackTimer - dt);
        Body.Position += Body.Velocity * dt;
        Body.Position = new Vector2(MathHelper.Clamp(Body.Position.X, 30, 1250), Math.Min(Body.Position.Y, groundY));

        _previousKeyboard = keyboard;
    }

    public void TakeDamage()
    {
        HitPoints = Math.Max(0, HitPoints - 1);
    }

    private bool Pressed(KeyboardState current, Keys key) =>
        current.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
}
