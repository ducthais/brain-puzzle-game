using UnityEngine;

// Tạo menu để click chuột phải tạo file Database trong Unity
[CreateAssetMenu(fileName = "NewLevelDatabase", menuName = "Game Data/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        [TextArea] public string hintMessage;
    }

    // Danh sách chứa thông tin của cả 20 level
    public LevelData[] allLevels;
}