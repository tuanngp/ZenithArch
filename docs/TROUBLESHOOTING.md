# Xử lý sự cố

[Tiếng Việt](TROUBLESHOOTING.md) | [English](TROUBLESHOOTING.en.md)

## Quy trình xử lý nhanh (60 giây)

1. Chạy `dotnet build` để lấy lỗi compile/generator mới nhất.
2. Chạy `rynor doctor` để xác định lỗi cấu hình/phụ thuộc.
3. Ưu tiên sửa FAIL checks trước WARN checks.
4. Chạy lại `dotnet build` và `rynor doctor` để xác nhận đã sạch lỗi.

## Bảng tra nhanh theo triệu chứng

| Triệu chứng | Chẩn đoán thường gặp | Sửa nhanh |
| --- | --- | --- |
| Không thấy file sinh trong `obj/` | `RYNOR006` | Thêm `[assembly: Architecture(...)]` trong `AssemblyConfig.cs` |
| Entity không được sinh handler/repository | `RYNOR005` | Đổi class thành `partial` và build lại |
| Build báo thiếu dependency | `RYNOR007` | Bổ sung package/framework theo gợi ý diagnostic |
| Bật endpoint nhưng không sinh route | `RYNOR012` | Bật cả `GenerateEndpoints = true` và `EnableExperimentalEndpoints = true` |
| Bật validation nhưng request sai vẫn chạy | `RYNOR015` hoặc wiring thiếu | Bật `GenerateDependencyInjection` hoặc đăng ký `RynorArchValidationBehavior<,>` thủ công |

## Chẩn đoán thường gặp

### `RYNOR001` Không tìm thấy entity

Không có class nào dùng `[Entity]` trong compilation hiện tại.

Kiểm tra:
- project đã tham chiếu `RynorArch.Abstractions`
- entity class nằm trong đúng project đang build
- entity đã được đánh dấu `[Entity]`

### `RYNOR002` AggregateRoot yêu cầu Entity

`[AggregateRoot]` chỉ hợp lệ trên class có `[Entity]`.

### `RYNOR003` Xung đột architecture pattern

Feature flags đang chọn không tương thích với architecture pattern.

Ví dụ:
- `UseUnitOfWork = true` với `Cqrs`
- `EnableValidation = true` với `Repository`

### `RYNOR004` Kiểu dữ liệu QueryFilter không hỗ trợ

`[QueryFilter]` hỗ trợ string, numeric, bool, `DateTime`, `Guid`, enum và nullable của các kiểu này.

Nếu cần lọc phức tạp, hãy bỏ `[QueryFilter]` và tự triển khai trong partial handler hoặc specification.

### `RYNOR005` Entity phải là partial

Generated extensions và partial hooks yêu cầu entity được khai báo `partial`.

### `RYNOR006` Thiếu architecture configuration

Thêm cấu hình ở mức assembly, ví dụ:

```csharp
using RynorArch.Abstractions.Attributes;
using RynorArch.Abstractions.Enums;

[assembly: Architecture(Pattern = ArchitecturePattern.Cqrs)]
```

RynorArch sẽ không sinh mã nếu thiếu cấu hình kiến trúc tường minh.

### `RYNOR007` Thiếu dependency bắt buộc

Một hoặc nhiều feature đang bật yêu cầu package/framework chưa có trong compilation.
Diagnostic sẽ gợi ý chính xác `PackageReference` hoặc `FrameworkReference` cần bổ sung.

Ví dụ thường gặp:
- Bật CQRS nhưng thiếu `MediatR`
- Bật validation nhưng thiếu `FluentValidation`
- Bật persistence nhưng thiếu `Microsoft.EntityFrameworkCore`
- Bật endpoint nhưng thiếu `Microsoft.AspNetCore.App`
- Bật caching decorators nhưng thiếu `Microsoft.Extensions.Caching.*`

### `RYNOR008` DbContextType không hợp lệ

`DbContextType` đã cấu hình nhưng kiểu không resolve được hoặc không kế thừa `Microsoft.EntityFrameworkCore.DbContext`.

Cách xử lý:
- đặt `DbContextType = typeof(YourDbContext)` với kiểu hợp lệ trong compilation
- hoặc bỏ `DbContextType` để fallback về `Microsoft.EntityFrameworkCore.DbContext`

### `RYNOR009` Thông báo endpoint behavior tối giản

Endpoint generation đã bật và compile thành công, nhưng endpoint sinh ra được giữ tối giản có chủ đích.
Hãy áp dụng hardening trước khi dùng cho API doanh nghiệp.
Checklist: `docs/ENDPOINT_HARDENING.md`.

### `RYNOR010` Thông báo cache behavior

Generated cache pipeline behaviors có per-entity invalidation contracts.
Hãy bảo đảm invalidator được đăng ký trong DI (generated DI helper sẽ làm việc này khi bật).
Hướng dẫn rollout vận hành: `docs/CACHING_OPERATIONS.md`.

