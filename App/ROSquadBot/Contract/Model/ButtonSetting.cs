namespace ROSquadBot.Contract.Model
{
    /// <summary>
    /// Настройки пользовательских кнопок
    /// </summary>
    public class ButtonSetting
    {
        /// <summary>
        /// Номер
        /// </summary>
        public int? Number { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Родитель
        /// </summary>
        public int? Parent { get; set; }

        /// <summary>
        /// Является родителем
        /// </summary>
        public bool IsParent { get; set; }

        /// <summary>
        /// Управляемая (модерируемая)
        /// </summary>
        public bool IsModerate { get; set; }
    }
}
