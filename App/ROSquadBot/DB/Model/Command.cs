using ROTGBot.Db.Attributes;

namespace ROTGBot.Db.Model
{
    [TableName("command")]
    public class Command : Entity
    {
        [ColumnName("userid")]
        public Guid UserId { get; set; }
        [ColumnName("commandtype")]
        public int CommandType { get; set; }
        [ColumnName("isactive")]
        public bool IsActive { get; set; }       

        [ColumnName("adddate")]
        [ColumnType("timestamp")]
        public DateTime AddDate { get; set; }
    }
}