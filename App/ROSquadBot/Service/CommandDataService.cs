using ROTGBot.Contract.Model;
using ROTGBot.Db.Interface;

namespace ROTGBot.Service
{
    public class CommandDataService(IRepository<Db.Model.Command> commandRepo, IRepository<Db.Model.CommandData> commandDataRepo): ICommandDataService
    {
        private readonly IRepository<Db.Model.Command> _commandRepo = commandRepo;
        private readonly IRepository<Db.Model.CommandData> _commandDataRepo = commandDataRepo;

        public async Task<Command> AddCommand(User user, CommandType commandType, CancellationToken token)
        {
            var exists = await _commandRepo.GetAsync(new Db.Model.Filter<Db.Model.Command>() {
                Selector = s => s.IsDeleted == false && s.UserId == user.Id && s.IsActive
            }, token);

            if(exists.Any())
            {
                throw new Exception("Есть активная задача у пользователя");
            }

            var result = await _commandRepo.AddAsync(new Db.Model.Command() {
                AddDate = DateTime.Now,
                CommandType = (int)commandType,
                IsActive = true,
                IsDeleted = false,
                UserId = user.Id
            }, true, token);

            return await Map(result, token);
        }

        private async Task<Command> Map(Db.Model.Command result, CancellationToken token)
        {
            var data = await _commandDataRepo.GetAsync(new Db.Model.Filter<Db.Model.CommandData>()
            {
                Selector = s => s.CommandId == result.Id
            }, token);

            return new Command()
            { 
                AddDate = result.AddDate,
                IsActive = result.IsActive,
                Id = result.Id,
                UserId = result.UserId,
                CommandType = (CommandType)result.CommandType,
                Messages = data.Select(s => new CommandData()
                {
                    AddDate = s.AddDate,
                    Id = s.Id,
                    CommandId = s.CommandId,
                    MessageId = s.MessageId,
                    TextValue = s.TextValue
                }).ToList()
            };
        }

        public async Task CloseCurrentCommand(Guid userId, CancellationToken token)
        {
            var exists = await _commandRepo.GetAsync(new Db.Model.Filter<Db.Model.Command>()
            {
                Selector = s => s.IsDeleted == false && s.UserId == userId && s.IsActive
            }, token);

            foreach(var com in exists)
            {
                com.IsActive = false;
                await _commandRepo.UpdateAsync(com, true, token);
            }
        }

        public async Task<Command> GetCurrentCommand(User user, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task<string[]> GetMessages(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task AddMessage(Guid id, string data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
