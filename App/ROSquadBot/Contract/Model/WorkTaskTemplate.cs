
namespace ROTGBot.Contract.Model
{
    public class WorkTaskTemplate : Entity
    {
        public string Text { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

    }

}
