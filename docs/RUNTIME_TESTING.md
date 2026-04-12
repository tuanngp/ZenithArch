# Hướng dẫn kiểm thử Runtime

[Tiếng Việt](RUNTIME_TESTING.md) | [English](RUNTIME_TESTING.en.md)

Tài liệu này xác minh runtime behavior theo luồng end-to-end, không chỉ kiểm tra hình dạng source được sinh.

## Vì sao tài liệu này tồn tại

`RynorArch.Generator.Tests` kiểm tra diagnostics và generated output contracts.
`RynorArch.Integration.Tests` kiểm tra runtime behavior với relational provider.

Cả hai đều cần thiết trước khi release.

## Dự án test

Runtime integration tests nằm tại:

- `tests/RynorArch.Integration.Tests`

Test host sử dụng:

- SQLite in-memory (`Microsoft.EntityFrameworkCore.Sqlite`)
- Generated CQRS handlers và MediatR pipeline behaviors từ `samples/RynorArch.Sample`
- Generated caching invalidators và validation behavior

## Các kịch bản đã bao phủ

Bộ runtime test hiện xác minh:

- CRUD roundtrip qua generated commands và handlers
- Soft-delete query exclusion (`ISoftDelete`)
- Audit timestamp stamping (`IAuditable`)
- Validation failure chặn persistence
- Transaction rollback theo request khi handler fail
- Cache population và invalidation cho `GetById` query pipeline

## Chạy local

```powershell
dotnet test tests/RynorArch.Integration.Tests/RynorArch.Integration.Tests.csproj -c Release
```

Để chạy full release gate:

```powershell
dotnet restore RynorArch.slnx
dotnet build RynorArch.slnx -c Release
dotnet test RynorArch.slnx -c Release
dotnet run --project src/RynorArch.Cli/RynorArch.Cli.csproj -- doctor samples/RynorArch.Sample
```

## Baseline performance cho generator (compile-time)

Chạy benchmark dạng dry cho các hot path của generator:

```powershell
dotnet run --project tests/RynorArch.Performance.Tests/RynorArch.Performance.Tests.csproj -c Release -- --filter *RunGenerator* --job Dry
```

Artifact benchmark được ghi vào:

- `tests/RynorArch.Performance.Tests/BenchmarkDotNet.Artifacts/results`

## Viết thêm runtime tests

1. Tái sử dụng `IntegrationTestHost` để khởi tạo service collection chạy trên SQLite.
2. Gửi generated commands qua `IMediator`.
3. Assert cả semantics ở mức API và trạng thái persistence trong `AppDbContext`.
4. Ưu tiên assert theo behavior thay vì so sánh text source.

## Ghi chú

- Giữ endpoint generation ở trạng thái experimental trừ khi có quyết định nâng cấp chính thức.
- Nếu không cấu hình `DbContextType`, generated handlers sẽ phụ thuộc `DbContext`; integration host hiện ánh xạ tường minh `DbContext` sang `AppDbContext` để DI ổn định.
