namespace RazorEngine.Templating.Parallel
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a default parallel query plan.
    /// </summary>
    /// <remarks>
    /// The <see cref="DefaultParallelQueryPlan{T}"/> uses the default <see cref="ParallelQuery{T}" />
    /// result. The degree of parallelism by default is <code>Math.Min(ProcessorCount, 64)</code>.
    /// </remarks>
    /// <typeparam name="T">The item type.</typeparam>
    public class DefaultParallelQueryPlan<T> : IParallelQueryPlan<T>
    {
        #region Methods
        /// <summary>
        /// Creates a parallel query for the specified source.
        /// </summary>
        /// <param name="source">The source enumerable.</param>
        /// <returns>The parallel query.</returns>
        public ParallelQuery<T> CreateQuery(IEnumerable<T> source)
        {
            return source.AsParallel().AsOrdered();
        }
        #endregion
    }
}