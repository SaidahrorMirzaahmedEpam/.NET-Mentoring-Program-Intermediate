using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionTrees.Task2.ExpressionMapping
{
    /// <summary>
    /// Builds a compiled <see cref="Mapper{TSource, TDestination}"/> that copies data from a
    /// source object into a freshly created destination object.
    ///
    /// By default every public, writable destination property is bound to the source property
    /// with the same name. When the two properties differ in data type but the value is
    /// convertible (e.g. <c>int</c> to <c>long</c>), a conversion is inserted automatically.
    ///
    /// Fields that differ in name and/or data type can be configured explicitly with
    /// <see cref="ForMember{TMember}"/>. An explicit configuration always wins over the
    /// by-name convention.
    /// </summary>
    public class MappingGenerator<TSource, TDestination>
    {
        // destination property name -> lambda producing its value from the source object.
        private readonly Dictionary<string, LambdaExpression> _customMappings =
            new Dictionary<string, LambdaExpression>();

        /// <summary>
        /// Customizes how a single destination member is filled, allowing the source member to
        /// have a different name and/or a different data type.
        /// </summary>
        /// <typeparam name="TMember">Type of the destination member.</typeparam>
        /// <param name="destinationMember">Selector of the destination member, e.g. <c>dst =&gt; dst.FullName</c>.</param>
        /// <param name="sourceValue">
        /// Expression producing the value from the source, e.g.
        /// <c>src =&gt; src.FirstName + " " + src.LastName</c> or <c>src =&gt; src.Count.ToString()</c>.
        /// </param>
        public MappingGenerator<TSource, TDestination> ForMember<TMember>(
            Expression<Func<TDestination, TMember>> destinationMember,
            Expression<Func<TSource, TMember>> sourceValue)
        {
            if (destinationMember == null) throw new ArgumentNullException(nameof(destinationMember));
            if (sourceValue == null) throw new ArgumentNullException(nameof(sourceValue));

            _customMappings[GetMemberName(destinationMember)] = sourceValue;
            return this;
        }

        public Mapper<TSource, TDestination> Generate()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "src");

            var bindings = new List<MemberBinding>();
            foreach (var destProp in GetWritableProperties(typeof(TDestination)))
            {
                var value = BuildValueExpression(destProp, sourceParam);
                if (value != null)
                {
                    bindings.Add(Expression.Bind(destProp, value));
                }
            }

            // () => new TDestination { Prop1 = ..., Prop2 = ... }
            var body = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);

            return new Mapper<TSource, TDestination>(lambda.Compile());
        }

        private Expression BuildValueExpression(PropertyInfo destProp, ParameterExpression sourceParam)
        {
            // 1. Explicit configuration wins; inline its body using the shared source parameter.
            if (_customMappings.TryGetValue(destProp.Name, out var custom))
            {
                return ParameterRebinder.ReplaceParameter(custom, sourceParam);
            }

            // 2. Convention: bind to the source property with the same name.
            var sourceProp = typeof(TSource).GetProperty(
                destProp.Name, BindingFlags.Public | BindingFlags.Instance);
            if (sourceProp == null || !sourceProp.CanRead)
            {
                return null;
            }

            Expression value = Expression.Property(sourceParam, sourceProp);
            if (!destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
            {
                // Same name, different but convertible data type (e.g. int -> long).
                value = Expression.Convert(value, destProp.PropertyType);
            }

            return value;
        }

        private static IEnumerable<PropertyInfo> GetWritableProperties(Type type)
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0);
        }

        private static string GetMemberName<TMember>(Expression<Func<TDestination, TMember>> selector)
        {
            var body = selector.Body;

            // Unwrap a Convert(...) that the compiler inserts when boxing value types.
            if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                body = unary.Operand;
            }

            if (body is MemberExpression member && member.Member is PropertyInfo)
            {
                return member.Member.Name;
            }

            throw new ArgumentException(
                "Destination selector must be a property access expression, e.g. dst => dst.Name.",
                nameof(selector));
        }

        /// <summary>
        /// Rewrites the single parameter of a configured lambda so its body can be embedded
        /// inside the generated mapping expression that uses a shared source parameter.
        /// </summary>
        private sealed class ParameterRebinder : ExpressionVisitor
        {
            private readonly ParameterExpression _from;
            private readonly Expression _to;

            private ParameterRebinder(ParameterExpression from, Expression to)
            {
                _from = from;
                _to = to;
            }

            public static Expression ReplaceParameter(LambdaExpression lambda, Expression replacement)
            {
                var parameter = lambda.Parameters[0];
                return new ParameterRebinder(parameter, replacement).Visit(lambda.Body);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _from ? _to : base.VisitParameter(node);
            }
        }
    }
}
