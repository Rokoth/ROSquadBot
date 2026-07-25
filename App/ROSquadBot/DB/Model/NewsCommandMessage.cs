using ROTGBot.Db.Attributes;

namespace ROTGBot.Db.Model
{
    [TableName("newscommandmessage")]
    public class NewsCommandMessage : Entity
    {
        [ColumnName("newsid")]
        public Guid NewsId { get; set; }
        [ColumnName("tgmessageid")]
        public long TGMessageId { get; set; }
        [ColumnName("textvalue")]
        public string? TextValue { get; set; }
    }
}