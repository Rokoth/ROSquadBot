namespace ROTGBot.Contract.Model
{
    public class NewsCommandMessage : Entity
    {       
        public Guid NewsId { get; set; }      
        public long TGMessageId { get; set; }
        public string? TextValue { get; set; }
    }

}
