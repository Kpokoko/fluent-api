using System;
using System.Linq;
using System.Text;

namespace ObjectPrinting;

public interface ISerializer
{
    public string Serialize<T>(object obj, int nestingLevel, PrintingConfig<T> config);
}

public class Serializer : ISerializer
{
    public string Serialize<T>(object obj, int nestingLevel, PrintingConfig<T> config)
    {
        if (obj == null)
            return "null" + Environment.NewLine;
        var type = obj.GetType();
        var finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        if (finalTypes.Contains(type))
        {
            return obj + Environment.NewLine;
        }
        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        var properties = type.GetProperties()
            .Where(x => !config.ExcludedTypes.Contains(x.PropertyType));
        foreach (var propertyInfo in properties)
        {
            sb.Append(identation + propertyInfo.Name + " = " +
                      config.GetSerializerForType(propertyInfo.PropertyType).Serialize(propertyInfo.GetValue(obj),
                          nestingLevel + 1, config));
        }
        return sb.ToString();
    }
}

public class GuidSerializer : ISerializer
{
    public string Serialize<T>(object obj, int nestingLevel, PrintingConfig<T> config)
    {
        return "999999999" + Environment.NewLine;
    }
}

public class MyItem : ISerializer
{
    public string Serialize<T>(object obj, int nestingLevel, PrintingConfig<T> config)
    {
        return "inner serializer" + Environment.NewLine;
    }
}