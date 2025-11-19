using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ObjectPrinting;

public interface ISerializer
{
    public Func<object?, int, int, string> SerializerFunc { get; }
    public Func<object?, int, int, string> GetSerializerFunc => SerializerFunc;
}

public class Serializer : ISerializer
{
    public Func<object?, int, int, string> SerializerFunc { get; }
    public Func<object?, int, int, string> GetSerializeFunc => SerializerFunc;

    public Serializer(Func<object?, int, int, string> serializerFunc)
    {
        this.SerializerFunc = serializerFunc;
    }
}