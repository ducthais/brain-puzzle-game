using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

/// <summary>
/// Kiểm thử kéo-thả vật thể (DraggableCharacter) bằng chuột giả lập của Input System.
/// InputTestFixture tạo môi trường input riêng → không ảnh hưởng chuột thật của Editor.
/// </summary>
public class UIDrag_Test : InputTestFixture
{
    private GameObject cameraObj;
    private GameObject draggableObj;
    private Rigidbody2D rb;
    private Mouse mouse;

    public override void Setup()
    {
        base.Setup();

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

        mouse = InputSystem.AddDevice<Mouse>();
    }

    public override void TearDown()
    {
        Object.Destroy(draggableObj);
        Object.Destroy(cameraObj);
        base.TearDown();
    }

    private Vector2 ScreenPointOf(Vector3 worldPos)
    {
        return Camera.main.WorldToScreenPoint(worldPos);
    }

    [UnityTest]
    public IEnumerator TC_UC02_DT05_PointerHold_ChangesToKinematic()
    {
        yield return new WaitForFixedUpdate(); // Đồng bộ collider vào thế giới vật lý

        Move(mouse.position, ScreenPointOf(draggableObj.transform.position), queueEventOnly: true);
        Press(mouse.leftButton, queueEventOnly: true);
        yield return null; // Update() đọc wasPressedThisFrame

        Assert.AreEqual(RigidbodyType2D.Kinematic, rb.bodyType,
            "Lỗi DT05: Vật thể không chuyển sang trạng thái Kinematic khi được cầm lên!");
        Assert.AreEqual(Vector2.zero, rb.linearVelocity, "Lỗi DT05: Cầm vật lên mà vật vẫn còn vận tốc!");
    }

    [UnityTest]
    public IEnumerator UIElement_Drag_FollowsPointer()
    {
        yield return new WaitForFixedUpdate();

        Move(mouse.position, ScreenPointOf(Vector3.zero), queueEventOnly: true);
        Press(mouse.leftButton, queueEventOnly: true);
        yield return null;

        Move(mouse.position, ScreenPointOf(new Vector3(2f, 1f, 0f)), queueEventOnly: true);
        yield return null;

        Assert.AreEqual(2f, draggableObj.transform.position.x, 0.05f, "Lỗi: Vật không đi theo con trỏ theo trục X!");
        Assert.AreEqual(1f, draggableObj.transform.position.y, 0.05f, "Lỗi: Vật không đi theo con trỏ theo trục Y!");
    }

    [UnityTest]
    public IEnumerator UIElement_OnDrop_ChangesToDynamicPhysics()
    {
        yield return new WaitForFixedUpdate();

        Move(mouse.position, ScreenPointOf(draggableObj.transform.position), queueEventOnly: true);
        Press(mouse.leftButton, queueEventOnly: true);
        yield return null;
        Assume.That(rb.bodyType, Is.EqualTo(RigidbodyType2D.Kinematic));

        Release(mouse.leftButton, queueEventOnly: true);
        yield return null;

        Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType,
            "Lỗi DT06/OnDrop: Vật thể không kích hoạt trọng lực (Dynamic) để rơi tự do!");
    }

    [UnityTest]
    public IEnumerator PointerPressOutsideObject_DoesNotPickUp()
    {
        yield return new WaitForFixedUpdate();

        Move(mouse.position, ScreenPointOf(new Vector3(4f, 4f, 0f)), queueEventOnly: true);
        Press(mouse.leftButton, queueEventOnly: true);
        yield return null;

        Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType, "Lỗi: Bấm ra ngoài vật mà vật vẫn bị cầm lên!");
    }
}
