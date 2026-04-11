# Tham chiếu Attribute

[Tiếng Việt](ATTRIBUTE_REFERENCE.md) | [English](ATTRIBUTE_REFERENCE.en.md)

## Architecture

Attribute cấu hình ở mức assembly để điều khiển hành vi source generator.

```csharp
[assembly: Architecture(Profile = ArchitectureProfile.CqrsQuickStart)]
```

Các trường chính:
- `Profile`: cấu hình khởi tạo nhanh theo mẫu.
- `Pattern`: `Cqrs`, `Repository`, hoặc `FullStack`.
- `GenerateDependencyInjection`: sinh extension DI và đăng ký runtime.
- `GenerateEndpoints` + `EnableExperimentalEndpoints`: cặp cờ opt-in cho endpoint generation.
- `GenerateCachingDecorators`: sinh cache behavior và invalidation artifacts.
- `DbContextType`: tùy chọn override kiểu DbContext cho CQRS handler constructor.
- `CqrsSaveMode`: chiến lược ghi dữ liệu cho CQRS write handlers.

## Entity

Đánh dấu kiểu cần được sinh mã.

Yêu cầu:
- class phải là `partial`
- nên kế thừa `EntityBase` để dùng convention chung

## AggregateRoot

Bật sinh domain event artifacts cho entity.

Yêu cầu:
- kiểu phải đồng thời có `[Entity]`

## QueryFilter

Đánh dấu property có thể lọc trong list query/specification được sinh.

Kiểu dữ liệu hỗ trợ:
- string
- numeric primitives
- bool
- DateTime
- Guid
- enum
- biến thể nullable của các kiểu trên

## Validation attributes

Đây là attribute của RynorArch (không phải `System.ComponentModel.DataAnnotations`):
- `[Required]`
- `[MinLength(n)]`
- `[MaxLength(n)]`
- `[Email]`

Khi bật validation generation, các attribute này được dùng để sinh FluentValidation rules.
