using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Công cụ test: mở khóa / xóa tiến trình chơi (qua ProgressService).
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

        ProgressService.UnlockUpTo(lastLevel);
        Debug.Log($"✅ Đã mở khóa Level 1 → Level {lastLevel}.");
    }

    [MenuItem("Tools/Test/Xóa tiến trình chơi")]
    public static void ResetProgress()
    {
        LocalProgressStore.DeleteAll();
        ProgressService.Reload();
        LevelSession.Clear();
        Debug.Log("✅ Đã xóa tiến trình và số sao, chỉ còn Level 1 được mở.");
    }
}
