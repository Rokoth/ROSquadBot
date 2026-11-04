using ROTGBot.Db.Attributes;

namespace ROTGBot.Db.Model
{
    [TableName("worktasktemplate")]
    public class WorkTaskTemplate:  Entity
    {
        [ColumnName("text")]
        public string Text { get; set; } = string.Empty;
        [ColumnName("createdby")]
        public Guid CreatedBy { get; set; }
        [ColumnName("modifiedby")]
        public Guid ModifiedBy { get; set; }
        [ColumnName("createddate")]
        public DateTime CreatedDate { get; set; }
        [ColumnName("modifieddate")]
        public DateTime ModifiedDate { get; set; }

    }
}