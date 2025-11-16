using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> _config;
    private Expression<Func<TOwner, TPropType>>? _propertySelector;

    public PropertyPrintingConfig(PrintingConfig<TOwner> config, Expression<Func<TOwner, TPropType>>? propertySelector)
    {
        this._config = config;
        this._propertySelector = propertySelector;
    }
    
    public PrintingConfig<TOwner> ParentConfig => _config;
    
    public string PropertyName => ((MemberExpression)_propertySelector.Body).Member.Name;
}