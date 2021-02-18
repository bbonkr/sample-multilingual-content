using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Sample.MultilingualContent.Data;
using Sample.MultilingualContent.Models;

namespace Sample.MultilingualContent.Repositories
{
    public interface IPostRepository
    {
        Task<IEnumerable<PostModel>> GetAllAsync(string languageId);
    }

    public class PostRepository: IPostRepository
    {
        public PostRepository(AppDbContext dbContext, ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            logger = loggerFactory.CreateLogger<PostRepository>();
        }

        public async Task<IEnumerable<PostModel>> GetAllAsync(string languageId)
        {
            var query = from post in dbContext.Posts
                        join title in dbContext.Localizations on post.TitleId equals title.Id
                        join content in dbContext.Localizations on post.ContentId equals content.Id
                        where title.LanguageId == languageId && content.LanguageId == languageId
                        select new PostModel(post.Id, title.Value, content.Value);

            var result = await query.ToListAsync();

            return result;
        }


        private readonly AppDbContext dbContext;
        private readonly ILogger logger;
    }
}
