using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

/// <summary>
/// Chơi thử tự động Level 21–28 (docs/GDD_Chuong6_7.md): mỗi màn giải được theo đúng cách thiết kế,
/// còn cách "cứ đi thẳng" thì không thắng được.
/// Nhân vật được điều khiển qua PlayerController.SetMoveDirection/PressJump (như nút ảo trên HUD).
/// Thao tác kéo thả (cầu xoay, đám mây) được mô phỏng bằng cách đặt trực tiếp vị trí/góc —
/// bản thân cơ chế kéo thả đã có test riêng (UIDrag_Test).
/// </summary>
public class Chapter67WalkthroughTests
{
    private PlayerController player;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private int levelId;
    private float groundTop;

    private static float Now => Time.realtimeSinceStartup;

    [SetUp]
    public void Setup()
    {
        ProgressService.UseStore(new InMemoryProgressStore());
        LevelSession.Clear();
        Time.timeScale = 1f;
        SceneManager.sceneLoaded += PrepareScene;
    }

    // Trả lại môi trường sạch cho các test chạy sau (chúng đọc tên scene đang mở, Camera.main, LevelManager...)
    [UnityTearDown]
    public IEnumerator Teardown()
    {
        SceneManager.sceneLoaded -= PrepareScene;
        Time.timeScale = 1f;

        Scene empty = SceneManager.CreateScene("ChapterTestEmpty_" + Guid.NewGuid().ToString("N"));
        SceneManager.SetActiveScene(empty);
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene != empty && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }
        DestroyGameSingletons();
        yield return null;

