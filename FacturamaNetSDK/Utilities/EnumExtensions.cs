using System.Runtime.Serialization;
using System.Reflection;

namespace FacturamaNetSDK.Utilities;

public static class EnumExtensions
{
    public static string ToApiValue<T>(this T value) where T : Enum
    {
        return typeof(T)
            .GetField(value.ToString())
            ?.GetCustomAttribute<EnumMemberAttribute>()
            ?.Value ?? value.ToString();
    }
}