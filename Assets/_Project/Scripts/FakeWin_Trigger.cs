using UnityEngine;

public class FakeWinTrigger : MonoBehaviour
{
    public GameObject fakeWinCanvas; // Kéo thả WinCanvas vào đây
    public GameObject hudCanvas;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            fakeWinCanvas.SetActive(true);
            // Tuỳ chọn: Tạm thời khóa di chuyển của Player ở đây nếu cần

            //if (hudCanvas != null)
            //{
            //    hudCanvas.SetActive(false);
            //}
            //Destroy(gameObject);
        }
    }
}