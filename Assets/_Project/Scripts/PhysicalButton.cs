using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PhysicalButton : MonoBehaviour
{
    public VirtualButton.ButtonType buttonType; // Chọn loại nút trong Inspector (Left, Right, Jump)
    private PlayerController _player;
    private Collider2D _col;

    private void Start()
    {
        _player = FindAnyObjectByType<PlayerController>();
        _col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Mouse.current == null || _player == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

        // KHI VỪA BẤM VÀO NÚT
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hit == _col)
            {
                // Vừa cho phép kéo nút đi (do script DraggableGround xử lý), 
                // vừa gửi lệnh cho nhân vật
                SendActionToPlayer(true);
            }
        }

        // KHI THẢ TAY RA
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // Nếu là nút di chuyển thì bắt buộc phải báo dừng lại
            if (buttonType == VirtualButton.ButtonType.Left || buttonType == VirtualButton.ButtonType.Right)
            {
                _player.SetMoveDirection(0f);
            }
        }
    }

    private void SendActionToPlayer(bool isPressing)
    {
        switch (buttonType)
        {
            case VirtualButton.ButtonType.Left:
                _player.SetMoveDirection(-1f);
                break;
            case VirtualButton.ButtonType.Right:
                _player.SetMoveDirection(1f);
                break;
            case VirtualButton.ButtonType.Jump:
                _player.PressJump();
                break;
        }
    }
}