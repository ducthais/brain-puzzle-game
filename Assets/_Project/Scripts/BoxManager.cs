using UnityEngine;
using TMPro;

public class BoxManager : MonoBehaviour
{
    [Header("Giao diện UI")]
    public TextMeshProUGUI countText;

    [Header("Thông số Thùng Gỗ")]
    public int totalVirtualBoxes = 9999;
    public int realBoxesInScene = 20; // Số lượng thùng gỗ thật bạn đặt trên Scene

    private int _boxesRemaining;
    private int _boxesPerDrop; // Số lượng ảo bị trừ khi 1 thùng thật rơi vào đích

    private void Start()
    {
        _boxesRemaining = totalVirtualBoxes;
        // Tính toán 1 thùng thật tương đương bao nhiêu thùng ảo
        _boxesPerDrop = Mathf.CeilToInt((float)totalVirtualBoxes / realBoxesInScene);
        UpdateUI();
    }

    // Hàm này sẽ được vùng DropZone gọi khi thùng gỗ trượt vào
    public void OnBoxDropped()
    {
        _boxesRemaining -= _boxesPerDrop;

        // Đảm bảo số không bị âm
        if (_boxesRemaining <= 0)
        {
            _boxesRemaining = 0;
            Debug.Log("Dọn thùng gỗ xong! Đường đã mở.");
            // Bạn có thể phát âm thanh thành công ở đây:
            // if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.winSound);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = $"Còn lại: {_boxesRemaining}";
        }
    }
}