using UnityEngine;

/// <summary>
/// Công tắc sàn: có nhân vật (kể cả bản sao PlayerClone) hoặc hộp đè lên thì mở khóa cờ / mở cửa,
/// rời khỏi công tắc thì trả lại như cũ.
/// Dò vùng bằng Physics2D.OverlapBoxAll mỗi bước vật lý → nhận được cả bản sao Static
/// (vật Static không phát sự kiện va chạm/trigger với nhau).
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Header("--- VÙNG NHẬN ---")]
    [Tooltip("Kích thước vùng dò phía trên công tắc (đơn vị thế giới)")]
    public Vector2 detectSize = new Vector2(1f, 0.4f);
    [Tooltip("Độ lệch tâm vùng dò so với vị trí công tắc")]
    public Vector2 detectOffset = new Vector2(0f, 0.3f);
    [Tooltip("Hộp đẩy được (PushableBox) đè lên cũng tính là bấm")]
    public bool acceptBoxes = true;

    [Header("--- KẾT QUẢ KHI BẤM ---")]
    [Tooltip("Các lá cờ được mở khóa khi đang bấm, khóa lại khi nhả")]
    public YellowFlag[] flagsToUnlock;
    [Tooltip("Các object bị ẩn khi đang bấm (VD: cánh cửa chắn đường)")]
    public GameObject[] hideWhilePressed;

    [Header("--- HÌNH ẢNH ---")]
    public SpriteRenderer plateRenderer;
    public Sprite idleSprite;
    public Sprite pressedSprite;

    public bool IsPressed { get; private set; }

    private void Start()
    {
        Apply(false);
    }

    private void FixedUpdate()
    {
        bool pressed = DetectSomethingOnTop();
        if (pressed != IsPressed) Apply(pressed);
    }

    private bool DetectSomethingOnTop()
    {
        Vector2 center = (Vector2)transform.position + detectOffset;
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, detectSize, 0f))
        {
            if (hit.CompareTag("Player")) return true;
            if (acceptBoxes && hit.GetComponent<PushableBox>() != null) return true;
        }
        return false;
    }

    private void Apply(bool pressed)
    {
        IsPressed = pressed;

        if (flagsToUnlock != null)
            foreach (YellowFlag flag in flagsToUnlock)
                if (flag != null) flag.isLocked = !pressed;

        if (hideWhilePressed != null)
            foreach (GameObject obj in hideWhilePressed)
                if (obj != null) obj.SetActive(!pressed);

        if (plateRenderer != null)
        {
            Sprite sprite = pressed ? pressedSprite : idleSprite;
            if (sprite != null) plateRenderer.sprite = sprite;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + detectOffset, detectSize);
    }
}
