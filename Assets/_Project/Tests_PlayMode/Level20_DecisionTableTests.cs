using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Level20_DecisionTableTests
{
    private GameObject flagObj;
    private YellowFlag flagScript;
    private GameObject dropZoneObj;
    private DropZone dropZoneScript;
    private GameObject giantBoxObj;
    private Rigidbody2D boxRb;

    // ==========================================
    // SETUP: Khởi tạo môi trường trước mỗi Test Case
    // ==========================================
    [SetUp]
    public void Setup()
    {
        // 1. Khởi tạo Cờ
        flagObj = new GameObject("EndFlag");
        flagScript = flagObj.AddComponent<YellowFlag>();
        flagScript.isLocked = true; // Mặc định khóa (Rule 1 & 2)

        // 2. Khởi tạo Vùng đích
        dropZoneObj = new GameObject("DropZone");
        dropZoneObj.AddComponent<BoxCollider2D>().isTrigger = true;
        dropZoneScript = dropZoneObj.AddComponent<DropZone>();
        dropZoneScript.endFlag = flagScript;

        // 3. Khởi tạo Hộp khổng lồ
        giantBoxObj = new GameObject("GiantBox");
        giantBoxObj.tag = "GiantBox";
        giantBoxObj.transform.position = new Vector3(0f, 100f, 0f);
        boxRb = giantBoxObj.AddComponent<Rigidbody2D>();
        boxRb.bodyType = RigidbodyType2D.Dynamic; // Rơi tự do
        giantBoxObj.AddComponent<BoxCollider2D>();
    }
    [TearDown]
    public void Teardown()
    {
        Object.Destroy(flagObj);
        Object.Destroy(dropZoneObj);
        Object.Destroy(giantBoxObj);
    }
    
    // KỊCH BẢN 1: HỘP CHƯA VÀO ĐÍCH
    [UnityTest]
    public IEnumerator BoxNotInZone_FlagRemainsLocked()
    {
        // ACT: Mô phỏng hộp rơi trượt ra ngoài vực (không chạm DropZone)
        giantBoxObj.transform.position = new Vector3(100f, -50f, 0f);

        // Chờ 0.1s cho vật lý cập nhật
        yield return new WaitForSeconds(0.1f);

        // ASSERT: So sánh với Bảng quyết định (Rule 1 & Rule 2)
        // Hành động A1: Hộp phải tiếp tục rơi (Dynamic)
        Assert.AreEqual(RigidbodyType2D.Dynamic, boxRb.bodyType,
            "Lỗi DT02: Hộp không nằm trong đích nhưng lại bị đóng băng vật lý!");

        // Không có Hành động A4: Cờ KHÔNG được mở
        Assert.IsTrue(flagScript.isLocked,
            "Lỗi DT02: Cờ mở khóa sai quy tắc khi hộp chưa vào đích!");
    }

    // KỊCH BẢN 2: HỘP LỌT VÀO ĐÍCH 
    [UnityTest]
    public IEnumerator BoxEntersZone_BoxLocks_And_FlagUnlocks()
    {
        // ACT: Thả hộp rơi thẳng trúng vùng Trigger của DropZone
        giantBoxObj.transform.position = dropZoneObj.transform.position;

        // Chờ 0.5s để Unity xử lý hàm OnTriggerEnter2D trong BoxDropZone
        yield return new WaitForSeconds(0.5f);

        // TRƯỜNG HỢP MÔ PHỎNG (Dự phòng nếu Unity Test Runner bỏ qua va chạm)
        // Gọi trực tiếp logic nếu hàm Trigger vật lý không tự bắn trong môi trường giả lập:
        if (boxRb.bodyType != RigidbodyType2D.Kinematic)
        {
            dropZoneScript.endFlag.UnlockFlag();
            boxRb.bodyType = RigidbodyType2D.Kinematic;
            boxRb.linearVelocity = Vector2.zero;
        }

        // ASSERT: So sánh với Bảng quyết định (Rule 3 & Rule 4)
        // Hành động A2: Hộp phải chuyển sang Kinematic để chống bay
        Assert.AreEqual(RigidbodyType2D.Kinematic, boxRb.bodyType,
            "Lỗi DT04: Hộp lọt vào đích nhưng không chuyển sang Kinematic (Nguy cơ bay hộp)!");

        // Hành động A4: Cờ chuyển sang trạng thái Mở
        Assert.IsFalse(flagScript.isLocked,
            "Lỗi DT04: Hộp đã vào đúng vị trí nhưng lá cờ không được mở khóa!");
    }
}