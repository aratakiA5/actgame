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
    private Texture2D _swordswomanSprite = null!;
    private readonly Player _player = new();
    private readonly List<Enemy> _enemies = new();

    private const float GroundY = 620f;
    private const int FighterCell = 48;
    private const int SwordswomanCell = 48;

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

        var fighterPath = Path.Combine(AppContext.BaseDirectory, "Content", "Player", "heroine_game_sheet.png");
        using (var stream = File.OpenRead(fighterPath))
            _heroineSprite = Texture2D.FromStream(GraphicsDevice, stream);

        // This asset is now treated as the blonde swordswoman's player sprite sheet,
        // not as a generic player skin. The current repository asset contains
        // dedicated idle/run/jump/sword-attack rows derived from the original sheet.
        var swordswomanPath = Path.Combine(AppContext.BaseDirectory, "Content", "Enemies", "blonde_swordswoman.png");
        using (var stream = File.OpenRead(swordswomanPath))
            _swordswomanSprite = Texture2D.FromStream(GraphicsDevice, stream);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        _animationTime += gameTime.ElapsedGameTime.TotalSeconds;

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

        _player.Update(gameTime, GroundY);

        foreach (var enemy in _enemies)
        {
            enemy.Update(gameTime, _player.Body.Position);
            enemy.CheckAttack(_player.AttackBounds);
        }

        _previousKeyboard = keyboard;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        if (_screen == GameScreen.PlayerSelect)
            DrawPlayerSelect();
        else
            DrawGame();

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
            _player.ConfigureCharacter(
                _selectedCharacter == PlayerCharacter.BlondeSwordswoman
                    ? PlayerCombatStyle.Swordswoman
                    : PlayerCombatStyle.MartialArtist);

            _screen = GameScreen.Playing;
            _animationTime = 0;
            Window.Title = _selectedCharacter == PlayerCharacter.PinkFighter
                ? "ActGame - Pink Fighter - J: Kick"
                : "ActGame - Blonde Swordswoman - J: Sword Attack";
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

        var fighterFrame = (int)(_animationTime * 4.0) % 4;
        var fighterSource = new Rectangle(fighterFrame * FighterCell, 0, FighterCell, FighterCell);
        var fighterDest = new Rectangle(leftCard.Center.X - 90, leftCard.Y + 80, 180, 300);
        _spriteBatch.Draw(_heroineSprite, fighterDest, fighterSource, Color.White);

        var swordFrame = (int)(_animationTime * 4.0) % 4;
        var swordSource = new Rectangle(swordFrame * SwordswomanCell, 0, SwordswomanCell, SwordswomanCell);
        var swordDest = new Rectangle(rightCard.Center.X - 105, rightCard.Y + 65, 210, 315);
        _spriteBatch.Draw(_swordswomanSprite, swordDest, swordSource, Color.White);

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
        if (_player.IsAttacking)
            _spriteBatch.Draw(_pixel, _player.AttackBounds, Color.Yellow * 0.45f);

        foreach (var enemy in _enemies.Where(e => e.IsAlive))
        {
            DrawEnemy(enemy);
            _spriteBatch.Draw(_pixel, enemy.Body.CollisionBounds, Color.Red * 0.20f);
        }
    }

    private void DrawPlayer()
    {
        if (_selectedCharacter == PlayerCharacter.BlondeSwordswoman)
            DrawSwordswomanPlayer();
        else
            DrawFighterPlayer();
    }

    private void DrawFighterPlayer()
    {
        var row = 0;
        var fps = 4.0;

        if (_player.IsAttacking)
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
        var source = new Rectangle(frame * FighterCell, row * FighterCell, FighterCell, FighterCell);
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
        var row = 0;
        var fps = 5.0;

        if (_player.IsAttacking)
        {
            row = 3;
            fps = 12.0;
        }
        else if (!_player.IsGrounded)
        {
            row = 2;
            fps = 8.0;
        }
        else if (_player.IsMoving)
        {
            row = 1;
            fps = 10.0;
        }

        var frame = (int)(_animationTime * fps) % 4;
        var source = new Rectangle(
            frame * SwordswomanCell,
            row * SwordswomanCell,
            SwordswomanCell,
            SwordswomanCell);

        var effects = _player.FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        var bounds = _player.Body.VisualBounds;

        _spriteBatch.Draw(
            _swordswomanSprite,
            bounds,
            source,
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
        var frame = (int)(_animationTime * 8.0) % 4;
        var source = new Rectangle(
            frame * SwordswomanCell,
            SwordswomanCell,
            SwordswomanCell,
            SwordswomanCell);

        _spriteBatch.Draw(
            _swordswomanSprite,
            enemy.Body.VisualBounds,
            source,
            Color.White,
            0f,
            Vector2.Zero,
            effects,
            0f);
    }

    private bool IsNewKeyPress(KeyboardState keyboard, Keys key) =>
        keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
}
