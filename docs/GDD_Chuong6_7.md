# GAME DESIGN DOCUMENT (GDD) — CHƯƠNG 6 & CHƯƠNG 7
## TÀI LIỆU THIẾT KẾ MÀN CHƠI MỞ RỘNG (LEVEL 21 – LEVEL 28)

> **Dự án:** Mr. Eggy Puzzle Game — Đồ án chuyên ngành IT6122  
> **Nhiệm vụ:** W1-10 (bản ý tưởng — Antigravity) → W1-12, W2-08, W3-07 (dựng màn — Claude, bản này)  
> **Nguyên tắc thiết kế:** Ưu tiên cơ chế có sẵn trong `Assets/_Project/Scripts/`. Chỉ thêm 2 script nhỏ: `PressurePlate` (công tắc sàn) và `ProximityActivator` (vùng kích hoạt bẫy).  
> **Cách dựng:** cả 8 màn được dựng bằng tool `Tools → Dựng màn → Chương 6–7 (Level 21–28)` ([`Chapter67LevelBuilder.cs`](../Assets/_Project/Scripts/Editor/Chapter67LevelBuilder.cs)). Muốn chỉnh màn → sửa số trong tool rồi chạy lại; không sửa tay trong scene (chạy lại tool sẽ ghi đè).  
> **Kiểm chứng:** [`Chapter67WalkthroughTests.cs`](../Assets/_Project/Tests_PlayMode/Chapter67WalkthroughTests.cs) — mỗi màn có 1 test chơi theo đúng lời giải (phải thắng) và 1 test chơi "theo bản năng" (không được thắng / phải chết).

> **Thay đổi so với bản ý tưởng W1-10:** bản cũ dựa trên một số giả định không khớp code thật — `PhysicalButton` là nút bấm trên HUD chứ không phải công tắc sàn; chết thì nạp lại scene chứ không để lại xác; `FallingTrap` rơi ngay khi vào màn; nút Pause không kéo được. Bản này thiết kế lại 8 màn cho khớp code.

---

## 1. Giới thiệu tổng quan 2 Chương mở rộng

```
[Hành trình 28 màn chơi]
├── Chương 1-5 (Level 1–20): Làm quen, Nhảy vô hạn, Kéo đất, Hộp vật lý, Bẫy troll cơ bản.
├── Chương 6 (Level 21–24): "Ảo Ảnh Song Trùng" — bản sao (Clone).
└── Chương 7 (Level 25–28): "Bẻ Cong Thứ Nguyên" — kéo thả vật thể & bẫy thị giác.
```

### 1.1. Các cơ chế dùng trong 2 chương

| Cơ chế | Script | Hoạt động |
|---|---|---|
| **Bản sao** | [`CloneManager.cs`](../Assets/_Project/Scripts/CloneManager.cs) + `PlayerClone.prefab` | Ở Chương 6, nút **⟳** trên HUD không chơi lại mà gọi `CreateCloneAndRespawn()`: để lại một bản sao đứng yên (Rigidbody Static, tag `Player`) đúng chỗ nhân vật — kể cả khi đang nhảy giữa không trung — rồi đưa nhân vật về điểm xuất phát. Bản sao là vật rắn: đứng lên được, đỡ được vật rơi, giữ được công tắc. |
| **Công tắc sàn** (mới) | [`PressurePlate.cs`](../Assets/_Project/Scripts/PressurePlate.cs) | Mỗi bước vật lý dò `OverlapBoxAll` phía trên công tắc; có vật tag `Player` (nhân vật, bản sao) hoặc `PushableBox` đè lên thì *bật*: ẩn các object trong `hideWhilePressed` (cửa, biểu tượng khóa), mở khóa các `YellowFlag` trong `flagsToUnlock`, đổi sprite. Rời đi → trở lại như cũ. Dò bằng overlap vì bản sao là Static, không sinh sự kiện va chạm. |
| **Vùng kích hoạt** (mới) | [`ProximityActivator.cs`](../Assets/_Project/Scripts/ProximityActivator.cs) | Có `Player` đi vào vùng thì bật các object/component đích. Dùng để treo sẵn `FallingTrap` (đang tắt) trên trần; bước vào vùng thì bẫy mới rơi. |
| **Bẫy rơi** | [`FallingTrap.cs`](../Assets/_Project/Scripts/FallingTrap.cs) | Rơi với `fallSpeed`, chạm vật không phải nhân vật/bẫy thì dừng (bị đỡ lại). Tag `Trap` → chạm là chết. |
| **Cầu xoay** | [`RotatableBridge.cs`](../Assets/_Project/Scripts/RotatableBridge.cs) | Chạm vào cầu rồi kéo lên/xuống để xoay, giới hạn bởi `maxTiltAngle`. |
| **Vật kéo được** | [`DraggableGround.cs`](../Assets/_Project/Scripts/DraggableGround.cs) | Chạm rồi kéo vật đi khắp màn hình (`lockYAxis = false`). |
| **Hộp đẩy** | prefab `block_planks` (tag `PushableBox`) | Hộp vật lý; nhân vật đi vào là đẩy được. |
| **Ảo ảnh** | Tilemap không có collider; cờ giả không có `YellowFlag` | Trông như mặt đất / lá cờ thật nhưng không đứng được / không thắng được. |