### Bật validation nhưng command sai vẫn pass

Khi `EnableValidation = true`, generated DI phải đăng ký `RynorArchValidationBehavior<,>`.

Kiểm tra:
- đã gọi `AddRynorArchDependencies()` ở startup hoặc có đăng ký thủ công tương đương
- MediatR được cấu hình đúng assembly chứa generated handlers
- có generated validator files (`*.Validation.g.cs`) trong `obj/`

Nếu `GenerateDependencyInjection = false`, cần đăng ký thủ công `IPipelineBehavior<,>` vào `RynorArchValidationBehavior<,>`.

### Endpoint sinh ra trả mã ghi dữ liệu không đúng kỳ vọng

Generated behavior hiện tại:
- `POST` trả `201 Created` với payload `{ id = <guid> }`
- `PUT`/`DELETE` trả `404` nếu resource thiếu, và `204` nếu thành công

Nếu vẫn thấy luôn `204`, hãy rebuild và kiểm tra `RynorArchEndpointExtensions.g.cs` trong `obj/`.

### `RYNOR011` Feature flag bị bỏ qua theo pattern đã chọn

Một feature flag được bật nhưng pattern hiện tại không hỗ trợ sinh artifact tương ứng.
Hãy căn chỉnh pattern và flags trong `[assembly: Architecture(...)]`.

### `RYNOR012` Endpoint generation cần experimental opt-in

`GenerateEndpoints` đã bật nhưng `EnableExperimentalEndpoints` chưa bật.

Sửa bằng cách bật tường minh:

```csharp
[assembly: Architecture(
    Pattern = ArchitecturePattern.Cqrs,
    GenerateEndpoints = true,
    EnableExperimentalEndpoints = true
)]
```

### `RYNOR013` CQRS save mode cần generated DI wiring

`CqrsSaveMode.PerRequestTransaction` đang bật trong khi `GenerateDependencyInjection` = false.

Cách xử lý:
- đặt `GenerateDependencyInjection = true`, hoặc
- đăng ký thủ công `IPipelineBehavior<,>` vào `RynorArchSaveChangesBehavior<,>`

### `RYNOR014` Khuyến nghị migration sang starter profile

Module hiện còn dùng cấu hình explicit-flag kiểu cũ.

Cách xử lý:
- đặt `Profile = ArchitectureProfile.CqrsQuickStart` / `RepositoryQuickStart` / `FullStackQuickStart`
- chỉ giữ explicit flags là các override có chủ đích

Mapping chi tiết: `docs/UPGRADING_PROFILES.md`.

### `RYNOR015` Validation cần generated DI wiring

`EnableValidation` đang bật trong khi `GenerateDependencyInjection` = false.

Cách xử lý:
- đặt `GenerateDependencyInjection = true`, hoặc
- đăng ký thủ công `IPipelineBehavior<,>` vào `RynorArchValidationBehavior<,>`

### `RYNOR016` Khuyến nghị checklist hardening endpoint

`GenerateEndpoints` đã bật và endpoint generation thành công.
Đây là diagnostic thông tin để nhắc kiểm tra hardening trước khi rollout production.

Tối thiểu cần kiểm tra:
- ranh giới phân quyền (`RequireAuthorization` và tách policy)
- problem details và exception mapping nhất quán
- observability (structured logs, traces, metrics)
- bảo vệ vòng đời API (versioning, idempotency khi cần)

## Debug generated output

- Mở `RynorArch.GenerationReport.g.cs` để xem artifacts đã sinh.
- Kiểm tra header metadata `rynor-artifact`, `rynor-entity`, `rynor-assumptions`.
- Dùng diagnostics làm tín hiệu chính trước khi debug body code sinh.

## Quản lý file sinh trong source control

- Mặc định không commit generated files.
- Chỉ commit nếu team có chủ đích review hoặc diff generated output.

## Lỗi build sau nâng cấp

- Xóa thư mục `bin/` và `obj/`.
- Restore package lại.
- So sánh output sinh mã trước/sau nâng cấp.
- Chạy lại `dotnet test RynorArch.slnx`.

## Troubleshooting với CLI doctor

Dùng `rynor doctor` như một readiness gate cho workflow tự động.

- `DR002`: chạy lệnh trong thư mục chứa `.csproj`.
- `DR004`: thêm `AssemblyConfig.cs` với `[assembly: Architecture(...)]`.
- `DR006`: nếu bật endpoint generation thì phải bật `EnableExperimentalEndpoints = true`.
- `DR009`-`DR013`: thêm package/framework được gợi ý trong output.
- `DR014`: bảo đảm mọi class có `[Entity]` đều là `partial`.
- `DR015`: build ít nhất một lần để generation report xuất hiện dưới `obj/`.

Contract và luồng xác minh tham chiếu: `docs/AI_AGENT_PLAYBOOK.md`.
