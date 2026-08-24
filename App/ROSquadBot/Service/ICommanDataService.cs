using ROTGBot.Contract.Model;

namespace ROTGBot.Service
{
    public interface ICommandDataService
    {
        Task<ROTGBot.Contract.Model.Command> AddCommand(User user, CommandType commandType, CancellationToken token);
        Task<ROTGBot.Contract.Model.Command> GetCurrentCommand(User user, CancellationToken token);
        Task<string[]> GetMessages(Guid id);
    }
}