
using System.Reflection;


namespace FacturamaNetSDK.Utilities
{
    public class QueryBuilder
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
                dictionary[property.Name] = value.ToString();
            }
            return dictionary;

        }
    }
}
