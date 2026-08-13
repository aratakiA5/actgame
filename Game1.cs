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
    private readonly Player _player = new();
    private readonly List<Enemy> _enemies = new();

    private const float GroundY = 620f;

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
        // Same Enemy class, deliberately very different dimensions.
        _enemies.Add(new Enemy(new Vector2(650, GroundY), new Vector2(56, 72), new Vector2(44, 64), 75));
        _enemies.Add(new Enemy(new Vector2(900, GroundY), new Vector2(105, 170), new Vector2(78, 150), 45));
        _enemies.Add(new Enemy(new Vector2(1120, GroundY), new Vector2(150, 105), new Vector2(128, 86), 60));
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

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
        _spriteBatch.Begin();

        // Ground
        _spriteBatch.Draw(_pixel, new Rectangle(0, (int)GroundY, 1280, 100), Color.ForestGreen);

        // Player: visual rectangle and smaller collision body are intentionally separate.
        _spriteBatch.Draw(_pixel, _player.Body.VisualBounds, Color.LightPink);
        _spriteBatch.Draw(_pixel, _player.Body.CollisionBounds, Color.Pink * 0.35f);

        if (_player.IsKicking)
            _spriteBatch.Draw(_pixel, _player.KickBounds, Color.Yellow * 0.7f);

        foreach (var enemy in _enemies.Where(e => e.IsAlive))
        {
            _spriteBatch.Draw(_pixel, enemy.Body.VisualBounds, Color.DarkRed);
            _spriteBatch.Draw(_pixel, enemy.Body.CollisionBounds, Color.Red * 0.35f);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
