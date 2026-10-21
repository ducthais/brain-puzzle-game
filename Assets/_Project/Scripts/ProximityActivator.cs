using UnityEngine;

/// <summary>
/// Khi có nhân vật (kể cả bản sao PlayerClone) đi vào vùng thì bật các object đích.
/// Dùng cho bẫy rơi: để object FallingTrap ở trạng thái tắt → được bật thì Start() chạy và bẫy bắt đầu rơi.
/// Dò vùng bằng Physics2D.OverlapBoxAll mỗi bước vật lý → nhận được cả bản sao Static.
/// </summary>
public class ProximityActivator : MonoBehaviour
{
    [Header("--- VÙNG KÍCH HOẠT ---")]
    public Vector2 zoneSize = new Vector2(2f, 3f);
    public Vector2 zoneOffset = Vector2.zero;

    [Header("--- KẾT QUẢ ---")]
    [Tooltip("Các object được bật khi có nhân vật đi vào vùng")]
    public GameObject[] activateTargets;
    [Tooltip("Các component được bật (VD: FallingTrap đang tắt → bẫy treo sẵn trên trần, bật lên mới rơi)")]
    public Behaviour[] enableTargets;
    [Tooltip("Chỉ kích hoạt một lần")]
    public bool triggerOnce = true;

    public bool HasTriggered { get; private set; }

    private void FixedUpdate()
    {
        if (HasTriggered && triggerOnce) return;
        if (!SomeoneInZone()) return;

        HasTriggered = true;
        if (activateTargets != null)
            foreach (GameObject target in activateTargets)
                if (target != null) target.SetActive(true);
        if (enableTargets != null)
            foreach (Behaviour target in enableTargets)
                if (target != null) target.enabled = true;
    }

    private bool SomeoneInZone()
    {
        Vector2 center = (Vector2)transform.position + zoneOffset;
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, zoneSize, 0f))
            if (hit.CompareTag("Player")) return true;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + zoneOffset, zoneSize);
    }
}
