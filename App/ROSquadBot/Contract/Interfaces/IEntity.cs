namespace ROSquadBot.Contract.Interfaces
{
    /// <summary>
    /// Общий интерфейс моделей  данных
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        Guid Id { get; set; }
    }
}
