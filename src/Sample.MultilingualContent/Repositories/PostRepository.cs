using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Sample.MultilingualContent.Data;
using Sample.MultilingualContent.Entities;
using Sample.MultilingualContent.Models;
using Sample.MultilingualContent.Services;

namespace Sample.MultilingualContent.Repositories
{
    public interface IPostRepository
    {
        Task<IEnumerable<PostModel>> GetAllAsync(string languageCode, int page, int take);

        Task<PostDetailModel> GetPost(string id, string languageCode);

        Task<PostDetailModel> SaveAsync( PostSaveRequestModel model);

        Task DeleteAsync(string id);
    }

    public class PostRepository : IPostRepository
    {
        private const string EMPTY_STRING = "";

        public PostRepository(AppDbContext dbContext, ITranslatorService translatorService, ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            this.translatorService = translatorService;
            logger = loggerFactory.CreateLogger<PostRepository>();
        }

        public async Task<IEnumerable<PostModel>> GetAllAsync(string languageCode, int page = 1, int take = 10)
        {
            var language = dbContext.Languages.Where(language => language.Code == languageCode).FirstOrDefault();

            if (language == null)
            {
                throw new Exception("Could not resolve the content language.");
            }

            var query = from post in dbContext.Posts
                        join title in dbContext.Localizations on post.TitleId equals title.Id
                        join content in dbContext.Localizations on post.ContentId equals content.Id
                        where !post.IsDeleted && title.LanguageId == language.Id && content.LanguageId == language.Id
                        orderby post.CreatedAt descending
                        select new PostModel(post.Id, title.Value, content.Value)
                        ;

            var skip = (page - 1) * take;
            var result = await query.Skip(skip).Take(take)
                .AsNoTracking()
                .ToListAsync();

            return result;
        }

        public async Task<PostDetailModel> GetPost(string id, string languageCode = EMPTY_STRING)
        {
            var languageQuery = dbContext.Languages.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(languageCode))
            {
                languageQuery = languageQuery.Where(lang => lang.Code == languageCode);
            }

            var languages = languageQuery.ToList();

            var query = dbContext.Posts
                .Include(post => post.Title).ThenInclude(localizationSet => localizationSet.Contents)
                .Include(post => post.Content).ThenInclude(localizationSet => localizationSet.Contents)
                .AsSingleQuery()
                .AsNoTracking()
                .Where(post => !post.IsDeleted && post.Id == id);


            var postEntry = await query.FirstOrDefaultAsync();
            if (postEntry != null)
            {
                var contents = languages.Select(lang => new PostSaveModel(
                      postEntry.Title.Contents.Where(x => lang.Id == x.LanguageId).FirstOrDefault().Value,
                      postEntry.Content.Contents.Where(x => lang.Id == x.LanguageId).FirstOrDefault().Value, 
                      lang.Code));


                return new PostDetailModel(postEntry.Id, contents);
            }

            return null;
        }

