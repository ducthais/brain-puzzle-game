using UnityEngine;
using UnityEngine.UI; // Yêu cầu để thao tác với thành phần Image và Button

public class SoundButtonUI : MonoBehaviour
{
    public enum SoundType { BGM, SFX }

    [Header("--- Cấu hình nút ---")]
    public SoundType typeOfSound; // Chọn đây là nút BGM hay SFX trong Inspector

    [Header("--- Hình ảnh trạng thái ---")]
    public Sprite spriteOn;  // Ảnh khi BẬT âm thanh (hình loa thường)
    public Sprite spriteOff; // Ảnh khi TẮT âm thanh (hình loa gạch chéo)

    private Image buttonImage;

    private void Awake()
    {
        // Lấy thành phần Image của chính nút bấm này
        buttonImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        // Mỗi khi bảng Pause hiện lên, cập nhật ngay hình ảnh nút cho đúng trạng thái hiện tại
        UpdateButtonIcon();
    }

    // Hàm xử lý khi người chơi bấm nút này
    public void OnButtonClick()
    {
        if (AudioManager.Instance == null) return;

        // 1. Gọi sang AudioManager tương ứng
        if (typeOfSound == SoundType.BGM)
        {
            AudioManager.Instance.ToggleMusic();
        }
        else
        {
            AudioManager.Instance.ToggleSFX();
        }

        // 2. Cập nhật lại hình ảnh hiển thị trên màn hình
        UpdateButtonIcon();
    }

    private void UpdateButtonIcon()
    {
        if (AudioManager.Instance == null || buttonImage == null) return;

        // Kiểm tra xem AudioManager đang mute cái nào để đổi ảnh tương ứng
        bool isMuted = (typeOfSound == SoundType.BGM) ? AudioManager.Instance.isMusicMuted : AudioManager.Instance.isSfxMuted;

        if (isMuted)
        {
            buttonImage.sprite = spriteOff;
        }
        else
        {
            buttonImage.sprite = spriteOn;
        }
    }
}