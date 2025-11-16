using System;
using System.Globalization;
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
        if (obj is null)
            return "null" + Environment.NewLine;
        if (!config.SerializedObjects.Add(obj))
            return "Already serialized!" + Environment.NewLine;
        var type = obj.GetType();
        var finalTypes = new[]
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        if (finalTypes.Contains(type))
        {
            var result = obj.ToString();
            if (config.CustomCultureInfos.ContainsKey(type))
                result = (obj as IFormattable).ToString(null, config.CustomCultureInfos[type]);
            return result + Environment.NewLine;
        }

        var identation = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        sb.AppendLine(type.Name);
        var properties = type.GetProperties()
            .Where(x => !config.ExcludedTypes.Contains(x.PropertyType)
                        && !config.ExcludedProperties.Contains(x.Name));
        foreach (var propertyInfo in properties)
        {
            if (config.IsPropertyNeedsUniqueSerialization(propertyInfo.Name, out var serializer))
            {
                sb.Append(identation + propertyInfo.Name + " = " +
                          serializer.Serialize(propertyInfo.GetValue(obj),
                              nestingLevel + 1, config));
                continue;
            }
            if (config.IsPropertyNeedsTrim(propertyInfo.Name, out var trimmLen))
            {
                var serializedInfo = identation + propertyInfo.Name + " = " +
                                     config.GetSerializerForType(propertyInfo.PropertyType).Serialize(
                                         propertyInfo.GetValue(obj),
                                         nestingLevel + 1, config).Substring(0, trimmLen);
                if (serializedInfo[^1] == '\n')
                    sb.Append(serializedInfo);
                else
                    sb.Append(serializedInfo + '\r' + '\n');
                continue;
            }

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

public class PhoneSerializer : ISerializer
{
    public string Serialize<T>(object obj, int nestingLevel, PrintingConfig<T> config)
    {
        return "my phone" + Environment.NewLine;
    }
}

public class NameSerializer : ISerializer
{
    public string Serialize<T>(object obj, int nestingLevel, PrintingConfig<T> config)
    {
        return "my name" + Environment.NewLine;
    }
}