using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType { Left, Right, Jump }
    public ButtonType buttonType;

    [SerializeField] private PlayerController player;

    void Start()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (player == null) return;

        switch (buttonType)
        {
            case ButtonType.Left:
                player.SetMoveDirection(-1f);
                break;
            case ButtonType.Right:
                player.SetMoveDirection(1f);
                break;
            case ButtonType.Jump:
                player.PressJump();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (player == null) return;
        if (buttonType == ButtonType.Left || buttonType == ButtonType.Right)
        {
            player.SetMoveDirection(0f);
        }
    }
}