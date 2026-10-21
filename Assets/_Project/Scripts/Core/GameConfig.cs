using System.Collections.Generic;
using System.Globalization;

/// <summary>Cấu hình bất biến; thiếu key hoặc sai giá trị thì dùng mặc định của game.</summary>
public sealed class GameConfig
{
    public const string LevelsPerAdKey = "interstitial_levels";
    public const string DeathsPerAdKey = "interstitial_deaths";
    public const string HintRequiresAdKey = "hint_requires_ad";
    public const string SkipEnabledKey = "skip_enabled";
    public const int DefaultLevelsPerAd = 2;
    public const int DefaultDeathsPerAd = 9; // Khớp giá trị đang đặt trong prefab AdManager
    public const int MinAdInterval = 1;
    public const int MaxAdInterval = 20;

    public int LevelsPerAd { get; }
    public int DeathsPerAd { get; }
    public bool HintRequiresAd { get; }
    public bool SkipEnabled { get; }

    public GameConfig(int levelsPerAd = DefaultLevelsPerAd, int deathsPerAd = DefaultDeathsPerAd,
        bool hintRequiresAd = true, bool skipEnabled = true)
    {
        LevelsPerAd = ValidateInterval(levelsPerAd, DefaultLevelsPerAd);
        DeathsPerAd = ValidateInterval(deathsPerAd, DefaultDeathsPerAd);
        HintRequiresAd = hintRequiresAd;
        SkipEnabled = skipEnabled;
    }

    public static GameConfig Parse(IReadOnlyDictionary<string, string> values)
    {
        return new GameConfig(
            ReadInterval(values, LevelsPerAdKey, DefaultLevelsPerAd),
            ReadInterval(values, DeathsPerAdKey, DefaultDeathsPerAd),
            ReadBool(values, HintRequiresAdKey), ReadBool(values, SkipEnabledKey));
    }

    private static int ValidateInterval(int value, int fallback)
        => value >= MinAdInterval && value <= MaxAdInterval ? value : fallback;

    private static int ReadInterval(IReadOnlyDictionary<string, string> values, string key, int fallback)
        => values != null && values.TryGetValue(key, out string raw)
            && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? ValidateInterval(value, fallback) : fallback;

    private static bool ReadBool(IReadOnlyDictionary<string, string> values, string key)
    {
        if (values != null && values.TryGetValue(key, out string raw) && bool.TryParse(raw, out bool value))
            return value;
        return true;
    }
}
