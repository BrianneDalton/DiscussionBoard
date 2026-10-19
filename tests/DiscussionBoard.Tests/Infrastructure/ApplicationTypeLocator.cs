using System.Reflection;

namespace DiscussionBoard.Tests.Infrastructure;

public static class ApplicationTypeLocator
{
    public static Assembly ApplicationAssembly => typeof(Program).Assembly;

    public static Type? FindByName(string typeName)
    {
        return ApplicationAssembly
            .GetTypes()
            .FirstOrDefault(type => string.Equals(type.Name, typeName, StringComparison.Ordinal));
    }

    public static Type RequireByName(string typeName)
    {
        Type? type = FindByName(typeName);

        Assert.True(
            type is not null,
            $"The application must contain a type named '{typeName}'. " +
            "If you have not reached that checkpoint yet, this failure is expected.");

        return type;
    }
}
