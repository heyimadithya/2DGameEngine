using RetroEngine.Core;

namespace ShootTheDuck;

internal static class Program
{
    private static void Main()
    {
        var engine = new Engine(
            title: "Shoot the Duck",
            width: 900,
            height: 600,
            targetFps: Engine.DefaultTargetFps);

        SceneManager.ChangeScene(new IntroScene());
        engine.Run();
    }
}
