using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DimonSmart.RegexUnitTester.TestAdapter;

internal sealed class RegexSourceResolver
{
    private readonly ConcurrentDictionary<string, Assembly> _assemblies = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, MemberInfo> _members = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, Regex> _regexes = new(StringComparer.Ordinal);

    public Regex Resolve(RegexTestDescriptor descriptor)
    {
        var memberKey = $"{Path.GetFullPath(descriptor.AssemblyPath)}|{descriptor.DeclaringTypeName}|{descriptor.SourceKind}|{descriptor.MemberName}";
        var regexKey = descriptor.SourceKind is RegexSourceKind.StringField or RegexSourceKind.StringProperty
            ? $"{memberKey}|{descriptor.RegexOptionsValue}"
            : memberKey;

        return _regexes.GetOrAdd(regexKey, _ => ResolveUncached(descriptor, memberKey));
    }

    private Regex ResolveUncached(RegexTestDescriptor descriptor, string memberKey)
    {
        var assembly = _assemblies.GetOrAdd(Path.GetFullPath(descriptor.AssemblyPath), Assembly.LoadFrom);
        var type = assembly.GetType(descriptor.DeclaringTypeName, throwOnError: false)
            ?? throw new InvalidOperationException($"Declaring type '{descriptor.DeclaringTypeName}' was not found.");
        var member = _members.GetOrAdd(memberKey, _ => FindMember(type, descriptor));

        try
        {
            return descriptor.SourceKind switch
            {
                RegexSourceKind.StringField => new Regex((string?)((FieldInfo)member).GetValue(null)
                    ?? throw NullMember(descriptor), (RegexOptions)descriptor.RegexOptionsValue),
                RegexSourceKind.StringProperty => new Regex((string?)((PropertyInfo)member).GetValue(null)
                    ?? throw NullMember(descriptor), (RegexOptions)descriptor.RegexOptionsValue),
                RegexSourceKind.RegexField => (Regex?)((FieldInfo)member).GetValue(null) ?? throw NullMember(descriptor),
                RegexSourceKind.RegexProperty => (Regex?)((PropertyInfo)member).GetValue(null) ?? throw NullMember(descriptor),
                RegexSourceKind.RegexMethod => (Regex?)((MethodInfo)member).Invoke(null, null) ?? throw NullMember(descriptor),
                _ => throw new InvalidOperationException($"Unsupported regex source '{descriptor.SourceKind}'.")
            };
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            throw new InvalidOperationException($"Regex member '{descriptor.DeclaringTypeName}.{descriptor.MemberName}' threw {exception.InnerException.GetType().FullName}: {exception.InnerException.Message}", exception.InnerException);
        }
    }

    private static MemberInfo FindMember(Type type, RegexTestDescriptor descriptor)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        return descriptor.SourceKind switch
        {
            RegexSourceKind.StringField or RegexSourceKind.RegexField => type.GetField(descriptor.MemberName, flags)
                ?? throw MissingMember(descriptor),
            RegexSourceKind.StringProperty or RegexSourceKind.RegexProperty => type.GetProperty(descriptor.MemberName, flags)
                ?? throw MissingMember(descriptor),
            RegexSourceKind.RegexMethod => type.GetMethod(descriptor.MemberName, flags)
                ?? throw MissingMember(descriptor),
            _ => throw MissingMember(descriptor)
        };
    }

    private static InvalidOperationException MissingMember(RegexTestDescriptor descriptor) =>
        new($"Regex member '{descriptor.DeclaringTypeName}.{descriptor.MemberName}' was not found.");

    private static InvalidOperationException NullMember(RegexTestDescriptor descriptor) =>
        new($"Regex member '{descriptor.DeclaringTypeName}.{descriptor.MemberName}' returned null.");
}
