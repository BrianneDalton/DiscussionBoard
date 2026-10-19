using System.Reflection;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Tests.Infrastructure;

public sealed class PageModelLocator
{
    private readonly Assembly _assembly;
    private readonly List<Func<Type, bool>> _requirements = [];
    private readonly List<string> _descriptions = [];

    private PageModelLocator(Assembly assembly)
    {
        _assembly = assembly;
    }

    public static PageModelLocator FromAssembly(Assembly assembly)
    {
        return new PageModelLocator(assembly);
    }

    public PageModelLocator WithProperty(string propertyName)
    {
        _requirements.Add(type => type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public) is not null);
        _descriptions.Add($"property '{propertyName}'");
        return this;
    }

    public PageModelLocator WithProperty<T>(string propertyName)
    {
        _requirements.Add(type => type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)?.PropertyType == typeof(T));
        _descriptions.Add($"property '{propertyName}' of type {typeof(T).Name}");
        return this;
    }

    public PageModelLocator WithHandler(string handlerName)
    {
        _requirements.Add(type => type.GetMethod(handlerName, BindingFlags.Instance | BindingFlags.Public) is not null);
        _descriptions.Add($"handler '{handlerName}'");
        return this;
    }

    public IReadOnlyList<Type> FindAll()
    {
        return _assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(PageModel).IsAssignableFrom(type))
            .Where(type => _requirements.All(requirement => requirement(type)))
            .OrderBy(type => type.FullName)
            .ToArray();
    }

    public Type Find()
    {
        IReadOnlyList<Type> matches = FindAll();
        string criteria = _descriptions.Count == 0 ? "the supplied criteria" : string.Join(", ", _descriptions);

        Assert.True(matches.Count > 0, $"No Razor PageModel was found with {criteria}.");
        Assert.True(
            matches.Count == 1,
            $"Expected one Razor PageModel with {criteria}, but found {matches.Count}: " +
            string.Join(", ", matches.Select(type => type.Name)));

        return matches[0];
    }
}
