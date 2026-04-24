# /gen-struc — Generate Feature Structure File

## Mô tả
Tự động phân tích code của một feature/popup và tạo file `.struc.md` tương ứng trong `.agent/features/`.

## Cách dùng
```
/gen-struc [FeatureName] [MainFile]
```

Ví dụ:
```
/gen-struc Fishing Assets/Scripts/Gameplay/UI/Fishing/FishingUI.cs
/gen-struc DailyReward Assets/Scripts/Gameplay/UI/DailyReward/DailyRewardPanel.cs
```

## Quy trình thực hiện

### Bước 1: Xác định scope
1. Đọc file chính (MainFile) bằng `script-read` hoặc `view_file`
2. Từ file chính, tìm tất cả files liên quan:
   - Cùng thư mục (sibling files)
   - DataWrapper được sử dụng
   - ConfigWrapper được sử dụng
   - Building/GridObject tương ứng (nếu có)

### Bước 2: Phân tích dependencies
Scan file chính và files liên quan, tìm:
- `this.GetManager<T>()` calls
- `.Instance` singleton calls
- `DataWrapper*` static calls
- `ConfigWrapper*` static calls
- `MessageManager` subscribe/send
- Inheritance chain

### Bước 3: Trace data flow
Xác định các flow chính:
- **Show/Init flow**: Entry point → setup → display
- **Core action flow**: User interaction → logic → data change → save
- **Complete/Close flow**: Cleanup → save → UI restore
- **Timer/Reset flow**: Coroutine countdown nếu có

### Bước 4: Document issues
Ghi nhận:
- Performance concerns (Update loops, large files, GC alloc)
- Code smells (duplicate code, magic numbers, tight coupling)
- Complexity hotspots (nested coroutines, complex state machines)

### Bước 5: Generate file
Tạo file `.agent/features/{feature_name}.struc.md` theo template trong rule `16_feature-structure.mdc`

## MCP Integration

| Bước | MCP Skills |
|------|------------|
| Đọc file chính | `script-read` |
| Tìm files liên quan | `assets-find` (filter by name/type) |
| Đọc files liên quan | `script-read` (lặp cho mỗi file) |
| Kiểm tra scene | `gameobject-find` (tìm building/prefab trên scene nếu cần) |
| Tạo file | `write_to_file` hoặc `script-update-or-create` |

## Output Format
Tuân theo template trong rule `16_feature-structure.mdc`:
- Overview, Files, Key Classes, Dependencies, Data Flow, Messages, Known Issues

## Lưu ý
- File `.struc.md` KHÔNG chứa code — chỉ chứa metadata
- Đặt `Status: draft` nếu chưa verify đầy đủ
- Cập nhật `Status: verified` sau khi user confirm
