# Tương thích

[Tiếng Việt](COMPATIBILITY.md) | [English](COMPATIBILITY.en.md)

## Môi trường phát triển đã xác thực

- .NET SDK: `10.0.x`
- Generator target của Zenith Arch: `netstandard2.0`
- Runtime target cho CLI: `net8.0`, `net9.0`, `net10.0`

## Kỳ vọng với dự án sử dụng

- Thêm `ZenithArch.Abstractions` như package reference thông thường.
- Thêm `ZenithArch.Generator` ở chế độ analyzer-only.
- Bắt buộc khai báo tường minh `[assembly: Architecture(...)]`; nếu thiếu thì không sinh mã.
- Nên ưu tiên `ArchitectureProfile.*QuickStart` để giảm số cờ cấu hình ban đầu.
- Khi dùng `Cqrs`, `Repository`, hoặc `FullStack`, hệ thống sẽ sinh lớp hạ tầng dùng chung cho CRUD/EF.
- Nếu dùng CQRS, có thể đặt `DbContextType = typeof(...)` để ràng buộc handler với DbContext cụ thể.
- Endpoint generation yêu cầu opt-in experimental rõ ràng (`GenerateEndpoints = true` và `EnableExperimentalEndpoints = true`).
- Gọi `AddZenithArchDependencies()` trong startup để wire handlers/repositories/validators/cache behaviors đã sinh.

## Phụ thuộc theo tính năng

| Tính năng | Phụ thuộc bắt buộc |
| --- | --- |
| CQRS / FullStack handlers | `MediatR` |
| Validation generation | `FluentValidation` |
| Repository / CQRS persistence | `Microsoft.EntityFrameworkCore` |
| Endpoint generation | ASP.NET Core shared framework |
| Caching decorators | `Microsoft.Extensions.Caching.*` |

## Khuyến nghị triển khai

- Bắt đầu từ một module hoặc bounded context.
- Pin version package ở môi trường production.
- Mỗi lần nâng cấp nên rà lại output sinh mã.
- Nếu code hiện tại đang phụ thuộc vào chi tiết nội bộ của generated repository, cần kế hoạch migration vì implementation hiện tại là wrapper mỏng trên generic base.
