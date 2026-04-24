# Refactor Singletons → Service Locator

## Mục đích
Chuyển đổi các Singleton không cần thiết sang sử dụng `ManagerContainer.GetManager<T>()` pattern đã có, giảm tight coupling và cải thiện testability.

## Input
- Tên class Singleton cần refactor (ví dụ: `NotiController`, `InfoInventory`)
- Hoặc: folder chứa các Singleton cần review

## Expected Output
- Class được chuyển từ `Singleton<T>` sang `GameManager` (nếu là manager logic)
- Hoặc: chuyển sang dependency injection qua constructor/method parameter
- Tất cả caller sites được cập nhật
- Không thay đổi behavior

## Step-by-step Refactor Plan

### Bước 1: Phân loại Singleton
Xác định loại Singleton:
- **Type A - Manager Logic**: Nên chuyển thành `GameManager : ScriptableObject` và đăng ký vào `ManagerContainer`
- **Type B - UI Controller**: Có thể giữ `BasePopup<T>` pattern hoặc chuyển sang reference injection
- **Type C - Scene-specific**: Có thể dùng `[SerializeField]` reference từ scene hierarchy

### Bước 2: Tạo interface (nếu cần)
```csharp
public interface INotiService
{
    void ShowNotification(string message);
    void ClearAll();
}
```

### Bước 3: Chuyển đổi class
```csharp
// TRƯỚC:
public class NotiController : Singleton<NotiController> { ... }

// SAU (Type A):
[CreateAssetMenu]
public class NotiController : GameManager, INotiService { ... }
```

### Bước 4: Cập nhật caller sites
```csharp
// TRƯỚC:
NotiController.Instance.ShowNotification("msg");

// SAU:
this.GetManager<NotiController>().ShowNotification("msg");
```

### Bước 5: Đăng ký trong ManagerContainer
- Tạo ScriptableObject asset trong project
- Thêm vào `ManagerContainer.managers` list

## Safety Checks

- [ ] Tìm TẤT CẢ reference đến `.Instance` của class cần refactor
- [ ] Kiểm tra `DontDestroyOnLoad` - nếu có, cần xem xét lifecycle
- [ ] Kiểm tra `Awake()`/`Start()` order dependency
- [ ] Chạy search: `grep -r "ClassName.Instance" Assets/Scripts/`
- [ ] Test trên device sau refactor
- [ ] Kiểm tra scene references trong Inspector (prefab, scene objects)

## Danh sách Singleton ưu tiên refactor

| Priority | Class | Lý do |
|----------|-------|-------|
| Cao | `NotiController` | Logic thuần, không cần MonoBehaviour |
| Cao | `InfoInventory` | Data display, có thể là GameManager |
| Trung bình | `RemoveAdController` | IAP logic, nên tách |
| Thấp | `UIPlay` | HUD chính, phức tạp, refactor sau |
| Thấp | `MapManager` | Scene-specific, nhiều dependency |

## Lưu ý
- KHÔNG refactor nhiều Singleton cùng lúc - từng cái một
- KHÔNG refactor `ManagerContainer`, `MapManager`, `UIPlay` ở bước đầu
- Mỗi refactor phải có commit riêng để dễ rollback

## MCP Integration

Sử dụng MCP skills cho từng bước:

| Bước | MCP Skills |
|------|------------|
| 1. Phân loại Singleton | `script-read` — đọc class, xem kế thừa `Singleton<T>` hay gì |
| 2. Tìm tất cả callers | `script-read` trên nhiều files — grep `.Instance` usage |
| 3. Tạo interface | `script-update-or-create` — tạo file interface mới |
| 4. Chuyển đổi class | `script-read` → `script-update-or-create` — sửa class từ Singleton sang GameManager |
| 5. Tạo SO asset | `assets-find` (kiểm tra SO đã tồn tại?) → qua Unity Editor tạo SO asset |
| 6. Update callers | `script-read` → `script-update-or-create` trên từng caller file |
| 7. Verify | `console-get-logs` (compilation) → `tests-run` (nếu có test) |

### Workflow:
```
script-read (Singleton class)
  → Xác định type A/B/C
  → script-update-or-create (Interface nếu cần)
  → script-update-or-create (Chuyển class → GameManager)
  → Lặp: script-read + script-update-or-create (Mỗi caller file)
  → console-get-logs (Check compilation)
  → tests-run (Verify behavior)
```

### Tip: Tìm tất cả Singleton usage
Dùng `script-execute` để chạy code scan:
```csharp
// Tìm tất cả reference đến ClassName.Instance
var results = System.IO.Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories)
    .Where(f => System.IO.File.ReadAllText(f).Contains("NotiController.Instance"));
```
