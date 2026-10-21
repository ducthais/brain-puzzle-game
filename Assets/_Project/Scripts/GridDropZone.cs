using UnityEngine;
using TMPro;

public class GridDropZone : MonoBehaviour
{
    [Header("Cài đặt Lưới 4x4")]
    public GameObject boxPrefab; // Kéo 1 cái prefab ảnh thùng gỗ vào đây
    public Transform startGridPoint; // Tạo 1 object rỗng làm mốc góc dưới cùng bên trái của lưới
    public float xSpacing = 1f; // Khoảng cách chiều ngang
    public float ySpacing = 1f; // Khoảng cách chiều dọc

    [Header("UI & Quản lý")]
    public TextMeshProUGUI countText;
    public int totalVirtualBoxes = 9999;

    private int _boxesDropped = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. TRƯỜNG HỢP LÀM THỦ CÔNG: Player mang từng thùng vào
        if (collision.CompareTag("Player"))
        {
            PlayerBoxCarrier player = collision.GetComponent<PlayerBoxCarrier>();
            if (player != null && player.isCarryingBox)
            {
                player.Drop();
                PlaceBoxInGrid();
                UpdateUI(20); // Dọn tay 1 thùng thì trừ 20 số lượng ảo
            }
        }
        // 2. TRƯỜNG HỢP THÔNG MINH: Nghiêng cầu làm cả đống thùng trượt vào
        else if (collision.CompareTag("BoxStack"))
        {
            UpdateUI(totalVirtualBoxes); // Hoàn thành ngay lập tức
            Debug.Log("Làm việc thông minh! Đã dọn xong 9999 thùng.");
            // Xóa luôn khối thùng đang trượt
            Destroy(collision.gameObject);
        }
    }

    private void PlaceBoxInGrid()
    {
        // Tính toán tọa độ theo lưới 4x4
        int row = _boxesDropped / 4;    // Hàng (Tầng)
        int col = _boxesDropped % 4;    // Cột

        Vector2 spawnPos = (Vector2)startGridPoint.position + new Vector2(col * xSpacing, row * ySpacing);

        GameObject newBox = Instantiate(boxPrefab, spawnPos, Quaternion.identity);

        // CỰC KỲ QUAN TRỌNG: Tắt va chạm để Player không bị vấp vào đống thùng đã xếp
        Collider2D col2d = newBox.GetComponent<Collider2D>();
        if (col2d != null) col2d.enabled = false;

        _boxesDropped++;
    }

    private void UpdateUI(int amountToSubtract)
    {
        totalVirtualBoxes -= amountToSubtract;
        if (totalVirtualBoxes < 0) totalVirtualBoxes = 0;

        if (countText != null) countText.text = $"Còn lại: {totalVirtualBoxes}";
    }
}