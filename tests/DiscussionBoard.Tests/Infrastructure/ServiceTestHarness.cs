using System.Collections;
using System.Reflection;
using DiscussionBoard.Data;

namespace DiscussionBoard.Tests.Infrastructure;

/// <summary>Late-bound domain helpers: Lesson tests compile before the domain exists.</summary>
public static class ServiceTestHarness
{
    public static Type Require(string name) => ApplicationTypeLocator.RequireByName(name);

    public static object NewEntity(string name, params (string Name, object? Value)[] values)
    {
        Type type = Require(name);
        object? instance = Activator.CreateInstance(type);
        Assert.NotNull(instance);
        foreach ((string property, object? value) in values)
        {
            PropertyInfo? info = type.GetProperty(property, BindingFlags.Instance | BindingFlags.Public);
            Assert.True(info is not null && info.CanWrite,
                $"{name} must expose a writable '{property}' property.");
            info.SetValue(instance, value);
        }
        return instance;
    }

    public static object Service(string name, ApplicationDbContext db)
    {
        Type type = Require(name);
        ConstructorInfo? constructor = type.GetConstructors()
            .FirstOrDefault(c => c.GetParameters().Length == 1 &&
                c.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(ApplicationDbContext)));
        Assert.True(constructor is not null,
            $"{name} must be constructible with ApplicationDbContext for isolated tests.");
        return constructor.Invoke([db]);
    }

    public static async Task<object?> CallAsync(object service, string method, params object[] args)
    {
        MethodInfo? info = service.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == method && m.GetParameters().Length == args.Length &&
                m.GetParameters().Zip(args).All(pair => pair.First.ParameterType.IsInstanceOfType(pair.Second)));
        Assert.True(info is not null,
            $"{service.GetType().Name}.{method} must accept the documented arguments.");
        object? result = info.Invoke(service, args);
        Assert.True(result is Task, $"{method} must return a Task.");
        await (Task)result;
        return result.GetType().GetProperty("Result")?.GetValue(result);
    }

    public static async Task<int> SaveAsync(ApplicationDbContext db, object entity)
    {
        db.Add(entity);
        await db.SaveChangesAsync();
        PropertyInfo? id = entity.GetType().GetProperty("Id");
        Assert.NotNull(id);
        return Convert.ToInt32(id.GetValue(entity));
    }

    public static async Task<object> CreateForumAsync(ApplicationDbContext db, string name)
    {
        object forum = NewEntity("Forum", ("Name", name), ("Description", "Lesson 1 test forum"));
        await SaveAsync(db, forum);
        return forum;
    }

    public static IReadOnlyList<object> AsItems(object? result, string requirement)
    {
        Assert.True(result is IEnumerable && result is not string, requirement);
        return ((IEnumerable)result).Cast<object>().ToArray();
    }
}
