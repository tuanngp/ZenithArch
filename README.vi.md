# Zenith Arch

[Tiếng Việt](https://github.com/tuanngp/ZenithArch/blob/main/README.vi.md) | [English](https://github.com/tuanngp/ZenithArch/blob/main/README.md)

## Zenith Arch là gì?

Zenith Arch là framework tự động hóa kiến trúc .NET ở compile-time bằng Roslyn Incremental Source Generator. Bạn mô tả kiến trúc một lần qua `[assembly: Architecture(...)]`, generator sẽ sinh phần CRUD/CQRS/DI/validation theo pattern đã chọn.

Mục tiêu chính:

- Giảm boilerplate lặp đi lặp lại.
- Giữ ranh giới kiến trúc rõ ràng và nhất quán.
- Có output sinh mã dễ kiểm tra trong `obj/`.
- Fail-fast khi cấu hình thiếu hoặc sai.

## Bắt đầu trong 5 phút

### 1) Cài package

```xml
<PackageReference Include="ZenithArch.Abstractions" Version="1.0.7" />
<PackageReference Include="ZenithArch.Generator" Version="1.0.7" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

Tùy chọn cài CLI:

```bash
dotnet tool install --global ZenithArch.Cli --version 1.0.7
dotnet tool update --global ZenithArch.Cli --version 1.0.7
```

### 2) Sinh cấu hình nhanh bằng CLI

```bash
rynor init
```

### 3) Scaffold entity đầu tiên

```bash
rynor scaffold Trip MyApp.Domain
```

### 4) Build để generator chạy

```bash
dotnet build
```

### 5) Wire runtime trong startup

```csharp
builder.Services.AddZenithArchDependencies();
```

Kết quả mong đợi sau 5 bước:

- Có `AssemblyConfig.cs`.
- Có entity/partials vừa scaffold.
- Có file sinh trong `obj/` và `ZenithArch.GenerationReport.g.cs`.
- Ứng dụng chạy được với DI đã đăng ký.

## Chọn pattern trong 30 giây

| Nếu bạn cần | Chọn pattern |
| --- | --- |
| Tách command/query rõ ràng, xử lý qua MediatR | `Cqrs` |
| Ưu tiên repository boundary và unit of work | `Repository` |
| Muốn dùng đồng thời CQRS và Repository | `FullStack` |

Starter profile tương ứng:

- `ArchitectureProfile.CqrsQuickStart`
- `ArchitectureProfile.RepositoryQuickStart`
- `ArchitectureProfile.FullStackQuickStart`

Lưu ý: flag tường minh trong `Architecture(...)` luôn ghi đè profile mặc định.

## Cấu hình mẫu an toàn cho dự án mới

```csharp
using ZenithArch.Abstractions.Attributes;
using ZenithArch.Abstractions.Enums;

[assembly: Architecture(
    Profile = ArchitectureProfile.CqrsQuickStart,
    Pattern = ArchitecturePattern.Cqrs,
    GenerateDependencyInjection = true
)]
```

Khi đã có DbContext cụ thể, bạn có thể bổ sung `DbContextType = typeof(...)`.

## Tính năng được hỗ trợ

### Cấu hình và sinh mã

- `UseSpecification`: sinh specification từ property có `[QueryFilter]`.
- `UseUnitOfWork`: sinh `IUnitOfWork` (Repository/FullStack).
- `EnableValidation`: sinh FluentValidation validator cho command ghi.
- `GenerateDependencyInjection`: sinh `AddZenithArchDependencies(...)`.
- `GenerateDtos`: sinh DTO record + mapping extension.
- `GenerateEfConfigurations`: sinh `IEntityTypeConfiguration<T>`.
- `GeneratePagination`: sinh artifacts phân trang/sắp xếp.

### Runtime behavior

- `CqrsSaveMode`: `PerHandler` hoặc `PerRequestTransaction`.
- `GenerateCachingDecorators`: sinh cache behaviors + invalidation contracts.
- Endpoint generation (experimental): cần cả `GenerateEndpoints = true` và `EnableExperimentalEndpoints = true`.
- Endpoint write semantics mặc định: POST `201`, PUT/DELETE `404` hoặc `204`.

### Domain và extensibility

- Attribute hỗ trợ: `[Entity]`, `[AggregateRoot]`, `[QueryFilter]`, `[MapTo(typeof(...))]`.
- Validation attributes: `[Required]`, `[MinLength]`, `[MaxLength]`, `[Email]`.
- Partial hooks: `OnValidate`, `OnBeforeHandle`, `OnAfterHandle`, `OnBeforeQuery`.
- Optional hooks: `IZenithArchExecutionObserver`, `ISecurityContext`.

### CLI và readiness gate

- `rynor init`
- `rynor scaffold <EntityName> [Namespace]`
- `rynor doctor [ProjectPath]`

`rynor doctor` trả về `NOT READY`, `READY WITH WARNINGS`, hoặc `READY` để bạn quyết định có thể đi tiếp hay không.

## Phụ thuộc theo tính năng

| Tính năng | Phụ thuộc bắt buộc |
| --- | --- |
| Handler CQRS / FullStack | MediatR |
| Sinh validation | FluentValidation |
| Persistence | Microsoft.EntityFrameworkCore |
| Sinh endpoint | Microsoft.AspNetCore.App |
| Caching decorators | Microsoft.Extensions.Caching.* |

## Checklist trước khi commit

1. Chạy `dotnet build` và bảo đảm không có lỗi generator.
2. Chạy `rynor doctor` và xử lý toàn bộ FAIL checks.
3. Nếu có endpoint/caching/transaction mode, chạy integration tests phù hợp.
4. Kiểm tra `ZenithArch.GenerationReport.g.cs` có đúng entities và artifacts kỳ vọng.

## Tương thích

- SDK đã xác thực: `.NET SDK 10.0.x`
- Generator target: `netstandard2.0`
- CLI runtime targets: `net8.0`, `net9.0`, `net10.0`

## Tài liệu theo mục tiêu

- Muốn setup nhanh từ đầu: [Bắt đầu nhanh](https://github.com/tuanngp/ZenithArch/blob/main/docs/GETTING_STARTED.md)
- Muốn tích hợp runtime vào startup: [Hướng dẫn tích hợp](https://github.com/tuanngp/ZenithArch/blob/main/docs/INTEGRATION_GUIDE.md)
- Muốn xem toàn bộ cờ tính năng: [Ma trận tính năng](https://github.com/tuanngp/ZenithArch/blob/main/docs/FEATURE_MATRIX.md)
- Muốn tra attribute: [Tham chiếu attribute](https://github.com/tuanngp/ZenithArch/blob/main/docs/ATTRIBUTE_REFERENCE.md)
- Muốn xử lý lỗi nhanh: [Xử lý sự cố](https://github.com/tuanngp/ZenithArch/blob/main/docs/TROUBLESHOOTING.md)
- Muốn rollout endpoint an toàn: [Hardening endpoint](https://github.com/tuanngp/ZenithArch/blob/main/docs/ENDPOINT_HARDENING.md)
- Muốn vận hành cache: [Vận hành caching](https://github.com/tuanngp/ZenithArch/blob/main/docs/CACHING_OPERATIONS.md)
- Muốn test runtime end-to-end: [Kiểm thử runtime](https://github.com/tuanngp/ZenithArch/blob/main/docs/RUNTIME_TESTING.md)
- Muốn nâng cấp version: [Nâng cấp](https://github.com/tuanngp/ZenithArch/blob/main/docs/UPGRADING.md)
- Muốn migration profile-first: [Nâng cấp profile](https://github.com/tuanngp/ZenithArch/blob/main/docs/UPGRADING_PROFILES.md)
- Muốn phát hành package: [Quy trình release](https://github.com/tuanngp/ZenithArch/blob/main/docs/RELEASING.md)
- Muốn workflow cho AI agent: [Cẩm nang AI agent](https://github.com/tuanngp/ZenithArch/blob/main/docs/AI_AGENT_PLAYBOOK.md)
- Theo dõi thay đổi version: [Changelog](https://github.com/tuanngp/ZenithArch/blob/main/CHANGELOG.md)
