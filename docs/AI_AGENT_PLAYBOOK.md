# Cẩm nang AI Agent

[Tiếng Việt](AI_AGENT_PLAYBOOK.md) | [English](AI_AGENT_PLAYBOOK.en.md)

Tài liệu này định nghĩa workflow mang tính quyết định cho AI agents khi làm việc với dự án RynorArch.

## Hợp đồng workflow

Mọi tác vụ agent cần tuân theo cấu trúc sau:

1. Yêu cầu đầu vào: file bắt buộc, config bắt buộc, dependency bắt buộc.
2. Đầu ra kỳ vọng: artifacts hoặc đường dẫn code phải tồn tại sau khi chạy.
3. Tiêu chí thành công: build/test/diagnostic checks phải pass.
4. Xử lý thất bại: hành động tiếp theo cụ thể khi check fail.

## Tác vụ 1: Khởi tạo architecture config

### Yêu cầu đầu vào

- Thư mục dự án có `.csproj`.
- Có `RynorArch.Abstractions` và `RynorArch.Generator` qua package hoặc project reference.

### Cách thực hiện

1. Chạy `rynor init`.
2. Chọn architecture profile.
3. Xác nhận tồn tại `AssemblyConfig.cs`.

### Đầu ra kỳ vọng

- `AssemblyConfig.cs`
- `README_NEXT_STEPS.md`

### Tiêu chí thành công

- `rynor doctor` không còn FAIL ở `DR002`, `DR004`, `DR007`, `DR008`.

## Tác vụ 2: Scaffold entity đầu tiên

### Yêu cầu đầu vào

- Đã có `AssemblyConfig.cs` hợp lệ.

### Cách thực hiện

1. Chạy `rynor scaffold <EntityName> <Namespace>`.
2. Build một lần bằng `dotnet build`.

### Đầu ra kỳ vọng

- `Domain/<EntityName>.cs`
- Tùy chọn CQRS extension partials ở `Cqrs/<EntityName>/`
- `RynorArch.GenerationReport.g.cs` dưới `obj/`

### Tiêu chí thành công

- `rynor doctor` PASS ở `DR014` và `DR015`.
- Build hoàn tất không có generator errors.

## Tác vụ 3: Tích hợp runtime wiring

### Yêu cầu đầu vào

- App startup đã có DI setup.

### Cách thực hiện

1. Gọi `builder.Services.AddRynorArchDependencies();`.
2. Nếu bật UnitOfWork, dùng `builder.Services.AddRynorArchDependencies<AppDbContext>();`.
3. Nếu bật endpoint generation, gọi `app.MapRynorArchEndpoints();`.

### Tiêu chí thành công

- Dependency checks từ `DR009` đến `DR013` không còn FAIL.
- App startup chạy thành công.

## Tác vụ 4: Readiness gate

### Cách thực hiện

1. Chạy `rynor doctor` tại project root.
2. Đọc dòng tổng kết.

### Quy tắc quyết định

- `NOT READY`: còn ít nhất một FAIL, bắt buộc xử lý trước khi đi tiếp.
- `READY WITH WARNINGS`: có thể tiếp tục ở môi trường dev, nên xử lý cảnh báo trước production.
- `READY`: không còn check chặn.

## Bản đồ lỗi thường gặp

- Thiếu architecture config: sửa bằng `rynor init`.
- Bật endpoint nhưng thiếu opt-in: đặt `EnableExperimentalEndpoints = true` hoặc tắt endpoint generation.
- Entity không phải partial: đánh dấu các class có `[Entity]` là `partial`.
- Thiếu generation report: chạy `dotnet build` một lần.

## Lệnh xác minh

```bash
dotnet build
rynor doctor
dotnet test RynorArch.slnx -v minimal
```

## Tài liệu liên quan

- `docs/GETTING_STARTED.md`
- `docs/INTEGRATION_GUIDE.md`
- `docs/TROUBLESHOOTING.md`
- `docs/ENDPOINT_HARDENING.md`
- `docs/CACHING_OPERATIONS.md`
