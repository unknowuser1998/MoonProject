# Optimize Update Loop

## Mục đích
Giảm số lượng `Update()` calls không cần thiết, chuyển sang event-driven hoặc coroutine-based approach để cải thiện performance trên mobile.

## Input
- File hoặc class có `Update()` cần optimize
- Hoặc: "scan all" để tìm tất cả Update() trong project

## Expected Output
- Update() được loại bỏ hoặc tối ưu
- Logic chuyển sang event-driven (MessageManager), coroutine, hoặc InvokeRepeating
- Không thay đổi behavior
- Giảm CPU usage trên mobile

## Step-by-step Optimization Plan

### Bước 1: Scan và phân loại tất cả Update()

Chạy search:
```
grep -rn "void Update()" Assets/Scripts/ --include="*.cs"
grep -rn "void LateUpdate()" Assets/Scripts/ --include="*.cs"
```

Phân loại:
- **Loại A - Có thể xóa hoàn toàn**: Logic chỉ chạy khi có event (click, data change)
- **Loại B - Chuyển sang coroutine**: Countdown, periodic check
- **Loại C - Cần giữ nhưng optimize**: Camera follow, input handling
- **Loại D - Giữ nguyên**: Frame-critical logic (animation, physics)

### Bước 2: Xử lý Loại A (xóa Update)

```csharp
// TRƯỚC:
void Update()
{
    if (dataChanged)
    {
        RefreshUI();
        dataChanged = false;
    }
}

// SAU: Dùng MessageManager
void Start()
{
    this.GetManager<MessageManager>().AddSubscriber(TeeMessageType.OnDataChange, this);
}

public void Handle(Message message)
{
    if (message.type == TeeMessageType.OnDataChange)
        RefreshUI();
}
```

### Bước 3: Xử lý Loại B (chuyển sang coroutine)

```csharp
// TRƯỚC:
private float timer = 0f;
void Update()
{
    timer -= Time.deltaTime;
    if (timer <= 0)
    {
        DoPeriodicCheck();
        timer = 5f;
    }
}

// SAU:
private static readonly WaitForSeconds Wait5s = new WaitForSeconds(5f);

private IEnumerator PeriodicCheckRoutine()
{
    while (true)
    {
        DoPeriodicCheck();
        yield return Wait5s;
    }
}

void OnEnable() => StartCoroutine(PeriodicCheckRoutine());
void OnDisable() => StopAllCoroutines();
```

### Bước 4: Xử lý Loại C (optimize Update)

```csharp
// TRƯỚC:
void Update()
{
    // Luôn chạy dù không cần
    txtCountdown.text = FormatTime(endTime - Time.time);
    btnClaim.interactable = CanClaim();
    imgProgress.fillAmount = GetProgress();
}

// SAU:
void Update()
{
    if (!_isActive) return; // Guard clause

    float remaining = _endTime - Time.time;
    if (remaining <= 0)
    {
        OnCountdownComplete();
        _isActive = false;
        return;
    }

    // Chỉ update text mỗi giây, không mỗi frame
    int seconds = Mathf.CeilToInt(remaining);
    if (seconds != _lastDisplayedSeconds)
    {
        _lastDisplayedSeconds = seconds;
        txtCountdown.text = FormatTime(remaining);
    }
}
```

## Safety Checks

- [ ] Profile TRƯỚC và SAU bằng Unity Profiler
- [ ] Kiểm tra timing accuracy (coroutine có thể không chính xác bằng Update)
- [ ] Test trên device thực (không chỉ Editor)
- [ ] Kiểm tra OnEnable/OnDisable lifecycle
- [ ] Đảm bảo coroutine được stop khi object disable

## Danh sách Update() cần review (dựa trên scan)

| File | Loại | Đề xuất |
|------|------|---------|
| `UIPlay.cs` | C | Optimize: guard clause + reduce frequency |
| `Animal.cs` | C | Optimize: chỉ update khi visible |
| `QuestElement.cs` | A | Xóa: chuyển sang event |
| `PopupGift*.cs` | B | Chuyển sang coroutine |
| `CraftingUI.cs` | B | Countdown → coroutine |
| `CardCollectionPopup.cs` | B | Timer → coroutine |
| `SalvageUI.cs` | B | Timer → coroutine |
| `FPSDisplay.cs` | D | Giữ nguyên (debug tool) |
| `CameraController.cs` (LateUpdate) | D | Giữ nguyên (camera follow) |

## MCP Integration

Sử dụng MCP skills cho từng bước:

| Bước | MCP Skills |
|------|------------|
| 1. Scan Update() | `script-read` — đọc từng file, tìm `void Update()` / `void LateUpdate()` |
| 2. Phân tích context | `script-read` — đọc toàn bộ file để hiểu Update() làm gì |
| 3. Áp dụng thay đổi | `script-update-or-create` — sửa file (xóa Update, thêm coroutine/event) |
| 4. Check compilation | `console-get-logs` — verify không có compilation errors |
| 5. Test runtime | `editor-application-set-state` → `console-get-logs` |
| 6. Profile | `screenshot-game-view` — capture FPS / visual state |

### Scan workflow:
```
assets-find (filter: "t:Script") 
  → Lọc danh sách scripts
  → script-read (từng file) 
  → Tìm "void Update()" / "void LateUpdate()"
  → Phân loại A/B/C/D
  → script-update-or-create (áp dụng optimization)
  → console-get-logs (verify)
```
