using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Dựng tự động Level 21–28 (Chương 6 "Ảo Ảnh Song Trùng", Chương 7 "Bẻ Cong Thứ Nguyên")
/// theo docs/GDD_Chuong6_7.md. Chạy lại được nhiều lần: mỗi lần xóa nội dung câu đố cũ và dựng lại.
///
/// Thông số vật lý dùng để thiết kế (PlayerController: speed 5, jumpForce 12, gravityScale 2.5):
/// nhảy cao ≈ 2.94, nhảy xa trên mặt phẳng ≈ 4.9, collider nhân vật cao ≈ 0.8 → bản sao (Static) là bậc cao ≈ 0.8.
/// </summary>
public static class Chapter67LevelBuilder
{
    private const string ScenesFolder = "Assets/_Project/Scenes";
    private const string TemplateScenePath = ScenesFolder + "/Level 20.unity";
    private const string TilesFolder = "Assets/_Project/Sprites/Tilemap/Tiles/";
    private const string ObjectSprites = "Assets/_Project/Sprites/Tilemap/Object_design/";
    private const string Backgrounds = "Assets/_Project/Sprites/Backgrounds/";
    private const string Prefabs = "Assets/_Project/Prefabs/";

    private static readonly string[] KeepInEnvironment =
        { "EventSystem", "BG_Mushroom", "Grid", "MainCamera", "Player", "GroundCheck", "YellowFlag" };

    private sealed class LevelSpec
    {
        public int Id;
        public string Prompt;   // Câu đố hiện trên HUD
        public string Hint;     // Gợi ý trong LevelDatabase
        public bool UsesClones; // Nút ⟳ tạo bản sao thay vì chơi lại
        public string Background;
        public Action<LevelBuild> Build;
    }

    private static readonly LevelSpec[] Levels =
    {
        new LevelSpec
        {
            Id = 21, UsesClones = true, Background = "background_fade_mushrooms.png",
            Prompt = "Bức tường quá cao để nhảy lên",
            Hint = "Nút ⟳ ở màn này không chơi lại mà để lại một bản sao đứng yên tại chỗ. Bản sao cũng là bậc thang!",
            Build = BuildLevel21
        },
        new LevelSpec
        {
            Id = 22, UsesClones = true, Background = "background_fade_mushrooms.png",
            Prompt = "Cửa chỉ mở khi có người đứng trên công tắc",
            Hint = "Một người giữ cửa, một người đi qua. Đứng trên công tắc rồi bấm ⟳.",
            Build = BuildLevel22
        },
        new LevelSpec
        {
            Id = 23, UsesClones = true, Background = "background_fade_mushrooms.png",
            Prompt = "Trần nhà hơi bất ổn...",
            Hint = "Nhảy lên ngay dưới mép trái tấm chông rồi bấm ⟳: bản sao giữa không trung sẽ thành cột chống trần.",
            Build = BuildLevel23
        },
        new LevelSpec
        {
            Id = 24, UsesClones = true, Background = "background_fade_mushrooms.png",
            Prompt = "Lá cờ trên tháp đang bị khóa",
            Hint = "Cần hai bản sao: một bản đứng giữ công tắc, một bản làm bậc thang ở chân tháp.",
            Build = BuildLevel24
        },
        new LevelSpec
        {
            Id = 25, Background = "background_fade_desert.png",
            Prompt = "Cây cầu bị hỏng rồi, đừng đụng vào nó!",
            Hint = "Chạm vào cây cầu rồi kéo lên/xuống để nó nằm ngang.",
            Build = BuildLevel25
        },
        new LevelSpec
        {
            Id = 26, Background = "background_fade_desert.png",
            Prompt = "Về đích thật nhanh!",
            Hint = "Không phải lá cờ nào cũng thật, không phải mặt đất nào cũng đứng được. Thử quay lại phía sau xem.",
            Build = BuildLevel26
        },
        new LevelSpec
        {
            Id = 27, Background = "background_fade_desert.png",
            Prompt = "Trời sắp sập rồi!",
            Hint = "Đám mây kéo được. Đặt nó dưới mép trái tấm chông trước khi đi qua.",
            Build = BuildLevel27
        },
        new LevelSpec
        {
            Id = 28, Background = "background_fade_desert.png",
            Prompt = "Thử thách cuối cùng",
            Hint = "Đẩy hộp lên công tắc để mở cửa, rồi kéo cầu nằm ngang.",
            Build = BuildLevel28
        },
    };

