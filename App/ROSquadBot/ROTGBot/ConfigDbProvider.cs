using Microsoft.EntityFrameworkCore;
using ROTGBot.Db.Context;
using ROTGBot.Db.Model;

namespace ROSquadBot
{
    public class ConfigDbProvider(Action<DbContextOptionsBuilder> options) : ConfigurationProvider
    {
        private readonly Action<DbContextOptionsBuilder> _options = options;

        public override void Load()
        {
            using var context = GetContext();
            AddData(context);
        }

        private DbPgContext GetContext() => new(GetBuilder(new DbContextOptionsBuilder<DbPgContext>()).Options);

        private void AddData(DbPgContext context) => GetSettings(context).ForEach(item => Data.Add(item.ParamName, item.ParamValue));

        private static List<Settings> GetSettings(DbPgContext context) => [.. context.Settings.AsNoTracking()];

        private DbContextOptionsBuilder<DbPgContext> GetBuilder(DbContextOptionsBuilder<DbPgContext> builder)
        {
            _options(builder);
            return builder;
        }
    }
}
