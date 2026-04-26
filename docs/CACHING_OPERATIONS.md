# Hướng dẫn vận hành Caching

[Tiếng Việt](CACHING_OPERATIONS.md) | [English](CACHING_OPERATIONS.en.md)

Tài liệu này mô tả kỳ vọng runtime khi bật `GenerateCachingDecorators = true`.

## Điều kiện tiên quyết runtime

- Đăng ký distributed cache provider (`AddDistributedMemoryCache` cho môi trường dev hoặc Redis cho production).
- Bảo đảm generated DI wiring được bật qua `AddZenithArchDependencies()`.
- Xác nhận cache key ổn định giữa các lần triển khai.

## Hành vi sinh mã mặc định

- Kết quả query đọc được cache qua generated query cache behaviors.
- Hệ thống sinh per-entity invalidation interfaces.
- Create/update/delete handlers gọi invalidator đã sinh khi DI wiring hoạt động.

## Guardrails cho production

- Đặt chính sách TTL rõ ràng theo entity/query (hot reads và cold reads).
- Thêm jitter vào TTL cho key lưu lượng cao để tránh hết hạn đồng loạt.
- Dùng key có namespace (`service:entity:id`) để tránh va chạm.
- Theo dõi cache hit ratio, eviction và độ trễ backend.

## Mẫu rollout phổ biến

1. Bắt đầu với distributed memory cache ở môi trường thấp.
2. Nâng lên Redis nhưng giữ nguyên key schema.
3. Có dashboard và alert trước khi bật ở module lưu lượng cao.

## Kiểm tra nhanh khi troubleshooting

- Xác nhận đã sinh `Get{Entity}ByIdQueryCacheBehavior.g.cs`.
- Xác nhận DI extension đã đăng ký cả invalidator và cache pipeline behavior.
- Xác nhận write handlers đi qua đường invalidation trong integration tests.
