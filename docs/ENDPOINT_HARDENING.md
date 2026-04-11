# Hướng dẫn hardening Endpoint

[Tiếng Việt](ENDPOINT_HARDENING.md) | [English](ENDPOINT_HARDENING.en.md)

Generated endpoint của RynorArch được thiết kế tối giản có chủ đích. Hãy áp dụng checklist dưới đây trước khi đưa vào production.

## 1. Thiết lập ranh giới phân quyền

- Áp dụng `RequireAuthorization()` theo route group hoặc từng endpoint.
- Tách policy cho luồng đọc và ghi (ví dụ `Trips.Read`, `Trips.Write`).
- Ràng buộc tenant nên được kiểm tra trong handler, không chỉ ở endpoint filters.

## 2. Chuẩn hóa problem details

- Cấu hình `UseExceptionHandler()` và `AddProblemDetails()` ở mức toàn cục.
- Ánh xạ domain exception và validation exception về RFC 7807 response.
- Giữ error contract ổn định cho phía client.

## 3. Rõ ràng semantics not-found và conflict

- Duy trì `404` cho tài nguyên không tồn tại.
- Trả `409` cho optimistic concurrency hoặc vi phạm invariant.
- Trả `422` khi validation fail sau bước request binding.

## 4. Xác thực request contract tại rìa hệ thống

- Tiếp tục dùng FluentValidation trong handler, đồng thời thêm check payload ở endpoint nếu cần.
- Từ chối payload quá lớn và content-type không hỗ trợ.
- Ưu tiên DTO contract tường minh thay vì lộ trực tiếp EF entity.

## 5. Bổ sung observability

- Ghi structured log gồm route, entity id, tenant id, correlation id.
- Phát trace quanh mỗi lần chạy endpoint và handler.
- Theo dõi p95 latency và tỉ lệ non-2xx theo từng route.

## 6. Hardening vòng đời API

- Định nghĩa chiến lược versioning trước khi mở cho consumer bên ngoài.
- Dùng idempotency key cho các luồng create/update có khả năng retry.
- Thêm integration tests cho toàn bộ generated routes.

## Mẫu startup khuyến nghị

```csharp
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.MapGroup("/api")
   .RequireAuthorization()
   .MapRynorArchEndpoints();
```
