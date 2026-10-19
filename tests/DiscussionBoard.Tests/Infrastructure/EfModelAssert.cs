using DiscussionBoard.Data;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DiscussionBoard.Tests.Infrastructure;

public static class EfModelAssert
{
    public static IEntityType RequireEntity(ApplicationDbContext context, string name)
    {
        Type type = ApplicationTypeLocator.RequireByName(name);
        IEntityType? entity = context.Model.FindEntityType(type);
        Assert.True(entity is not null, $"ApplicationDbContext must map {name} as an EF Core entity.");
        return entity;
    }
}
