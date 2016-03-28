using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;


namespace MacomberMapClient.Integration.ReflectionOrm
{
    public static class PropertyMapping
    {
        private static readonly Dictionary<Type, PropertyMap> PropertyMaps = new Dictionary<Type, PropertyMap>();

        public static PropertyMap GetPropertyMap(this Type type)
        {
            PropertyMap map;
            if (!PropertyMaps.TryGetValue(type, out map))
            {
                map = new PropertyMap(type);
                PropertyMaps[type] = map;
            }

            return map;
        }
    }

    public class PropertyMap
    {
        readonly Dictionary<string, FastProperty> _map = new Dictionary<string, FastProperty>();

        public bool TryGetValue(string name, out FastProperty value)
        {
            bool found = _map.TryGetValue(name, out value);
            // try prefix of _ if not found
            return found || _map.TryGetValue("_" + name, out value);
        }

        public PropertyMap(Type type)
        {
            // foreach (var fieldInfo in type.GetFields())
            // {
            //     if (!_map.ContainsKey(fieldInfo.Name))
            //         _map.Add(fieldInfo.Name, new FastProperty(fieldInfo));
            // }
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!_map.ContainsKey(fieldInfo.Name))
                    _map.Add(fieldInfo.Name, new FastProperty(fieldInfo));
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                if (!_map.ContainsKey(propertyInfo.Name))
                    _map.Add(propertyInfo.Name, new FastProperty(propertyInfo));
            }
        }
    }

    public class FastProperty
    {
        private readonly Func<object, object> _getDelegate;
        private readonly Action<object, object> _setDelegate;

        public string Name { get; }
        public bool IsProperty { get; }
        public MemberInfo Member { get; }

        public Type ReferenceType => IsProperty ? ((PropertyInfo)Member).PropertyType : ((FieldInfo)Member).FieldType;

        public FastProperty(MemberInfo info)
        {
            var property = info as PropertyInfo;
            if (property != null)
            {
                IsProperty = true;
                Name = property.Name;
                Member = property;
                _getDelegate = GetGetProperty<object,object>(property);
                _setDelegate = GetSetProperty<object,object>(property);
                return;
            }

            var field = info as FieldInfo;
            if (field != null)
            {
                IsProperty = false;
                Name = field.Name;
                Member = field;
                _setDelegate = GetSetField<object, object>(field);
                _getDelegate = CreateGetter<object, object>(field);
                return;
            }
        }

        private static Action<object, object> GetSetProperty<TSource, TProp>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite || propertyInfo.SetMethod.IsPrivate) return (s, p) => { ; };
            
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            UnaryExpression instanceCast = (!propertyInfo.DeclaringType.IsValueType) ? Expression.TypeAs(instance, propertyInfo.DeclaringType) : Expression.Convert(instance, propertyInfo.DeclaringType);
            UnaryExpression valueCast = (!propertyInfo.PropertyType.IsValueType) ? Expression.TypeAs(value, propertyInfo.PropertyType) : Expression.Convert(value, propertyInfo.PropertyType);
            return Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, propertyInfo.GetSetMethod(), valueCast), new ParameterExpression[] { instance, value }).Compile();
        }

        private Func<object, object> GetGetProperty<TSource,TValue>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite) return (s) => null;

            var instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = (!propertyInfo.DeclaringType.IsValueType) ? Expression.TypeAs(instance, propertyInfo.DeclaringType) : Expression.Convert(instance, propertyInfo.DeclaringType);
            return Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast, propertyInfo.GetGetMethod()), typeof(object)), instance).Compile();
        }

        private Func<TSource,TValue> GetGetField<TSource,TValue>(FieldInfo fieldInfo)
        {
            //parameter "target", the object on which to set the field `field`
            ParameterExpression targetExp = Expression.Parameter(typeof(TSource), "target");

            //parameter "value" the value to be set in the `field` on "target"
            ParameterExpression valueExp = Expression.Parameter(typeof(object), "value");

            //cast the target from object to its correct type
            Expression castTartgetExp = fieldInfo.DeclaringType.IsValueType
                                            ? Expression.Unbox(targetExp, fieldInfo.DeclaringType)
                                            : Expression.Convert(targetExp, fieldInfo.DeclaringType);

            //cast the value to its correct type
            Expression castValueExp = Expression.Convert(valueExp, fieldInfo.FieldType);

            //the field `field` on "target"
            MemberExpression fieldExp = Expression.Field(castTartgetExp, fieldInfo);

            return Expression.Lambda<Func<TSource, TValue>>(fieldExp, targetExp).Compile();
        }

        static Func<S, T> CreateGetter<S, T>(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, field.ReflectedType, new Type[1] { typeof(S) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Func<S, T>)setterMethod.CreateDelegate(typeof(Func<S, T>));
        }

        private static Action<TSource, TValue> GetSetField<TSource, TValue>(FieldInfo fieldInfo)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            //cast the target from object to its correct type
            UnaryExpression instanceCastBox = fieldInfo.DeclaringType.IsValueType ? Expression.Unbox(instance, fieldInfo.DeclaringType) : Expression.Convert(instance, fieldInfo.DeclaringType);
            UnaryExpression instanceCast   = !fieldInfo.DeclaringType.IsValueType ? Expression.TypeAs(instance, fieldInfo.DeclaringType) : Expression.Convert(instance, fieldInfo.DeclaringType);

            //cast the value to its correct type
            UnaryExpression valueCastShort = Expression.Convert(value, fieldInfo.FieldType);
            UnaryExpression valueCast = !fieldInfo.FieldType.IsValueType ? Expression.TypeAs(value, fieldInfo.FieldType) : Expression.Convert(value, fieldInfo.FieldType);

            //the field `field` on "target"
            MemberExpression fieldExp = fieldInfo.Attributes.HasFlag(FieldAttributes.Static) ? Expression.Field(null, fieldInfo) : Expression.Field(instanceCastBox, fieldInfo);
            //MemberExpression fieldExp = Expression.Field(targetExp, field);

            //assign the "value" to the `field` 
            BinaryExpression assignExp = Expression.Assign(fieldExp, valueCastShort);
            //BinaryExpression assignExp = Expression.Bind(field, )

            //compile the whole thing
            return Expression.Lambda<Action<TSource, TValue>>(assignExp, instance, value).Compile();
        }

        public object Get(object instance)
        {
            return this._getDelegate(instance);
        }

        public void Set(object instance, object value)
        {
            this._setDelegate(instance, value);
        }
    }
}
