# KIẾN TRÚC MÃ NGUỒN — MR. EGGY

> Task W1-06. Sơ đồ viết bằng PlantUML: dán vào https://www.plantuml.com/plantuml hoặc dùng extension PlantUML của VS Code để xuất ảnh cho báo cáo.

## 1. Kiến trúc TRƯỚC khi tái cấu trúc

`LevelManager` ôm nhiều trách nhiệm; nhiều lớp đọc/ghi thẳng `PlayerPrefs` với key dạng chuỗi rải rác.

```plantuml
@startuml KienTruc_Truoc
skinparam classAttributeIconSize 0
title Kiến trúc trước tái cấu trúc

class LevelManager <<MonoBehaviour>> {
  +levelDatabase : LevelDatabase
  +nextLevelName : string
  +OnClickHintButton()
  +OnClickCo_Canvas1()
  +OnClickNhay_Canvas2()
  +WinGame()
  -GetCurrentLevelNumber() : int
  .. tra cứu hint (LINQ) ..
  .. ghi PlayerPrefs "MaxUnlockedLevel", "SavedLevel" ..
}
class LevelSelectManager <<MonoBehaviour>> {
  +RefreshLevelButtons()
  +LoadLevel(levelId)
  .. đọc PlayerPrefs "MaxUnlockedLevel" ..
}
class MenuController <<MonoBehaviour>> {
  +OnConfirmYesClicked()
  .. ghi PlayerPrefs (reset) ..
}
class PlayerDeath <<MonoBehaviour>>
class PremiumManager <<MonoBehaviour>> {
  +IsPremium : bool
  .. PlayerPrefs "isPremium" ..
}
class AdManager <<MonoBehaviour>>
class LevelDatabase <<ScriptableObject>>
database PlayerPrefs

LevelManager --> LevelDatabase
LevelManager --> PremiumManager
LevelManager --> AdManager
LevelManager --> PlayerPrefs
LevelSelectManager --> PlayerPrefs
MenuController --> PlayerPrefs
PremiumManager --> PlayerPrefs
PlayerDeath --> AdManager
PlayerDeath --> LevelManager : ReplayLevel()
@enduml
```

**Vấn đề:**
- Logic tiến trình trộn với UI → muốn test phải dựng cả GameObject, Canvas.
- Không có chỗ "cắm" Cloud Save hay Analytics mà không sửa nhiều lớp.
- Key PlayerPrefs viết tay ở nhiều nơi → dễ sai chính tả, khó đổi.

## 2. Kiến trúc SAU khi tái cấu trúc

```plantuml
@startuml KienTruc_Sau
skinparam classAttributeIconSize 0
title Kiến trúc sau tái cấu trúc

package "Scripts (MonoBehaviour - giao diện & gameplay)" {
  class LevelManager {
    +LastStars : int
    +WinGame()
    -SkipLevel()
    -CompleteLevel(skipped)
  }
  class LevelSelectManager {
    +RefreshLevelButtons()
    +LoadLevel(levelId)
  }
  class MenuController
  class PlayerDeath
}

package "Scripts/Services (cần UnityEngine)" {
  class ProgressService <<static>> {
    +MaxUnlockedLevel : int
    +IsUnlocked(id) : bool
    +GetStars(id) : int
    +CompleteLevel(id, next, stars)
    +ResetProgress()
    +MergeFrom(remote)
    +UseStore(store)
    +Changed : event
  }
  class GameAnalytics <<static>> {
    +Reporter : IAnalyticsReporter
    +UseReporter(r)
  }
  class HintService <<static>> {
    +GetHint(db, levelName) : string
    +CanOpenHintDirectly(premium, unlocked) : bool
  }
  class LocalProgressStore
}

package "Scripts/Core (C# thuần - unit test được)" {
  interface IProgressStore {
    +Load() : ProgressData
    +Save(data)
  }
  interface IAnalyticsReporter
  class ProgressData {
    +maxUnlockedLevel : int
    +savedLevel : string
    +stars : List<LevelStars>
    +updatedAtUtc : string
  }
  class ProgressMerger <<static>> {
    +Merge(local, remote) : ProgressData
  }
  class StarCalculator <<static>> {
    +Calculate(completed, skipped, deaths, hints) : int
  }
  class LevelSession {
    +Current : LevelSession
    +Begin(levelId, now)
    +RecordDeath()
    +RecordHint()
  }
  class LevelNaming <<static>>
  class InMemoryProgressStore
  class NullAnalyticsReporter
}

package "Luồng B - Codex (dự kiến)" #EEEEEE {
  class CloudProgressStore
  class UnityAnalyticsReporter
}

LevelManager --> ProgressService
LevelManager --> HintService
LevelManager --> StarCalculator
LevelManager --> LevelSession
LevelManager --> GameAnalytics
LevelSelectManager --> ProgressService
MenuController --> ProgressService
PlayerDeath --> LevelSession
PlayerDeath --> GameAnalytics

ProgressService --> IProgressStore
ProgressService --> ProgressMerger
ProgressService ..> ProgressData
GameAnalytics --> IAnalyticsReporter

IProgressStore <|.. LocalProgressStore
IProgressStore <|.. InMemoryProgressStore
IProgressStore <|.. CloudProgressStore
IAnalyticsReporter <|.. NullAnalyticsReporter
IAnalyticsReporter <|.. UnityAnalyticsReporter
@enduml
```

