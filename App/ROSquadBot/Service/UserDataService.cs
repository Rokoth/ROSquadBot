using ROTGBot.Contract.Model;
using ROTGBot.Db.Interface;
using ROTGBot.Db.Model;
using User = Telegram.BotAPI.AvailableTypes.User;

namespace ROTGBot.Service
{

    public class UserDataService(IRepository<Db.Model.User> userRepo,
        IRepository<Role> roleRepo,
        IRepository<UserRole> userRoleRepo) : IUserDataService
    {
        private readonly IRepository<Db.Model.User> _userRepo = userRepo;
        private readonly IRepository<Role> _roleRepo = roleRepo;
        private readonly IRepository<UserRole> _userRoleRepo = userRoleRepo;

        public async Task<Contract.Model.User?> GetUserByTGId(long tgId, CancellationToken cancellationToken)
        {
            var user = (await _userRepo.GetAsync(new Filter<Db.Model.User>()
            {
                Selector = s => s.TGId == tgId
            }, cancellationToken)).FirstOrDefault();
                        
            return await Map(user, cancellationToken);
        }

        public async Task<Contract.Model.User?> AddOrUpdateUser(long tgId, string tgUserName, string tgFullName, long? chatId, CancellationToken cancellationToken)
        {
            var user = (await _userRepo.GetAsync(new Filter<Db.Model.User>()
            {
                Selector = s => s.TGId == tgId
            }, cancellationToken)).FirstOrDefault();

            if (user == null)
            {
                if(chatId == null)
                {
                    return null;
                }

                var allUsers = await _userRepo.GetAsync(new Filter<Db.Model.User>()
                {
                    Selector = s => !s.IsDeleted
                }, cancellationToken);

                int lastNumber = 1;

                if(allUsers.Count != 0)
                {
                    lastNumber = allUsers.Max(s => s.Number);
                }
                
                user = await _userRepo.AddAsync(new Db.Model.User()
                {
                    Id = Guid.NewGuid(),
                    Description = tgFullName,//$"{tguser.FirstName} {tguser.LastName} (@{tguser.Username})",
                    IsDeleted = false,
                    Name = tgFullName,
                    TGLogin = tgUserName,
                    TGId = tgId,
                    ChatId = chatId.Value,
                    LastSendDate = DateTime.Now.AddHours(-1),
                    Number = lastNumber + 1
                }, true, cancellationToken);

                var userRole = (await _roleRepo.GetAsync(new Filter<Role>() { Selector = s => s.Name == Enum.GetName(RoleEnum.user) }, cancellationToken)).First();

                await _userRoleRepo.AddAsync(new UserRole()
                {
                    Id = Guid.NewGuid(),
                    IsDeleted = false,
                    RoleId = userRole.Id,
                    UserId = user.Id
                }, true, cancellationToken);
            }
            else if(chatId != null && user.ChatId != chatId)
            {
                user.ChatId = chatId.Value;
                user.Name = tgFullName;
                user.TGLogin = tgUserName;
                await _userRepo.UpdateAsync(user, true, cancellationToken);
            }
            return await Map(user, cancellationToken);
        }

        private async Task<Contract.Model.User?> Map(Db.Model.User? user, CancellationToken cancellationToken)
            => user == null ? null : new Contract.Model.User()
        {
            ChatId = user.ChatId,
            Description = user.Description,
            Id = user.Id,
            IsNotify = user.IsNotify,
            Name = user.Name,
            Roles = await GetUserRoles(user, cancellationToken),
            TGId = user.TGId,
            TGLogin = user.TGLogin,
            LastSendDate = user.LastSendDate,
            Number = user.Number
        };

        private async Task<List<RoleEnum>> GetUserRoles(Db.Model.User user, CancellationToken cancellationToken)
        {
            return (await GetUserRoles(user.Id, cancellationToken)).Select(Enum.Parse<RoleEnum>)?.ToList()
                ?? [RoleEnum.user];
        }

        private async Task<string[]> GetUserRoles(Guid userId, CancellationToken token)
        {
            string[] roles = [];
            var userRoles = (await _userRoleRepo.GetAsync(new Filter<UserRole>() { Selector = s => s.UserId == userId }, token)).Select(s => s.RoleId).Distinct().ToArray();
            if (userRoles.Length != 0)
            {
                roles = [.. (await _roleRepo.GetAsync(new Filter<Role>() { Selector = s => userRoles.Contains(s.Id) }, token)).Select(s => s.Name)];
            }

            return roles;
        }

        public async Task SetRole(string login, RoleEnum role, CancellationToken token)
        {
            var user = (await _userRepo.GetAsync(new Filter<Db.Model.User>()
            {
                Selector = s => s.TGLogin != null && s.TGLogin == login
            }, token)).FirstOrDefault();

            if (user != null)
            {
                var newRole = (await _roleRepo.GetAsync(new Filter<Role>() { Selector = s => s.Name == Enum.GetName(typeof(RoleEnum), role) }, token)).First();

                await _userRoleRepo.AddAsync(new UserRole()
                {
                    Id = Guid.NewGuid(),
                    IsDeleted = false,
                    RoleId = newRole.Id,
                    UserId = user.Id
                }, true, token);
            }
        }

        public async Task<bool> SwitchUserNotify(Guid userId, CancellationToken token)
        {
            var user = await _userRepo.GetAsync(userId, token);
            user.IsNotify = !user.IsNotify;
            await _userRepo.UpdateAsync(user, true, token);
            return user.IsNotify;
        }

        public async Task SetUserSendDate(Guid userId, CancellationToken token)
        {
            var user = await _userRepo.GetAsync(userId, token);
            user.LastSendDate = DateTime.Now;
            await _userRepo.UpdateAsync(user, true, token);            
        }

        public async Task<Contract.Model.User?> GetUser(Guid userId, CancellationToken token)
        {
            var user = await _userRepo.GetAsync(userId, token);
            return await Map(user, token);
        }


        public async Task<List<Contract.Model.User>> GetDemandUsers(CancellationToken token)
        {
            List<Contract.Model.User> result = [];
            var users = await _userRepo.GetAsync(new Filter<Db.Model.User>()
            {
                Selector = s => !s.IsDeleted
            }, token);
            foreach (var item in users)
            {
                var roles = await GetUserRoles(item.Id, token);
                if (roles.Length != 1 || roles.First() != "user")
                {
                    continue;
                }
                var res = await Map(item, token);
                if (res == null)
                {
                    continue;
                }
                result.Add(res);
            }
            return result;
        }
    }
}