Thông số vật lý dùng khi thiết kế (từ `PlayerController`: speed 5, jumpForce 12, gravityScale 2.5): **nhảy cao ≈ 2.94**, **nhảy xa trên mặt phẳng ≈ 4.9**, collider nhân vật cao ≈ 0.8. Một ô đất cao ≈ 1.17, rộng ≈ 1.21. Mỗi màn nằm gọn trong một khung hình (camera size 5); nhân vật xuất phát trong vùng an toàn của màn hình 4:3 (`PlayerDeath` giết khi nhân vật ra khỏi viewport).

Chữ trên HUD (`HintTextNormal`) là **câu đố** của màn; nút 💡 là **gợi ý** (`GameHintData`).

---

## 2. Chương 6: "Ảo Ảnh Song Trùng" (Level 21 – Level 24)

Chủ đề: nút **⟳** biến thành nút *tạo bản sao*. Người chơi phải nhận ra "chơi lại" là một công cụ, rồi dùng bản sao lần lượt làm bậc thang → người giữ cửa → cột chống trần → phối hợp hai bản sao.

### Level 21: Bậc Thang Thân Xác
- **Câu đố HUD:** *"Bức tường quá cao để nhảy lên"* · **Độ khó:** 2/5 (nhập môn bản sao)
- **Bố cục:**
```
                                    [CỜ]
                         ████████████████
                         ████████████████   ← tường 3 ô ≈ 3.5 > nhảy cao 2.94
  [P]                    ████████████████
 ████████████████████████████████████████████
```
  - Nhân vật x = −6.5; tường/tháp từ x = 1.8 đến hết màn, cao 3 ô; cờ x = 6.5 trên đỉnh tháp.
- **Lời giải:** đi tới chân tường → bấm ⟳ (bản sao cao ≈ 0.8 đứng lại) → đi lại chân tường, nhảy lên bản sao rồi nhảy lên tháp (0.8 + 2.94 > 3.5) → chạm cờ.
- **Chơi theo bản năng:** nhảy không tới đỉnh, kẹt ở chân tường.
- **Gợi ý 💡:** *"Nút ⟳ ở màn này không chơi lại mà để lại một bản sao đứng yên tại chỗ. Bản sao cũng là bậc thang!"*
- **Test:** `Level21_CloneAtWallFoot_BecomesStep`, `Level21_WithoutClone_CannotClimbWall`.

### Level 22: Người Giữ Cửa
- **Câu đố HUD:** *"Cửa chỉ mở khi có người đứng trên công tắc"* · **Độ khó:** 3/5
- **Bố cục:**
```
                           ██████████████████████  ← trần phủ cả phía trên cửa
                              ║
  [P]        [CÔNG TẮC]       ║ CỬA            [CỜ]
 ██████████████████████████████████████████████████
```
  - Nhân vật x = −7; công tắc `PressurePlate` x = −2.5; cửa (cột ván gỗ) x = 3.9, nối từ sàn lên trần; trần 1 ô từ x = 3.2 trở đi; cờ x = 7.
- **Lời giải:** đứng lên công tắc → cửa biến mất → bấm ⟳ (bản sao ở lại giữ công tắc, nhân vật về đầu màn) → đi thẳng qua cửa đang mở → chạm cờ.
- **Chơi theo bản năng:** bước khỏi công tắc là cửa đóng lại; trần phủ phía trên cửa nên chồng bản sao để trèo qua cũng không lọt.
- **Gợi ý 💡:** *"Một người giữ cửa, một người đi qua. Đứng trên công tắc rồi bấm ⟳."*
- **Test:** `Level22_CloneHoldsPlate_DoorStaysOpen`, `Level22_DoorBlocksWithoutHelper`.

