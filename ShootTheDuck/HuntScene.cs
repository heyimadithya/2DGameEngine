using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;
using RetroEngine.Graphics;
using RetroEngine.Input;

namespace ShootTheDuck;

public sealed class HuntScene : Scene
{
    private const int MaxAmmo = 6;
    private const int FinalWave = 10;
    private const float WaveIntroDuration = 1.4f;

    private readonly List<Duck> _ducks = [];
    private readonly List<Coin> _coins = [];
    private readonly List<BonusItem> _bonuses = [];

    private int _wave = 1;
    private int _ammo = MaxAmmo;
    private int _ducksToSpawn;
    private float _spawnTimer;
    private float _waveTimer;

    private bool _waveIntro = true;
    private bool _gameOver;
    private bool _betweenWaves;
    private bool _won;

    private float _doublePointsTimer;
    private float _slowMoTimer;
    private float _rapidReloadTimer;
    private float _magnetTimer;
    private float _bonusBannerTimer;
    private string _bonusBanner = "";

    private float _shake;
    private float _muzzleFlash;

    protected override void Load()
    {
        Raylib.HideCursor();
        Renderer.ClearColor = new Color(110, 180, 235, 255);
        PlayerProgress.ResetRun();
        WorldClock.EnemyScale = 1f;
        BeginWave(1);
    }

    public override void Unload()
    {
        WorldClock.EnemyScale = 1f;
        _ducks.Clear();
        _coins.Clear();
        _bonuses.Clear();
        base.Unload();
    }

    public override void Update(float deltaTime)
    {
        if (_gameOver)
        {
            if (InputManager.IsKeyPressed(KeyboardKey.Space) || InputManager.WasLeftClicked())
            {
                SceneManager.ChangeScene(new IntroScene());
            }

            return;
        }

        TickBonusTimers(deltaTime);
        WorldClock.EnemyScale = _slowMoTimer > 0f ? 0.45f : 1f;

        _muzzleFlash = Math.Max(0f, _muzzleFlash - deltaTime);
        _shake = Math.Max(0f, _shake - deltaTime * 4f);
        _bonusBannerTimer = Math.Max(0f, _bonusBannerTimer - deltaTime);

        if (_waveIntro || _betweenWaves)
        {
            _waveTimer -= deltaTime;
            base.Update(deltaTime);
            CleanupLists();

            if (_waveTimer <= 0f)
            {
                if (_waveIntro)
                {
                    _waveIntro = false;
                }
                else
                {
                    _betweenWaves = false;
                    if (_wave >= FinalWave)
                    {
                        TriggerGameOver(won: true);
                    }
                    else
                    {
                        BeginWave(_wave + 1);
                    }
                }
            }

            return;
        }

        HandleShooting();
        HandleReload();

        _spawnTimer -= deltaTime;
        if (_ducksToSpawn > 0 && _spawnTimer <= 0f)
        {
            SpawnDuck();
            _ducksToSpawn--;
            _spawnTimer = Math.Max(0.4f, 1.05f - _wave * 0.05f);
        }

        base.Update(deltaTime);

        if (_magnetTimer > 0f)
        {
            foreach (Coin coin in _coins)
            {
                coin.EnableMagnet();
            }
        }

        for (int i = _ducks.Count - 1; i >= 0; i--)
        {
            Duck duck = _ducks[i];

            if (!duck.IsDead &&
                (duck.Position.X < -130f || duck.Position.X > Raylib.GetScreenWidth() + 130f))
            {
                PlayerProgress.RegisterMiss();
                AddEntity(new FloatingText(
                    new Vector2(Math.Clamp(duck.Position.X, 40, Raylib.GetScreenWidth() - 80), 80),
                    "ESCAPED",
                    Color.Red,
                    0.75f,
                    18));
                RemoveEntity(duck);
                _ducks.RemoveAt(i);
                continue;
            }

            if (duck.IsDead && duck.IsOffScreen(Raylib.GetScreenWidth(), Raylib.GetScreenHeight()))
            {
                RemoveEntity(duck);
                _ducks.RemoveAt(i);
            }
        }

        CleanupLists();
        EvaluateWaveState();
    }

