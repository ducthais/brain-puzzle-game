using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Bảng quyết định Level 20: hộp khổng lồ vào DropZone → hộp khóa lại (Kinematic) và cờ mở khóa.
/// Kết quả hoàn toàn do OnTriggerEnter2D thật của DropZone quyết định (không gán hộ).
/// </summary>
public class Level20_DecisionTableTests
{
    private GameObject flagObj;
    private YellowFlag flagScript;
    private GameObject dropZoneObj;
    private GameObject giantBoxObj;
    private Rigidbody2D boxRb;

    [SetUp]
    public void Setup()
    {
        // 1. Cờ mặc định khóa (Rule 1 & 2)
        flagObj = new GameObject("EndFlag");
        flagScript = flagObj.AddComponent<YellowFlag>();
        flagScript.isLocked = true;

        // 2. Vùng đích (trigger 2x2 tại gốc tọa độ)
        dropZoneObj = new GameObject("DropZone");
        BoxCollider2D zoneCollider = dropZoneObj.AddComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;
        zoneCollider.size = new Vector2(2f, 2f);
        dropZoneObj.AddComponent<DropZone>().endFlag = flagScript;

        // 3. Hộp khổng lồ ở xa vùng đích, không chịu trọng lực để vị trí ổn định
        giantBoxObj = new GameObject("GiantBox");
        giantBoxObj.tag = "GiantBox";
        giantBoxObj.transform.position = new Vector3(100f, 100f, 0f);
        boxRb = giantBoxObj.AddComponent<Rigidbody2D>();
        boxRb.bodyType = RigidbodyType2D.Dynamic;
        boxRb.gravityScale = 0f;
        giantBoxObj.AddComponent<BoxCollider2D>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(flagObj);
        Object.Destroy(dropZoneObj);
        Object.Destroy(giantBoxObj);
    }

    private static IEnumerator WaitPhysicsSteps(int steps)
    {
        for (int i = 0; i < steps; i++) yield return new WaitForFixedUpdate();
    }

    // KỊCH BẢN 1: HỘP CHƯA VÀO ĐÍCH
    [UnityTest]
    public IEnumerator BoxNotInZone_FlagRemainsLocked()
    {
        boxRb.position = new Vector2(100f, -50f);

        yield return WaitPhysicsSteps(3);

        // Hộp tiếp tục là vật thể động, cờ vẫn khóa
        Assert.AreEqual(RigidbodyType2D.Dynamic, boxRb.bodyType,
            "Lỗi DT02: Hộp không nằm trong đích nhưng lại bị đóng băng vật lý!");
        Assert.IsTrue(flagScript.isLocked,
            "Lỗi DT02: Cờ mở khóa sai quy tắc khi hộp chưa vào đích!");
    }

    // KỊCH BẢN 2: HỘP LỌT VÀO ĐÍCH
    [UnityTest]
    public IEnumerator BoxEntersZone_BoxLocks_And_FlagUnlocks()
    {
        yield return WaitPhysicsSteps(1);

        boxRb.position = Vector2.zero;

        yield return WaitPhysicsSteps(3);

        // Hành động A2: hộp chuyển Kinematic chống bay; A4: cờ mở
        Assert.AreEqual(RigidbodyType2D.Kinematic, boxRb.bodyType,
            "Lỗi DT04: Hộp lọt vào đích nhưng không chuyển sang Kinematic (Nguy cơ bay hộp)!");
        Assert.AreEqual(Vector2.zero, boxRb.linearVelocity, "Lỗi DT04: Hộp vào đích nhưng vẫn còn vận tốc!");
        Assert.IsFalse(flagScript.isLocked,
            "Lỗi DT04: Hộp đã vào đúng vị trí nhưng lá cờ không được mở khóa!");
    }

    // KỊCH BẢN 3: VẬT KHÁC (KHÔNG PHẢI HỘP KHỔNG LỒ) VÀO ĐÍCH
    [UnityTest]
    public IEnumerator OtherObjectEntersZone_FlagRemainsLocked()
    {
        giantBoxObj.tag = "Untagged";
        yield return WaitPhysicsSteps(1);

        boxRb.position = Vector2.zero;

        yield return WaitPhysicsSteps(3);

        Assert.AreEqual(RigidbodyType2D.Dynamic, boxRb.bodyType, "Lỗi: Vật không phải hộp khổng lồ lại bị khóa!");
        Assert.IsTrue(flagScript.isLocked, "Lỗi: Vật sai loại vào đích mà cờ vẫn mở!");
    }
}
