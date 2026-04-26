# Bắt đầu nhanh

[Tiếng Việt](GETTING_STARTED.md) | [English](GETTING_STARTED.en.md)

Mục tiêu của tài liệu này: giúp bạn đi từ dự án trống đến trạng thái chạy được với Zenith Arch trong ít bước nhất.

## Trước khi bắt đầu

- Dùng .NET SDK 10.0.x (khuyến nghị).
- Đang đứng ở thư mục có `.csproj`.
- Đã cài CLI `zenith` nếu muốn theo lộ trình CLI-first.

## Bước 1: Cài package

```xml
<PackageReference Include="ZenithArch.Abstractions" Version="1.0.6" />
<PackageReference Include="ZenithArch.Generator" Version="1.0.6" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

Kết quả mong đợi: project đã có đủ package lõi để generator hoạt động.

## Bước 2: Tạo cấu hình kiến trúc

### Cách A: Dùng CLI (nhanh nhất, khuyến nghị)

```bash
zenith init
```

### Cách B: Tạo thủ công

Tạo file `AssemblyConfig.cs`:

```csharp
using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Enums;

[assembly: Architecture(
    Profile = ArchitectureProfile.CqrsQuickStart,
    Pattern = ArchitecturePattern.Cqrs,
    GenerateDependencyInjection = true
)]
```

Kết quả mong đợi: có `AssemblyConfig.cs` chứa `[assembly: Architecture(...)]`.

## Bước 3: Tạo entity đầu tiên

```csharp
using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Base;

namespace MyApp.Domain;

[Entity]
public partial class Trip : EntityBase
{
    [QueryFilter]
    public string Destination { get; set; } = string.Empty;
}
```

Điểm bắt buộc: class phải là `partial`, nếu không bạn sẽ gặp `ZENITH005`.

## Bước 4: Build để sinh mã

```bash
dotnet build
```

Kiểm tra output sinh mã trong `obj/` và `ZenithArch.GenerationReport.g.cs`.

Kết quả mong đợi:

- Không có lỗi generator.
- Có generation report và các `.g.cs` tương ứng.

## Bước 5: Đăng ký runtime

Trong startup của ứng dụng:

```csharp
builder.Services.AddZenithArchDependencies();
```

Nếu dùng `UseUnitOfWork = true` (Repository/FullStack), ưu tiên:

```csharp
builder.Services.AddZenithArchDependencies<AppDbContext>();
```

Generated DI extension sẽ đăng ký handlers/repositories (theo pattern), validators và cache behaviors (nếu bật).

## Bước 6: Chạy readiness check

```bash
zenith doctor
```

Mục tiêu: không còn FAIL checks trước khi đi tiếp.

## Lỗi thường gặp nhất và cách xử lý nhanh

- `ZENITH005`: class có `[Entity]` phải là `partial`.
- `ZENITH006`: thiếu `AssemblyConfig.cs` hoặc thiếu `[assembly: Architecture(...)]`.
- `ZENITH007`: thiếu package phụ thuộc của feature đang bật.

Mẹo: sau khi sửa lỗi cấu hình/phụ thuộc, luôn chạy lại `dotnet build` rồi `zenith doctor`.

## Kết thúc tài liệu này, bạn nên có

1. Một `AssemblyConfig.cs` hợp lệ.
2. Ít nhất một entity có `[Entity]` và `partial`.
3. Generated output dưới `obj/`.
4. Startup đã gọi `AddZenithArchDependencies(...)`.
5. `zenith doctor` không còn FAIL.

## Lộ trình cho AI agent

Nếu dùng AI agent để setup, nên chạy theo thứ tự:

1. `zenith init`
2. `zenith scaffold Trip MyApp.Domain`
3. `dotnet build`
4. `zenith doctor` và xử lý toàn bộ FAIL checks

Xem thêm tại `docs/AI_AGENT_PLAYBOOK.md`.
