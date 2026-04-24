# Ask - Phân tích vấn đề Unity (chỉ phân tích, KHÔNG sửa code)

## Mục đích
Phân tích vấn đề Unity mà user mô tả, đưa ra đánh giá và đề xuất giải pháp.
**KHÔNG ĐƯỢC sửa bất kỳ file nào.**

## Input
- Mô tả vấn đề hoặc câu hỏi từ user
- File/module liên quan (nếu có)

## Expected Output
- Tóm tắt vấn đề
- Phân tích nguyên nhân
- Bảng so sánh giải pháp
- Đề xuất best choice + rủi ro

## Quy trình

### Bước 1: Tóm tắt
- Tóm lại vấn đề trong 1-2 câu
- Xác nhận scope: module nào bị ảnh hưởng?

### Bước 2: Điều tra
- Đọc file liên quan
- Trace dependency: ai gọi module này? Module này gọi ai?
- Check MessageManager subscribers nếu liên quan cross-module
- Check Singleton.Instance usage

### Bước 3: Phân tích
- Root cause (nếu là bug)
- Trade-offs (nếu là design decision)
- Impact assessment: thay đổi ảnh hưởng bao nhiêu file?

### Bước 4: Đề xuất (KHÔNG implement)

| Giải pháp | Ưu điểm | Nhược điểm | Effort | Risk |
|-----------|---------|------------|--------|------|
| Option A | ... | ... | Low | Low |
| Option B | ... | ... | Medium | Medium |
| Option C | ... | ... | High | Low |

**Best choice**: Option X vì...

**Rủi ro cần lưu ý**: ...

## MCP Integration

Khi điều tra, sử dụng MCP skills để lấy thông tin trực tiếp từ Unity Editor:

| Bước | MCP Skills |
|------|------------|
| Tìm file/asset liên quan | `assets-find` với filter theo tên hoặc type |
| Đọc script cần phân tích | `script-read` để xem nội dung |
| Kiểm tra scene hierarchy | `gameobject-find` để tìm object, `scene-get-data` để xem root objects |
| Xem component data | `gameobject-component-get` để inspect serialized fields |
| Check Unity logs | `console-get-logs` để xem errors/warnings hiện tại |
| Capture trạng thái | `screenshot-game-view` hoặc `screenshot-scene-view` |
| Kiểm tra Editor state | `editor-application-get-state` (play mode, compilation) |

## Lưu ý đặc biệt cho MoonProject

- Kiểm tra `DataController` save/load impact nếu liên quan data
- Kiểm tra `ManagerContainer` init order nếu liên quan startup
- Kiểm tra IAP/Ads flow nếu liên quan monetization
- KHÔNG đề xuất thay đổi `GameData` structure mà không có migration plan
