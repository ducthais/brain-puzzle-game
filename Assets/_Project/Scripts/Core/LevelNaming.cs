/// <summary>
/// Quy ước tên scene màn chơi: "Level &lt;số&gt;" (VD: "Level 5").
/// </summary>
public static class LevelNaming
{
    public const string Prefix = "Level ";

    public static string SceneNameFor(int levelId) => Prefix + levelId;

    public static bool TryParseLevelNumber(string sceneName, out int levelId)
    {
        levelId = 0;
        if (string.IsNullOrEmpty(sceneName) || !sceneName.StartsWith(Prefix)) return false;
        return int.TryParse(sceneName.Substring(Prefix.Length), out levelId) && levelId > 0;
    }
}
