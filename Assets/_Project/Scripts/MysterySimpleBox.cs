using System.Collections;
using System.Linq;
using UnityEngine;

public class MysterySimpleBox : MonoBehaviour
{
    [Header("Cấu hình Hộp (Box)")]
    [Tooltip("Hình ảnh chiếc hộp trống rỗng sau khi bị đập")]
    public Sprite emptyBoxSprite;
    private SpriteRenderer boxRenderer;
    private bool isHit = false;

    [Header("Cấu hình Ngôi sao (Star)")]
    [Tooltip("Kéo Object Ngôi sao nằm bên trong hộp vào đây")]
    public GameObject starObject;
    [Tooltip("Độ cao tối đa ngôi sao sẽ nảy lên")]
    public float jumpHeight = 1.2f;

    void Start()
    {
        // Tự động tìm SpriteRenderer của hộp
        boxRenderer = GetComponent<SpriteRenderer>();

        // Ẩn ngôi sao đi khi vừa vào màn chơi
        if (starObject != null)
        {
            var starRenderer = starObject.GetComponent<SpriteRenderer>();
            if (starRenderer != null) starRenderer.enabled = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra nếu chưa bị đập và đối tượng va chạm là Người chơi
        if (!isHit && collision.gameObject.CompareTag("Player"))
        {
            // Kiểm tra va chạm từ dưới lên (đầu người chơi cụng vào đáy hộp)
            ContactPoint2D contact = collision.contacts.FirstOrDefault();
            if (contact.normal.y > 0.5f)
            {
                TriggerBoxEvent();
            }
        }
    }

    private void TriggerBoxEvent()
    {
        isHit = true;

        // Đổi hình ảnh sang hộp rỗng
        if (boxRenderer != null && emptyBoxSprite != null)
        {
            boxRenderer.sprite = emptyBoxSprite;
        }

        // Chạy hiệu ứng ngôi sao nảy lên
        if (starObject != null)
        {
            StartCoroutine(StarBounceRoutine());
        }
    }

    private IEnumerator StarBounceRoutine()
    {
        // Hiện ngôi sao lên
        var starRenderer = starObject.GetComponent<SpriteRenderer>();
        if (starRenderer != null) starRenderer.enabled = true;

        // Tách ngôi sao ra khỏi hộp để tránh bị ảnh hưởng vị trí
        starObject.transform.SetParent(null);

        Vector3 startPos = starObject.transform.position;
        Vector3 peakPos = startPos + new Vector3(0, jumpHeight, 0);

        float duration = 1.0f; // Tổng thời gian chạy hiệu ứng và biến mất (1 giây)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;

            // Hiệu ứng SmoothStep giúp ngôi sao nảy vọt lên nhanh rồi chậm lại ở đỉnh cực mượt
            starObject.transform.position = Vector3.Lerp(startPos, peakPos, Mathf.SmoothStep(0f, 1f, normalizedTime));

            yield return null;
        }

        // Sau đúng 1 giây, biến mất hoàn toàn
        Destroy(starObject);
    }
}