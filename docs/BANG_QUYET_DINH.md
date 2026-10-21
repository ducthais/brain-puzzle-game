# BẢNG QUYẾT ĐỊNH (DECISION TABLES) — HỆ THỐNG MR. EGGY

> **Tài liệu kiểm thử phần mềm — Đồ án chuyên ngành IT6122**  
> **Áp dụng kỹ thuật kiểm thử Hộp đen (Black-box Testing): Bảng quyết định (Decision Table Testing)**  
> Tất cả các bảng dưới đây được đối chiếu và khớp 100% với mã nguồn thực tế trong `Assets/_Project/Scripts/Core/` và `Scripts/Services/`.

---

## 1. Bảng quyết định 1: Luật tính 3 sao kết thúc màn (`StarCalculator.Calculate`)

### 1.1. Mô tả nghiệp vụ
Khi người chơi chạm đích (hoặc bấm nút "Nhảy" bỏ qua màn), hệ thống tính toán số sao đạt được (từ 0 đến 3 sao) dựa trên 4 biến trạng thái của phiên chơi:
- `completed`: Người chơi có hoàn thành màn chơi hay không.
- `skipped`: Người chơi có dùng quyền trợ giúp "Nhảy" (bỏ qua màn) hay không.
- `deaths`: Số lần nhân vật chết trong suốt phiên chơi của màn đó.
- `hintsUsed`: Số lần người chơi bấm mở bảng nội dung gợi ý trong màn đó.

### 1.2. Bảng quyết định chi tiết

| Thành phần | Mã | Điều kiện / Hành động | R1 (Chưa xong) | R2 (Bỏ qua) | R3 (Hoàn hảo) | R4 (Xem hint) | R5 (Có chết) | R6 (Chết & Hint) |
|---|---|---|:---:|:---:|:---:|:---:|:---:|:---:|
| **Điều kiện (Conditions)** | C1 | Đã qua màn (`completed = true`)? | **K** | **C** | **C** | **C** | **C** | **C** |
| | C2 | Dùng nút "Nhảy" bỏ qua (`skipped = true`)? | — | **C** | **K** | **K** | **K** | **K** |
| | C3 | Không chết (`deaths <= 0`)? | — | — | **C** | **C** | **K** | **K** |
| | C4 | Không xem gợi ý (`hintsUsed <= 0`)? | — | — | **C** | **K** | **C** | **K** |
| **Hành động (Actions)** | A1 | Đạt 0 sao (`stars = 0`) | **X** | | | | | |
| | A2 | Đạt 1 sao (`stars = 1`) | | **X** | | | | **X** |
| | A3 | Đạt 2 sao (`stars = 2`) | | | | **X** | **X** | |
| | A4 | Đạt 3 sao (`stars = 3`) | | | **X** | | | |

