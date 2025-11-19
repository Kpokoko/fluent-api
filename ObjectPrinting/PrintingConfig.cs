using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly IReadOnlySet<Type> excludedTypes;
        private readonly IReadOnlyDictionary<Type, ISerializer> typeSerializers;
        private readonly IReadOnlySet<object> serializedObjects;
        private readonly IReadOnlyDictionary<Type, CultureInfo> customCultureInfos;
        private readonly IReadOnlyDictionary<string, ISerializer> propertySerializers;
        private readonly IReadOnlyDictionary<string, int> maxStringLength;
        private readonly IReadOnlySet<string> excludedProperties;
        
        public PrintingConfig() : this(
            new HashSet<Type>(),
            new Dictionary<Type, ISerializer>(),
            new HashSet<object>(ReferenceEqualityComparer.Instance),
            new Dictionary<Type, CultureInfo>(),
            new Dictionary<string, ISerializer>(),
            new Dictionary<string, int>(),
            new HashSet<string>())
        {}

        private PrintingConfig(
            IReadOnlySet<Type> excludedTypes,
            IReadOnlyDictionary<Type, ISerializer> typeSerializers,
            IReadOnlySet<object> serializedObjects,
            IReadOnlyDictionary<Type, CultureInfo> customCultureInfos,
            IReadOnlyDictionary<string, ISerializer> propertySerializers,
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
            var tempSet = new HashSet<Type>(excludedTypes);
            tempSet.Add(typeof(T));
            return new PrintingConfig<TOwner>(tempSet,
                typeSerializers,
                serializedObjects,
                customCultureInfos,
                propertySerializers,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> WithTypeSerializtionType<T>(ISerializer serializer)
        {
            var tempDict = new Dictionary<Type, ISerializer>(typeSerializers);
            tempDict.Add(typeof(T), serializer);
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
            tempDict.Add(typeof(T), cultureInfo);
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                serializedObjects,
                tempDict,
                propertySerializers,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> WithPropertySerialization(string propertyName, ISerializer serializer)
        {
            var tempDict = new Dictionary<string, ISerializer>(propertySerializers);
            tempDict.Add(propertyName, serializer);
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                serializedObjects,
                customCultureInfos,
                tempDict,
                maxStringLength,
                excludedProperties);
        }

        public PrintingConfig<TOwner> TrimStringToLength(string propertyName, int length)
        {
            var tempDict = new Dictionary<string, int>(maxStringLength);
            tempDict.Add(propertyName, length);
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
            tempSet.Add(propertyName);
            return new PrintingConfig<TOwner>(excludedTypes,
                typeSerializers,
                serializedObjects,
                customCultureInfos,
                propertySerializers,
                maxStringLength,
                tempSet);
        }

        public PropertyPrintingConfig<TOwner, TPropType> GetProperty<TPropType>(
            Expression<Func<TOwner, TPropType>> targetProperty)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, targetProperty);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Exclude<TPropType>(
            Expression<Func<TOwner, TPropType>> targetProperty)
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this, targetProperty);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj is null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            return GetSerializerForType(type).Serialize(obj, nestingLevel, this);
        }

        public ISerializer GetSerializerForType(Type type)
        {
            if (typeSerializers.ContainsKey(type))
                return typeSerializers[type];
            return new Serializer();
        }
        
        public bool IsPropertyNeedsUniqueSerialization(string propertyName, out ISerializer serializer)
        {
            serializer = null;
            if (propertySerializers.ContainsKey(propertyName))
            {
                serializer = propertySerializers[propertyName];
                return true;
            }
            return false;
        }

        public bool IsPropertyNeedsTrim(string propertyName, out int trimLength)
        {
            trimLength = 0;
            if (maxStringLength.ContainsKey(propertyName))
            {
                trimLength = maxStringLength[propertyName];
                return true;
            }
            return false;
        }
    }
}