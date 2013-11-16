using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Web.UI.WebControls;

namespace System.Web.UI.WebControls 
{
    public static class GenericUtility
    {
        public static T ConvertTo<T>(this object value)
        {
            return (T)ConvertTo(value, typeof(T));
        }
             
        public static object ConvertTo(this object value, Type type)
        {
            object theResult = default(Type);
            IFormatProvider formatter = null;

            Type valueType = type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                valueType = type.GetGenericArguments()[0];
            }

            if (valueType == typeof(DateTime))
            {
                formatter = CultureInfo.CurrentCulture.DateTimeFormat;
            }
            else
            {
                formatter = CultureInfo.CurrentCulture.NumberFormat;
            }
            
            if (value != null && string.IsNullOrEmpty(value.ToString()) == false)
            {
                Converter<object, object> convert = null;
                if (valueType == value.GetType())
                {
                    convert = p => p;
                }
                else if (valueType == typeof(Guid))
                {
                    convert = p => new Guid(p.ToString());
                }
                else if (valueType.BaseType == typeof(Enum))
                {
                    convert = p => Enum.Parse(valueType, p.ToString(), true);
                }
                /* else if (valueType == typeof(System.Data.Linq.Binary) && value is byte[])
                {
                    convert = p => new System.Data.Linq.Binary((byte[])p);
                } */
                else if (valueType == typeof(TimeSpan))
                {
                    convert = p => TimeSpan.Parse(p.ToString());
                }
                else
                {
                    convert = p => Convert.ChangeType(p, valueType, formatter);
                }

                theResult = convert(value);

            }

            return theResult;
        }

        public static void BuildObject<T>(this IDictionary values, T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "Object to be setup must not be NULL");
            }

            ParameterExpression paramExp = Expression.Parameter(typeof(T), typeof(T).Name);

            foreach (var key in values.Keys)
            {
                if (values[key] == null)
                {
                    continue;
                }

                MemberInfo[] members = obj.GetType().GetMember(key.ToString());

                if (members != null && members.Length == 1)
                {
                    var member = members[0];
                    if (member.MemberType == MemberTypes.Property)
                    {
                        var property = member as PropertyInfo;
                        if (property.CanWrite)
                        {
                            var value = ConvertTo(values[key], property.PropertyType);
                            property.SetValue(obj, value, new object[] { });
                        }
                    }
                    else if (member.MemberType == MemberTypes.Field)
                    {
                        var field = member as FieldInfo;
                        if (field.IsInitOnly == false)
                        {
                            var value = ConvertTo(values[key], field.FieldType);
                            field.SetValue(obj, value);
                        }
                    }
                }
            }
        }

        public static IQueryable Sort(this IQueryable query, string sortExpression)
        {
            var theResult = query;
            if (!string.IsNullOrEmpty(sortExpression))
            {
                theResult = query.OrderBy(sortExpression);
            }

            return theResult;
        }

        public static IQueryable Page(this IQueryable query, int startRowIndex, int maximumRows)
        {
            var theResult = query;
            if (maximumRows > 0)
            {
                theResult = query.Skip(startRowIndex).Take(maximumRows);
            }

            return theResult;
        }

    }
}
