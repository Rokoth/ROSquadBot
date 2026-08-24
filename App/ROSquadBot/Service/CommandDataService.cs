using ROTGBot.Db.Interface;

namespace ROTGBot.Service
{
    public class CommandDataService(IRepository<Db.Model.Command> commandRepo, IRepository<Db.Model.CommandData> commandDataRepo): ICommandDataService
    {
        private readonly IRepository<Db.Model.Command> _commandRepo = commandRepo;
        private readonly IRepository<Db.Model.CommandData> _commandDataRepo = commandDataRepo;
    }
}
