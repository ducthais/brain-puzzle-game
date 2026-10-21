using System.Collections;
using System.Linq;
using UnityEngine;

public class BoxAndStarController : MonoBehaviour
{
    [Header("Box Settings")]
    public Sprite emptyBoxSprite;
    private SpriteRenderer boxRenderer;
    private bool isHit = false;

    [Header("Star Settings")]
    public GameObject starObject;
    [Tooltip("Kéo đối tượng DragHandle vào đây để sao bay thẳng tới chóp cỏ")]
    public Transform targetPlatform;
    public float moveSpeed = 5f;

    void Start()
    {
        var renderers = GetComponents<SpriteRenderer>();
        boxRenderer = renderers.FirstOrDefault();

        if (starObject != null)
        {
            var starRenderers = starObject.GetComponents<SpriteRenderer>();
            var starRenderer = starRenderers.FirstOrDefault();
            if (starRenderer != null) starRenderer.enabled = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isHit && collision.gameObject.CompareTag("Player"))
        {
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
        if (boxRenderer != null && emptyBoxSprite != null)
        {
            boxRenderer.sprite = emptyBoxSprite;
        }

        if (starObject != null && targetPlatform != null)
        {
            StartCoroutine(StarAnimationRoutine());
        }
    }

    private IEnumerator StarAnimationRoutine()
    {
        var starRenderers = starObject.GetComponents<SpriteRenderer>();
        var starRenderer = starRenderers.FirstOrDefault();
        if (starRenderer != null) starRenderer.enabled = true;

        starObject.transform.SetParent(null);

        Vector3 startPos = starObject.transform.position;
        Vector3 peakPos = startPos + new Vector3(0, 1f, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            starObject.transform.position = Vector3.Lerp(startPos, peakPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        // Ngôi sao bay thẳng tới vị trí của DragHandle (chóp cỏ)
        Vector3 targetPos = targetPlatform.position;

        while (Vector3.Distance(starObject.transform.position, targetPos) > 0.1f)
        {
            starObject.transform.position = Vector3.MoveTowards(starObject.transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // TÌM SCRIPT Ở ĐỐI TƯỢNG CHA (MiddlePlatformTilemap) BẰNG CẤU TRÚC AN TOÀN
        var platformScripts = targetPlatform.GetComponentsInParent<DraggablePlatform>();
        DraggablePlatform platformScript = platformScripts.FirstOrDefault();

        if (platformScript != null)
        {
            platformScript.UnlockPlatform(); // Log "Bệ đã mở khóa" sẽ chỉ hiện ra lúc này!
        }

        Destroy(starObject);
    }
}