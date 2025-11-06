using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public readonly IEnumerable<Type> ExcludedTypes;
        public readonly Dictionary<Type, ISerializer> CustomSerializers;
        public PrintingConfig() : this(
            new List<Type>(),
            new Dictionary<Type, ISerializer>())
        {}

        private PrintingConfig(IEnumerable<Type> excludedTypes, Dictionary<Type, ISerializer> customSerializers)
        {
            this.ExcludedTypes = excludedTypes;
            this.CustomSerializers = customSerializers;
        }

        public PrintingConfig<TOwner> ExcludePropertyOfType<T>()
        {
            return new PrintingConfig<TOwner>(ExcludedTypes.Append(typeof(T)), CustomSerializers);
        }

        public PrintingConfig<TOwner> WithTypeSerializtionType<T>(ISerializer serializer)
        {
            var tempDict = new Dictionary<Type, ISerializer>(CustomSerializers);
            tempDict.Add(typeof(T), serializer);
            return new PrintingConfig<TOwner>(ExcludedTypes, tempDict);
        }
        
        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            return GetSerializerForType(type).Serialize(obj, nestingLevel, this);
        }

        public ISerializer GetSerializerForType(Type type)
        {
            if (CustomSerializers.ContainsKey(type))
                return CustomSerializers[type];
            return new Serializer();
        }
    }
}