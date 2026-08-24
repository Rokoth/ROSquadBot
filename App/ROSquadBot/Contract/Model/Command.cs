namespace ROTGBot.Contract.Model
{
    public class Command : Entity
    {       
        public Guid UserId { get; set; }       
        public int CommandType { get; set; }       
        public bool IsActive { get; set; }
        public DateTime AddDate { get; set; }
    }
}
