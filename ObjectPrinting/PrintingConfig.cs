using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public readonly HashSet<Type> ExcludedTypes;
        public readonly Dictionary<Type, ISerializer> TypeSerializers;
        public readonly HashSet<object> SerializedObjects;
        public readonly Dictionary<Type, CultureInfo> CustomCultureInfos;
        public readonly Dictionary<string, ISerializer> PropertySerializers;
        public readonly Dictionary<string, int> MaxStringLength;
        public readonly HashSet<string> ExcludedProperties;
        public PrintingConfig() : this(
            new HashSet<Type>(),
            new Dictionary<Type, ISerializer>(),
            new (ReferenceEqualityComparer.Instance),
            new Dictionary<Type, CultureInfo>(),
            new Dictionary<string, ISerializer>(),
            new Dictionary<string, int>(),
            new HashSet<string>())
        {}

        private PrintingConfig(
            HashSet<Type> excludedTypes,
            Dictionary<Type, ISerializer> typeSerializers,
            HashSet<object> serializedObjects,
            Dictionary<Type, CultureInfo> customCultureInfos,
            Dictionary<string, ISerializer> propertySerializers,
            Dictionary<string, int> maxStringLength,
            HashSet<string> excludedProperties)
        {
            this.ExcludedTypes = excludedTypes;
            this.TypeSerializers = typeSerializers;
            this.SerializedObjects = serializedObjects;
            this.CustomCultureInfos = customCultureInfos;
            this.PropertySerializers = propertySerializers;
            this.MaxStringLength = maxStringLength;
            this.ExcludedProperties = excludedProperties;
        }

        public PrintingConfig<TOwner> ExcludePropertyOfType<T>()
        {
            var tempSet = new HashSet<Type>(ExcludedTypes);
            tempSet.Add(typeof(T));
            return new PrintingConfig<TOwner>(tempSet,
                TypeSerializers,
                SerializedObjects,
                CustomCultureInfos,
                PropertySerializers,
                MaxStringLength,
                ExcludedProperties);
        }

        public PrintingConfig<TOwner> WithTypeSerializtionType<T>(ISerializer serializer)
        {
            var tempDict = new Dictionary<Type, ISerializer>(TypeSerializers);
            tempDict.Add(typeof(T), serializer);
            return new PrintingConfig<TOwner>(ExcludedTypes,
                tempDict,
                SerializedObjects,
                CustomCultureInfos,
                PropertySerializers,
                MaxStringLength,
                ExcludedProperties);
        }

        public PrintingConfig<TOwner> SetTypeCulture<T>(CultureInfo cultureInfo)
            where T : IFormattable
        {
            var tempDict = new Dictionary<Type, CultureInfo>(CustomCultureInfos);
            tempDict.Add(typeof(T), cultureInfo);
            return new PrintingConfig<TOwner>(ExcludedTypes,
                TypeSerializers,
                SerializedObjects,
                tempDict,
                PropertySerializers,
                MaxStringLength,
                ExcludedProperties);
        }

        public PrintingConfig<TOwner> WithPropertySerialization(string propertyName, ISerializer serializer)
        {
            var tempDict = new Dictionary<string, ISerializer>(PropertySerializers);
            tempDict.Add(propertyName, serializer);
            return new PrintingConfig<TOwner>(ExcludedTypes,
                TypeSerializers,
                SerializedObjects,
                CustomCultureInfos,
                tempDict,
                MaxStringLength,
                ExcludedProperties);
        }

        public PrintingConfig<TOwner> TrimStringToLength(string propertyName, int length)
        {
            var tempDict = new Dictionary<string, int>(MaxStringLength);
            tempDict.Add(propertyName, length);
            return new PrintingConfig<TOwner>(ExcludedTypes,
                TypeSerializers,
                SerializedObjects,
                CustomCultureInfos,
                PropertySerializers,
                tempDict,
                ExcludedProperties);
        }

        public PrintingConfig<TOwner> Exclude(string propertyName)
        {
            var tempSet = new HashSet<string>(ExcludedProperties);
            tempSet.Add(propertyName);
            return new PrintingConfig<TOwner>(ExcludedTypes,
                TypeSerializers,
                SerializedObjects,
                CustomCultureInfos,
                PropertySerializers,
                MaxStringLength,
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
            if (TypeSerializers.ContainsKey(type))
                return TypeSerializers[type];
            return new Serializer();
        }
        
        public bool IsPropertyNeedsUniqueSerialization(string propertyName, out ISerializer serializer)
        {
            serializer = null;
            if (PropertySerializers.ContainsKey(propertyName))
            {
                serializer = PropertySerializers[propertyName];
                return true;
            }
            return false;
        }

        public bool IsPropertyNeedsTrim(string propertyName, out int trimLength)
        {
            trimLength = 0;
            if (MaxStringLength.ContainsKey(propertyName))
            {
                trimLength = MaxStringLength[propertyName];
                return true;
            }
            return false;
        }
    }
}