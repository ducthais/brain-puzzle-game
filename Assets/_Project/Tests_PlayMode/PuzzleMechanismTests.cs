using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Test riêng cho 2 cơ chế mới của Chương 6–7: công tắc sàn (PressurePlate) và vùng kích hoạt (ProximityActivator).
/// Vật đè lên là Rigidbody Static giống bản sao PlayerClone — loại vật không sinh sự kiện va chạm,
/// nên đây cũng là bằng chứng cách dò bằng OverlapBoxAll hoạt động.
/// </summary>
public class PuzzleMechanismTests
{
    // Đặt xa gốc tọa độ để không đụng vật thể của scene test
    private static readonly Vector2 Origin = new Vector2(500f, 500f);
    private readonly List<GameObject> created = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in created)
            if (go != null) Object.Destroy(go);
        created.Clear();
    }

    [UnityTest]
    public IEnumerator PressurePlate_StaticCloneOnTop_UnlocksFlagAndHidesDoor_ThenRestores()
    {
        YellowFlag flag = New("Co").AddComponent<YellowFlag>();
        GameObject door = New("Cua");
        PressurePlate plate = New("CongTac", Origin).AddComponent<PressurePlate>();
        plate.flagsToUnlock = new[] { flag };
        plate.hideWhilePressed = new[] { door };
        yield return FixedSteps(2);

        Assert.IsFalse(plate.IsPressed);
        Assert.IsTrue(flag.isLocked, "Chưa ai đứng lên thì cờ phải khóa");
        Assert.IsTrue(door.activeSelf, "Chưa ai đứng lên thì cửa phải đóng");

        GameObject clone = Body("BanSao", Origin + plate.detectOffset, "Player", RigidbodyType2D.Static);
        yield return FixedSteps(2);
        Assert.IsTrue(plate.IsPressed, "Bản sao Static đè lên phải được nhận");
        Assert.IsFalse(flag.isLocked, "Đang bấm thì cờ được mở khóa");
        Assert.IsFalse(door.activeSelf, "Đang bấm thì cửa biến mất");

        Object.Destroy(clone);
        yield return FixedSteps(2);
        Assert.IsFalse(plate.IsPressed);
        Assert.IsTrue(flag.isLocked, "Rời công tắc thì cờ khóa lại");
        Assert.IsTrue(door.activeSelf, "Rời công tắc thì cửa đóng lại");
    }

    [UnityTest]
    public IEnumerator PressurePlate_AcceptsPushableBox_OnlyWhenAllowed()
    {
        PressurePlate plate = New("CongTac", Origin).AddComponent<PressurePlate>();
        GameObject box = Body("Hop", Origin + plate.detectOffset, "Untagged", RigidbodyType2D.Kinematic);
        box.AddComponent<PushableBox>();

        plate.acceptBoxes = false;
        yield return FixedSteps(2);
        Assert.IsFalse(plate.IsPressed, "acceptBoxes = false thì hộp không tính");

        plate.acceptBoxes = true;
        yield return FixedSteps(2);
        Assert.IsTrue(plate.IsPressed, "Hộp PushableBox đè lên phải bấm được công tắc");
    }

    [UnityTest]
    public IEnumerator PressurePlate_IgnoresOtherObjects()
    {
        PressurePlate plate = New("CongTac", Origin).AddComponent<PressurePlate>();
        Body("Da", Origin + plate.detectOffset, "Untagged", RigidbodyType2D.Static);
        yield return FixedSteps(2);
        Assert.IsFalse(plate.IsPressed, "Vật thường (không phải nhân vật/hộp) không bấm được công tắc");
    }

    [UnityTest]
    public IEnumerator ProximityActivator_EnablesTargets_OnlyWhenPlayerEntersZone()
    {
        GameObject hidden = New("BayAn");
        hidden.SetActive(false);
        BoxCollider2D disabled = New("BayTat", Origin + new Vector2(20f, 0f)).AddComponent<BoxCollider2D>();
        disabled.enabled = false;

        ProximityActivator zone = New("Vung", Origin).AddComponent<ProximityActivator>();
        zone.zoneSize = new Vector2(2f, 2f);
        zone.activateTargets = new[] { hidden };
        zone.enableTargets = new Behaviour[] { disabled };

        GameObject player = Body("NhanVat", Origin + new Vector2(5f, 0f), "Player", RigidbodyType2D.Kinematic);
        yield return FixedSteps(3);
        Assert.IsFalse(zone.HasTriggered, "Nhân vật ở ngoài vùng thì chưa kích hoạt");
        Assert.IsFalse(hidden.activeSelf);
        Assert.IsFalse(disabled.enabled);

        player.GetComponent<Rigidbody2D>().position = Origin;
        yield return FixedSteps(3);
        Assert.IsTrue(zone.HasTriggered, "Nhân vật vào vùng phải kích hoạt");
        Assert.IsTrue(hidden.activeSelf, "Object đích được bật");
        Assert.IsTrue(disabled.enabled, "Component đích được bật");
    }

    // ==========================================
    // HỖ TRỢ
    // ==========================================
    private GameObject New(string name, Vector2? position = null)
    {
        GameObject go = new GameObject(name);
        go.transform.position = position ?? Origin + new Vector2(-30f, 0f);
        created.Add(go);
        return go;
    }

    private GameObject Body(string name, Vector2 position, string tag, RigidbodyType2D type)
    {
        GameObject go = New(name, position);
        go.tag = tag;
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = type;
        go.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.8f);
        return go;
    }

    private static IEnumerator FixedSteps(int count)
    {
        for (int i = 0; i < count; i++) yield return new WaitForFixedUpdate();
    }
}
