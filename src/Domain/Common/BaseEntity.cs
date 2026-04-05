namespace TechSpherex.CleanArchitecture.Domain.Common;

// Copyright (c) 2026 TechSpherex
public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
