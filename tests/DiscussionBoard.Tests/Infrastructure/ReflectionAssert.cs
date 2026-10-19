using System.Reflection;

namespace DiscussionBoard.Tests.Infrastructure;

public static class ReflectionAssert
{
    public static PropertyInfo HasProperty(Type type, string propertyName, Type expectedType)
    {
        PropertyInfo? property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

        Assert.True(
            property is not null,
            $"{type.Name} must contain a public property named '{propertyName}'.");

        Assert.True(
            property.PropertyType == expectedType,
            $"{type.Name}.{propertyName} must have type {FriendlyName(expectedType)}, " +
            $"but its type is {FriendlyName(property.PropertyType)}.");

        return property;
    }

    public static PropertyInfo HasProperty<T>(Type type, string propertyName)
    {
        return HasProperty(type, propertyName, typeof(T));
    }

    public static void InheritsFrom(Type type, Type expectedBaseType)
    {
        Assert.True(
            expectedBaseType.IsAssignableFrom(type) && type != expectedBaseType,
            $"{type.Name} must inherit from {FriendlyName(expectedBaseType)}.");
    }

    public static TAttribute HasAttribute<TAttribute>(MemberInfo member)
        where TAttribute : Attribute
    {
        TAttribute? attribute = member.GetCustomAttribute<TAttribute>();

        Assert.True(
            attribute is not null,
            $"{member.DeclaringType?.Name}.{member.Name} must have [{typeof(TAttribute).Name.Replace("Attribute", string.Empty)}].");

        return attribute;
    }

    public static MethodInfo HasMethod(Type type, string methodName)
    {
        MethodInfo? method = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(candidate => candidate.Name == methodName);

        Assert.True(method is not null, $"{type.Name} must contain a public method named '{methodName}'.");
        return method;
    }

    private static string FriendlyName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        string name = type.Name[..type.Name.IndexOf('`')];
        string arguments = string.Join(", ", type.GetGenericArguments().Select(FriendlyName));
        return $"{name}<{arguments}>";
    }
}
