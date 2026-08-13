using ActGame.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ActGame;

public sealed class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private Texture2D _heroineSprite = null!;
    private Texture2D _enemySprite = null!;
    private readonly Player _player = new();
    private readonly List<Enemy> _enemies = new();

    private const float GroundY = 620f;
    private const int SpriteCell = 48;
    private double _animationTime;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720
        };
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Same swordswoman sprite can be displayed at different physical sizes.
        _enemies.Add(new Enemy(new Vector2(650, GroundY), new Vector2(86, 120), new Vector2(46, 104), 75));
        _enemies.Add(new Enemy(new Vector2(900, GroundY), new Vector2(112, 156), new Vector2(60, 136), 45));
        _enemies.Add(new Enemy(new Vector2(1120, GroundY), new Vector2(72, 100), new Vector2(40, 88), 90));
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        var heroinePath = Path.Combine(AppContext.BaseDirectory, "Content", "Player", "heroine_game_sheet.png");
        using (var stream = File.OpenRead(heroinePath))
            _heroineSprite = Texture2D.FromStream(GraphicsDevice, stream);

        var enemyPath = Path.Combine(AppContext.BaseDirectory, "Content", "Enemies", "blonde_swordswoman.png");
        using (var stream = File.OpenRead(enemyPath))
            _enemySprite = Texture2D.FromStream(GraphicsDevice, stream);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

        _animationTime += gameTime.ElapsedGameTime.TotalSeconds;
        _player.Update(gameTime, GroundY);

        foreach (var enemy in _enemies)
        {
            enemy.Update(gameTime, _player.Body.Position);
            enemy.CheckKick(_player.KickBounds);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _spriteBatch.Draw(_pixel, new Rectangle(0, (int)GroundY, 1280, 100), Color.ForestGreen);

        DrawPlayer();

        // Debug collision / attack boxes remain visible while tuning combat.
        _spriteBatch.Draw(_pixel, _player.Body.CollisionBounds, Color.Pink * 0.20f);
        if (_player.IsKicking)
            _spriteBatch.Draw(_pixel, _player.KickBounds, Color.Yellow * 0.45f);

        foreach (var enemy in _enemies.Where(e => e.IsAlive))
        {
            DrawEnemy(enemy);
            _spriteBatch.Draw(_pixel, enemy.Body.CollisionBounds, Color.Red * 0.20f);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawPlayer()
    {
        var row = 0;
        var fps = 4.0;

        if (_player.IsKicking)
        {
            row = 3;
            fps = 12.0;
        }
        else if (!_player.IsGrounded)
        {
            row = 2;
            fps = 7.0;
        }
        else if (_player.IsMoving)
        {
            row = 1;
            fps = 10.0;
        }

        var frame = (int)(_animationTime * fps) % 4;
        var source = new Rectangle(frame * SpriteCell, row * SpriteCell, SpriteCell, SpriteCell);
        var effects = _player.FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        _spriteBatch.Draw(
            _heroineSprite,
            _player.Body.VisualBounds,
            source,
            Color.White,
            0f,
            Vector2.Zero,
            effects,
            0f);
    }

    private void DrawEnemy(Enemy enemy)
    {
        // The source PNG is one game-ready frame extracted from the library sprite sheet.
        // Flip it toward the heroine without changing its collision dimensions.
        var facesRight = _player.Body.Position.X >= enemy.Body.Position.X;
        var effects = facesRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        _spriteBatch.Draw(
            _enemySprite,
            enemy.Body.VisualBounds,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            effects,
            0f);
    }
}
