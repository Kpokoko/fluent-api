using System;
using System.Linq.Expressions;

namespace ObjectPrinting;

public class PropertyPrintingConfig<TOwner, TPropType>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly Expression<Func<TOwner, TPropType>>? selector;

    public PropertyPrintingConfig(PrintingConfig<TOwner> config, Expression<Func<TOwner, TPropType>>? selector)
    {
        this.config = config;
        this.selector = ValidateExpression(selector);
    }

    private Expression<Func<TOwner, TPropType>> ValidateExpression(Expression<Func<TOwner, TPropType>>? validatingSelector)
    {
        if (validatingSelector == null)
            throw new ArgumentNullException(nameof(validatingSelector));
        if (validatingSelector.Body is not MemberExpression memberExpression)
            throw new ArgumentException($"{nameof(validatingSelector)} must select property or field");
        if (!IsDirectPropertyAccess(memberExpression, validatingSelector.Parameters[0]))
            throw new ArgumentException("Property selector should only access properties of the parameter, not external variables");
        return validatingSelector;
    }
    
    private bool IsDirectPropertyAccess(Expression? expression, ParameterExpression parameter)
    {
        while (expression is MemberExpression member)
            expression = member.Expression;
        return expression == parameter;
    }

    
    public PrintingConfig<TOwner> ParentConfig => config;
    public Expression<Func<TOwner, TPropType>>? PropertySelector => selector;
    
    public string PropertyName => ((MemberExpression)selector!.Body).Member.Name;
}