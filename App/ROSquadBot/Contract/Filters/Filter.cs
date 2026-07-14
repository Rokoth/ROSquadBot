using ROSquadBot.Contract.Interfaces;
using ROTGBot.Contract.Model;

namespace ROSquadBot.Contract.Filters
{
    /// <summary>
    /// Фильтр для списка Entity
    /// </summary>
    /// <param name="size">Page size</param>
    /// <param name="page">Page number</param>
    /// <param name="sort">Sort field</param>
    public abstract class Filter<T>(int? size = null, int? page = null, string? sort = null) : IFilter<T> where T : Entity
    {
        /// <summary>
        /// Page size
        /// </summary>
        public int Size { get; } = size ?? Constants.DefaultFilterSize;
        /// <summary>
        /// Page number
        /// </summary>
        public int Page { get; } = page ?? 0;
        /// <summary>
        /// Sort field
        /// </summary>
        public string Sort { get; } = sort ?? Constants.DefaultFilterSort;
    }
}
