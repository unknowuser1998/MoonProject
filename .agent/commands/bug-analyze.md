# Bug Analyze - Phân tích bug (chỉ phân tích, KHÔNG sửa code)

## Mục đích
Phân tích bug dựa trên evidence, tìm root cause.
**KHÔNG ĐƯỢC sửa code.**

## Input
- Mô tả bug (expected vs actual behavior)
- Error log / stack trace (nếu có)
- Bước reproduce (nếu biết)
- Mode: `error` | `performance` | `build` | `asset`

## Expected Output
- Root cause analysis
- Affected files/modules
- Reproduction path
- Impact assessment
- **KHÔNG đề xuất fix cụ thể** - chỉ xác định nguyên nhân

## Quy trình theo mode

### Mode: `error`
1. Parse error message / stack trace
2. Locate source file và line number
3. Trace call stack: ai gọi method này?
4. Check preconditions: null check, data validity
5. Check timing: Awake/Start order, async race condition
6. Identify root cause

### Mode: `performance`
1. Xác định symptom: FPS drop, memory spike, freeze
2. Check Update() loops trong module liên quan
3. Check coroutine leaks (start without stop)
4. Check GC allocation patterns
5. Check texture/asset sizes
6. Identify bottleneck

### Mode: `build`
1. Parse build error
2. Check assembly definitions
3. Check define symbols
4. Check platform-specific code (`#if UNITY_ANDROID`)
5. Check package dependencies
6. Identify conflict

### Mode: `asset`
1. Check import settings
2. Check references (missing, broken)
3. Check Addressables groups
4. Check file size and format
5. Identify issue

## Template output

```
## Bug Analysis Report

### Symptom
[Mô tả hiện tượng]

### Reproduction
1. [Bước 1]
2. [Bước 2]
3. → Bug xuất hiện

### Root Cause
[Giải thích tại sao bug xảy ra]

### Affected Files
- `path/to/file1.cs` (line XX)
- `path/to/file2.cs` (line YY)

### Impact
- [Module/feature nào bị ảnh hưởng]
- [Severity: Critical / High / Medium / Low]

### Evidence
- [Stack trace / log / screenshot reference]
```

## MCP Integration

Sử dụng MCP skills để lấy evidence trực tiếp từ Unity Editor:

| Mode | MCP Skills ưu tiên |
|------|---------------------|
| `error` | `console-get-logs` (parse errors), `script-read` (xem source), `editor-application-get-state` (check compilation) |
| `performance` | `screenshot-game-view` (capture trạng thái), `script-read` (tìm Update loops), `console-get-logs` (check warnings) |
| `build` | `console-get-logs` (build errors), `package-list` (check packages), `editor-application-get-state` |
| `asset` | `assets-find` (tìm asset), `assets-get-data` (check import settings), `screenshot-scene-view` |

### Workflow ưu tiên:
1. `console-clear-logs` — Clear logs trước khi reproduce
2. Reproduce bug (hoặc `editor-application-set-state` để enter play mode)
3. `console-get-logs` — Thu thập logs sau reproduce
4. `screenshot-game-view` — Capture trạng thái visual
5. `script-read` — Đọc source files liên quan từ stack trace

## Lưu ý cho MoonProject

### Bugs thường gặp:
- **NullReferenceException**: Check ManagerContainer init order, Singleton lifecycle
- **Data corruption**: Check BinaryFormatter compatibility, save timing
- **UI not showing**: Check BasePopup prefab reference, UIHolder
- **Ads not loading**: Check AdsController init, network, MAX SDK config
- **IAP not completing**: Check CustomPack.OnCompletePurchase, receipt validation
