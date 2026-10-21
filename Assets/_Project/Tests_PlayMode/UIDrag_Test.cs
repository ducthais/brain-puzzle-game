using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

/// <summary>
/// Kiểm thử kéo-thả vật thể (DraggableCharacter) bằng chuột ảo của Input System.
/// Chuột ảo được thêm riêng cho test và gỡ bỏ ở TearDown.
/// </summary>
public class UIDrag_Test
{
    private GameObject cameraObj;
    private GameObject draggableObj;
    private Rigidbody2D rb;
    private Mouse mouse;
    private InputSettings.BackgroundBehavior originalBackground;
    private InputSettings.EditorInputBehaviorInPlayMode originalEditorBehavior;

    [SetUp]
    public void Setup()
    {
        // Chạy test không có Game view được focus (VD: dòng lệnh/CI) → cho input luôn tới game,
        // trả lại giá trị cũ ở TearDown
        InputSettings settings = InputSystem.settings;
        originalBackground = settings.backgroundBehavior;
        originalEditorBehavior = settings.editorInputBehaviorInPlayMode;
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

        cameraObj = new GameObject("Test_MainCamera");
        cameraObj.tag = "MainCamera";
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cameraObj.transform.position = new Vector3(0f, 0f, -10f);

        draggableObj = new GameObject("PauseButton_Physics");
        rb = draggableObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f; // Đứng yên tại chỗ để con trỏ trỏ trúng
        draggableObj.AddComponent<BoxCollider2D>().size = new Vector2(2f, 2f);
        draggableObj.AddComponent<DraggableCharacter>();

        mouse = InputSystem.AddDevice<Mouse>("Test_Mouse");
        mouse.MakeCurrent(); // DraggableCharacter đọc Pointer.current
    }

    [TearDown]
    public void Teardown()
    {
        if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
        Object.Destroy(draggableObj);
        Object.Destroy(cameraObj);
        InputSystem.settings.backgroundBehavior = originalBackground;
        InputSystem.settings.editorInputBehaviorInPlayMode = originalEditorBehavior;
    }

    // Gửi trạng thái chuột; Input System xử lý ở đầu frame kế tiếp → Update() thấy wasPressed/ReleasedThisFrame
    private IEnumerator SendMouse(Vector3 worldPos, bool pressed)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        MouseState state = new MouseState { position = screenPos };
        if (pressed) state = state.WithButton(MouseButton.Left);
        InputSystem.QueueStateEvent(mouse, state);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TC_UC02_DT05_PointerHold_ChangesToKinematic()
    {
        yield return new WaitForFixedUpdate(); // Đồng bộ collider vào thế giới vật lý
        yield return SendMouse(draggableObj.transform.position, pressed: false);

        yield return SendMouse(draggableObj.transform.position, pressed: true);

        Assert.AreEqual(RigidbodyType2D.Kinematic, rb.bodyType,
            "Lỗi DT05: Vật thể không chuyển sang trạng thái Kinematic khi được cầm lên!");
        Assert.AreEqual(Vector2.zero, rb.linearVelocity, "Lỗi DT05: Cầm vật lên mà vật vẫn còn vận tốc!");
    }

    [UnityTest]
    public IEnumerator UIElement_Drag_FollowsPointer()
    {
        yield return new WaitForFixedUpdate();
        yield return SendMouse(Vector3.zero, pressed: false);
        yield return SendMouse(Vector3.zero, pressed: true);

        yield return SendMouse(new Vector3(2f, 1f, 0f), pressed: true);

        Assert.AreEqual(2f, draggableObj.transform.position.x, 0.05f, "Lỗi: Vật không đi theo con trỏ theo trục X!");
        Assert.AreEqual(1f, draggableObj.transform.position.y, 0.05f, "Lỗi: Vật không đi theo con trỏ theo trục Y!");
    }

    [UnityTest]
    public IEnumerator UIElement_OnDrop_ChangesToDynamicPhysics()
    {
        yield return new WaitForFixedUpdate();
        yield return SendMouse(draggableObj.transform.position, pressed: false);
        yield return SendMouse(draggableObj.transform.position, pressed: true);
        Assume.That(rb.bodyType, Is.EqualTo(RigidbodyType2D.Kinematic));

        yield return SendMouse(draggableObj.transform.position, pressed: false);

        Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType,
            "Lỗi DT06/OnDrop: Vật thể không kích hoạt trọng lực (Dynamic) để rơi tự do!");
    }

    [UnityTest]
    public IEnumerator PointerPressOutsideObject_DoesNotPickUp()
    {
        yield return new WaitForFixedUpdate();
        Vector3 outside = new Vector3(4f, 4f, 0f);
        yield return SendMouse(outside, pressed: false);

        yield return SendMouse(outside, pressed: true);

        Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType, "Lỗi: Bấm ra ngoài vật mà vật vẫn bị cầm lên!");
    }
}