### Level 23: Cột Chống Trần
- **Câu đố HUD:** *"Trần nhà hơi bất ổn..."* · **Độ khó:** 3/5
- **Bố cục:**
```
             ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼  ← tấm chông treo (FallingTrap đang tắt)
                 ┊ vùng kích hoạt ┊
  [P]            ┊                ┊          [CỜ]
 ██████████████████████████████████████████████████
```
  - Tấm chông "TamChong" x ∈ [−1.3; 4.6], treo cao; vùng kích hoạt `ProximityActivator` x ∈ [0.3; 4.6] — mép trái tấm chông thò ra ngoài vùng. Cờ x = 7.
- **Lời giải:** đứng dưới mép trái tấm chông (x ≈ −0.9, còn ngoài vùng) → nhảy lên, gần đỉnh thì bấm ⟳ → bản sao lơ lửng giữa không trung → đi qua vùng: tấm chông rơi, bị bản sao đỡ lại ở độ cao an toàn → đi bên dưới tới cờ.
- **Chơi theo bản năng:** bước vào vùng, tấm chông rơi trúng đầu → chết, chơi lại.
- **Gợi ý 💡:** *"Nhảy lên ngay dưới mép trái tấm chông rồi bấm ⟳: bản sao giữa không trung sẽ thành cột chống trần."*
- **Test:** `Level23_MidAirClone_HoldsUpFallingSlab`, `Level23_WalkingStraight_IsCrushed`.

### Level 24: Cờ Bị Khóa (màn kết Chương 6)
- **Câu đố HUD:** *"Lá cờ trên tháp đang bị khóa"* · **Độ khó:** 4/5
- **Bố cục:**
```
                                         [!] [CỜ]  ← biểu tượng khóa
                                  ███████████████
  [P]     [CÔNG TẮC]              ███████████████   ← tháp 3 ô
 ██████████████████        ███████████████████████
                   ▲▲▲▲▲▲▲  (hố chông ≈ 2.4)
```
  - Nhân vật x = −7.5; công tắc x = −5 (mở khóa cờ, ẩn biểu tượng "!"); hố chông giữa x ≈ −1.6 … 1.6; tháp 3 ô từ x = 4.2; cờ x = 7.2 trên tháp (`YellowFlag.isLocked` = true khi công tắc chưa bị đè).
- **Lời giải:** nhảy qua hố tới chân tháp, bấm ⟳ (bản sao 1 làm bậc) → về đầu màn, đứng lên công tắc, bấm ⟳ (bản sao 2 giữ khóa mở) → về đầu màn, nhảy qua bản sao 2, qua hố, lên bản sao 1, lên tháp → chạm cờ. Làm theo thứ tự ngược lại cũng được.
- **Chơi theo bản năng:** trèo lên được tháp nhưng chạm cờ đang khóa thì không thắng.
- **Gợi ý 💡:** *"Cần hai bản sao: một bản đứng giữ công tắc, một bản làm bậc thang ở chân tháp."*
- **Test:** `Level24_TwoClones_UnlockFlagAndClimbTower`, `Level24_LockedFlag_DoesNotWin`.

---

## 3. Chương 7: "Bẻ Cong Thứ Nguyên" (Level 25 – Level 28)

Chủ đề: không còn bản sao (⟳ trở lại là chơi lại). Người chơi phải **chạm/kéo vật thể trên màn hình** và **không tin vào mắt mình**.

### Level 25: Cây Cầu Hỏng
- **Câu đố HUD:** *"Cây cầu bị hỏng rồi, đừng đụng vào nó!"* · **Độ khó:** 3/5
- **Bố cục:**
```
  [P]                 ╲  cầu gỗ nghiêng 45°                  [CỜ]
 ████████████████      ╲                      ████████████████████
                  ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲  (hố chông ≈ 6.0 > nhảy xa 4.9)
```
  - Hố x ≈ −3.2 … 3.2; cầu "CauXoay" (`RotatableBridge`) bắc trên hố, nghiêng −45°.
- **Lời giải:** câu đố bảo "đừng đụng", nhưng phải chạm vào cầu và kéo cho nó nằm ngang → đi qua → chạm cờ.
- **Chơi theo bản năng:** hố quá rộng để nhảy qua; đứng lên cầu nghiêng 45° là trượt xuống chông.
- **Gợi ý 💡:** *"Chạm vào cây cầu rồi kéo lên/xuống để nó nằm ngang."*
- **Test:** `Level25_FlattenBridge_CrossPit`, `Level25_TiltedBridge_CannotCross`.

