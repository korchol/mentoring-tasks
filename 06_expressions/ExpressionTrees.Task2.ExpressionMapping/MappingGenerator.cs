using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTrees.Task2.ExpressionMapping
{
    public class MappingGenerator
    {
        public Mapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            var sourceParam = Expression.Parameter(typeof(TSource));

            var destination = Expression.New(typeof(TDestination));

            List<MemberBinding> bindings = new List<MemberBinding>();

            var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var destinationProperties = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var destProp in destinationProperties)
            {
                var sourceProp = sourceProperties.FirstOrDefault(sp => sp.Name == destProp.Name && sp.PropertyType == destProp.PropertyType);

                if (sourceProp != null)
                {
                    var sourceValue = Expression.Property(sourceParam, sourceProp);

                    var binding = Expression.Bind(destProp, sourceValue);

                    bindings.Add(binding);
                }
            }

            var body = Expression.MemberInit(destination, bindings);

            var mapFunction =
                Expression.Lambda<Func<TSource, TDestination>>(
                    body,
                    sourceParam
                );

            return new Mapper<TSource, TDestination>(mapFunction.Compile());
        }
    }
}
