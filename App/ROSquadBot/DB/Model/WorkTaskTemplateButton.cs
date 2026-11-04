using ROTGBot.Db.Attributes;

namespace ROTGBot.Db.Model
{
    [TableName("worktasktemplatebutton")]
    public class WorkTaskTemplateButton : Entity
    {
        [ColumnName("text")]
        public string Text { get; set; } = string.Empty;
        [ColumnName("worktasktemplateid")]
        public Guid WorkTaskTemplateId { get; set; }
        [ColumnName("order")]
        public int Order { get; set; }
    }
}