### Level 26: Cờ Giả
- **Câu đố HUD:** *"Về đích thật nhanh!"* · **Độ khó:** 3.5/5 (troll tâm lý)
- **Bố cục:**
```
 [BIỂN "Không có gì ở đây đâu"]  [P]                    [CỜ GIẢ]
  (cờ thật nấp sau biển)
 █████████████████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
                                  ↑ từ x = −0.6: mặt đất ảo (không có va chạm)
```
  - Nhân vật x = −4.5, phía trước là cờ giả x = 4.5. Cờ thật x = −7.6, bị tấm biển (vẽ đè lên trên) che kín.
- **Lời giải:** đừng tin lá cờ phía trước — quay lại, đi về bên trái, chạm vào tấm biển → thắng.
- **Chơi theo bản năng:** chạy về phía cờ giả, rơi xuyên qua mặt đất ảo → chết.
- **Gợi ý 💡:** *"Không phải lá cờ nào cũng thật, không phải mặt đất nào cũng đứng được. Thử quay lại phía sau xem."*
- **Test:** `Level26_RealFlagIsBehindTheSign`, `Level26_FakeFlagSide_FallsThroughIllusion`.

### Level 27: Mái Che Bằng Mây
- **Câu đố HUD:** *"Trời sắp sập rồi!"* · **Độ khó:** 4/5
- **Bố cục:** giống Level 23 (tấm chông treo + vùng kích hoạt), thêm một **đám mây kéo được** (`DraggableGround`, rộng 2.4) ở góc trên bên trái; không có bản sao. Nhân vật x = −7.5, cờ x = 7.2.
- **Lời giải:** gợi nhớ Level 23 nhưng đổi công cụ: kéo đám mây xuống dưới mép trái tấm chông (ngoài vùng kích hoạt) làm cột chống → đi qua, tấm chông rơi và mắc lên mây → chạm cờ.
- **Chơi theo bản năng:** bị tấm chông đè → chết.
- **Gợi ý 💡:** *"Đám mây kéo được. Đặt nó dưới mép trái tấm chông trước khi đi qua."*
- **Test:** `Level27_CloudAsRoof_StopsSlab`, `Level27_WithoutRoof_IsCrushed`.

### Level 28: Thử Thách Cuối Cùng (màn kết toàn game)
- **Câu đố HUD:** *"Thử thách cuối cùng"* · **Độ khó:** 5/5
- **Bố cục:**
```
                                      ████████████████████  ← trần
                                           ║
  [P] [HỘP]  [CÔNG TẮC]▄▄   ╲ cầu 45°      ║ CỬA     [CỜ]
 ██████████████████████       ╲        ████████████████████
                      ↑ bậc chặn 1 ô
                        ▲▲▲▲▲▲▲▲▲▲▲▲▲  (hố chông ≈ 6.0)
```
  - Nhân vật x = −7.6; hộp gỗ x = −5.8; công tắc nằm sát chân một bậc chặn cao 1 ô (mép bờ trái hố); hố x ≈ −1.9 … 4.6 có cầu xoay nghiêng −45°; cửa x = 5.1 (công tắc ẩn cửa); cờ x = 7.8.
- **Lời giải:** đẩy hộp tới khi nó tựa vào bậc chặn — lúc đó hộp nằm đúng trên công tắc, cửa mở → kéo cầu nằm ngang → nhảy lên hộp, lên bậc, đi qua cầu, qua cửa → chạm cờ. Kết hợp ba cơ chế của các chương trước: hộp (Ch.4), công tắc (Ch.6), cầu xoay (Ch.7).
- **Chơi theo bản năng:** không có gì giữ công tắc → cửa đóng; cầu nghiêng thì trượt xuống chông.
- **Gợi ý 💡:** *"Đẩy hộp lên công tắc để mở cửa, rồi kéo cầu nằm ngang."*
- **Test:** `Level28_PushBoxOntoPlate_FlattenBridge`, `Level28_DoorClosedWithoutBox`.

---

## 4. Đường cong độ khó toàn Game (Difficulty Curve: Level 1 – Level 28)

### 4.1. Bảng phân phối độ khó và Cơ chế chính từng màn

