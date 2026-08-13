using ActGame.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ActGame;

public sealed class Game1 : Game
{
    private enum GameScreen { PlayerSelect, Playing }
    private enum PlayerCharacter { PinkFighter, BlondeSwordswoman }

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private Texture2D _heroineSprite = null!;
    private Texture2D _swordswomanSprite = null!;
    private Texture2D _enemySprite = null!;
    private readonly Player _player = new();
    private readonly List<Enemy> _enemies = new();

    private const float GroundY = 620f;
    private const int FighterCell = 48;
    private const int SwordswomanColumns = 4;
    private const int SwordswomanRows = 4;

    private GameScreen _screen = GameScreen.PlayerSelect;
    private PlayerCharacter _selectedCharacter = PlayerCharacter.PinkFighter;
    private KeyboardState _previousKeyboard;
    private double _animationTime;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = 1280, PreferredBackBufferHeight = 720 };
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

        _heroineSprite = LoadPng(Path.Combine("Content", "Player", "heroine_game_sheet.png"));
        _swordswomanSprite = LoadPng(Path.Combine("Content", "Player", "002_swordswoman.png"));
        _enemySprite = LoadPng(Path.Combine("Content", "Enemies", "blonde_swordswoman.png"));
    }

    private Texture2D LoadPng(string relativePath)
    {
        var path = Path.Combine(AppContext.BaseDirectory, relativePath);
        using var stream = File.OpenRead(path);
        return Texture2D.FromStream(GraphicsDevice, stream);
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

        // Point sampling is intentional. Linear filtering blends RGB values from fully
        // transparent pixels into visible edge pixels and creates a white halo around
        // transparent PNG sprites when they are scaled down.
        _spriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp);

        if (_screen == GameScreen.PlayerSelect) DrawPlayerSelect(); else DrawGame();
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void UpdatePlayerSelect(KeyboardState keyboard)
    {
        if (IsNewKeyPress(keyboard, Keys.Left) || IsNewKeyPress(keyboard, Keys.A)) _selectedCharacter = PlayerCharacter.PinkFighter;
        if (IsNewKeyPress(keyboard, Keys.Right) || IsNewKeyPress(keyboard, Keys.D)) _selectedCharacter = PlayerCharacter.BlondeSwordswoman;

        if (IsNewKeyPress(keyboard, Keys.Enter) || IsNewKeyPress(keyboard, Keys.Space))
        {
            _player.ConfigureCharacter(_selectedCharacter == PlayerCharacter.BlondeSwordswoman ? PlayerCombatStyle.Swordswoman : PlayerCombatStyle.MartialArtist);
            _screen = GameScreen.Playing;
            _animationTime = 0;
            Window.Title = _selectedCharacter == PlayerCharacter.PinkFighter ? "ActGame - Pink Fighter - J: Kick" : "ActGame - Blonde Swordswoman - J: Sword Attack";
        }
        else if (IsNewKeyPress(keyboard, Keys.Escape)) Exit();
        else Window.Title = BuildSelectTitle();
    }

    private string BuildSelectTitle()
    {
        var name = _selectedCharacter == PlayerCharacter.PinkFighter ? "Pink Fighter" : "Blonde Swordswoman";
        return $"ActGame - PLAYER SELECT: {name}  [Left/Right] Select  [Enter] Start";
    }

    private Rectangle SwordswomanSource(int column, int row)
    {
        var x0 = column * _swordswomanSprite.Width / SwordswomanColumns;
        var x1 = (column + 1) * _swordswomanSprite.Width / SwordswomanColumns;
        var y0 = row * _swordswomanSprite.Height / SwordswomanRows;
        var y1 = (row + 1) * _swordswomanSprite.Height / SwordswomanRows;
        return new Rectangle(x0, y0, x1 - x0, y1 - y0);
    }

    private void DrawPlayerSelect()
    {
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), new Color(20, 28, 48));
        var leftCard = new Rectangle(250, 150, 330, 430);
        var rightCard = new Rectangle(700, 150, 330, 430);
        DrawSelectCard(leftCard, _selectedCharacter == PlayerCharacter.PinkFighter);
        DrawSelectCard(rightCard, _selectedCharacter == PlayerCharacter.BlondeSwordswoman);

        var fighterFrame = (int)(_animationTime * 4.0) % 4;
        _spriteBatch.Draw(_heroineSprite, new Rectangle(leftCard.Center.X - 90, leftCard.Y + 80, 180, 300), new Rectangle(fighterFrame * FighterCell, 0, FighterCell, FighterCell), Color.White);

        var swordFrame = (int)(_animationTime * 4.0) % 4;
        _spriteBatch.Draw(_swordswomanSprite, new Rectangle(rightCard.Center.X - 105, rightCard.Y + 65, 210, 315), SwordswomanSource(swordFrame, 0), Color.White);
        DrawArrow(new Vector2(170, 365), false);
        DrawArrow(new Vector2(1110, 365), true);
    }

    private void DrawSelectCard(Rectangle card, bool selected)
    {
        var border = selected ? 8 : 3;
        _spriteBatch.Draw(_pixel, card, selected ? Color.Gold : Color.SlateGray);
        _spriteBatch.Draw(_pixel, new Rectangle(card.X + border, card.Y + border, card.Width - border * 2, card.Height - border * 2), selected ? new Color(52, 62, 92) : new Color(36, 43, 65));
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
        if (_player.IsAttacking) _spriteBatch.Draw(_pixel, _player.AttackBounds, Color.Yellow * 0.45f);
        foreach (var enemy in _enemies.Where(e => e.IsAlive))
        {
            DrawEnemy(enemy);
            _spriteBatch.Draw(_pixel, enemy.Body.CollisionBounds, Color.Red * 0.20f);
        }
    }

    private void DrawPlayer()
    {
        if (_selectedCharacter == PlayerCharacter.BlondeSwordswoman) DrawSwordswomanPlayer(); else DrawFighterPlayer();
    }

    private void DrawFighterPlayer()
    {
        var row = _player.IsAttacking ? 3 : !_player.IsGrounded ? 2 : _player.IsMoving ? 1 : 0;
        var fps = _player.IsAttacking ? 12.0 : !_player.IsGrounded ? 7.0 : _player.IsMoving ? 10.0 : 4.0;
        var frame = (int)(_animationTime * fps) % 4;
        var effects = _player.FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        _spriteBatch.Draw(_heroineSprite, _player.Body.VisualBounds, new Rectangle(frame * FighterCell, row * FighterCell, FighterCell, FighterCell), Color.White, 0f, Vector2.Zero, effects, 0f);
    }

    private void DrawSwordswomanPlayer()
    {
        var row = _player.IsAttacking ? 3 : !_player.IsGrounded ? 2 : _player.IsMoving ? 1 : 0;
        var fps = _player.IsAttacking ? 12.0 : !_player.IsGrounded ? 8.0 : _player.IsMoving ? 10.0 : 4.0;
        var frame = (int)(_animationTime * fps) % 4;
        var effects = _player.FacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        _spriteBatch.Draw(_swordswomanSprite, _player.Body.VisualBounds, SwordswomanSource(frame, row), Color.White, 0f, Vector2.Zero, effects, 0f);
    }

    private void DrawEnemy(Enemy enemy)
    {
        var facesRight = _player.Body.Position.X >= enemy.Body.Position.X;
        var effects = facesRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        var frame = (int)(_animationTime * 8.0) % 4;
        const int cell = 48;
        _spriteBatch.Draw(_enemySprite, enemy.Body.VisualBounds, new Rectangle(frame * cell, cell, cell, cell), Color.White, 0f, Vector2.Zero, effects, 0f);
    }

    private bool IsNewKeyPress(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
}
