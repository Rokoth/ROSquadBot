namespace ROTGBot.Contract.Model
{
    public class NewsCommand : Entity
    {
        public string Title { get; set; } = "Новая задача";
        public string Description { get; set; } = "Новая задача";
        public NewsCommandState State { get; set; } = NewsCommandState.New;
        public Guid CreatorId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public NewsCommandType Type { get; set; } = NewsCommandType.Simple;
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Number { get; set; }
    }

    public enum NewsCommandState
    {
        New = 1,
        InProgress = 2,
        Stopped = 3,
        Full = 4,
        Closed = 5
    }

    public enum NewsCommandType
    {
        Simple = 1,
        MultiSelect = 2,
        Custom = 20
    }
}
