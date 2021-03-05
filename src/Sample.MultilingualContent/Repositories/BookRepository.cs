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
    public interface IBookRepository {
        Task<IEnumerable<BookModel>> GetAllAsync(string languageCode, int page, int take);

        Task<BookModel> GetBookAsync(string id, string languageCode);

        Task<BookModel> SaveAsync(BookSaveRequestModel model);

        Task DeleteAsync(string id);
    }

    public class BookRepository : IBookRepository
    {
        private const string EMPTY_STRING = "";

        public BookRepository(AppDbContext dbContext, ITranslatorService translatorService, ILoggerFactory loggerFactory)
        {
            this.dbContext = dbContext;
            this.translatorService = translatorService;
            logger = loggerFactory.CreateLogger<BookRepository>();
        }

        public async Task<IEnumerable<BookModel>> GetAllAsync(string languageCode = EMPTY_STRING, int page = 1, int take = 10)
        {
            var skip = (page - 1) * take;

            var query = dbContext.Books
                .Include(x => x.Localizations)
                .Where(x => !x.IsDeleted && (string.IsNullOrWhiteSpace(languageCode) || (!string.IsNullOrWhiteSpace(languageCode) && x.Localizations.Any(l => l.Language.Code == languageCode))))
                .AsSingleQuery()
                .OrderBy(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(x => new BookModel(x.Id, x.Localizations.Where(l => l.Language.Code == languageCode).Select(l => new BookDetail(l.Language.Code, l.Title, l.Author))));

            var books = await query.AsNoTracking().ToListAsync();

            return books;
        }

        public async Task<BookModel> GetBookAsync(string id, string languageCode = EMPTY_STRING)
        {
            var query = dbContext.Books
                .Include(x => x.Localizations)
                .Where(x => x.Id == id && !x.IsDeleted && (string.IsNullOrWhiteSpace(languageCode) || (!string.IsNullOrWhiteSpace(languageCode) && x.Localizations.Any(l => l.Language.Code == languageCode))))
                .AsSingleQuery()
                .Select(x => new BookModel(x.Id, x.Localizations.Select(l => new BookDetail(l.Language.Code, l.Title, l.Author))));

            var book = await query.AsNoTracking().FirstOrDefaultAsync();

            return book;
        }

        public async Task<BookModel> SaveAsync(BookSaveRequestModel model)
        {
            var bookId = model.Id;

            var book = dbContext.Books
                .Include(x => x.Localizations)
                .Where(b => !b.IsDeleted && b.Id == model.Id)
                .FirstOrDefault();

            if (!String.IsNullOrWhiteSpace(model.Id) && book == null)
            {
                throw new RecordNotFoundException($"Could not find a book ({model.Id})");
            }

            var languages = dbContext.Languages.Where(language => !language.IsDeleted).AsNoTracking().ToList();

            var bookDetails = model.Details.Select(detail => new BookLocalization
            {
                LanguageId = languages.Where(x => x.Code == detail.LanguageCode).FirstOrDefault()?.Id,
                Title = detail.Title,
                Author = detail.Author
            }).ToList();


            if (model.UseTranslation)
            {
                var criteriaContent = model.Details.Where(x => x.LanguageCode == model.CriteriaLanguageCode).FirstOrDefault();

                if (criteriaContent != null)
                {
                    var criteriaLanguage = languages.Where(x => x.Code == model.CriteriaLanguageCode).FirstOrDefault();
                    var targetLanguages = languages.Where(x => x.Code != model.CriteriaLanguageCode).ToList();

                    if (criteriaLanguage != null)
                    {
                        var translationResult = await translatorService.TranslateAsync(new TranslationRequestModel
                        {
                            Inputs = new[] {
                                    new TranslationRequestInputModel(criteriaContent.Title),
                                    new TranslationRequestInputModel(criteriaContent.Author)
                                },
                            ToLanguages = targetLanguages.Select(x => x.Code).ToArray(),
                            FromLanguage = model.CriteriaLanguageCode,
                            TextType = model.IsHtmlContent ? TextTypes.Html : TextTypes.Plain,
                            IsTranslationEachLanguage = model.IsTranslationEachLanguage,
                        });

                        var translationResultIndex = 0;

                        translationResult.ToList().ForEach(result =>
                        {

                            if (translationResultIndex == 0)
                            {
                                // Title
                                foreach (var t in result.Translations)
                                {
                                    // Translated language
                                    var language = languages.Where(x => x.Code.Equals(t.To, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                    if (language != null)
                                    {
                                        var bookDetail = bookDetails.Where(x => x.LanguageId == language.Id).FirstOrDefault();
                                        if (bookDetail == null)
                                        {
                                            bookDetails.Add(new BookLocalization
                                            {
                                                LanguageId = language.Id,
                                                Title = t.Text,
                                            });
                                        }
                                        else
                                        {
                                            bookDetail.Title = t.Text;
                                        }
                                    }
                                }
                            }

                            if (translationResultIndex == 1)
                            {
                                // Author
                                foreach (var t in result.Translations)
                                {
                                    // Translated language
                                    var language = languages.Where(x => x.Code.Equals(t.To, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                    if (language != null)
                                    {
                                        var bookDetail = bookDetails.Where(x => x.LanguageId == language.Id).FirstOrDefault();
                                        if (bookDetail == null)
                                        {
                                            bookDetails.Add(new BookLocalization
                                            {
                                                LanguageId = language.Id,
                                                Author = t.Text,
                                            });
                                        }
                                        else
                                        {
                                            bookDetail.Author = t.Text;
                                        }
                                    }
                                }
                            }

                            translationResultIndex++;
                        });
                    }
                }
            }

            if (book == null)
            {
                var newBooks = new Book
                {
                    Localizations = bookDetails,
                };

                var insertedEntry = dbContext.Books.Add(newBooks);

                bookId = insertedEntry.Entity.Id;
            }
            else
            {
                foreach (var detail in bookDetails)
                {
                    var detailValue = book.Localizations.Where(t => t.LanguageId == detail.LanguageId).FirstOrDefault();
                    if (detailValue != null)
                    {
                        detailValue.Title = detail.Title;
                        detailValue.Author = detail.Author;
                    }
                    else
                    {
                        book.Localizations.Add(detail);
                    }
                }
            }

            await dbContext.SaveChangesAsync();

            var bookModel = await GetBookAsync(bookId);

            return bookModel;
        }

        public async Task DeleteAsync(string id)
        {
            var entry = dbContext.Books
              .Where(book => !book.IsDeleted && book.Id == id).FirstOrDefault();

            if (entry == null)
            {
                throw new RecordNotFoundException($"Could not find a book ({id})");
            }

            entry.IsDeleted = true;

            await dbContext.SaveChangesAsync();
        }


        private readonly AppDbContext dbContext;
        private readonly ITranslatorService translatorService;
        private readonly ILogger logger;
    }
}