        public async Task<PostDetailModel> SaveAsync(PostSaveRequestModel model)
        {
            var postId = model.Id;

            var post = dbContext.Posts
                .Include(post => post.Title).ThenInclude(localizationSet => localizationSet.Contents)
                .Include(post => post.Content).ThenInclude(localizationSet => localizationSet.Contents)
                .Where(post => !post.IsDeleted && post.Id == model.Id).FirstOrDefault();

            if(!String.IsNullOrWhiteSpace( model.Id) && post == null)
            {
                throw new RecordNotFoundException($"Could not find a post ({model.Id})");
            }

            var languages = dbContext.Languages.ToList();

            var titleSet = model.PostContents.Select(postContent => new Localization
            {
                LanguageId = languages.Where(x=>x.Code == postContent.LanguageCode).FirstOrDefault()?.Id,
                Value = postContent.Title,
            }).ToList();

            var contentSet = model.PostContents.Select(postContent => new Localization
            {
                LanguageId = languages.Where(x => x.Code == postContent.LanguageCode).FirstOrDefault()?.Id,
                Value = postContent.Content,
            }).ToList();

            if (model.UseTranslation)
            {
                var criteriaContent = model.PostContents.Where(x => x.LanguageCode == model.CriteriaLanguageCode).FirstOrDefault();

                if (criteriaContent != null)
                {
                    var criteriaLanguage = languages.Where(x => x.Code == model.CriteriaLanguageCode).FirstOrDefault();
                    var targetLanguages = languages.Where(x => x.Code != model.CriteriaLanguageCode).ToList();

                    if (criteriaLanguage != null)
                    {
                        var titleTranslationResult = await translatorService.TranslateAsync(new TranslationRequestModel
                        {
                            Inputs = new[] {
                                    new TranslationRequestInputModel(criteriaContent.Title),
                                    new TranslationRequestInputModel(criteriaContent.Content)
                                },
                            TranslateToLanguages = targetLanguages.Select(x => x.Code).ToArray(),
                        });

                        if (titleTranslationResult.Length > 0)
                        {
                            // Title
                            foreach (var t in titleTranslationResult[0].Translations)
                            {
                                // Translated language
                                var language = languages.Where(x => x.Code.Equals(t.To, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                if (language != null)
                                {
                                    var title = titleSet.Where(x => x.LanguageId == language.Id).FirstOrDefault();
                                    if (title == null)
                                    {
                                        titleSet.Add(new Localization
                                        {
                                            LanguageId = language.Id,
                                            Value = t.Text,
                                        });
                                    }
                                    else
                                    {
                                        title.Value = t.Text;
                                    }
                                }
                            }
                        }

                        if (titleTranslationResult.Length > 1)
                        {
                            // Content
                            foreach (var t in titleTranslationResult[1].Translations)
                            {
                                // Translated language
                                var language = languages.Where(x => x.Code.Equals(t.To, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                if (language != null)
                                {
                                    var content = contentSet.Where(x => x.LanguageId == language.Id).FirstOrDefault();
                                    if (content == null)
                                    {
                                        contentSet.Add(new Localization
                                        {
                                            LanguageId = language.Id,
                                            Value = t.Text,
                                        });
                                    }
                                    else
                                    {
                                        content.Value = t.Text;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (post == null)
            {
                var newPost = new Post
                {
                    Title = new LocalizationSet
                    {
                        Contents = titleSet,
                    },
                    Content = new LocalizationSet
                    {
                        Contents = contentSet,
                    },
                };

                var insertedPostEntry = dbContext.Posts.Add(newPost);

                postId = insertedPostEntry.Entity.Id;
            }
            else
            {
                foreach (var title in titleSet)
                {
                    var titleValue = post.Title.Contents.Where(t => t.LanguageId == title.LanguageId).FirstOrDefault();
                    if (titleValue != null)
                    {
                        titleValue.Value = title.Value;
                    }
                    else
                    {
                        post.Title.Contents.Add(title);
                    }
                }

                foreach (var content in contentSet)
                {
                    var contentValue = post.Content.Contents.Where(t => t.LanguageId == content.LanguageId).FirstOrDefault();
                    if (contentValue != null)
                    {
                        contentValue.Value = content.Value;
                    }
                    else
                    {
                        post.Content.Contents.Add(contentValue);
                    }
                }
            }

            await dbContext.SaveChangesAsync();

            var postModel = await GetPost(postId);

            return postModel;
        }

        public async Task DeleteAsync(string id)
        {
            var post = dbContext.Posts
              .Include(post => post.Title).ThenInclude(localizationSet => localizationSet.Contents)
              .Include(post => post.Content).ThenInclude(localizationSet => localizationSet.Contents)
              .Where(post => !post.IsDeleted && post.Id == id).FirstOrDefault();

            if (post == null)
            {
                throw new RecordNotFoundException($"Could not find a post ({id})");
            }

            post.IsDeleted = true;

            await dbContext.SaveChangesAsync();
        }

        private readonly AppDbContext dbContext;
        private readonly ITranslatorService translatorService;
        private readonly ILogger logger;
    }
}
