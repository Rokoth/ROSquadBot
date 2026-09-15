namespace ROTGBot.Contract.Model
{
    public class Command : Entity
    {       
        public Guid UserId { get; set; }       
        public CommandType CommandType { get; set; }       
        public bool IsActive { get; set; }
        public DateTime AddDate { get; set; }
        public List<CommandData> Messages { get; set; }
    }

    public class CommandData : Entity
    {
        public Guid CommandId { get; set; }
        public string TextValue { get; set; }
        public long MessageId { get; set; }
        public DateTime AddDate { get; set; }
    }
}
