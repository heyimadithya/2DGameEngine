using System.Numerics;
using Raylib_cs;
using RetroEngine.Core;
using RetroEngine.Graphics;
using RetroEngine.Input;
using RetroEngine.Physics;

namespace FlappyBird;

public sealed class GameScene : Scene
{
    private const float PipeSpawnInterval = 1.6f;
    private const float PipeGapMin = 110f;
    private const float PipeGapMax = 160f;
    private const float PipeMargin = 40f;

    private static int HighScore;

    private readonly List<Pipe> _pipes = [];
    private readonly Random _random = new();

    private Bird? _bird;
    private float _spawnTimer;
    private int _score;
    private bool _gameOver;

    protected override void Load()
    {
        Renderer.ClearColor = new Color(135, 206, 235, 255);
        ResetRound();
    }

    public override void Unload()
    {
        _pipes.Clear();
        _bird = null;
        base.Unload();
    }

    public override void Update(float deltaTime)
    {
        if (_gameOver)
        {
            if (InputManager.IsKeyPressed(KeyboardKey.Space) || InputManager.WasLeftClicked())
            {
                SceneManager.ChangeScene(new GameScene());
            }

            return;
        }

        base.Update(deltaTime);

        if (_bird is null)
        {
            return;
        }

        // Floor contact ends the run (bird is clamped to the floor in Bird.Update).
        if (_bird.Position.Y + _bird.Height >= Raylib.GetScreenHeight())
        {
            TriggerGameOver();
            return;
        }

        _spawnTimer -= deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnPipePair();
            _spawnTimer = PipeSpawnInterval;
        }

        for (int i = _pipes.Count - 1; i >= 0; i--)
        {
            Pipe pipe = _pipes[i];

            if (Physics.CheckCollision(_bird.Bounds, pipe.Bounds))
            {
                TriggerGameOver();
                return;
            }

            if (!pipe.IsTop && !pipe.HasScored && _bird.Position.X > pipe.Position.X + pipe.Width)
            {
                pipe.HasScored = true;
                _score++;
            }

            if (pipe.IsOffScreen())
            {
                RemoveEntity(pipe);
                _pipes.RemoveAt(i);
            }
        }
    }

    public override void Draw()
    {
        base.Draw();

        Raylib.DrawText($"Score: {_score}", 16, 16, 28, Color.White);
        Raylib.DrawText($"Best: {HighScore}", 16, 52, 20, Color.DarkGray);

        if (_gameOver)
        {
            const string title = "GAME OVER";
            const string hint = "Space / Click to restart";
            int screenW = Raylib.GetScreenWidth();
            int screenH = Raylib.GetScreenHeight();

            int titleWidth = Raylib.MeasureText(title, 48);
            int hintWidth = Raylib.MeasureText(hint, 22);

            Raylib.DrawText(title, (screenW - titleWidth) / 2, screenH / 2 - 40, 48, Color.Red);
            Raylib.DrawText(hint, (screenW - hintWidth) / 2, screenH / 2 + 20, 22, Color.White);
        }
    }

    private void ResetRound()
    {
        _pipes.Clear();
        _score = 0;
        _gameOver = false;
        _spawnTimer = 1.0f;

        float screenH = Raylib.GetScreenHeight();
        _bird = new Bird(new Vector2(120f, screenH * 0.45f));
        AddEntity(_bird);

        SpawnPipePair();
    }

    private void SpawnPipePair()
    {
        float screenW = Raylib.GetScreenWidth();
        float screenH = Raylib.GetScreenHeight();

        float gap = PipeGapMin + (float)_random.NextDouble() * (PipeGapMax - PipeGapMin);
        float gapCenter = PipeMargin + gap * 0.5f +
                          (float)_random.NextDouble() * (screenH - PipeMargin * 2f - gap);

        float topHeight = MathF.Max(20f, gapCenter - gap * 0.5f);
        float bottomY = gapCenter + gap * 0.5f;
        float bottomHeight = MathF.Max(20f, screenH - bottomY);

        var top = new Pipe(new Vector2(screenW + 10f, 0f), topHeight, isTop: true);
        var bottom = new Pipe(new Vector2(screenW + 10f, bottomY), bottomHeight, isTop: false);

        _pipes.Add(top);
        _pipes.Add(bottom);
        AddEntity(top);
        AddEntity(bottom);
    }

    private void TriggerGameOver()
    {
        _gameOver = true;
        HighScore = Math.Max(HighScore, _score);

        if (_bird is not null)
        {
            _bird.IsActive = false;
        }

        foreach (Pipe pipe in _pipes)
        {
            pipe.IsActive = false;
        }
    }
}
