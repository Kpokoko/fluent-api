namespace ObjectPrinting;

public static class PropertyPrintingConfigExtensions
{
    public static PrintingConfig<TOwner> SetSerializationStyle<TOwner, TPropType>
        (this PropertyPrintingConfig<TOwner, TPropType> propertyConfig, Serializer serializer)
    {
        var propertyName = propertyConfig.PropertyName;
        return propertyConfig.ParentConfig.WithPropertySerialization(propertyName, serializer);
    }

    public static PrintingConfig<TOwner> TrimStringToLength<TOwner>
        (this PropertyPrintingConfig<TOwner, string> propertyConfig, int length)
    {
        return propertyConfig.ParentConfig.TrimStringToLength(propertyConfig.PropertySelector!, length);
    }

    public static PrintingConfig<TOwner> Exclude<TOwner, TPropType>
        (this PropertyPrintingConfig<TOwner, TPropType> propertyConfig)
    {
        var propertyName = propertyConfig.PropertyName;
        return propertyConfig.ParentConfig.Exclude(propertyName);
    }
}