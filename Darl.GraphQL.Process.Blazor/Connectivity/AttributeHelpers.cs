using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Darl.GraphQL.Process.Blazor.Connectivity
{
    public static class AttributeHelpers
    {

        public static int GetMaxLength<T>(Expression<Func<T, string>> propertyExpression)
        {
            return GetPropertyAttributeValue<T, string, MaxLengthAttribute, int>(propertyExpression, attr => attr.Length);
        }

        //Optional Extension method
        public static int GetMaxLength<T>(this T instance, Expression<Func<T, string>> propertyExpression)
        {
            return GetMaxLength(propertyExpression);
        }


        //Required generic method to get any property attribute from any class
        public static TValue GetPropertyAttributeValue<T, TOut, TAttribute, TValue>(Expression<Func<T, TOut>> propertyExpression, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = (PropertyInfo)expression.Member;
            var attr = propertyInfo.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (attr == null)
            {
                return default;
            }

            return valueSelector(attr);
        }
    }
}
