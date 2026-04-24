# Extract Pure Logic từ MonoBehaviour

## Mục đích
Tách business logic ra khỏi MonoBehaviour thành pure C# class, giúp code dễ test, tái sử dụng, và giảm coupling với Unity lifecycle.

## Input
- Tên MonoBehaviour class cần extract (ví dụ: `Farm`, `Animal`, `OrderBoardUI`)
- Hoặc: mô tả logic cần tách

## Expected Output
- Pure C# class chứa business logic (không kế thừa MonoBehaviour)
- MonoBehaviour chỉ còn Unity lifecycle hooks và delegate sang pure class
- Unit test có thể viết cho pure class

## Step-by-step Refactor Plan

### Bước 1: Identify logic cần tách
Trong MonoBehaviour, tìm các method KHÔNG dùng:
- `transform`, `gameObject`, `GetComponent`
- `StartCoroutine`, `Invoke`
- `SerializeField` fields
- Unity lifecycle (`Awake`, `Start`, `Update`, `OnEnable`...)

### Bước 2: Tạo pure class

```csharp
namespace DreamyHarvest.Gameplay.Logic
{
    public class FarmLogic
    {
        private readonly FarmState _state;
        private readonly ConfigWrapperPlant _config;

        public FarmLogic(FarmState state, ConfigWrapperPlant config)
        {
            _state = state;
            _config = config;
        }

        public bool CanPlant(string plantId)
        {
            return !_state.HasPlant && _config.GetPlantData(plantId) != null;
        }

        public PlantResult TryPlant(string plantId, int currentCoins)
        {
            var plantData = _config.GetPlantData(plantId);
            if (plantData == null) return PlantResult.InvalidPlant;
            if (currentCoins < plantData.cost) return PlantResult.NotEnoughCoins;
            if (_state.HasPlant) return PlantResult.SlotOccupied;

            _state.plantId = plantId;
            _state.timeStampPlant = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return PlantResult.Success;
        }

        public bool CanHarvest(long currentTime)
        {
            if (!_state.HasPlant) return false;
            var plantData = _config.GetPlantData(_state.plantId);
            return (currentTime - _state.timeStampPlant) >= plantData.harvestTime;
        }
    }

    public enum PlantResult
    {
        Success,
        InvalidPlant,
        NotEnoughCoins,
        SlotOccupied
    }
}
```

### Bước 3: MonoBehaviour delegate sang pure class

```csharp
public class Farm : MonoBehaviour
{
    private FarmLogic _logic;

    private void Start()
    {
        var config = this.GetManager<ConfigController>().GetConfigWrapper<ConfigWrapperPlant>();
        _logic = new FarmLogic(farmState, config);
    }

    public void TryPlanting(string plantId)
    {
        int coins = this.GetManager<DataController>().GetDataWrapper<DataWrapperItem>().Coin;
        var result = _logic.TryPlant(plantId, coins);

        switch (result)
        {
            case PlantResult.Success:
                StartCoroutine(Grow());
                break;
            case PlantResult.NotEnoughCoins:
                // Show UI feedback
                break;
        }
    }
}
```

### Bước 4: Viết unit test

```csharp
[Test]
public void TryPlant_WhenHasEnoughCoins_ReturnsSuccess()
{
    var state = new FarmState();
    var config = CreateTestConfig();
    var logic = new FarmLogic(state, config);

    var result = logic.TryPlant("wheat", 100);

    Assert.AreEqual(PlantResult.Success, result);
    Assert.AreEqual("wheat", state.plantId);
}
```

## Safety Checks

- [ ] Logic mới phải cho kết quả GIỐNG HỆT logic cũ
- [ ] MonoBehaviour vẫn hoạt động bình thường sau extract
- [ ] Không break serialize data (Inspector references)
- [ ] Chạy game test các flow liên quan
- [ ] Kiểm tra edge case: null data, empty state

## Ưu tiên Extract

| Priority | Class | Logic cần tách |
|----------|-------|----------------|
| Cao | `Farm` | Plant/Harvest calculation |
| Cao | `Animal` | Feed/Harvest/Growth timing |
| Cao | `OrderBoardUI` | Order matching/completion logic |
| Trung bình | `Tree` | Collect/Growth cycle |
| Trung bình | `TradeBoatUI` | Trade calculation |

## MCP Integration

Sử dụng MCP skills cho từng bước:

| Bước | MCP Skills |
|------|------------|
| 1. Identify logic | `script-read` — đọc MonoBehaviour, tìm methods không dùng Unity API |
| 2. Tạo pure class | `script-update-or-create` — tạo file `.cs` mới trong `Scripts/Gameplay/Logic/` |
| 3. Update MonoBehaviour | `script-read` → `script-update-or-create` — sửa MonoBehaviour delegate sang pure class |
| 4. Verify compilation | `console-get-logs` — check compilation errors sau khi tạo/sửa scripts |
| 5. Chạy test | `tests-run` — chạy unit tests cho pure logic class |
| 6. Check runtime | `editor-application-set-state` (enter play) → `console-get-logs` (check runtime errors) |

### Workflow:
```
script-read (MonoBehaviour)
  → Phân tích logic cần tách
  → script-update-or-create (Pure class mới)
  → script-update-or-create (Update MonoBehaviour)
  → console-get-logs (Check compilation)
  → script-update-or-create (Unit test)
  → tests-run (Verify)
```
