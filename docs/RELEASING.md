# Phát hành

[Tiếng Việt](RELEASING.md) | [English](RELEASING.en.md)

## Quy trình phát hành local

Dùng script của repository để tăng version trung tâm, test, pack và tùy chọn push package:

```powershell
./publish.ps1 -Increment Patch
./publish.ps1 -Increment Minor
./publish.ps1 -Increment Major -Push
```

Mặc định script sẽ chạy thêm ApiCompat baseline cho `ZenithArch.Abstractions` so với bản NuGet trước đó.

## Quy trình phát hành tự động

- CI xác minh restore, build, test và pack trên push/pull request.
- Push tag khớp mẫu `v*` sẽ kích hoạt release workflow.
- Release workflow chỉ push package khi `NUGET_API_KEY` đã được cấu hình trong repository secrets.

## Checklist phát hành

- Cập nhật `CHANGELOG.md`.
- Xác nhận ví dụ trong `README.md` khớp version package hiện tại và support matrix.
- Chạy `dotnet restore ZenithArch.slnx`.
- Chạy `dotnet build ZenithArch.slnx -c Release`.
- Chạy `dotnet test ZenithArch.slnx -c Release`.
- Chạy compile check theo từng framework của `ZenithArch.Abstractions` (`netstandard2.0`, `netstandard2.1`, `net8.0`, `net9.0`).
- Xác nhận ApiCompat baseline pass bằng `eng/Validate-AbstractionsApiCompat.ps1`.
- Xác nhận artifact coverage của Abstractions được tạo và line coverage đạt tối thiểu 80%.
- Chạy benchmark compile-time cho generator (`dotnet run --project tests/ZenithArch.Performance.Tests/ZenithArch.Performance.Tests.csproj -c Release -- --filter *RunGenerator* --job Dry`).
- Xác nhận benchmark artifacts được tạo tại `tests/ZenithArch.Performance.Tests/BenchmarkDotNet.Artifacts/results`.
- Chạy `dotnet test tests/ZenithArch.Integration.Tests/ZenithArch.Integration.Tests.csproj -c Release`.
- Chạy `dotnet run --project src/ZenithArch.Cli/ZenithArch.Cli.csproj -- doctor samples/ZenithArch.Sample`.
- Xác nhận các kịch bản runtime trong `docs/RUNTIME_TESTING.md` đều xanh.
- Kiểm tra artifacts trong `artifacts/` trước khi publish.
