using ActGame.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ActGame;

public sealed class Game1 : Game
{
    private enum GameScreen
    {
        PlayerSelect,
        Playing
    }

    private enum PlayerCharacter
    {
        PinkFighter,
        BlondeSwordswoman
    }

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private Texture2D _heroineSprite = null!;
    private Texture2D _enemySprite = null!;
    private readonly Player _player = new();
    private readonly List<Enemy> _enemies = new();

    private const float GroundY = 620f;
    private const int SpriteCell = 48;

    private GameScreen _screen = GameScreen.PlayerSelect;
    private PlayerCharacter _selectedCharacter = PlayerCharacter.PinkFighter;
    private KeyboardState _previousKeyboard;
    private double _animationTime;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720
        };
        IsMouseVisible = true;
        Window.Title = "ActGame - PLAYER SELECT: Left/Right, Enter to start";
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
        var keyboard = Keyboard.GetState();

        if (_screen == GameScreen.PlayerSelect)
        {
            UpdatePlayerSelect(keyboard);
            _previousKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        if (IsNewKeyPress(keyboard, Keys.Escape))
        {
            _screen = GameScreen.PlayerSelect;
            Window.Title = BuildSelectTitle();
            _previousKeyboard = keyboard;
            base.Update(gameTime);
            return;
        }

        _animationTime += gameTime.ElapsedGameTime.TotalSeconds;
        _player.Update(gameTime, GroundY);

        foreach (var enemy in _enemies)
        {
            enemy.Update(gameTime, _player.Body.Position);
            enemy.CheckKick(_player.KickBounds);
        }

        _previousKeyboard = keyboard;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        if (_screen == GameScreen.PlayerSelect)
        {
            DrawPlayerSelect();
        }
        else
        {
            DrawGame();
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void UpdatePlayerSelect(KeyboardState keyboard)
    {
        if (IsNewKeyPress(keyboard, Keys.Left) || IsNewKeyPress(keyboard, Keys.A))
            _selectedCharacter = PlayerCharacter.PinkFighter;

        if (IsNewKeyPress(keyboard, Keys.Right) || IsNewKeyPress(keyboard, Keys.D))
            _selectedCharacter = PlayerCharacter.BlondeSwordswoman;

        if (IsNewKeyPress(keyboard, Keys.Enter) || IsNewKeyPress(keyboard, Keys.Space))
        {
            _screen = GameScreen.Playing;
            _animationTime = 0;
            Window.Title = _selectedCharacter == PlayerCharacter.PinkFighter
                ? "ActGame - Pink Fighter"
                : "ActGame - Blonde Swordswoman";
        }
        else if (IsNewKeyPress(keyboard, Keys.Escape))
        {
            Exit();
        }
        else
        {
            Window.Title = BuildSelectTitle();
        }
    }

    private string BuildSelectTitle()
    {
        var name = _selectedCharacter == PlayerCharacter.PinkFighter
            ? "Pink Fighter"
            : "Blonde Swordswoman";

        return $"ActGame - PLAYER SELECT: {name}  [Left/Right] Select  [Enter] Start";
    }

    private void DrawPlayerSelect()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), new Color(20, 28, 48));

        var leftCard = new Rectangle(250, 150, 330, 430);
        var rightCard = new Rectangle(700, 150, 330, 430);

        DrawSelectCard(leftCard, _selectedCharacter == PlayerCharacter.PinkFighter);
        DrawSelectCard(rightCard, _selectedCharacter == PlayerCharacter.BlondeSwordswoman);

        // Pink fighter preview: first idle frame from the 4x4 game sheet.
        var heroineSource = new Rectangle(0, 0, SpriteCell, SpriteCell);
        var heroineDest = new Rectangle(leftCard.Center.X - 90, leftCard.Y + 80, 180, 300);
        _spriteBatch.Draw(_heroineSprite, heroineDest, heroineSource, Color.White);

        // Blonde swordswoman preview: game-ready enemy PNG.
        var swordswomanDest = new Rectangle(rightCard.Center.X - 105, rightCard.Y + 65, 210, 315);
        _spriteBatch.Draw(_enemySprite, swordswomanDest, Color.White);

        // Simple arrows make the selection control visible even without a font asset.
        DrawArrow(new Vector2(170, 365), false);
        DrawArrow(new Vector2(1110, 365), true);
    }

    private void DrawSelectCard(Rectangle card, bool selected)
    {
        var border = selected ? 8 : 3;
        var borderColor = selected ? Color.Gold : Color.SlateGray;
        var fillColor = selected ? new Color(52, 62, 92) : new Color(36, 43, 65);

        _spriteBatch.Draw(_pixel, card, borderColor);
        var inner = new Rectangle(card.X + border, card.Y + border, card.Width - border * 2, card.Height - border * 2);
        _spriteBatch.Draw(_pixel, inner, fillColor);
    }

    private void DrawArrow(Vector2 center, bool right)
    {
        const int size = 18;
        for (var i = 0; i < 5; i++)
        {
            var width = (i + 1) * size / 2;
            var y = (int)center.Y - size * 2 + i * size;
            var x = right ? (int)center.X - width : (int)center.X;
            _spriteBatch.Draw(_pixel, new Rectangle(x, y, width, size - 3), Color.White);
        }
    }

    private void DrawGame()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(0, (int)GroundY, 1280, 100), Color.ForestGreen);

        DrawPlayer();

        _spriteBatch.Draw(_pixel, _player.Body.CollisionBounds, Color.Pink * 0.20f);
        if (_player.IsKicking)
            _spriteBatch.Draw(_pixel, _player.KickBounds, Color.Yellow * 0.45f);

        foreach (var enemy in _enemies.Where(e => e.IsAlive))
        {
            DrawEnemy(enemy);
            _spriteBatch.Draw(_pixel, enemy.Body.CollisionBounds, Color.Red * 0.20f);
        }
    }

    private void DrawPlayer()
    {
        if (_selectedCharacter == PlayerCharacter.BlondeSwordswoman)
        {
            DrawSwordswomanPlayer();
            return;
        }

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

    private void DrawSwordswomanPlayer()
    {
        var effects = _player.FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        var bounds = _player.Body.VisualBounds;
        var destination = new Rectangle(bounds.X - 12, bounds.Y - 12, bounds.Width + 24, bounds.Height + 12);

        _spriteBatch.Draw(
            _enemySprite,
            destination,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            effects,
            0f);
    }

    private void DrawEnemy(Enemy enemy)
    {
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

    private bool IsNewKeyPress(KeyboardState keyboard, Keys key) =>
        keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
}