*Ghi chú ký hiệu:* `C` = Có / Đúng (True); `K` = Không / Sai (False); `—` = Không quan tâm (Don't care); `X` = Hành động được thực thi.

### 1.3. Ánh xạ mã nguồn và Ca kiểm thử tự động
- **Mã nguồn thực thi:** [`Assets/_Project/Scripts/Core/StarCalculator.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/Core/StarCalculator.cs)
- **Kiểm thử tự động EditMode:** [`Assets/_Project/Tests_EditMode/StarCalculatorTests.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Tests_EditMode/StarCalculatorTests.cs)
  - `Calculate_FollowsDecisionTable` (các test case `R1_ChuaQuaMan_0Sao`, `R2_BoQuaMan_1Sao`, `R3_KhongChet_KhongHint_3Sao`, `R4_KhongChet_CoHint_2Sao`, `R5_CoChet_KhongHint_2Sao`, `R6_CoChet_CoHint_1Sao`).
  - `Calculate_OneDeath_LosesNoDeathStar` (kiểm tra giá trị biên: `deaths = 1` lập tức mất sao).
  - `Calculate_NegativeCounters_TreatedAsZero` (xử lý dữ liệu bất thường: đếm âm xem như 0).
- **Kiểm thử tự động PlayMode:** [`Assets/_Project/Tests_PlayMode/StarAndProgressTests.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Tests_PlayMode/StarAndProgressTests.cs)
  - `WinGame_PerfectRun_AwardsThreeStars_AndUnlocksNextLevel` (ứng với R3).
  - `WinGame_WithDeath_AwardsTwoStars` (ứng với R5).
  - `OnClickNhay_Canvas2_SkipsLevelWithOneStar` (ứng với R2).

---

## 2. Bảng quyết định 2: Mở gợi ý trực tiếp (`HintService.CanOpenHintDirectly`)

### 2.1. Mô tả nghiệp vụ
Khi người chơi bấm vào biểu tượng bóng đèn (Gợi ý) trên HUD màn chơi:
- Nếu tài khoản đã mua gói **Premium** (VIP) HOẶC đã xem quảng cáo mở khóa thành công trong màn hiện tại (`unlockedThisLevel = true`), game sẽ mở thẳng `HintCanvas2` (nội dung câu đố) mà không bắt xem quảng cáo.
- Nếu là tài khoản miễn phí và chưa mở khóa ở màn này, game hiển thị popup `HintCanvas1` hỏi người chơi có muốn xem quảng cáo nhận thưởng hay không.

### 2.2. Bảng quyết định chi tiết

| Thành phần | Mã | Điều kiện / Hành động | R1 (Free chưa mở) | R2 (Premium) | R3 (Free đã xem Ads) | R4 (Premium đã xem) |
|---|---|---|:---:|:---:|:---:|:---:|
| **Điều kiện (Conditions)** | C1 | Tài khoản Premium (`isPremium = true`)? | **K** | **C** | **K** | **C** |
| | C2 | Đã mở khóa ở màn này (`unlockedThisLevel = true`)? | **K** | **K** | **C** | **C** |
| **Hành động (Actions)** | A1 | Hiện popup hỏi xem Ads (`hintCanvas1 = True`, `hintCanvas2 = False`) | **X** | | | |
| | A2 | Mở thẳng nội dung gợi ý (`hintCanvas2 = True`, `hintCanvas1 = False`) | | **X** | **X** | **X** |

### 2.3. Ánh xạ mã nguồn và Ca kiểm thử tự động
- **Mã nguồn thực thi:** [`Assets/_Project/Scripts/Services/HintService.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/Services/HintService.cs)
- **Kiểm thử tự động EditMode:** [`Assets/_Project/Tests_EditMode/LevelSessionAndNamingTests.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Tests_EditMode/LevelSessionAndNamingTests.cs)
  - Test case: `CanOpenHintDirectly_DecisionTable(bool isPremium, bool unlocked, bool expected)` (kiểm thử vét cạn cả 4 luật R1–R4).
- **Kiểm thử tự động PlayMode / Tích hợp:** [`Assets/_Project/Tests_EditMode/LevelManagerTest.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Tests_EditMode/LevelManagerTest.cs)
  - `Test_ClickHint_PremiumUser_OpensCanvas2Immediately` (R2).
  - `Test_ClickHint_FreeUser_OpensCanvas1First` (R1).

---

## 3. Bảng quyết định 3: Tiến trình mở khóa màn và lưu sao (`ProgressService.CompleteLevel`)

### 3.1. Mô tả nghiệp vụ
Khi người chơi chiến thắng màn chơi hiện tại (`levelId`):
1. **Mở khóa màn:** Nếu `levelId` lớn hơn hoặc bằng `maxUnlockedLevel` hiện tại thì tăng `maxUnlockedLevel = levelId + 1`. Ngược lại, nếu người chơi chơi lại một màn cũ trong quá khứ thì giữ nguyên `maxUnlockedLevel`.
2. **Cập nhật sao:** So sánh số sao vừa đạt (`stars`) với kỷ lục sao cũ của màn đó (`GetStars(levelId)`). Chỉ ghi đè khi số sao mới lớn hơn kỷ lục cũ (`SetStarsIfHigher`).
3. **Màn lưu trữ chơi tiếp:** Nếu có cung cấp `nextLevelName` hợp lệ thì cập nhật `savedLevel = nextLevelName`.

### 3.2. Bảng quyết định chi tiết

| Thành phần | Mã | Điều kiện / Hành động | R1 (Màn mới, sao cao) | R2 (Màn mới, sao thấp/bỏ qua) | R3 (Chơi lại, sao cao hơn) | R4 (Chơi lại, sao thấp hơn) | R5 (Tên scene kế rỗng) |
|---|---|---|:---:|:---:|:---:|:---:|:---:|
| **Điều kiện (Conditions)** | C1 | Là màn cao nhất hiện tại (`levelId >= maxUnlockedLevel`)? | **C** | **C** | **K** | **K** | **C** |
| | C2 | Số sao mới cao hơn kỷ lục cũ (`newStars > oldStars`)? | **C** | **C** | **C** | **K** | **C** |
| | C3 | Có tên màn kế (`!string.IsNullOrEmpty(nextLevelName)`)? | **C** | **C** | **C** | **C** | **K** |
| **Hành động (Actions)** | A1 | Tăng màn mở khóa: `maxUnlockedLevel = levelId + 1` | **X** | **X** | | | **X** |
| | A2 | Giữ nguyên `maxUnlockedLevel` | | | **X** | **X** | |
| | A3 | Cập nhật kỷ lục sao mới (`SetStarsIfHigher`) | **X** | **X** | **X** | | **X** |
| | A4 | Giữ nguyên số sao cũ của màn đó | | | | **X** | |
| | A5 | Lưu màn tiếp theo vào `savedLevel` | **X** | **X** | **X** | **X** | |
| | A6 | Giữ nguyên `savedLevel` cũ | | | | | **X** |
| | A7 | Bắn sự kiện `Changed` và gọi `Store.Save()` | **X** | **X** | **X** | **X** | **X** |

### 3.3. Ánh xạ mã nguồn và Ca kiểm thử tự động
- **Mã nguồn thực thi:** [`Assets/_Project/Scripts/Services/ProgressService.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/Services/ProgressService.cs) & [`Assets/_Project/Scripts/Core/ProgressData.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/Core/ProgressData.cs)
- **Kiểm thử tự động EditMode:** [`Assets/_Project/Tests_EditMode/ProgressServiceTests.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Tests_EditMode/ProgressServiceTests.cs)
  - `CompleteLevel_UnlocksNextLevel_AndSaves` (R1).
  - `ReplayingOldLevel_DoesNotLowerMaxUnlocked` (R3, R4).
  - `CompleteLevel_WithFewerStars_KeepsBestStars` (R4).
  - `Changed_IsRaisedOnEveryCommit` (A7).
- **Kiểm thử tự động PlayMode:** [`Assets/_Project/Tests_PlayMode/StarAndProgressTests.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Tests_PlayMode/StarAndProgressTests.cs)
  - `WinGame_ReplayingWithFewerStars_DoesNotLowerSavedStars` (R4).

---

## 4. Bảng quyết định 4: Gộp tiến trình Local và Cloud (`ProgressMerger.Merge`)

### 4.1. Mô tả nghiệp vụ
Được gọi khi game tải thành công dữ liệu lưu trữ từ đám mây (Unity Cloud Save) về máy người chơi:
- Nguyên tắc cốt lõi: **"Luật vị tha" — luôn lấy giá trị tốt nhất cho người chơi**, không bao giờ làm mất tiến trình đã vượt qua ở bất kỳ thiết bị nào.
- `maxUnlockedLevel`: Lấy giá trị lớn nhất `max(local, remote)`.
- `savedLevel`: Lấy theo bên có `maxUnlockedLevel` lớn hơn; nếu hai bên bằng nhau (hòa) thì ưu tiên giữ `savedLevel` của Local.
- `stars[level]`: Lấy `max(local, remote)` độc lập cho từng màn chơi.
- `updatedAtUtc`: Lấy chuỗi thời gian muộn hơn (so sánh theo chuẩn ISO 8601).
- Xử lý biên: Nếu một trong hai bên là `null`, trả về bản sao (Clone) của bên còn lại.

### 4.2. Bảng quyết định chi tiết

| Thành phần | Mã | Điều kiện / Hành động | R1 (Null Local) | R2 (Null Remote) | R3 (Remote tiến xa hơn) | R4 (Local tiến xa hơn) | R5 (Tiến trình bằng nhau) |
|---|---|---|:---:|:---:|:---:|:---:|:---:|
| **Điều kiện (Conditions)** | C1 | Dữ liệu Local tồn tại (`local != null`)? | **K** | **C** | **C** | **C** | **C** |
| | C2 | Dữ liệu Remote tồn tại (`remote != null`)? | **C** | **K** | **C** | **C** | **C** |
| | C3 | Mức độ mở khóa: So sánh `remote.max` với `local.max` | — | — | `remote > local` | `remote < local` | `remote == local` |
| | C4 | Thời gian cập nhật: `remote.updatedAtUtc` mới hơn `local`? | — | — | Tùy thời điểm | Tùy thời điểm | Tùy thời điểm |
| **Hành động (Actions)** | A1 | Trả về bản sao nguyên vẹn của Remote | **X** | | | | |
| | A2 | Trả về bản sao nguyên vẹn của Local | | **X** | | | |
| | A3 | Chọn `maxUnlockedLevel` theo Remote | | | **X** | | **X** |
| | A4 | Chọn `maxUnlockedLevel` theo Local | | | | **X** | **X** |
| | A5 | Chọn `savedLevel` theo Remote | | | **X** | | |
| | A6 | Chọn `savedLevel` theo Local | | | | **X** | **X** (Ưu tiên hòa) |
| | A7 | Gộp sao: Lấy `max(local, remote)` từng màn | | | **X** | **X** | **X** |
| | A8 | Lấy mốc thời gian muộn hơn | | | **X** | **X** | **X** |

### 4.3. Ánh xạ mã nguồn và Ca kiểm thử tự động
- **Mã nguồn thực thi:** [`Assets/_Project/Scripts/Core/ProgressMerger.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/Core/ProgressMerger.cs)
- **Kiểm thử tự động EditMode:** [`Assets/_Project/Tests_EditMode/ProgressMergerTests.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Tests_EditMode/ProgressMergerTests.cs)
  - `Merge_TakesHigherMaxUnlockedLevel` (R3, R4).
  - `Merge_SavedLevelFollowsSideWithMoreProgress` (R3, R4).
  - `Merge_EqualProgress_KeepsLocalSavedLevel` (R5: kiểm tra luật hòa).
  - `Merge_TakesHigherStarsPerLevel` (A7: gộp sao chéo màn).
  - `Merge_TakesLaterUpdatedAt` (A8: so sánh mốc ISO 8601).
  - `Merge_NullSides_ReturnsCopyOfOther` (R1, R2).
  - `Merge_DoesNotModifyInputs` (tính bất biến của tham số đầu vào).
- **Kiểm thử tích hợp Service:** [`Assets/_Project/Tests_EditMode/ProgressServiceTests.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Tests_EditMode/ProgressServiceTests.cs)
  - `MergeFrom_AppliesCloudProgress` (kiểm tra `ProgressService` lưu và phát sự kiện sau khi gộp).
