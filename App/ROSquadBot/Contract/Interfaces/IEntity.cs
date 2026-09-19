namespace ROSquadBot.Contract.Interfaces
{
    /// <summary>
    /// Общий интерфейс моделей
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        Guid Id { get; set; }
    }
}
