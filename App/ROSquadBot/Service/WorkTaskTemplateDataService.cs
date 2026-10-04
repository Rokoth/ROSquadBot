using ROTGBot.Db.Interface;

namespace ROTGBot.Service
{
    public class WorkTaskTemplateDataService(IRepository<Db.Model.WorkTaskTemplate> templateRepo): IWorkTaskTemplateDataService
    {
        private readonly IRepository<Db.Model.WorkTaskTemplate> _templateRepo = templateRepo;

        public async Task<List<Contract.Model.WorkTaskTemplate>> GetWorkTaskTemplates(CancellationToken token)
        {

        }

        public async Task<Contract.Model.WorkTaskTemplate> GetWorkTaskTemplate(Guid  id, CancellationToken token)
        {

        }

        public async Task<Contract.Model.WorkTaskTemplate> CreateWorkTaskTemplate(Contract.Model.WorkTaskTemplate crestor, Guid userId, CancellationToken token)
        {

        }

        public async Task<Contract.Model.WorkTaskTemplate> UpdateWorkTaskTemplate(Contract.Model.WorkTaskTemplate crestor, Guid userId, CancellationToken token)
        {

        }

        public async Task<bool> DeleteWorkTaskTemplate(Guid id, Guid userId, CancellationToken token)
        {

        }

        private Contract.Model.WorkTaskTemplate Map(Db.Model.WorkTaskTemplate workTaskTemplate)
        {

        }
    }
}
