# Hướng dẫn tích hợp

[Tiếng Việt](INTEGRATION_GUIDE.md) | [English](INTEGRATION_GUIDE.en.md)

Tài liệu này giúp bạn wiring startup đúng ngay từ đầu theo pattern đang dùng.

## Chọn nhanh kiểu đăng ký DI

| Trường hợp | Cách đăng ký |
| --- | --- |
| Dùng CQRS hoặc FullStack cơ bản | `AddRynorArchDependencies()` |
| Dùng Repository/FullStack có `UseUnitOfWork = true` | `AddRynorArchDependencies<AppDbContext>()` |
| Muốn tự cấu hình MediatR | `AddRynorArchDependencies(registerMediatR: false)` |

## Thiết lập CQRS / FullStack

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using RynorArch.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddRynorArchDependencies();

var app = builder.Build();
app.Run();
```

Ghi chú:
- `AddRynorArchDependencies()` có thể tự động đăng ký MediatR theo mặc định.
- Nếu bạn đã có cấu hình MediatR riêng, dùng `AddRynorArchDependencies(registerMediatR: false)`.

Kết quả mong đợi:

- Generated handlers được resolve từ DI.
- Validation/caching behavior (nếu bật) chạy trong pipeline.

Validation behavior:
- Khi `EnableValidation = true`, generated DI sẽ đăng ký `RynorArchValidationBehavior<,>`.
- Generated `Create*` và `Update*` validators chạy tự động trong MediatR pipeline behavior.
- Hook partial `OnValidate(...)` trong handler vẫn dùng được cho domain check bổ sung.

Optional runtime hooks:
- Generated handlers resolve `ISecurityContext` (nếu có đăng ký) và truyền metadata `UserId`/`TenantId` cho observer hooks.
- Generated handlers và validation behavior resolve `IRynorArchExecutionObserver` dưới dạng `IEnumerable<T>`, nên có thể đăng ký 0, 1 hoặc nhiều observer.

```csharp
using RynorArch.Abstractions.Interfaces;

builder.Services.AddScoped<ISecurityContext, HttpSecurityContext>();
builder.Services.AddSingleton<IRynorArchExecutionObserver, StructuredExecutionObserver>();
```

Nếu không đăng ký implementation nào, generated handlers vẫn hoạt động bình thường.

## Thiết lập Repository

```csharp
using Microsoft.EntityFrameworkCore;
using RynorArch.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddRynorArchDependencies<AppDbContext>();
```

Ghi chú:
- Khi `UseUnitOfWork = true`, ưu tiên `AddRynorArchDependencies<TDbContext>()` để auto-wire generated `IUnitOfWork`.
- Overload không generic vẫn hợp lệ nếu bạn không dùng UnitOfWork.

Kết quả mong đợi:

- Repository interfaces và implementations được resolve từ DI.
- UnitOfWork adapter được đăng ký tự động khi dùng overload generic.

## Endpoint generation

Khi bật endpoint generation:

```csharp
using RynorArch.Endpoints;

app.MapRynorArchEndpoints();
```

Bắt buộc có đủ 2 cờ:
- `GenerateEndpoints = true`
- `EnableExperimentalEndpoints = true`

Trước khi rollout production, bắt buộc rà checklist tại `docs/ENDPOINT_HARDENING.md`.

Generated write semantics:
- `POST` trả `201 Created` với body `{ id = <guid> }`.
- `PUT` trả `404` nếu resource không tồn tại, ngược lại `204`.
- `DELETE` trả `404` nếu resource không tồn tại, ngược lại `204`.

## Caching decorators

Khi `GenerateCachingDecorators = true`:
- đăng ký distributed cache provider (ví dụ Redis hoặc memory-backed distributed cache)
- gọi `AddRynorArchDependencies()` để wire generated cache behaviors và invalidators
- theo checklist rollout trong `docs/CACHING_OPERATIONS.md`

## Save mode

Nếu `CqrsSaveMode = CqrsSaveMode.PerRequestTransaction`:
- giữ generated DI wiring, hoặc
- đăng ký thủ công `IPipelineBehavior<,>` vào `RynorArchSaveChangesBehavior<,>`

Khi nào nên dùng `PerRequestTransaction`:

- Một request có nhiều thao tác ghi và cần commit/rollback đồng nhất.
- Bạn muốn đảm bảo tính nhất quán mạnh hơn so với save ngay từng handler.

## Domain events cho aggregate root

Khi entity có `[AggregateRoot]`, generated CQRS write handlers raise domain events trước khi persistence:
- `Create*Handler` raise `{Entity}CreatedEvent`
- `Update*Handler` raise `{Entity}UpdatedEvent`
- `Delete*Handler` raise `{Entity}DeletedEvent`

Các event type được sinh dưới namespace `{EntityNamespace}.DomainEvents`.

## Checklist xác minh sau tích hợp

1. Chạy `dotnet build` và bảo đảm không lỗi generator.
2. Chạy `rynor doctor` và xử lý toàn bộ FAIL checks.
3. Nếu bật endpoint, gọi thử POST/PUT/DELETE và xác nhận mã trạng thái đúng.
4. Nếu bật caching, chạy một kịch bản read-after-write để xác nhận invalidation.

## Mẫu tích hợp cho AI agent

Khi agent triển khai thay đổi, dùng gate tối thiểu sau:

1. Áp dụng cấu hình và code changes.
2. Chạy `dotnet build`.
3. Chạy `rynor doctor`.
4. Chỉ tiếp tục nếu kết quả là `READY` hoặc `READY WITH WARNINGS`.

Để xác minh runtime behavior, chạy các kịch bản trong `docs/RUNTIME_TESTING.md`.

Chi tiết contract và output kỳ vọng nằm ở `docs/AI_AGENT_PLAYBOOK.md`.