**Nguyên tắc:**
- `Core/` không phụ thuộc MonoBehaviour → test EditMode nhanh, không cần scene.
- Thay nơi lưu (Local → Cloud) hoặc bật Analytics chỉ bằng `ProgressService.UseStore(...)` / `GameAnalytics.UseReporter(...)`, không sửa `LevelManager`.
- Premium **không** nằm trong `ProgressData`: do `PremiumManager` quản lý, Google Play tự khôi phục giao dịch không tiêu hao (non-consumable) → tránh việc dữ liệu tiến trình cũ ghi đè trạng thái vừa mua.

## 3. Sơ đồ tuần tự — Thắng màn

```plantuml
@startuml TuanTu_ThangMan
title Thắng màn (chạm cờ vàng)
actor "Người chơi" as P
participant YellowFlag
participant LevelManager
participant LevelSession
participant StarCalculator
participant ProgressService
participant IProgressStore
participant GameAnalytics

P -> YellowFlag : chạm cờ
YellowFlag -> LevelManager : WinGame()
LevelManager -> LevelManager : CompleteLevel(skipped=false)\nhiện WinPanel, timeScale = 0
LevelManager -> LevelSession : Deaths, HintsUsed, Elapsed()
LevelManager -> StarCalculator : Calculate(true, false, deaths, hints)
StarCalculator --> LevelManager : stars
LevelManager -> ProgressService : CompleteLevel(id, nextLevelName, stars)
ProgressService -> ProgressService : mở khóa id+1, lưu sao (chỉ tăng)
ProgressService -> IProgressStore : Save(data)
ProgressService --> ProgressService : phát sự kiện Changed
LevelManager -> GameAnalytics : LevelComplete(id, duration, deaths, hints, stars)
LevelManager -> LevelSession : Finish()
@enduml
```

## 4. Sơ đồ tuần tự — Chết và chơi lại

```plantuml
@startuml TuanTu_Chet
title Nhân vật chết → nạp lại scene, giữ bộ đếm
participant PlayerDeath
participant LevelSession
participant GameAnalytics
participant AdManager
participant LevelManager
participant "LevelManager (scene mới)" as LM2

PlayerDeath -> LevelSession : RecordDeath()
PlayerDeath -> GameAnalytics : PlayerDeath(id, time)
PlayerDeath -> AdManager : OnPlayerDeath(callback)
AdManager --> PlayerDeath : (sau quảng cáo nếu đủ số lần chết)
PlayerDeath -> LevelManager : ReplayLevel() → LoadScene
LM2 -> LevelSession : Begin(id) → cùng màn, chưa Finish\n→ giữ Deaths, Attempt + 1
LM2 -> GameAnalytics : LevelStart(id, attempt)
@enduml
```

## 5. Sơ đồ trạng thái — Vòng đời một màn

```plantuml
@startuml TrangThai_Man
title Vòng đời một màn chơi
[*] --> DangChoi : Nạp scene "Level N"\nLevelSession.Begin
DangChoi --> TamDung : Pause
TamDung --> DangChoi : Resume
DangChoi --> XemGoiY : Mở gợi ý\n(HintsUsed + 1)
XemGoiY --> DangChoi : Đóng gợi ý
XemGoiY --> ThangMan : Bấm "Nhảy"\n(skipped → 1 sao)
DangChoi --> Chet : Chạm bẫy / rơi khỏi camera
Chet --> DangChoi : Nạp lại scene\n(Deaths + 1, Attempt + 1)
DangChoi --> ThangMan : Chạm cờ (cờ không khóa)
ThangMan --> [*] : Màn tiếp / LevelSelect
@enduml
```