    public override void Draw()
    {
        DrawWorldBackground();

        if (_shake > 0f)
        {
            var cam = new Camera2D
            {
                Offset = new Vector2(
                    (Random.Shared.NextSingle() - 0.5f) * 8f * _shake,
                    (Random.Shared.NextSingle() - 0.5f) * 8f * _shake),
                Target = Vector2.Zero,
                Rotation = 0f,
                Zoom = 1f
            };
            Raylib.BeginMode2D(cam);
            base.Draw();
            Raylib.EndMode2D();
        }
        else
        {
            base.Draw();
        }

        DrawHud();
        IntroScene.DrawCrosshair(InputManager.MousePosition);

        if (_muzzleFlash > 0f)
        {
            Vector2 m = InputManager.MousePosition;
            int a = (int)Math.Clamp(180 * (_muzzleFlash / 0.08f), 0, 255);
            Raylib.DrawCircle((int)m.X, (int)m.Y, 10f, new Color(255, 220, 80, a));
        }

        if (_waveIntro)
        {
            DrawCenteredBanner($"WAVE {_wave}", $"Ammo: {MaxAmmo}   Shoot ducks · collect coins · grab bonuses", Color.White);
        }
        else if (_betweenWaves)
        {
            DrawCenteredBanner("WAVE CLEAR!", $"Score {PlayerProgress.Score}   Coins {PlayerProgress.Coins}", Color.Gold);
        }
        else if (_gameOver)
        {
            DrawGameOver();
        }

        if (_bonusBannerTimer > 0f)
        {
            int w = Raylib.MeasureText(_bonusBanner, 30);
            int a = (int)(255 * Math.Clamp(_bonusBannerTimer, 0f, 1f));
            Raylib.DrawText(_bonusBanner, (Raylib.GetScreenWidth() - w) / 2, 120, 30, new Color(255, 255, 180, a));
        }
    }

    private void HandleShooting()
    {
        if (!InputManager.WasLeftClicked())
        {
            return;
        }

        Vector2 aim = InputManager.MousePosition;

        foreach (Coin coin in _coins)
        {
            if (coin.TryCollect(aim))
            {
                AddEntity(new FloatingText(aim, $"+{coin.Value}¢", Color.Gold, 0.7f, 16));
                return;
            }
        }

        foreach (BonusItem bonus in _bonuses)
        {
            if (bonus.TryCollect(aim))
            {
                ApplyBonus(bonus.Type);
                return;
            }
        }

        if (_ammo <= 0)
        {
            AddEntity(new FloatingText(aim + new Vector2(-24, -20), "EMPTY — R", Color.Orange, 0.55f, 18));
            return;
        }

        _ammo--;
        _muzzleFlash = 0.08f;
        _shake = 0.22f;

        foreach (Duck duck in _ducks)
        {
            if (!duck.TryHit(aim))
            {
                continue;
            }

            int basePoints = duck.PointValue;
            if (_doublePointsTimer > 0f)
            {
                basePoints *= 2;
            }

            int before = PlayerProgress.Score;
            PlayerProgress.RegisterHit(basePoints, coinReward: 0);
            int gained = PlayerProgress.Score - before;

            AddEntity(new FloatingText(
                duck.Position,
                $"+{gained}  x{PlayerProgress.Multiplier:0.00}",
                duck.Kind == DuckKind.Golden ? Color.Gold : Color.White,
                1.0f,
                24));
            AddEntity(new HitSpark(duck.Center, duck.Tint));

            int drops = Math.Max(1, duck.CoinValue);
            for (int c = 0; c < drops; c++)
            {
                var coin = new Coin(duck.Center + new Vector2(c * 8f, -4f), 1);
                if (_magnetTimer > 0f)
                {
                    coin.EnableMagnet();
                }

                _coins.Add(coin);
                AddEntity(coin);
            }

            if (duck.Kind is DuckKind.BonusCarrier ||
                (duck.Kind == DuckKind.Golden && Random.Shared.NextSingle() < 0.5f) ||
                Random.Shared.NextSingle() < 0.12f)
            {
                var item = new BonusItem(duck.Center, BonusItem.Roll());
                _bonuses.Add(item);
                AddEntity(item);
            }

            return;
        }

        PlayerProgress.RegisterMiss();
        AddEntity(new FloatingText(aim + new Vector2(8, -18), "MISS", new Color(255, 180, 180, 255), 0.45f, 16));
    }

