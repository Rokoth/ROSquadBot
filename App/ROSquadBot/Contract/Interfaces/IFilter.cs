using ROTGBot.Contract.Model;

namespace ROSquadBot.Contract.Interfaces
{
    /// <summary>
    /// интерфейс фильтра
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFilter<T> where T : Entity
    {
        /// <summary>
        /// Страница
        /// </summary>
        int? Page { get; }
        /// <summary>
        /// Размер
        /// </summary>
        int? Size { get; }
        /// <summary>
        /// Поле сортировки
        /// </summary>
        string Sort { get; }
    }
}
