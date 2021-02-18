using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Sample.MultilingualContent.Data;
using Sample.MultilingualContent.Models;

namespace Sample.MultilingualContent.Repositories
{
    public interface ILanguageRepository {

        Task<IEnumerable<LanguageModel>> GetAllAsync();
    }

    public class LanguageRepository : ILanguageRepository
    {
        public LanguageRepository(AppDbContext dbContext, ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            this.logger = loggerFactory.CreateLogger<LanguageRepository>();
        }

        public async Task<IEnumerable<LanguageModel>> GetAllAsync()
        {
            var query = dbContext.Languages.Select(lang => new LanguageModel(lang.Id, lang.Code, lang.Name));

            var result = await query.ToListAsync();

            return result;
        }

        private readonly AppDbContext dbContext;
        private readonly ILogger logger;
    }
}
