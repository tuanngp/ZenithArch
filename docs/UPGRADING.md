# Nâng cấp

[Tiếng Việt](UPGRADING.md) | [English](UPGRADING.en.md)

## Quy trình nâng cấp

1. Cập nhật version package trên một branch riêng.
2. Chạy `dotnet test RynorArch.slnx`.
3. So sánh output sinh mã trên một dự án mẫu đại diện.
4. Đọc `CHANGELOG.md` để nắm thay đổi về hành vi và diagnostics.
5. Chỉ rollout sang các module khác khi module nâng cấp đầu tiên đã ổn định.

## Migration profile-first (khuyến nghị)

Cấu hình profile-first là hướng ưu tiên cho module mới và module nâng cấp.
Xem mapping trước/sau tại `docs/UPGRADING_PROFILES.md`.

Khi migration:

1. Chọn starter profile gần nhất.
2. Gỡ explicit flags trùng với mặc định của profile.
3. Giữ lại các flag override có chủ đích.

## Ghi chú migration sau hybrid refactor

Generator hiện giảm kích thước source theo entity bằng cách chuyển phần CRUD/EF dùng chung vào lớp hạ tầng generic được sinh.

Điều này có nghĩa:

- generated repositories là wrapper mỏng trên `CrudRepository<TEntity>`
- CQRS handlers vẫn sinh theo từng entity, nhưng persistence logic chuyển sang shared helpers
- lớp hạ tầng dùng chung được sinh một lần mỗi compilation

## Ảnh hưởng thay đổi breaking

Bạn có thể cần sửa code nếu trước đây:

- phụ thuộc vào nội dung/members cụ thể của generated repository implementation
- kế thừa hoặc sao chép generated repository implementation
- giả định mỗi entity luôn có một standalone CRUD implementation đầy đủ

## Hướng migration

1. Thay mọi phụ thuộc vào generated repository internals bằng repository interface hoặc generated handler public types.
2. Rebuild và kiểm tra shared infrastructure file mới cùng output repository mỏng hơn.
3. Xác minh soft-delete, auditable, specification và CQRS save-mode trên một module đại diện trước khi rollout rộng.

## Ghi chú tối ưu sâu

Nhánh tối ưu mới chủ yếu là thay đổi nội bộ và cố gắng giữ nguyên public generated type shape từ hybrid refactor.

Các thay đổi chính:

- shared CRUD runtime cache per-entity traits cho các query path lặp lại
- specification apply nội bộ tách thành list/count branch nhưng giữ API public như cũ
- `IUnitOfWork` sinh một lần mỗi compilation thay vì theo từng entity
- CQRS list filtering và specification filtering dùng chung generation rules để giảm lệch hành vi

Phần lớn hệ thống sử dụng không cần migration ngoài rebuild và kiểm tra output đại diện.

## Chính sách version

- Patch: sửa diagnostics/metadata/docs/generated output mà thường không cần rewrite phía consumer.
- Minor: thêm tính năng generator tùy chọn hoặc output mở rộng không phá vỡ.
- Major: thay đổi generated API shape, convention lớn hoặc loại bỏ feature flags.

## Khuyến nghị an toàn cho phía consumer

- Không auto-upgrade generator package trong ứng dụng production.
- Giữ ít nhất một sample project pin ở version production hiện tại để so sánh.
- Xem mọi thay đổi generated output là có khả năng breaking kể cả khi compile vẫn pass.

## Checklist trải nghiệm dev khi nâng cấp

Sau khi nâng cấp, ngoài compile success, cần kiểm tra observability và diagnostics:

1. Xác nhận `RynorArch.GenerationReport.g.cs` được sinh và liệt kê đúng entities/artifacts.
2. Kiểm tra header metadata `rynor-artifact` trong file sinh để bảo đảm traceability.
3. Rà `RYNOR007`-`RYNOR013` và xử lý toàn bộ lỗi trước rollout.
4. Nếu bật CQRS, xác minh `DbContextType` (nếu set) resolve được về `DbContext` hợp lệ (`RYNOR008`).
5. Nếu bật endpoint generation, xác minh đã có explicit experimental opt-in (`RYNOR012`).
6. Nếu dùng `UseUnitOfWork` trong Repository, chuyển startup wiring sang `AddRynorArchDependencies<TDbContext>()` để auto-register generated `IUnitOfWork` adapter.
