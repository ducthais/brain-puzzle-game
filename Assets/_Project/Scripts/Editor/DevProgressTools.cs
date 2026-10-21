using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Công cụ test: mở khóa / xóa tiến trình chơi (PlayerPrefs "MaxUnlockedLevel").
/// </summary>
public static class DevProgressTools
{
    [MenuItem("Tools/Test/Mở khóa tất cả Level")]
    public static void UnlockAllLevels()
    {
        int lastLevel = EditorBuildSettings.scenes
            .Select(s => Regex.Match(System.IO.Path.GetFileNameWithoutExtension(s.path), @"^Level (\d+)$"))
            .Where(m => m.Success)
            .Select(m => int.Parse(m.Groups[1].Value))
            .DefaultIfEmpty(1)
            .Max();

        PlayerPrefs.SetInt("MaxUnlockedLevel", lastLevel);
        PlayerPrefs.Save();
        Debug.Log($"✅ Đã mở khóa Level 1 → Level {lastLevel}.");
    }

    [MenuItem("Tools/Test/Xóa tiến trình chơi")]
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey("MaxUnlockedLevel");
        PlayerPrefs.DeleteKey("SavedLevel");
        PlayerPrefs.Save();
        Debug.Log("✅ Đã xóa tiến trình, chỉ còn Level 1 được mở.");
    }
}
