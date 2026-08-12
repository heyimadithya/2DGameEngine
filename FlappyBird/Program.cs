using RetroEngine.Core;

namespace FlappyBird;

internal static class Program
{
    private static void Main()
    {
        var engine = new Engine(
            title: "Flappy Bird",
            width: 400,
            height: 600,
            targetFps: Engine.DefaultTargetFps);

        SceneManager.ChangeScene(new GameScene());
        engine.Run();
    }
}