    private void HandleReload()
    {
        if (!InputManager.IsKeyPressed(KeyboardKey.R))
        {
            return;
        }

        if (_ammo >= MaxAmmo && _rapidReloadTimer <= 0f)
        {
            return;
        }

        int cap = _rapidReloadTimer > 0f ? MaxAmmo + 2 : MaxAmmo;
        _ammo = cap;
        AddEntity(new FloatingText(
            new Vector2(Raylib.GetScreenWidth() * 0.5f - 50f, Raylib.GetScreenHeight() - 130f),
            "RELOADED",
            Color.SkyBlue,
            0.55f,
            20));
    }

    private void ApplyBonus(BonusType type)
    {
        switch (type)
        {
            case BonusType.DoublePoints:
                _doublePointsTimer = 8f;
                break;
            case BonusType.SlowMo:
                _slowMoTimer = 6f;
                break;
            case BonusType.RapidReload:
                _rapidReloadTimer = 10f;
                _ammo = MaxAmmo + 2;
                break;
            case BonusType.CoinMagnet:
                _magnetTimer = 8f;
                foreach (Coin coin in _coins)
                {
                    coin.EnableMagnet();
                }

                break;
            case BonusType.ExtraAmmo:
                _ammo += 3;
                break;
        }

        _bonusBanner = BonusItem.TypeDescription(type);
        _bonusBannerTimer = 1.6f;
        AddEntity(new FloatingText(
            InputManager.MousePosition + new Vector2(-30, -40),
            _bonusBanner,
            Color.Magenta,
            1.1f,
            22));
    }

    private void BeginWave(int wave)
    {
        _wave = wave;
        _ammo = MaxAmmo;
        _ducksToSpawn = 4 + wave;
        _spawnTimer = 0.25f;
        _waveIntro = true;
        _betweenWaves = false;
        _waveTimer = WaveIntroDuration;

        foreach (Duck d in _ducks)
        {
            RemoveEntity(d);
        }

        _ducks.Clear();
    }

