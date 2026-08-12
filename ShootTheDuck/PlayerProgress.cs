namespace ShootTheDuck;

/// <summary>
/// Shared run stats that survive scene changes (intro → hunt → restart).
/// </summary>
public static class PlayerProgress
{
    public static int Score { get; set; }
    public static int Coins { get; set; }
    public static int HighScore { get; set; }
    public static int DucksHit { get; set; }
    public static int BestCombo { get; set; }
    public static float Multiplier { get; set; } = 1f;
    public static int Combo { get; set; }

    public static void ResetRun()
    {
        Score = 0;
        // Keep lifetime coins across restarts within the same session.
        DucksHit = 0;
        Combo = 0;
        Multiplier = 1f;
    }

    public static void RegisterHit(int basePoints, int coinReward)
    {
        Combo++;
        Multiplier = 1f + Math.Min(Combo, 10) * 0.25f;
        int gained = (int)MathF.Round(basePoints * Multiplier);
        Score += gained;
        Coins += coinReward;
        DucksHit++;
        BestCombo = Math.Max(BestCombo, Combo);
        HighScore = Math.Max(HighScore, Score);
    }

    public static void RegisterMiss()
    {
        Combo = 0;
        Multiplier = 1f;
    }
}
