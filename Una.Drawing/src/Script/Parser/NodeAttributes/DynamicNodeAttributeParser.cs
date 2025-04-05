using System.Linq;
using System.Reflection;

namespace Una.Drawing.Script.Parser;

internal class DynamicNodeAttributeParser : INodeAttributeParser
{
    public bool Apply(Node node, string name, dynamic value)
    {
        PropertyInfo? property = FindProperty(node, name);
        if (property == null) return false;
        
        if (!property.CanWrite) {
            throw new Exception($"Property '{name}' on node '{node.GetType().Name}' is read-only.");
        };
        
        Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        
        if (propertyType.IsEnum) {
            // Convert the value to the enum type.
            value = Enum.Parse(propertyType, value.ToString().Replace("-", ""), true);
        } else if (propertyType == typeof(Color)) {
            // Convert the value to a Color.
            value = new Color(value as string ?? (value is uint ui ? ui : value));
        } else if (propertyType == typeof(float)) {
            // Convert the value to a float.
            value = float.Parse(value.ToString());
        } else if (propertyType == typeof(int)) {
            // Convert the value to an int.
            value = int.Parse(value.ToString());
        } else if (propertyType == typeof(bool)) {
            // Convert the value to a bool.
            value = bool.Parse(value.ToString());
        }
        
        try {
            property.SetValue(node, value);
        } catch (ArgumentException ex) {
            throw new Exception($"Failed to set property '{name}' on node '{node.GetType().Name}': {ex.Message}");
        } catch (TargetInvocationException ex) {
            throw new Exception($"Failed to set property '{name}' on node '{node.GetType().Name}': {ex.InnerException?.Message}");
        }
        
        return true;
    }

    private static PropertyInfo? FindProperty(Node node, string name)
    {
        Type type = node.GetType();
        
        PropertyInfo? property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        if (property != null) return property;
        
        // Allow kebab-case names (e.g. "border-radius" to "borderradius").
        name = name.Replace("-", "").ToLowerInvariant();

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .FirstOrDefault(prop => prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}