    private void SpawnDuck()
    {
        Duck duck = Duck.SpawnRandom(Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), _wave);
        duck.Velocity *= 1f + (_wave - 1) * 0.07f;
        _ducks.Add(duck);
        AddEntity(duck);
    }

    private void EvaluateWaveState()
    {
        if (_ducksToSpawn > 0)
        {
            return;
        }

        bool anyLive = false;
        foreach (Duck duck in _ducks)
        {
            if (!duck.IsDead && duck.IsActive)
            {
                anyLive = true;
                break;
            }
        }

        if (anyLive)
        {
            return;
        }

        _betweenWaves = true;
        _waveTimer = 2.0f;
    }

    private void TriggerGameOver(bool won)
    {
        _gameOver = true;
        _won = won;
        PlayerProgress.HighScore = Math.Max(PlayerProgress.HighScore, PlayerProgress.Score);
        WorldClock.EnemyScale = 1f;
        Raylib.ShowCursor();
    }

    private void TickBonusTimers(float dt)
    {
        _doublePointsTimer = Math.Max(0f, _doublePointsTimer - dt);
        _slowMoTimer = Math.Max(0f, _slowMoTimer - dt);
        _rapidReloadTimer = Math.Max(0f, _rapidReloadTimer - dt);
        _magnetTimer = Math.Max(0f, _magnetTimer - dt);
    }

    private void CleanupLists()
    {
        _coins.RemoveAll(c => !c.IsActive || c.Collected);
        _bonuses.RemoveAll(b => !b.IsActive || b.Collected);
    }

    private void DrawWorldBackground()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        Renderer.Clear(new Color(110, 180, 235, 255));
        Raylib.DrawCircle((int)(sw * 0.85f), (int)(sh * 0.16f), 36f, new Color(255, 240, 170, 255));
        Raylib.DrawRectangle(0, sh - 90, sw, 90, new Color(55, 150, 60, 255));
        Raylib.DrawRectangle(0, sh - 90, sw, 12, new Color(40, 120, 45, 255));
        Raylib.DrawEllipse(sw / 2, sh - 40, 220, 55, new Color(35, 110, 45, 255));
        Raylib.DrawEllipse(sw / 2 - 180, sh - 30, 120, 40, new Color(30, 100, 40, 255));
        Raylib.DrawEllipse(sw / 2 + 180, sh - 30, 120, 40, new Color(30, 100, 40, 255));
    }

    private void DrawHud()
    {
        int sw = Raylib.GetScreenWidth();

        Raylib.DrawText($"SCORE {PlayerProgress.Score}", 16, 14, 28, Color.White);
        Raylib.DrawText($"BEST {PlayerProgress.HighScore}", 16, 46, 18, Color.DarkGray);
        Raylib.DrawText($"COINS {PlayerProgress.Coins}", 16, 70, 20, Color.Gold);

        string multi = $"COMBO {PlayerProgress.Combo}   x{PlayerProgress.Multiplier:0.00}";
        Raylib.DrawText(multi, 16, 96, 20, PlayerProgress.Combo > 0 ? Color.Orange : Color.Gray);

        Raylib.DrawText($"WAVE {_wave}/{FinalWave}", sw - 170, 14, 28, Color.White);

        int ammoSlots = Math.Max(MaxAmmo, _ammo);
        int ammoX = sw - 16 - Math.Min(ammoSlots, 10) * 18;
        Raylib.DrawText("AMMO", ammoX - 70, 52, 18, Color.White);
        int shown = Math.Min(ammoSlots, 10);
        for (int i = 0; i < shown; i++)
        {
            Color c = i < _ammo ? Color.Yellow : new Color(60, 60, 60, 255);
            Raylib.DrawRectangle(ammoX + i * 18, 52, 12, 22, c);
        }

        if (_ammo == 0)
        {
            Raylib.DrawText("Press R to reload", ammoX - 70, 80, 16, Color.SkyBlue);
        }

        int bx = 16;
        const int by = 128;
        if (_doublePointsTimer > 0f)
        {
            DrawBonusChip(ref bx, by, $"x2 {_doublePointsTimer:0.0}s", new Color(255, 120, 60, 255));
        }

        if (_slowMoTimer > 0f)
        {
            DrawBonusChip(ref bx, by, $"SLOW {_slowMoTimer:0.0}s", new Color(120, 200, 255, 255));
        }

        if (_rapidReloadTimer > 0f)
        {
            DrawBonusChip(ref bx, by, $"RLD {_rapidReloadTimer:0.0}s", new Color(120, 255, 140, 255));
        }

        if (_magnetTimer > 0f)
        {
            DrawBonusChip(ref bx, by, $"MAG {_magnetTimer:0.0}s", new Color(255, 220, 60, 255));
        }
    }

    private static void DrawBonusChip(ref int x, int y, string text, Color color)
    {
        int w = Raylib.MeasureText(text, 16) + 16;
        Raylib.DrawRectangleRounded(new Rectangle(x, y, w, 26), 0.4f, 6, color);
        Raylib.DrawText(text, x + 8, y + 5, 16, Color.Black);
        x += w + 8;
    }

    private static void DrawCenteredBanner(string title, string subtitle, Color titleColor)
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, sh / 2 - 70, sw, 140, new Color(0, 0, 0, 140));
        int tw = Raylib.MeasureText(title, 48);
        int swid = Raylib.MeasureText(subtitle, 20);
        Raylib.DrawText(title, (sw - tw) / 2, sh / 2 - 40, 48, titleColor);
        Raylib.DrawText(subtitle, (sw - swid) / 2, sh / 2 + 20, 20, Color.RayWhite);
    }

    private void DrawGameOver()
    {
        int sw = Raylib.GetScreenWidth();
        int sh = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 160));
        string title = _won ? "HUNT COMPLETE!" : "GAME OVER";
        DrawCenteredBanner(
            title,
            $"Score {PlayerProgress.Score}   Coins {PlayerProgress.Coins}   Best Combo {PlayerProgress.BestCombo}",
            _won ? Color.Gold : Color.Red);
        const string hint = "Click / Space — return to title";
        int hw = Raylib.MeasureText(hint, 22);
        Raylib.DrawText(hint, (sw - hw) / 2, sh / 2 + 70, 22, Color.White);
    }
}
