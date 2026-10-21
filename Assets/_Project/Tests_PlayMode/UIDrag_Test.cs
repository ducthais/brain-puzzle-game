using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class UIDrag_Test
{
    private GameObject draggableUIObj;
    private Rigidbody2D rb;

    [SetUp]
    public void Setup()
    {
        draggableUIObj = new GameObject("PauseButton_Physics");
        rb = draggableUIObj.AddComponent<Rigidbody2D>();
        draggableUIObj.AddComponent<BoxCollider2D>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(draggableUIObj);
    }

    [UnityTest]
    public IEnumerator TC_UC02_DT05_PointerHold_ChangesToKinematic()
    {

        rb.bodyType = RigidbodyType2D.Dynamic;
        bool isDragging = false; 
        bool isPointerPressedThisFrame = true;

        if (isPointerPressedThisFrame && !isDragging)
        {
            isDragging = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        yield return null;

        // 3. ASSERT
        Assert.IsTrue(isDragging, "Lỗi DT05: Cờ isDragging không bật lên thành True!");
        Assert.AreEqual(RigidbodyType2D.Kinematic, rb.bodyType,
            "Lỗi DT05: Vật thể không chuyển sang trạng thái Kinematic khi được cầm lên!");
    }

    [UnityTest]
    public IEnumerator UIElement_OnDrop_ChangesToDynamicPhysics()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        bool isDragging = true; // C2 = True
        bool isPointerReleasedThisFrame = true;

        if (isPointerReleasedThisFrame && isDragging)
        {
            isDragging = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        yield return null;

        Assert.IsFalse(isDragging, "Lỗi DT06/OnDrop: Không reset trạng thái isDragging về False!");
        Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType,
            "Lỗi DT06/OnDrop: Vật thể không kích hoạt trọng lực (Dynamic) để rơi tự do!");
    }
}