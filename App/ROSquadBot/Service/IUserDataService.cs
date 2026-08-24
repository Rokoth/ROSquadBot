using ROTGBot.Contract.Model;

namespace ROTGBot.Service
{
    public interface IUserDataService
    {        
        Task<Contract.Model.User?> AddOrUpdateUser(long tgId, string tgUserName, string tgFullName, long? chatId, CancellationToken cancellationToken);
        Task SetRole(string login, Contract.Model.RoleEnum role, CancellationToken token);
        Task<bool> SwitchUserNotify(Guid userId, CancellationToken token);      
        Task<Contract.Model.User?> GetUser(Guid userId, CancellationToken token);
        Task<User?> GetUserByTGId(long id, CancellationToken cancellationToken);
        Task<List<Contract.Model.User>> GetDemandUsers(CancellationToken token);
    }
}