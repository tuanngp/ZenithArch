# Nâng cấp sang cấu hình Profile-First

[Tiếng Việt](UPGRADING_PROFILES.md) | [English](UPGRADING_PROFILES.en.md)

Cấu hình legacy với nhiều cờ tường minh vẫn chạy, nhưng profile-first dễ bảo trì hơn.

## Vì sao nên migration

- Giảm lặp lại cờ cấu hình giữa các service.
- Giảm rủi ro config drift giữa các module.
- Đơn giản hơn cho onboarding và review.

## Bản đồ mapping

- Module thiên CQRS: `ArchitectureProfile.CqrsQuickStart`
- Module thiên Repository: `ArchitectureProfile.RepositoryQuickStart`
- Module end-to-end: `ArchitectureProfile.FullStackQuickStart`

## Ví dụ migration

Trước:

```csharp
[assembly: Architecture(
    Pattern = ArchitecturePattern.FullStack,
    UseSpecification = true,
    UseUnitOfWork = true,
    EnableValidation = true,
    GenerateDependencyInjection = true,
    GenerateDtos = true,
    GenerateEfConfigurations = true,
    GeneratePagination = true,
    CqrsSaveMode = CqrsSaveMode.PerRequestTransaction
)]
```

Sau:

```csharp
[assembly: Architecture(
    Profile = ArchitectureProfile.FullStackQuickStart,
    Pattern = ArchitecturePattern.FullStack,
    GenerateDependencyInjection = true,
    CqrsSaveMode = CqrsSaveMode.PerRequestTransaction
)]
```

Chỉ giữ explicit flags cho các khác biệt có chủ đích so với profile mặc định.

## Checklist migration

1. Chọn `Profile` gần nhất cho từng module.
2. Gỡ các flag trùng với mặc định của profile.
3. Chỉ giữ flag khác biệt có chủ đích.
4. Rebuild và so sánh `RynorArch.GenerationReport.g.cs`.
5. Chạy integration tests trên một module thí điểm trước khi rollout rộng.
