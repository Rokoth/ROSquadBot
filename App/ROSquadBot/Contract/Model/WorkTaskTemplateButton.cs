
namespace ROTGBot.Contract.Model
{
    public class WorkTaskTemplateButton : Entity
    {
        public string Text { get; set; } = string.Empty;
        public Guid WorkTaskTemplateId { get; set; }
        public int Order { get; set; }
    }

}