    // ==========================================
    // MENU
    // ==========================================
    [MenuItem("Tools/Dựng màn/Chương 6–7 (Level 21–28)")]
    public static void BuildAllMenu()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Đang ở Play mode",
                "Không thể sửa scene khi game đang chạy.\nHãy bấm nút ▶ Play lần nữa để dừng game, rồi chạy lại lệnh này.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildAll();
    }

    /// <summary>Dựng toàn bộ 8 màn (gọi được từ dòng lệnh: -executeMethod Chapter67LevelBuilder.BuildAll).</summary>
    public static void BuildAll()
    {
        EnsureScenesExist();

        try
        {
            for (int i = 0; i < Levels.Length; i++)
            {
                LevelSpec spec = Levels[i];
                EditorUtility.DisplayProgressBar("Dựng màn", "Level " + spec.Id, (float)i / Levels.Length);
                BuildLevel(spec);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Đã dựng xong Level 21–28.");
    }

    // Tạo các scene còn thiếu (copy màn mẫu, thêm Build Profiles, nút LevelSelect, Hint)
    private static void EnsureScenesExist()
    {
        List<string> missingHints = new List<string>();
        foreach (LevelSpec spec in Levels)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath(spec.Id)) == null) missingHints.Add(spec.Hint);
            else if (missingHints.Count > 0)
                throw new InvalidOperationException($"❌ Thiếu scene trước Level {spec.Id}; các màn phải liên tiếp.");
        }
        if (missingHints.Count > 0)
            CreateNewLevelTool.CreateLevelsFromTemplate(TemplateScenePath, missingHints, 4);
    }

    private static string ScenePath(int id) => $"{ScenesFolder}/Level {id}.unity";

    private static void BuildLevel(LevelSpec spec)
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath(spec.Id), OpenSceneMode.Single);
        LevelBuild b = new LevelBuild(scene, spec);
        spec.Build(b);
        b.Finish();

        CreateNewLevelTool.AddHint("Level " + spec.Id, spec.Hint);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"✅ Đã dựng Level {spec.Id}");
    }

    // ==========================================
    // CHƯƠNG 6 — BẢN SAO (nút ⟳ = CloneManager.CreateCloneAndRespawn)
    // ==========================================

    // Tường cao 3 ô (≈3.5) > nhảy cao 2.94 → đứng sát chân tường bấm ⟳, bản sao thành bậc thang (0.8 + 2.94 > 3.5)
    private static void BuildLevel21(LevelBuild b)
    {
        b.Ground(-9.6f, 9.6f, 0);
        b.Ground(1.8f, 9.6f, 3);
        b.PlacePlayer(-6.5f);
        b.PlaceFlag(6.5f, b.TopY(3));
    }

    // Công tắc mở cửa; cửa nằm giữa sàn và trần nên bản sao không trèo qua được
    private static void BuildLevel22(LevelBuild b)
    {
        b.Ground(-9.6f, 9.6f, 0);
        b.Block(3.2f, 9.6f, 4, 4); // Trần (1 ô, bắt đầu sau chữ câu đố trên HUD) phủ cả phía trên cửa → chồng bản sao cũng không lọt vào phòng
        b.PlacePlayer(-7f);
        b.PlaceFlag(7f, b.TopY(0));

        GameObject door = b.Door(3.9f, b.TopY(0), b.BottomY(4));
        b.Plate(-2.5f, b.TopY(0), hide: new[] { door });
    }

    // Tấm chông treo trên trần rơi khi có người bước vào; bản sao giữa không trung ở mép trái làm cột chống
    private static void BuildLevel23(LevelBuild b)
    {
        b.Ground(-9.6f, 9.6f, 0);
        b.PlacePlayer(-7f);
        b.PlaceFlag(7f, b.TopY(0));

        float top = b.TopY(0);
        b.HangingSlab(-1.3f, 4.6f, top + 4.75f, zoneLeft: 0.3f, zoneRight: 4.6f, zoneBottom: top, zoneTop: top + 3f);
    }

    // Kết hợp: bản sao giữ công tắc mở khóa cờ + bản sao làm bậc thang lên tháp, có thêm một hố phải nhảy qua
    private static void BuildLevel24(LevelBuild b)
    {
        var left = b.Ground(-9.6f, -1.6f, 0);
        var right = b.Ground(1.6f, 9.6f, 0);
        b.Ground(4.2f, 9.6f, 3);
        b.Pit(left, right);
        b.PlacePlayer(-7.5f);
        b.PlaceFlag(7.2f, b.TopY(3));

        GameObject lockIcon = b.SpriteBox("KhoaCo", ObjectSprites + "block_strong_exclamation.png",
            new Vector2(6.1f, b.TopY(3) + 1.6f), new Vector2(0.7f, 0.7f), 2);
        b.Plate(-5f, b.TopY(0), flags: b.Flags, hide: new[] { lockIcon });
    }

    // ==========================================
    // CHƯƠNG 7 — BẺ CONG THỨ NGUYÊN (kéo thả, bẫy thị giác)
    // ==========================================

    // Hố rộng > nhảy xa 4.9; cầu nghiêng 45° (đứng lên là trượt xuống chông) → kéo cho nằm ngang
    private static void BuildLevel25(LevelBuild b)
    {
        var left = b.Ground(-9.6f, -3.2f, 0);
        var right = b.Ground(3.2f, 9.6f, 0);
        b.Pit(left, right);
        b.PlacePlayer(-7f);
        b.PlaceFlag(7f, b.TopY(0));
        b.BridgeOver(left, right, b.TopY(0), -45f);
    }

    // Cờ giả đứng trên mặt đất ảo (không có va chạm); cờ thật nấp sau tấm biển phía sau lưng
    private static void BuildLevel26(LevelBuild b)
    {
        b.Ground(-9.6f, -0.6f, 0);
        b.IllusionGround(-0.6f, 9.6f, 0);
        b.PlacePlayer(-4.5f);
        b.PlaceFlag(-7.6f, b.TopY(0));
        b.FakeFlag(4.5f, b.TopY(0));
        b.Sign(-7.6f, b.TopY(0), "Không có gì\nở đây đâu", 5);
    }

    // Như Level 23 nhưng không có bản sao: kéo đám mây làm mái che
    private static void BuildLevel27(LevelBuild b)
    {
        b.Ground(-9.6f, 9.6f, 0);
        b.PlacePlayer(-7.5f);
        b.PlaceFlag(7.2f, b.TopY(0));

        float top = b.TopY(0);
        b.HangingSlab(-1.3f, 4.6f, top + 4.75f, zoneLeft: 0.3f, zoneRight: 4.6f, zoneBottom: top, zoneTop: top + 3f);
        b.DraggableCloud(-6f, top + 3.6f, 2.4f);
    }

    // Đẩy hộp lên công tắc mở cửa + kéo cầu nằm ngang qua hố chông.
    // Bậc chặn cao 1 ô ngay sau công tắc: hộp dừng đúng trên công tắc, nhân vật nhảy lên bậc đi tiếp
    private static void BuildLevel28(LevelBuild b)
    {
        var left = b.Ground(-9.6f, -1.9f, 0);
        var stopper = b.Ground(-3.1f, -1.9f, 1);
        var right = b.Ground(4.6f, 9.6f, 0);
        b.Block(4.3f, 9.6f, 4, 4); // Trần phủ cả phía trên cửa
        b.Pit(left, right);
        b.PlacePlayer(-7.6f); // Trong vùng an toàn của màn 4:3 (PlayerDeath giết khi ra ngoài viewport)
        b.PlaceFlag(7.8f, b.TopY(0));

        float top = b.TopY(0);
        b.Box(-5.8f, top);
        GameObject door = b.Door(5.1f, top, b.BottomY(4));
        b.Plate(stopper.left - 0.62f, top, hide: new[] { door });
        b.BridgeOver(left, right, top, -45f);
    }

    // ==========================================
    // BỘ DỰNG MỘT MÀN
    // ==========================================
    private sealed class LevelBuild
    {
        private readonly Scene scene;
        private readonly LevelSpec spec;
        private readonly Transform puzzle;
        private readonly Tilemap ground;
        private readonly Tilemap death;
        private readonly Camera cam;
        private readonly GameObject player;
        private readonly Transform startPos;
        private readonly GameObject flagRoot;
        public readonly YellowFlag[] Flags;
        private Tilemap illusion;

        public LevelBuild(Scene scene, LevelSpec spec)
        {
            this.scene = scene;
            this.spec = spec;

            GameObject environment = scene.GetRootGameObjects().First(g => g.name == "Environment");
            foreach (Transform child in environment.transform.Cast<Transform>().ToList())
                if (!KeepInEnvironment.Contains(child.name)) Object.DestroyImmediate(child.gameObject);
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == "CloneManager") Object.DestroyImmediate(root);

            puzzle = new GameObject("Puzzle").transform;
            puzzle.SetParent(environment.transform, false);
            puzzle.position = Vector3.zero;

            Tilemap[] maps = environment.GetComponentsInChildren<Tilemap>(true);
            ground = maps.First(m => m.name == "Ground_Tilemap");
            death = maps.First(m => m.CompareTag("DeathZone"));
            foreach (Tilemap extra in maps.Where(m => m != ground && m != death))
                Object.DestroyImmediate(extra.gameObject); // Mặt đất ảo của lần dựng trước
            ground.ClearAllTiles();
            death.ClearAllTiles();

            cam = environment.GetComponentsInChildren<Camera>(true).First();
            cam.transform.position = new Vector3(0f, 2.4f, -10f);
            cam.orthographicSize = 5f;

            player = Object.FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include).gameObject;
            flagRoot = environment.transform.Find("YellowFlag").gameObject;
            Flags = flagRoot.GetComponentsInChildren<YellowFlag>(true);
            foreach (YellowFlag flag in Flags) SetFlagLocked(flag, false); // Màn mẫu (Level 20) có cờ đang khóa

            startPos = new GameObject("StartPos").transform;
            startPos.SetParent(puzzle, false);

            SetupBackground(environment.transform.Find("BG_Mushroom"));
            PaintDeathZone();
        }

        // ---------- Tọa độ ô ----------
        private int Col(float worldX) => ground.WorldToCell(new Vector3(worldX, 0f)).x;
        private int BaseRow => ground.WorldToCell(new Vector3(0f, -0.3f)).y;
        private int BottomRow => ground.WorldToCell(new Vector3(0f, -3.6f)).y;

        /// <summary>Độ cao mặt trên của tầng thứ "level" (0 = mặt đất chính).</summary>
        public float TopY(int level) => ground.CellToWorld(new Vector3Int(0, BaseRow + level + 1, 0)).y;
        public float BottomY(int level) => ground.CellToWorld(new Vector3Int(0, BaseRow + level, 0)).y;

        // ---------- Địa hình ----------
        /// <summary>Khối đất từ mép trái đến mép phải (theo tâm ô), mặt trên ở tầng "level", kéo xuống dưới đáy màn hình.</summary>
        public (float left, float right) Ground(float x0, float x1, int level)
        {
            int c0 = Col(x0 + 0.3f), c1 = Col(x1 - 0.3f);
            PaintBlock(ground, c0, c1, BaseRow + level, BottomRow);
            return (ground.CellToWorld(new Vector3Int(c0, 0, 0)).x, ground.CellToWorld(new Vector3Int(c1 + 1, 0, 0)).x);
        }

        /// <summary>Hố giữa hai khối đất: rải chông dưới đáy, trả về chiều rộng thật.</summary>
        public float Pit((float left, float right) leftGround, (float left, float right) rightGround)
        {
            float width = rightGround.left - leftGround.right;
            PitSpikes(leftGround.right, rightGround.left);
            Debug.Log($"   Level {spec.Id}: hố rộng {width:0.00} (nhảy xa tối đa ≈ 4.9)");
            return width;
        }

        /// <summary>Khối treo (VD: trần nhà) từ tầng "fromLevel" đến "toLevel".</summary>
        public void Block(float x0, float x1, int fromLevel, int toLevel) =>
            PaintBlock(ground, Col(x0 + 0.3f), Col(x1 - 0.3f), BaseRow + toLevel, BaseRow + fromLevel);

        /// <summary>Mặt đất chỉ có hình, không có va chạm (bẫy thị giác).</summary>
        public void IllusionGround(float x0, float x1, int level)
        {
            if (illusion == null)
            {
                GameObject go = new GameObject("Illusion_Tilemap", typeof(Tilemap), typeof(TilemapRenderer));
                go.transform.SetParent(ground.transform.parent, false);
                go.transform.localPosition = ground.transform.localPosition;
                go.transform.localScale = ground.transform.localScale;
                go.layer = ground.gameObject.layer;
                go.GetComponent<TilemapRenderer>().sortingOrder = ground.GetComponent<TilemapRenderer>().sortingOrder;
                illusion = go.GetComponent<Tilemap>();
            }
            PaintBlock(illusion, Col(x0 + 0.3f), Col(x1 - 0.3f), BaseRow + level, BottomRow);
        }

        private static void PaintBlock(Tilemap map, int c0, int c1, int rTop, int rBottom)
        {
            for (int c = c0; c <= c1; c++)
            for (int r = rBottom; r <= rTop; r++)
            {
                string h = c == c0 ? "left" : c == c1 ? "right" : "";
                string v = r == rTop ? "top" : r == rBottom ? "bottom" : "";
                string name = v.Length > 0 && h.Length > 0 ? $"{v}_{h}" : v.Length > 0 ? v : h.Length > 0 ? h : "center";
                map.SetTile(new Vector3Int(c, r, 0), Tile("terrain_grass_block_" + name + "_0"));
            }
        }

        private static TileBase Tile(string name)
        {
            TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(TilesFolder + name + ".asset");
            if (tile == null) throw new FileNotFoundException("Thiếu tile " + name);
            return tile;
        }

        // Viền tử thần ngoài khung hình: đáy và hai bên
        private void PaintDeathZone()
        {
            TileBase red = Tile("block_red_0");
            Vector3Int bl = death.WorldToCell(new Vector3(-11f, -5.5f));
            Vector3Int tr = death.WorldToCell(new Vector3(11f, 9f));
            for (int x = bl.x; x <= tr.x; x++) death.SetTile(new Vector3Int(x, bl.y, 0), red);
            for (int y = bl.y; y <= tr.y; y++)
            {
                death.SetTile(new Vector3Int(bl.x, y, 0), red);
                death.SetTile(new Vector3Int(tr.x, y, 0), red);
            }
        }

        // ---------- Nhân vật & cờ ----------
        public void PlacePlayer(float x)
        {
            Vector3 pos = new Vector3(x, TopY(0) + 0.65f, 0f);
            Transform groundCheck = player.transform.parent.Find("GroundCheck");
            Vector3 checkOffset = groundCheck != null ? groundCheck.position - player.transform.position : Vector3.zero;
            player.transform.position = pos;
            if (groundCheck != null) groundCheck.position = pos + checkOffset;
            startPos.position = pos;
        }

        public void PlaceFlag(float x, float surfaceY)
        {
            Transform pole = Flags.Select(f => f.transform).OrderByDescending(t => t.lossyScale.y).First();
            flagRoot.transform.position += new Vector3(x, surfaceY + 1.1f, 0f) - pole.position;
        }

        // ---------- Đồ vật ----------
        public GameObject SpriteBox(string name, string spritePath, Vector2 center, Vector2 size, int order = 0)
        {
            Sprite sprite = LoadSprite(spritePath);
            GameObject go = new GameObject(name);
            go.transform.SetParent(puzzle, false);
            go.transform.position = center;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            Vector2 native = sprite.bounds.size;
            go.transform.localScale = new Vector3(size.x / native.x, size.y / native.y, 1f);
            return go;
        }

        /// <summary>Cánh cửa chắn đường từ mặt đất lên đến đáy trần.</summary>
        public GameObject Door(float x, float bottomY, float topY)
        {
            float height = topY - bottomY;
            GameObject door = new GameObject("Cua");
            door.transform.SetParent(puzzle, false);
            door.transform.position = new Vector3(x, bottomY + height / 2f, 0f);
            door.AddComponent<BoxCollider2D>().size = new Vector2(1f, height);
            int count = Mathf.Max(1, Mathf.RoundToInt(height));
            float step = height / count;
            for (int i = 0; i < count; i++)
            {
                GameObject plank = SpriteBox("Van_" + (i + 1), ObjectSprites + "block_planks.png",
                    new Vector2(x, bottomY + step * (i + 0.5f)), new Vector2(1f, step), 1);
                plank.transform.SetParent(door.transform, true);
            }
            return door;
        }

        /// <summary>Công tắc sàn: có người/hộp đè lên → mở khóa cờ, ẩn các object (cửa, biểu tượng khóa).</summary>
        public PressurePlate Plate(float x, float surfaceY, YellowFlag[] flags = null, GameObject[] hide = null)
        {
            GameObject go = SpriteBox("CongTac", ObjectSprites + "block_strong_empty.png",
                new Vector2(x, surfaceY + 0.12f), new Vector2(1.2f, 0.3f), 1);
            PressurePlate plate = go.AddComponent<PressurePlate>();
            plate.detectSize = new Vector2(1.1f, 0.6f);
            plate.detectOffset = new Vector2(0f, 0.45f);
            plate.flagsToUnlock = flags ?? new YellowFlag[0];
            plate.hideWhilePressed = hide ?? new GameObject[0];
            plate.plateRenderer = go.GetComponent<SpriteRenderer>();
            plate.idleSprite = LoadSprite(ObjectSprites + "block_strong_empty.png");
            plate.pressedSprite = LoadSprite(ObjectSprites + "block_strong_empty_active.png");
            if (flags != null) foreach (YellowFlag flag in flags) SetFlagLocked(flag, true);
            return plate;
        }

        // Cờ là prefab instance: phải ghi nhận override, nếu không lúc lưu scene giá trị cũ sẽ quay lại
        private static void SetFlagLocked(YellowFlag flag, bool locked)
        {
            flag.isLocked = locked;
            PrefabUtility.RecordPrefabInstancePropertyModifications(flag);
            EditorUtility.SetDirty(flag);
        }

        /// <summary>Hàng chông dưới đáy hố (chạm là chết).</summary>
        public void PitSpikes(float x0, float x1)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "spikes_0.prefab");
            float width = 0.96f;
            int count = Mathf.Max(1, Mathf.FloorToInt((x1 - x0) / width));
            float start = (x0 + x1) / 2f - (count - 1) * width / 2f;
            for (int i = 0; i < count; i++)
            {
                GameObject spike = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                spike.name = "Chong_" + (i + 1);
                spike.tag = "Trap";
                spike.transform.SetParent(puzzle, false);
                spike.transform.position = new Vector3(start + i * width, -2.25f, 0f);
                spike.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            }
        }

        /// <summary>Tấm chông treo trên trần, rơi xuống (FallingTrap) khi có người bước vào vùng kích hoạt.</summary>
        public void HangingSlab(float x0, float x1, float y, float zoneLeft, float zoneRight, float zoneBottom, float zoneTop)
        {
            float width = x1 - x0;
            GameObject slab = new GameObject("TamChong");
            slab.tag = "Trap";
            slab.transform.SetParent(puzzle, false);
            slab.transform.position = new Vector3((x0 + x1) / 2f, y, 0f);

            Rigidbody2D rb = slab.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Treo đứng yên; FallingTrap.Start() chuyển sang Dynamic và cho rơi
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            slab.AddComponent<BoxCollider2D>().size = new Vector2(width, 0.9f);
            FallingTrap trap = slab.AddComponent<FallingTrap>();
            trap.fallSpeed = 10f;
            trap.enabled = false;

            int count = Mathf.Max(1, Mathf.RoundToInt(width / 0.8f));
            float step = width / count;
            for (int i = 0; i < count; i++)
            {
                float cx = x0 + step * (i + 0.5f);
                GameObject block = SpriteBox("Khoi_" + (i + 1), ObjectSprites + "block_strong_empty.png",
                    new Vector2(cx, y + 0.2f), new Vector2(step, 0.5f), 1);
                block.transform.SetParent(slab.transform, true);
                GameObject spike = SpriteBox("Gai_" + (i + 1), ObjectSprites + "trap.png",
                    new Vector2(cx, y - 0.25f), new Vector2(step, 0.4f), 1);
                spike.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                spike.transform.SetParent(slab.transform, true);
            }

            GameObject zone = new GameObject("VungKichHoat");
            zone.transform.SetParent(puzzle, false);
            zone.transform.position = new Vector3((zoneLeft + zoneRight) / 2f, (zoneBottom + zoneTop) / 2f, 0f);
            ProximityActivator activator = zone.AddComponent<ProximityActivator>();
            activator.zoneSize = new Vector2(zoneRight - zoneLeft, zoneTop - zoneBottom);
            activator.enableTargets = new Behaviour[] { trap };
        }

        /// <summary>Cầu xoay bắc qua hố giữa hai khối đất (dài hơn hố 1.2 để gác lên hai mép).</summary>
        public void BridgeOver((float left, float right) leftGround, (float left, float right) rightGround, float surfaceY, float tiltDegrees)
        {
            float gap = rightGround.left - leftGround.right;
            Bridge((leftGround.right + rightGround.left) / 2f, surfaceY, gap + 1.2f, tiltDegrees);
        }

        /// <summary>Cầu xoay (RotatableBridge) bắc ngang hố, nghiêng sẵn một góc.</summary>
        public void Bridge(float centerX, float surfaceY, float length, float tiltDegrees)
        {
            Sprite sprite = LoadSprite(ObjectSprites + "bridge_logs.png");
            GameObject bridge = new GameObject("CauXoay");
            bridge.transform.SetParent(puzzle, false);
            bridge.transform.position = new Vector3(centerX, surfaceY - 0.25f, 0f);
            bridge.transform.rotation = Quaternion.Euler(0f, 0f, tiltDegrees);

            SpriteRenderer sr = bridge.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = new Vector2(length, 0.6f);
            sr.sortingOrder = 1;

            BoxCollider2D col = bridge.AddComponent<BoxCollider2D>();
            col.size = new Vector2(length, 0.45f);
            Rigidbody2D rb = bridge.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            RotatableBridge rotatable = bridge.AddComponent<RotatableBridge>();
            rotatable.rotationSpeed = 12f;
            rotatable.maxTiltAngle = Mathf.Abs(tiltDegrees) + 5f;
        }

        /// <summary>Cờ giả: chỉ có hình, không có YellowFlag.</summary>
        public void FakeFlag(float x, float surfaceY)
        {
            GameObject fake = Object.Instantiate(flagRoot, puzzle);
            fake.name = "CoGia";
            foreach (YellowFlag f in fake.GetComponentsInChildren<YellowFlag>(true)) Object.DestroyImmediate(f);
            foreach (Collider2D c in fake.GetComponentsInChildren<Collider2D>(true)) Object.DestroyImmediate(c);
            Transform pole = fake.GetComponentsInChildren<SpriteRenderer>(true).OrderByDescending(t => t.transform.lossyScale.y).First().transform;
            fake.transform.position += new Vector3(x, surfaceY + 1.1f, 0f) - pole.position;
        }

        /// <summary>Tấm biển gỗ có chữ, vẽ đè lên các vật phía sau.</summary>
        public void Sign(float x, float surfaceY, string text, int order)
        {
            GameObject sign = SpriteBox("BienBao", ObjectSprites + "sign.png", new Vector2(x, surfaceY + 1.3f), new Vector2(2.8f, 2.6f), order);
            GameObject label = new GameObject("Chu", typeof(RectTransform));
            label.transform.SetParent(sign.transform, false);
            TextMeshPro tmp = label.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 3.2f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.2f, 0.12f, 0.05f);
            tmp.sortingOrder = order + 1;
            RectTransform rect = (RectTransform)label.transform;
            rect.sizeDelta = new Vector2(2.4f, 1.2f);
            rect.position = new Vector3(x, surfaceY + 1.7f, 0f);
            rect.localScale = new Vector3(1f / sign.transform.localScale.x, 1f / sign.transform.localScale.y, 1f);
        }

        /// <summary>Đám mây kéo thả tự do (DraggableGround bỏ khóa trục Y).</summary>
        public void DraggableCloud(float x, float y, float width)
        {
            Sprite sprite = LoadSprite("Assets/_Project/Sprites/Tilemap/Block_design/terrain_grass_cloud.png");
            GameObject cloud = new GameObject("MayKeoDuoc");
            cloud.transform.SetParent(puzzle, false);
            cloud.transform.position = new Vector3(x, y, 0f);
            SpriteRenderer sr = cloud.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;
            Vector2 native = sprite.bounds.size;
            cloud.transform.localScale = new Vector3(width / native.x, 0.7f / native.y, 1f);
            cloud.AddComponent<BoxCollider2D>();
            DraggableGround drag = cloud.AddComponent<DraggableGround>();
            SerializedObject so = new SerializedObject(drag);
            so.FindProperty("lockYAxis").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Hộp gỗ đẩy được (prefab block_planks_0).</summary>
        public void Box(float x, float surfaceY)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "block_planks_0.prefab");
            GameObject box = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            box.name = "HopGo";
            box.transform.SetParent(puzzle, false);
            Bounds bounds = box.GetComponent<SpriteRenderer>().bounds;
            box.transform.position = new Vector3(x, surfaceY + bounds.extents.y + 0.02f, 0f);
        }

        // ---------- Hoàn thiện ----------
        private void SetupBackground(Transform bg)
        {
            if (bg == null) return;
            bg.position = new Vector3(cam.transform.position.x, cam.transform.position.y + 0.2f, 0f);
            Sprite sprite = LoadSprite(Backgrounds + spec.Background);
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null && sprite != null) sr.sprite = sprite;
        }

        public void Finish()
        {
            CreateNewLevelTool.SetHudLevelLabel("Level " + spec.Id);
            foreach (TextMeshProUGUI t in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
            {
                if (t.name != "HintTextNormal") continue;
                t.text = spec.Prompt;
                EditorUtility.SetDirty(t);
            }

            LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>(FindObjectsInactive.Include);
            Button replay = Object.FindObjectsByType<Button>(FindObjectsInactive.Include).FirstOrDefault(btn => btn.name == "BtnReplay");
            if (replay == null) throw new InvalidOperationException("❌ Không tìm thấy nút BtnReplay trên HUD.");
            for (int i = replay.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(replay.onClick, i);

            if (spec.UsesClones)
            {
                GameObject managerObj = new GameObject("CloneManager");
                SceneManager.MoveGameObjectToScene(managerObj, scene);
                CloneManager clones = managerObj.AddComponent<CloneManager>();
                clones.clonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "PlayerClone.prefab");
                clones.player = player;
                clones.startPoint = startPos;
                UnityEventTools.AddPersistentListener(replay.onClick, clones.CreateCloneAndRespawn);
            }
            else
            {
                UnityEventTools.AddPersistentListener(replay.onClick, levelManager.ReplayLevel);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(replay);
            EditorUtility.SetDirty(levelManager);
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
            if (sprite == null) throw new FileNotFoundException("Thiếu sprite " + path);
            return sprite;
        }
    }
}
