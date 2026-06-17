using System;

namespace ExpressionTrees.Task2.ExpressionMapping
{
    /// <summary>
    /// Convenience entry point that builds a <see cref="Mapper{TSource, TDestination}"/>
    /// using only the by-name / convertible-type convention (no custom member configuration).
    /// For custom field mappings use <see cref="MappingGenerator{TSource, TDestination}"/>.
    /// </summary>
    public class MappingGenerator
    {
        public Mapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            return new MappingGenerator<TSource, TDestination>().Generate();
        }
    }
}
