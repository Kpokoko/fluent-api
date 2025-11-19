using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly Expression<Func<TOwner, TPropType>>? propertySelector;

    public PropertyPrintingConfig(PrintingConfig<TOwner> config, Expression<Func<TOwner, TPropType>>? propertySelector)
    {
        this.config = config;
        this.propertySelector = ValidateExpression(propertySelector);
    }

    private Expression<Func<TOwner, TPropType>> ValidateExpression(Expression<Func<TOwner, TPropType>>? propertySelector)
    {
        if (propertySelector == null)
            throw new ArgumentNullException(nameof(propertySelector));
        if (propertySelector.Body is not MemberExpression memberExpression)
            throw new ArgumentException($"{nameof(propertySelector)} must select property");
        return propertySelector;
    }
    
    public PrintingConfig<TOwner> ParentConfig => config;
    public Expression<Func<TOwner, TPropType>>? PropertySelector => propertySelector;
    
    public string PropertyName => ((MemberExpression)propertySelector!.Body).Member.Name;
}