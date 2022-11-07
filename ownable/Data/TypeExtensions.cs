using System.Reflection;

namespace ownable.Data;

public static class TypeExtensions
{
    public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = true)
        where T : Attribute
    {
        return provider.IsDefined(typeof(T), inherit);
    }
}