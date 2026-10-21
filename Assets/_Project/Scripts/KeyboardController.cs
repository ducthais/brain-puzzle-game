using UnityEngine;
using UnityEngine.InputSystem; // Bắt buộc phải có cái này

[RequireComponent(typeof(PlayerController))]
public class KeyboardController : MonoBehaviour
{
    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // Nếu không có Player hoặc không cắm bàn phím thì bỏ qua
        if (_player == null || Keyboard.current == null) return;

        // Đọc trạng thái các phím
        bool isLeftPressed = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
        bool isRightPressed = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;

        // 1. DI CHUYỂN
        if (isLeftPressed && !isRightPressed)
        {
            _player.SetMoveDirection(-1f);
        }
        else if (isRightPressed && !isLeftPressed)
        {
            _player.SetMoveDirection(1f);
        }
        else
        {
            // Nhả tay ra (hoặc bấm 2 nút cùng lúc) thì cho nhân vật dừng lại
            _player.SetMoveDirection(0f);
        }

        // 2. NHẢY (wasPressedThisFrame = vừa bấm xuống ở frame này)
        if (Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            _player.PressJump();
        }
    }
}