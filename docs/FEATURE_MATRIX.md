# Ma trận tính năng

[Tiếng Việt](FEATURE_MATRIX.md) | [English](FEATURE_MATRIX.en.md)

| Tính năng | Cqrs | Repository | FullStack | Ghi chú |
| --- | --- | --- | --- | --- |
| UseSpecification | Có | Có | Có | Sinh specification từ thuộc tính có `[QueryFilter]` |
| UseUnitOfWork | Không | Có | Có | Bị bỏ qua trong CQRS thuần |
| EnableValidation | Có | Không | Có | Sinh FluentValidation validator cho command |
| GenerateDependencyInjection | Có | Có | Có | Sinh `AddZenithArchDependencies()` |
| GenerateEndpoints | Có | Không | Có | Cần `EnableExperimentalEndpoints = true` |
| GenerateDtos | Có | Có | Có | DTO records và mapping extensions |
| GenerateEfConfigurations | Có | Có | Có | EF configuration partials |
| GenerateCachingDecorators | Có | Không | Có | Query cache behaviors và invalidator |
| GeneratePagination | Có | Có | Có | Artifacts phân trang |
| CqrsSaveMode | Có | Không | Có | `PerHandler` hoặc `PerRequestTransaction` |
| DbContextType | Có | N/A | Có | Override kiểu DbContext cho CQRS handler |

## Starter profile

| Profile | Mục tiêu sử dụng | Mặc định chính |
| --- | --- | --- |
| CqrsQuickStart | Module API/service dùng command-query handlers | CQRS + validation + specification + generated DI |
| RepositoryQuickStart | Module phân lớp ưu tiên repository boundary | Repository + specification + unit of work + generated DI |
| FullStackQuickStart | Module cần đồng thời CQRS và repository artifacts | FullStack + các cờ năng suất phổ biến + generated DI |

Mặc định profile chỉ là điểm bắt đầu. Flag tường minh trong `Architecture(...)` luôn được ưu tiên.

## Cổng kiểm tra readiness cho agent

| Cổng | Công cụ | Điều kiện pass |
| --- | --- | --- |
| Khám phá cấu hình | `zenith doctor` (`DR002`, `DR004`) | Tìm thấy `.csproj` và có architecture config |
| An toàn kiến trúc | `zenith doctor` (`DR005`, `DR006`) | Pattern/profile hợp lệ và endpoint opt-in đúng |
| Đồng bộ phụ thuộc | `zenith doctor` (`DR007`-`DR013`) | Đủ package/framework theo feature đang bật |
| Hình dạng entity | `zenith doctor` (`DR014`) | Tất cả class có `[Entity]` đều là `partial` |
| Dấu mốc generation | `zenith doctor` (`DR015`) | Có report sinh mã dưới `obj/` sau khi build |
| Ngữ nghĩa runtime | `dotnet test tests/ZenithArch.Integration.Tests` | CRUD, soft-delete, audit, validation, transaction và cache pass |