        ProgressService.UseStore(null);
        LevelSession.Clear();
    }

    // AdManager, PremiumManager, OnlineServicesBootstrap... sống qua DontDestroyOnLoad; giữ lại object của Test Runner
    private static void DestroyGameSingletons()
    {
        GameObject probe = new GameObject("probe");
        Object.DontDestroyOnLoad(probe);
        foreach (GameObject root in probe.scene.GetRootGameObjects())
        {
            bool fromGame = root.GetComponentsInChildren<MonoBehaviour>(true)
                .Any(c => c != null && c.GetType().Assembly.GetName().Name.StartsWith("MainGameScripts"));
            if (fromGame) Object.Destroy(root);
        }
        Object.Destroy(probe);
    }

    // ==========================================
    // CHƯƠNG 6
    // ==========================================
    [UnityTest]
    public IEnumerator Level21_CloneAtWallFoot_BecomesStep()
    {
        yield return Load(21);
        yield return MoveUntilBlocked(1f);
        yield return Clone();
        yield return Traverse(1f);
        AssertWon();
    }

    [UnityTest]
    public IEnumerator Level21_WithoutClone_CannotClimbWall()
    {
        yield return Load(21);
        yield return Traverse(1f, timeout: 5f);
        AssertNotWon();
    }

    [UnityTest]
    public IEnumerator Level22_CloneHoldsPlate_DoorStaysOpen()
    {
        yield return Load(22);
        PressurePlate plate = Object.FindAnyObjectByType<PressurePlate>();
        yield return WalkTo(plate.transform.position.x);
        yield return Clone();
        yield return FixedSteps(3);
        Assert.IsTrue(plate.IsPressed, "Bản sao đứng trên công tắc nhưng công tắc không nhận");

        yield return Traverse(1f);
        AssertWon();
    }

    [UnityTest]
    public IEnumerator Level22_DoorBlocksWithoutHelper()
    {
        yield return Load(22);
        yield return Traverse(1f, timeout: 5f);
        AssertNotWon();
    }

    [UnityTest]
    public IEnumerator Level23_MidAirClone_HoldsUpFallingSlab()
    {
        yield return Load(23);
        Collider2D slab = GameObject.Find("TamChong").GetComponent<Collider2D>();
        yield return WalkTo(slab.bounds.min.x + 0.4f);
        player.PressJump();
        yield return Wait(() => FeetY > groundTop + 1.5f, 1f);
        yield return Clone();

        yield return Traverse(1f);
        AssertWon();
        Assert.Greater(slab.bounds.min.y, groundTop + 2f, "Tấm chông phải dừng trên đầu bản sao");
    }

    [UnityTest]
    public IEnumerator Level23_WalkingStraight_IsCrushed()
    {
        yield return Load(23);
        yield return Traverse(1f, timeout: 5f);
        Assert.IsTrue(Dead, "Đi thẳng vào hành lang phải bị tấm chông đè");
    }

    [UnityTest]
    public IEnumerator Level24_TwoClones_UnlockFlagAndClimbTower()
    {
        yield return Load(24);
        YellowFlag[] flags = Object.FindObjectsByType<YellowFlag>(FindObjectsInactive.Exclude);
        Assert.IsTrue(flags.All(f => f.isLocked), "Cờ phải bị khóa lúc đầu");

        // Bản sao 1: đứng ở chân tháp làm bậc thang
        float towerFoot = FirstSolidToTheRight(X, groundTop + 2f) - 0.5f;
        yield return Traverse(1f, until: () => X >= towerFoot - 0.1f);
        yield return Settle();
        yield return Clone();

        // Bản sao 2: giữ công tắc
        PressurePlate plate = Object.FindAnyObjectByType<PressurePlate>();
        yield return WalkTo(plate.transform.position.x);
        yield return Clone();
        yield return FixedSteps(3);
        Assert.IsTrue(flags.All(f => !f.isLocked), "Bản sao trên công tắc phải mở khóa cờ — " + CloneReport(plate.transform.position));

        yield return Traverse(1f);
        AssertWon();
    }

    [UnityTest]
    public IEnumerator Level24_LockedFlag_DoesNotWin()
    {
        yield return Load(24);
        float towerFoot = FirstSolidToTheRight(X, groundTop + 2f) - 0.5f;
        yield return Traverse(1f, until: () => X >= towerFoot - 0.1f);
        yield return Settle();
        yield return Clone();
        yield return Traverse(1f, timeout: 8f); // Lên được tháp nhưng cờ còn khóa
        AssertNotWon();
    }

    // ==========================================
    // CHƯƠNG 7
    // ==========================================
    [UnityTest]
    public IEnumerator Level25_FlattenBridge_CrossPit()
    {
        yield return Load(25);
        GameObject.Find("CauXoay").transform.rotation = Quaternion.identity; // Mô phỏng kéo cầu nằm ngang
        yield return FixedSteps(2);
        yield return Traverse(1f);
        AssertWon();
    }

    [UnityTest]
    public IEnumerator Level25_TiltedBridge_CannotCross()
    {
        yield return Load(25);
        yield return Traverse(1f, timeout: 5f);
        AssertNotWon();
    }

    [UnityTest]
    public IEnumerator Level26_RealFlagIsBehindTheSign()
    {
        yield return Load(26);
        yield return Traverse(-1f);
        AssertWon();
    }

    [UnityTest]
    public IEnumerator Level26_FakeFlagSide_FallsThroughIllusion()
    {
        yield return Load(26);
        yield return Traverse(1f, timeout: 5f);
        Assert.IsTrue(Dead, "Mặt đất ảo phải làm nhân vật rơi xuống");
    }

    [UnityTest]
    public IEnumerator Level27_CloudAsRoof_StopsSlab()
    {
        yield return Load(27);
        Collider2D slab = GameObject.Find("TamChong").GetComponent<Collider2D>();
        Transform cloud = GameObject.Find("MayKeoDuoc").transform;
        cloud.position = new Vector3(slab.bounds.min.x + 0.8f, groundTop + 2.6f, 0f); // Mô phỏng kéo mây
        yield return FixedSteps(2);

        yield return Traverse(1f);
        AssertWon();
    }

    [UnityTest]
    public IEnumerator Level27_WithoutRoof_IsCrushed()
    {
        yield return Load(27);
        yield return Traverse(1f, timeout: 5f);
        Assert.IsTrue(Dead, "Không có mái che phải bị tấm chông đè");
    }

    [UnityTest]
    public IEnumerator Level28_PushBoxOntoPlate_FlattenBridge()
    {
        yield return Load(28);
        PressurePlate plate = Object.FindAnyObjectByType<PressurePlate>();
        Transform box = GameObject.Find("HopGo").transform;

        yield return Move(1f, () => box.position.x >= plate.transform.position.x - 0.1f);
        yield return FixedSteps(3);
        Assert.IsTrue(plate.IsPressed, "Hộp trên công tắc phải mở cửa");

        GameObject.Find("CauXoay").transform.rotation = Quaternion.identity;
        yield return FixedSteps(2);
        yield return Traverse(1f);
        AssertWon();
    }

    [UnityTest]
    public IEnumerator Level28_DoorClosedWithoutBox()
    {
        yield return Load(28);
        GameObject.Find("CauXoay").transform.rotation = Quaternion.identity;
        Object.Destroy(GameObject.Find("HopGo"));
        yield return FixedSteps(2);
        yield return Traverse(1f, timeout: 6f);
        AssertNotWon();
    }

    // ==========================================
    // HỖ TRỢ
    // ==========================================
    // Chạy trước Update đầu tiên của scene (kể cả lần nạp lại khi chết).
    // Batchmode không có cửa sổ → cố định khung hình ngang 16:9 như điện thoại, nếu không PlayerDeath
    // tính viewport sai; tắt quảng cáo ảo theo số lần chết (đóng băng Time.timeScale làm test treo).
    private static void PrepareScene(Scene scene, LoadSceneMode mode)
    {
        if (Camera.main != null) Camera.main.aspect = 16f / 9f;
        if (AdManager.Instance != null) AdManager.Instance.deathsPerAd = int.MaxValue;
    }

    private IEnumerator Load(int id)
    {
        levelId = id;
        SceneManager.LoadScene("Level " + id);
        yield return null;
        yield return null;
        player = Object.FindAnyObjectByType<PlayerController>();
        rb = player.GetComponent<Rigidbody2D>();
        col = player.GetComponent<BoxCollider2D>();
        yield return Wait(() => OnGround, 2f);
        yield return FixedSteps(5);
        groundTop = FeetY;
    }

    private bool Won => ProgressService.GetStars(levelId) > 0;
    private bool Dead => player == null; // Chết → nạp lại scene → object cũ bị hủy
    private float X => col.bounds.center.x;
    private float FeetY => col.bounds.min.y;

    private bool SolidAt(Vector2 point) =>
        Physics2D.OverlapPointAll(point).Any(c => !c.isTrigger && c.attachedRigidbody != rb);

    private bool OnGround => !Dead &&
        (SolidAt(new Vector2(X - 0.2f, FeetY - 0.08f)) || SolidAt(new Vector2(X + 0.2f, FeetY - 0.08f)));

    private bool GapAhead(float dir) =>
        !SolidAt(new Vector2(X + dir * 0.6f, FeetY - 0.3f)) && !SolidAt(new Vector2(X + dir * 0.6f, FeetY - 1f));

    private float FirstSolidToTheRight(float fromX, float y)
    {
        for (float x = fromX; x < fromX + 25f; x += 0.05f)
            if (SolidAt(new Vector2(x, y))) return x;
        Assert.Fail("Không tìm thấy vật cản phía bên phải");
        return 0f;
    }

    private static string CloneReport(Vector2 target) =>
        $"đích {target}, bản sao: " + string.Join("; ", Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude)
            .Where(c => c.name.StartsWith("PlayerClone")).Select(c => c.bounds.ToString()));

    // Bấm ⟳: để lại bản sao, nhân vật về điểm xuất phát (đồng bộ collider trước khi đọc vị trí mới)
    private IEnumerator Clone()
    {
        Object.FindAnyObjectByType<CloneManager>().CreateCloneAndRespawn();
        Physics2D.SyncTransforms();
        yield return FixedSteps(1);
    }

    private static IEnumerator FixedSteps(int count)
    {
        for (int i = 0; i < count; i++) yield return new WaitForFixedUpdate();
    }

    private IEnumerator Wait(Func<bool> until, float timeout)
    {
        float end = Now + timeout;
        while (!Dead && !until() && Now < end) yield return null;
    }

    private IEnumerator Settle()
    {
        yield return Wait(() => OnGround && Mathf.Abs(rb.linearVelocity.y) < 0.05f, 2f);
        yield return FixedSteps(2);
    }

    private IEnumerator Move(float dir, Func<bool> until, float timeout = 6f)
    {
        float end = Now + timeout;
        while (!Dead && !Won && !until() && Now < end)
        {
            player.SetMoveDirection(dir);
            yield return null;
        }
        if (!Dead) player.SetMoveDirection(0f);
    }

    private IEnumerator WalkTo(float x)
    {
        yield return Move(Mathf.Sign(x - X), () => Mathf.Abs(X - x) < 0.12f);
        yield return Settle();
    }

    private IEnumerator MoveUntilBlocked(float dir, float timeout = 8f)
    {
        float end = Now + timeout, lastX = X, stuckSince = -1f;
        while (!Dead && !Won && Now < end)
        {
            player.SetMoveDirection(dir);
            yield return null;
            if (Dead) yield break;
            if (OnGround && Mathf.Abs(X - lastX) < 0.003f)
            {
                if (stuckSince < 0f) stuckSince = Now;
                else if (Now - stuckSince > 0.25f) break;
            }
            else stuckSince = -1f;
            lastX = X;
        }
        if (!Dead) player.SetMoveDirection(0f);
        yield return Settle();
    }

    // Đứng yên, nhảy, gần đỉnh thì mới đi tới (không áp sát tường lúc đang bay lên)
    private IEnumerator ClimbStep(float dir)
    {
        player.SetMoveDirection(0f);
        yield return FixedSteps(1);
        player.PressJump();
        yield return Wait(() => rb.linearVelocity.y < 0.5f, 1.5f);
        yield return Move(dir, () => OnGround && rb.linearVelocity.y <= 0.05f, 2f);
    }

    /// <summary>Đi theo một hướng: gặp hố thì nhảy qua, bị chặn thì nhảy lên bậc.</summary>
    private IEnumerator Traverse(float dir, Func<bool> until = null, float timeout = 15f)
    {
        float end = Now + timeout, lastX = X, stuckSince = -1f;
        while (!Dead && !Won && Now < end && (until == null || !until()))
        {
            player.SetMoveDirection(dir);
            if (OnGround && rb.linearVelocity.y <= 0.1f && GapAhead(dir))
            {
                player.PressJump();
                yield return Wait(() => !OnGround, 0.3f);
            }
            yield return null;
            if (Dead || Won) break;

            bool stuck = OnGround && Mathf.Abs(X - lastX) < 0.003f;
            lastX = X;
            if (!stuck) { stuckSince = -1f; continue; }
            if (stuckSince < 0f) { stuckSince = Now; continue; }
            if (Now - stuckSince > 0.2f)
            {
                yield return ClimbStep(dir);
                stuckSince = -1f;
                if (!Dead) lastX = X;
            }
        }
        if (!Dead) player.SetMoveDirection(0f);
    }

    private void AssertWon()
    {
        Assert.IsFalse(Dead, $"Level {levelId}: nhân vật đã chết");
        Assert.IsTrue(Won, $"Level {levelId}: chưa thắng màn");
    }

    private void AssertNotWon()
    {
        Assert.IsFalse(Won, $"Level {levelId}: không được thắng khi chưa giải câu đố");
    }
}
