using ROTGBot.Db.Attributes;

namespace ROTGBot.Db.Model
{
    [TableName("newscommand")]
    public class NewsCommand : Entity
    {
        [ColumnName("title")]
        public string Title { get; set; }
        [ColumnName("description")]
        public string Description { get; set; }
        [ColumnName("state")]
        public int State { get; set; }
        [ColumnName("creatorid")]
        public Guid CreatorId { get; set; }
        [ColumnName("createddate")]
        public DateTime CreatedDate { get; set; }
        [ColumnName("type")]
        public int Type { get; set; }
        [ColumnName("begindate")]
        public DateTime BeginDate { get; set; }
        [ColumnName("enddate")]
        public DateTime EndDate { get; set; }
        [ColumnName("number")]
        public int? Number { get; set; }
    }
}