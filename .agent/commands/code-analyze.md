# Code Analyze - Phân tích cấu trúc code (chỉ phân tích, KHÔNG sửa code)

## Mục đích
Phân tích kiến trúc, flow, dependencies, patterns của một module hoặc feature.
**KHÔNG ĐƯỢC sửa code hay đề xuất "Best Choice".**

## Input
- Tên module/folder/class cần phân tích
- Mode: `architecture` | `flow` | `dependencies` | `patterns`

## Expected Output theo mode

### Mode: `architecture`
- Mô tả kiến trúc module
- Class diagram (text-based)
- Responsibility của mỗi class
- Coupling analysis

### Mode: `flow`
- Sequence diagram (text-based) cho user action chính
- Entry point → processing → output
- Async flows (coroutine, callback)

### Mode: `dependencies`
- Dependency graph: module này phụ thuộc gì?
- Ai phụ thuộc module này?
- Circular dependencies (nếu có)
- MessageManager messages liên quan

### Mode: `patterns`
- Design patterns đang dùng
- Anti-patterns phát hiện
- So sánh với best practices

## Quy trình

### Bước 1: Thu thập
- Liệt kê tất cả file trong module
- Đọc class declarations, interfaces, inheritance
- Tìm `using` statements → external dependencies

### Bước 2: Phân tích
- Map relationships: inheritance, composition, delegation
- Trace `this.GetManager<T>()` calls
- Trace `MessageManager.SendMessage()` / `AddSubscriber()`
- Trace `Singleton.Instance` access
- Tìm `Update()`, coroutine patterns

### Bước 3: Báo cáo
- Trình bày findings theo mode đã chọn
- Highlight điểm mạnh và điểm yếu
- **KHÔNG đề xuất fix** - chỉ phân tích thực trạng

## Ví dụ output (mode: dependencies)

```
Module: Gameplay/Event/
├── Depends on:
│   ├── DataController (GetManager)
│   ├── ConfigController (GetManager) 
│   ├── MessageManager (Subscribe/Send)
│   ├── FirebaseServiceController (API calls)
│   └── UIPlay (popup display)
├── Depended by:
│   ├── UIPlay → hiển thị leaderboard button
│   ├── MapManager → trigger event check
│   └── QuestPanel → event quest integration
└── Messages:
    ├── Sends: OnEarnScoreRanking, OnDataChange
    └── Subscribes: OnDoneLoadMap, OnLevelUp
```

## MCP Integration

Sử dụng MCP skills để thu thập dữ liệu phân tích:

| Mode | MCP Skills ưu tiên |
|------|---------------------|
| `architecture` | `script-read` (đọc class declarations), `assets-find` (tìm ScriptableObject/Prefab), `gameobject-component-list-all` (list component types) |
| `flow` | `script-read` (trace method calls), `gameobject-find` (xem runtime hierarchy), `gameobject-component-get` (inspect component data) |
| `dependencies` | `script-read` (check using/GetManager), `assets-find` (tìm references), `console-get-logs` (check runtime messages) |
| `patterns` | `script-read` (detect patterns), `assets-find` với `t:Script` (list all scripts), `package-list` (check packages) |

### Tips:
- Dùng `assets-find` với filter `t:Script {className}` để tìm nhanh script file
- Dùng `gameobject-find` với `name` để xem runtime component list
- Dùng `scene-get-data` để liệt kê root objects trong scene đang mở
