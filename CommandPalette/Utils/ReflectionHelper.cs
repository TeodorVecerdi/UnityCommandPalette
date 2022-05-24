using System;
using System.Linq;
using System.Linq.Expressions;

namespace CommandPalette.Utils {
    public static class ReflectionHelper {
        public static object GetDefaultValue(Type type) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            Expression<Func<object>> e = Expression.Lambda<Func<object>>(Expression.Convert(Expression.Default(type), typeof(object)));
            return e.Compile()();
        }

        public static bool InheritsFrom<T>(this Type type) {
            return type.InheritsFrom(typeof(T));
        }

        public static bool InheritsFrom(this Type type, Type baseType) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (baseType == null) throw new ArgumentNullException(nameof(baseType));

            if (baseType.IsAssignableFrom(type)) return true;
            if (type.IsInterface && !baseType.IsInterface) return false;
            if (baseType.IsInterface) return type.GetInterfaces().Contains(baseType);

            for (Type type1 = type; type1 != null; type1 = type1.BaseType) {
                if (type1 == baseType || baseType.IsGenericTypeDefinition && type1.IsGenericType && type1.GetGenericTypeDefinition() == baseType) {
                    return true;
                }
            }

            return false;
        }
    }
}