| Level | Chương | Cơ chế chủ đạo | Độ khó (1–5) | Vai trò nhịp độ (Pacing Role) |
|:---:|:---:|---|:---:|---|
| **L01** | **Ch.1** | Di chuyển cơ bản (Trái/Phải/Nhảy) | 1 | Giới thiệu (Onboarding) |
| **L02** | Ch.1 | Cạm bẫy hố chông thụt thò | 1 | Khám phá (Discovery) |
| **L03** | Ch.1 | Bẫy gai rơi bất ngờ | 2 | Thử thách nhẹ |
| **L04** | Ch.1 | Màn kết chương: Nhảy đà tốc độ | **3** | Cao trào Chương 1 (Boss L1) |
| **L05** | **Ch.2** | Triple Jump (Nhảy 3 lần trên không) | 2 | Giới thiệu cơ chế mới |
| **L06** | Ch.2 | Nhảy liên hoàn qua vực sâu | 2 | Thực hành cơ chế |
| **L07** | Ch.2 | Căn nhịp nhảy tránh bẫy rơi | 3 | Thử thách nâng cao |
| **L08** | Ch.2 | Màn kết chương: Triple Jump + Chông di động | **3.5** | Cao trào Chương 2 |
| **L09** | **Ch.3** | Kéo thả đất (`DraggableGround`) | 2 | Phá vỡ quy tắc: Tương tác chạm |
| **L10** | Ch.3 | Kéo đất làm khiên chắn | 2.5 | Vận dụng kéo đất |
| **L11** | Ch.3 | Kéo đất nối đường trên vực thẳm | 3 | Thử thách khéo léo |
| **L12** | Ch.3 | Màn kết chương: Kéo cờ về phía nhân vật | **3.5** | Màn troll kinh điển |
| **L13** | **Ch.4** | Đẩy hộp & Vác hộp (`PlayerBoxCarrier`) | 2.5 | Giới thiệu vật lý thùng gỗ |
| **L14** | Ch.4 | Xếp chồng 2 hộp làm bậc thang | 3 | Tư duy logic khối |
| **L15** | Ch.4 | Vác hộp chặn đường lăn của đá | 3 | Căn thời gian (Timing) |
| **L16** | Ch.4 | Màn kết chương: Thả hộp vào DropZone kích hoạt cờ | **4** | Cao trào Chương 4 |
| **L17** | **Ch.5** | Bẫy troll camera biên (`PlayerDeath`) | 3 | Troll không gian |
| **L18** | Ch.5 | Cờ di động né tránh nhân vật | 3.5 | Troll đuổi bắt |
| **L19** | Ch.5 | Đảo ngược trọng lực màn chơi | 3.5 | Thay đổi tư duy |
| **L20** | Ch.5 | Màn kết chương: Hộp khổng lồ `GiantBox` vào đích | **4** | Cao trào Chương 5 |
| **L21** | **Ch.6** | Bản sao làm bậc thang lên tường cao | 2 | Giới thiệu Clone (Thả lỏng) |
| **L22** | Ch.6 | Bản sao giữ công tắc sàn `PressurePlate` mở cửa | 3 | Vận dụng Clone |
| **L23** | Ch.6 | Bản sao giữa không trung chống tấm chông rơi | 3 | Thử thách kết hợp |
| **L24** | Ch.6 | Màn kết chương: 2 bản sao (mở khóa cờ + bậc thang) + hố chông | **4** | Cao trào Chương 6 |
| **L25** | **Ch.7** | Kéo cầu xoay (`RotatableBridge`) nằm ngang qua hố | 3 | Giới thiệu kéo thả |
| **L26** | Ch.7 | Cờ giả + mặt đất ảo, cờ thật nấp sau biển | 3.5 | Màn troll tâm lý |
| **L27** | Ch.7 | Kéo đám mây (`DraggableGround`) chống tấm chông rơi | 4 | Đổi công cụ cho bài toán L23 |
| **L28** | Ch.7 | Đại kết cục: Hộp + Công tắc + Cầu xoay | **5** | ĐỈNH CAO THỬ THÁCH (FINAL BOSS) |

---

### 4.2. Biểu đồ nhịp độ (Pacing Graph — Dạng sóng hình sin răng cưa)

Đường cong độ khó tuân thủ quy luật sóng răng cưa (Sawtooth Wave Curve): Độ khó tăng dần từ đầu chương đến màn kết chương, sau đó hạ nhiệt ở đầu chương tiếp theo để người chơi làm quen với cơ chế mới (tránh ức chế dồn dập), rồi lại leo thang lên đỉnh cao mới.

```
Độ khó (1 - 5)
 5 |                                                                     [*] L28
   |                                                                /
 4 |                 [*] L16             [*] L20             [*] L24   [*] L27
   |                /                   /                   /         /
 3 |     [*] L04   /         [*] L12   /         [*] L22,L23   [*] L25, L26 (3.5)
   |    /         /         /         /         /
 2 | [*] L01-03 [*] L05-07 [*] L09-10  /       [*] L21 (Làm quen Clone)
   |                                  /
 1 |_________________________________L13-14___________________________________
     L01       L05       L09       L13       L17       L21       L25       L28
     (Chương 1) (Chương 2) (Chương 3) (Chương 4) (Chương 5) (Chương 6) (Chương 7)
```
