using ROTGBot.Db.Attributes;

namespace ROTGBot.Db.Model
{
    [TableName("commanddata")]
    public class CommandData : Entity
    {
        [ColumnName("commandid")]
        public Guid CommandId { get; set; }        

        [ColumnName("TextValue")]
        public string TextValue { get; set; }

        [ColumnName("messageid")]
        public long MessageId { get; set; }
        
        [ColumnName("adddate")]
        [ColumnType("timestamp")]
        public DateTime AddDate { get; set; }
    }
}