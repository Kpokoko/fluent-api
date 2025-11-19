using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly IReadOnlySet<Type> excludedTypes;
        private readonly IReadOnlyDictionary<Type, Serializer> typeSerializers;
        private readonly HashSet<object> serializedObjects;
        private readonly IReadOnlyDictionary<Type, CultureInfo> customCultureInfos;
        private readonly IReadOnlyDictionary<string, Serializer> propertySerializers;
        private readonly IReadOnlyDictionary<string, int> maxStringLength;
        private readonly IReadOnlySet<string> excludedProperties;
        private const int MaxDeepnessLevel = 2;
        private const char Tabchar = '\t';
        private const char ResetCaretChar = '\r';
        private const char NewLineChar = '\n';
        private readonly string environmentNewLine = Environment.NewLine;
        private const string EqualityWithSpaces = " = ";

        private static HashSet<Type> _finalTypes =
        [
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
            typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(double), typeof(string), typeof(bool),
            typeof(decimal), typeof(char), typeof(string), typeof(Guid),
            typeof(DateTime), typeof(TimeSpan)
        ];
        
        public PrintingConfig() : this(
            new HashSet<Type>(),
            new Dictionary<Type, Serializer>(),
            new HashSet<object>(ReferenceEqualityComparer.Instance),
            new Dictionary<Type, CultureInfo>(),
            new Dictionary<string, Serializer>(),
            new Dictionary<string, int>(),
            new HashSet<string>())
        {}

        private PrintingConfig(
            IReadOnlySet<Type> excludedTypes,
            IReadOnlyDictionary<Type, Serializer> typeSerializers,
            HashSet<object> serializedObjects,
            IReadOnlyDictionary<Type, CultureInfo> customCultureInfos,
            IReadOnlyDictionary<string, Serializer> propertySerializers,
            IReadOnlyDictionary<string, int> maxStringLength,
            IReadOnlySet<string> excludedProperties)
        {
            this.excludedTypes = excludedTypes;
            this.typeSerializers = typeSerializers;
            this.serializedObjects = serializedObjects;
            this.customCultureInfos = customCultureInfos;
            this.propertySerializers = propertySerializers;
            this.maxStringLength = maxStringLength;
            this.excludedProperties = excludedProperties;
        }

        public PrintingConfig<TOwner> ExcludePropertyOfType<T>()
        {
            if (excludedTypes.Contains(typeof(T)))
                throw new InvalidOperationException($"The type {typeof(T)} is already excluded.");
            var tempSet = new HashSet<Type>(excludedTypes) { typeof(T) };
            return new PrintingConfig<TOwner>(tempSet,
                typeSerializers,
                serializedObjects,
                customCultureInfos,
                propertySerializers,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> WithTypeSerializationStyle<T>(Serializer serializer)
        {
            var tempDict = new Dictionary<Type, Serializer>(typeSerializers);
            if (!tempDict.TryAdd(typeof(T), serializer))
                throw new InvalidOperationException($"Type {typeof(T).FullName} has already been set serialization type");
            return new PrintingConfig<TOwner>(excludedTypes,
                tempDict,
                serializedObjects,
                customCultureInfos,
                propertySerializers,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> SetTypeCulture<T>(CultureInfo cultureInfo)
            where T : IFormattable
        {
            var tempDict = new Dictionary<Type, CultureInfo>(customCultureInfos);
            if (!tempDict.TryAdd(typeof(T), cultureInfo))
                throw new InvalidOperationException($"Type {typeof(T).FullName} has already been set culture info");
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                serializedObjects,
                tempDict,
                propertySerializers,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> WithPropertySerialization(string propertyName, Serializer serializer)
        {
            var tempDict = new Dictionary<string, Serializer>(propertySerializers);
            if (!tempDict.TryAdd(propertyName, serializer))
                throw new InvalidOperationException($"Property {propertyName} has already been set serialization type");
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                serializedObjects,
                customCultureInfos,
                tempDict,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> TrimStringToLength(Expression<Func<TOwner, string>> propertyExpr, int length)
        {
            var member = (MemberExpression)propertyExpr.Body;
            var tempDict = new Dictionary<string, int>(maxStringLength);
            if (!tempDict.TryAdd(member.Member.Name, length))
                throw new InvalidOperationException($"Property {member.Member.Name} has already been trim length");
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                serializedObjects,
                customCultureInfos,
                propertySerializers,
                tempDict,
                excludedProperties);
        }

        public PrintingConfig<TOwner> Exclude(string propertyName)
        {
            var tempSet = new HashSet<string>(excludedProperties);
            if (!tempSet.Add(propertyName))
                throw new InvalidOperationException($"Property {propertyName} has already been excluded");
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                serializedObjects,
                customCultureInfos,
                propertySerializers,
                maxStringLength,
                tempSet);
        }

        public PropertyPrintingConfig<TOwner, TPropType> GetPropertyByName<TPropType>(
            Expression<Func<TOwner, TPropType>> targetProperty)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, targetProperty);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object? obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj is null)
                return "null" + environmentNewLine;

            var type = obj.GetType();
            return GetSerializerForType(type).SerializerFunc.Invoke(obj, nestingLevel, 0);
        }
        
        private string Serialize(object? obj, int nestingLevel, int deepnessLevel)
        {
            if (obj is null)
                return "null" + environmentNewLine;
            var type = obj.GetType();
            if (TrySerializeAsBaseFields(obj, type, out var serialized) && serialized is not null)
                return serialized;

            var sb = new StringBuilder();
            sb.Append(SerializeComplexField(type, deepnessLevel, obj,  nestingLevel));
            return sb.ToString();
        }

        private bool TrySerializeAsBaseFields(object obj, Type type, out string? result)
        {
            result = null;
            if (customCultureInfos.TryGetValue(type, out var info))
            {
                result = obj.ToString();
                result = (obj as IFormattable)?.ToString(null, info) + environmentNewLine;
                return true;
            }
            if (_finalTypes.Contains(type))
            {
                result = obj.ToString() + environmentNewLine;
                return true;
            }
            return false;
        }

        private string SerializeComplexField(Type type, int deepnessLevel, object obj, int nestingLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine(type.Name);
            var properties = type.GetProperties()
                .Where(x => !excludedTypes.Contains(x.PropertyType)
                            && !excludedProperties.Contains(x.Name));
            foreach (var propertyInfo in properties)
            {
                if (deepnessLevel == MaxDeepnessLevel)
                    return $"Deepness exceeded!" + environmentNewLine;
                sb.Append(SerializeProperty(propertyInfo, obj, nestingLevel, deepnessLevel));
            }
            return sb.ToString();
        }

        private string SerializeProperty(PropertyInfo propertyInfo, object obj, int nestingLevel, int deepnessLevel)
        {
            var sb = new StringBuilder();
            if (IsPropertyNeedsUniqueSerialization(propertyInfo.Name, out var serializer))
            {
                sb.Append(BuildSerializedField(propertyInfo, serializer, obj, nestingLevel, deepnessLevel));
                return sb.ToString();
            }
            if (IsPropertyNeedsTrim(propertyInfo.Name, out var trimLen))
            {
                var identation = new string(Tabchar, nestingLevel + 1);
                var serializedInfo = BuildSerializedField(propertyInfo, null, obj, nestingLevel, deepnessLevel);
                var dataPrefixLength = identation.Length + propertyInfo.Name.Length + EqualityWithSpaces.Length;
                serializedInfo = serializedInfo.Substring(0, dataPrefixLength + trimLen)
                    .TrimEnd(new char[] {ResetCaretChar, NewLineChar, Tabchar});
                
                sb.Append(serializedInfo + ResetCaretChar + NewLineChar);
                return sb.ToString();
            }
            sb.Append(BuildSerializedField(propertyInfo, null, obj, nestingLevel, deepnessLevel));
            return sb.ToString();
        }

        private string BuildSerializedField(PropertyInfo propertyInfo, Serializer? serializer,
            object? obj, int nestingLevel, int deepnessLevel)
        {
            var identation = new string(Tabchar, nestingLevel + 1);
            var sb = new StringBuilder();
            sb.Append(identation + propertyInfo.Name + EqualityWithSpaces);
            if (serializer is null)
                serializer = GetSerializerForType(propertyInfo.PropertyType);
            sb.Append(serializer.SerializerFunc.Invoke(propertyInfo.GetValue(obj), nestingLevel + 1, deepnessLevel + 1));
            return sb.ToString();
        }

        private Serializer GetSerializerForType(Type type)
        {
            if (typeSerializers.TryGetValue(type, out var forType))
                return forType;
            return new Serializer(Serialize);
        }
        
        private bool IsPropertyNeedsUniqueSerialization(string propertyName, out Serializer? serializer)
        {
            serializer = null;
            if (propertySerializers.TryGetValue(propertyName, out var propertySerializer))
            {
                serializer = propertySerializer;
                return true;
            }
            return false;
        }

        private bool IsPropertyNeedsTrim(string propertyName, out int trimLength)
        {
            trimLength = 0;
            if (maxStringLength.TryGetValue(propertyName, out var value))
            {
                trimLength = value;
                return true;
            }
            return false;
        }
    }
}