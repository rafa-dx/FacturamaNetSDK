using System.Reflection;
using System.Text.Json.Serialization;

namespace FacturamaNetSDK.Utilities
{
    internal static class QueryBuilder
    {
        public static Dictionary<string, string?> FromObject(object obj)
        {
            var dictionary = new Dictionary<string, string?>();

            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = property.GetValue(obj);
                if (value == null)
                {
                    continue;
                }
                dictionary[ParameterName(property)] = value.ToString();
            }
            return dictionary;
        }

        private static string ParameterName(PropertyInfo property) =>
            property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
    }
}
