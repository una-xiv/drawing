using Dalamud.Interface;
using System.Reflection;

namespace Una.Drawing.NodeParser;

internal static class NodeAttributeParser
{
    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache = [];

    internal static void ApplyProperty(dynamic obj, string elementName, string name, string value)
    {
        if (obj == null) throw new Exception("Object cannot be null");
        if (name == "") throw new Exception("Name cannot be empty");

        Type type = obj.GetType();
        name = name.ToLowerInvariant().Replace("-", "").Trim();

        if (false == PropertyCache.TryGetValue(type, out Dictionary<string, PropertyInfo>? properties)) {
            properties = CreateTypePropertyCache(type);
            PropertyCache.Add(type, properties);
        }

        if (false == properties.TryGetValue(name, out PropertyInfo? property)) {
            throw new Exception($"Node type \"{obj.GetType().Name}\" has no public property named \"{name}\".");
        }

        Type propType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        
        if (!(value.StartsWith('{') && value.EndsWith('}'))) {
            if (propType == typeof(FontAwesomeIcon)) {
                if (!Enum.TryParse<FontAwesomeIcon>(value, true, out var icon)) {
                    throw new Exception($"Invalid FontAwesomeIcon: {value}.");
                }
                property.SetValue(obj, icon);
                return;
            }

            try {
                object convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(obj, convertedValue);
            } catch (Exception e) {
                throw new Exception($"Failed to set property \"{name}\" of element \"{elementName}\". {e.InnerException?.Message ?? e.Message} @ {e.InnerException?.StackTrace}");
            }
            
            return;
        }

        value = value[1..^1].Trim();

        if (value == "") {
            throw new Exception(
                $"Empty expression in attribute \"{name}\" of element \"{elementName}\" in UDT \"{obj.UdtFilename}\".");
        }

        dynamic? parsedValue = ExpressionParser.ExpressionParser.Parse(value);

        try {
            property.SetValue(obj, Convert.ChangeType(parsedValue, propType));
        } catch (InvalidCastException) {
            throw new Exception(
                $"Attribute \"{name}\" of element \"{elementName}\" has an invalid type. Expected \"{propType.Name}\", got \"{(parsedValue?.GetType().Name ?? "NULL")}\"."
            );
        } catch (Exception e) {
            throw new Exception(
                $"{e.GetType().Name} Failed to parse attribute \"{name}\" of element \"{elementName}\". {e.Message}"
            );
        }
    }

    internal static void Dispose()
    {
        PropertyCache.Clear();
    }

    private static Dictionary<string, PropertyInfo> CreateTypePropertyCache(Type type)
    {
        Dictionary<string, PropertyInfo> properties = [];

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
            properties.Add(property.Name.ToLowerInvariant(), property);
        }

        return properties;
    }
}