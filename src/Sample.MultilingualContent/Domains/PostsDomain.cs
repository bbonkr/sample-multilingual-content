using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using kr.bbon.Azure.Translator.Services;
using kr.bbon.Azure.Translator.Services.Models;
using kr.bbon.Azure.Translator.Services.Models.TextTranslation.TranslationRequest;
using kr.bbon.Azure.Translator.Services.Strategies;

using Sample.MultilingualContent.Entities;
using Sample.MultilingualContent.Models;
using Sample.MultilingualContent.Repositories;

using DocumentTranslationRequestModels = kr.bbon.Azure.Translator.Services.Models.DocumentTranslation.TranslationRequest;

namespace Sample.MultilingualContent.Domains
{
    public interface IPostsDomain
    {
        Task DeleteAsync(string postId);
        Task<PostDetailModel> FindByIdAsync(string id, string languageCode = "");
        Task<IEnumerable<PostModel>> GetPostsAsync(string languageCode, int page = 1, int take = 10);
        Task<PostDetailModel> SaveUsingTextTranslationAsync(PostSaveRequestModel model);

        Task<PostDetailModel> SaveUsingDocumentTranslationAsync(PostSaveRequestModel model);
    }

    public class PostsDomain : IPostsDomain
    {
        public PostsDomain(
            IPostRepository postRepository,
            ILanguageRepository languageRepository,
            ITextTranslatorService textTranslatorService,
            IDocumentTranslationService documentTranslationService,
            IStorageService<TranslateAzureBlobStorageContainer> storageService,
            ITranslatedDocumentNamingStrategy namingStrategy
            )
        {
            this.postRepository = postRepository;
            this.languageRepository = languageRepository;
            this.textTranslatorService = textTranslatorService;
            this.documentTranslationService = documentTranslationService;
            this.storageService = storageService;
            this.namingStrategy = namingStrategy;
        }

        public async Task<IEnumerable<PostModel>> GetPostsAsync(string languageCode, int page = 1, int take = 10)
        {
            var posts = await postRepository.GetAllAsync(languageCode, page, take);

            return posts;
        }

        public async Task<PostDetailModel> FindByIdAsync(string id, string languageCode = "")
        {
            var post = await postRepository.GetPostAsync(id, languageCode);

            return post;
        }

        /// <summary>
        /// Save a post data. It uses text translation on Azure Translator Service.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<PostDetailModel> SaveUsingTextTranslationAsync(PostSaveRequestModel model)
        {
            var languages = await languageRepository.GetAllAsync();

            var titleSet = model.PostContents.Select(postContent => new Localization
            {
                LanguageId = languages.Where(x => x.Code == postContent.LanguageCode).FirstOrDefault()?.Id,
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
                        var translationResult = await textTranslatorService.TranslateAsync(new RequestModel
                        {
                            Inputs = new[] {
                                    new Input(criteriaContent.Title),
                                    new Input(criteriaContent.Content)
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

                            if (translationResultIndex == 1)
                            {
                                // Content
                                foreach (var t in result.Translations)
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

                            translationResultIndex++;
                        });
                    }
                }
            }

            var post = await postRepository.SaveAsync(model.Id, titleSet, contentSet);

            return post;
        }

        public async Task<PostDetailModel> SaveUsingDocumentTranslationAsync(PostSaveRequestModel model)
        {
            var languages = await languageRepository.GetAllAsync();

            var titleSet = model.PostContents.Select(postContent => new Localization
            {
                LanguageId = languages.Where(x => x.Code == postContent.LanguageCode).FirstOrDefault()?.Id,
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
                        var translationResult = await textTranslatorService.TranslateAsync(new RequestModel
                        {
                            Inputs = new[] {
                                    new Input(criteriaContent.Title),
                                    //new Input(criteriaContent.Content)
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

                            //if (translationResultIndex == 1)
                            //{
                            //    // Content
                            //    foreach (var t in result.Translations)
                            //    {
                            //        // Translated language
                            //        var language = languages.Where(x => x.Code.Equals(t.To, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                            //        if (language != null)
                            //        {
                            //            var content = contentSet.Where(x => x.LanguageId == language.Id).FirstOrDefault();
                            //            if (content == null)
                            //            {
                            //                contentSet.Add(new Localization
                            //                {
                            //                    LanguageId = language.Id,
                            //                    Value = t.Text,
                            //                });
                            //            }
                            //            else
                            //            {
                            //                content.Value = t.Text;
                            //            }
                            //        }
                            //    }
                            //}

                            //translationResultIndex++;
                        });

                        // Source html file. 
                        var sourceBlob = await storageService.CreateAsync(
                            GenerateNewDocumentName(".html"),
                            criteriaContent.Content,
                            "text/html");

                        var targets = targetLanguages.Select(language =>
                        {

                            var targetBlobName = namingStrategy.GetTranslatedDocumentName(sourceBlob.BlobName, language.Code);

                            return new DocumentTranslationTarget
                            {
                                BlobName = targetBlobName,
                                LanguageCode = language.Code,
                                LanguageId = language.Id,
                                TargetInput = new DocumentTranslationRequestModels.TargetInput
                                {
                                    TargetUrl = storageService.GenerateBlobSasUri(targetBlobName),
                                    Language = language.Code,
                                    StorageSource = DocumentTranslationRequestModels.StorageSources.AzureBlob,
                                },
                            };
                        });

                        var documentTranslationRequestModel = new DocumentTranslationRequestModels.RequestModel
                        {
                            Inputs = new DocumentTranslationRequestModels.BatchInput[]
                            {
                                new DocumentTranslationRequestModels.BatchInput
                                {
                                    Source = new DocumentTranslationRequestModels.SourceInput
                                    {
                                        SourceUrl = storageService.GenerateBlobSasUri(sourceBlob.BlobName),
                                        Language = criteriaLanguage.Code,
                                        StorageSource = DocumentTranslationRequestModels.StorageSources.AzureBlob,
                                    },
                                    StorageType = DocumentTranslationRequestModels.StorageInputTypes.File,
                                    Targets = targets.Select(x => x.TargetInput),
                                },
                            }
                        };

                        var documentTranslationResult = await documentTranslationService.RequestTranslation(documentTranslationRequestModel);

                        if (documentTranslationResult == null)
                        {
                            var statusCode = HttpStatusCode.NotAcceptable;
                            var message = "Document translation failed.";
                            throw new ApiHttpStatusException<ErrorModel<int>>(
                                statusCode,
                                message,
                                new ErrorModel<int>
                                {
                                    Code = (int)statusCode,
                                    Message = message,
                                });
                        }

                        var translationJobCompleted = false;
                        do
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            var jobStatusResult = await documentTranslationService.GetJobStatusAsync(documentTranslationResult.Id);
                            if (jobStatusResult.Status == kr.bbon.Azure.Translator.Services.Models.DocumentTranslation.GetJobStatus.JobStatus.Succeeded)
                            {
                                translationJobCompleted = true;
                                break;
                            }
                        } while (true);

                        if (translationJobCompleted)
                        {
                            #region translationJobCompleted

                            foreach (var target in targets)
                            {
                                using (var stream = await storageService.LoadBlob(target.BlobName))
                                {
                                    if (stream != null)
                                    {
                                        using (var reader = new StreamReader(stream))
                                        {
                                            var stringContent = await reader.ReadToEndAsync();

                                            if (!string.IsNullOrWhiteSpace(stringContent))
                                            {
                                                var content = contentSet.Where(x => x.LanguageId == target.LanguageId).FirstOrDefault();
                                                if (content == null)
                                                {
                                                    contentSet.Add(new Localization
                                                    {
                                                        LanguageId = target.LanguageId,
                                                        Value = stringContent,
                                                    });
                                                }
                                                else
                                                {
                                                    content.Value = stringContent;
                                                }
                                            }
                                        }
                                    }
                                }

                                // Delete a translated file.
                                await storageService.DeleteAsync(target.BlobName);
                            }

                            #endregion

                            await storageService.DeleteAsync(sourceBlob.BlobName);
                        }
                    }
                }
            }

            var post = await postRepository.SaveAsync(model.Id, titleSet, contentSet);

            return post;
        }

        public async Task DeleteAsync(string postId)
        {
            await postRepository.DeleteAsync(postId);
        }


        private string GenerateNewDocumentName(string extension)
        {
            var guidPart = Guid.NewGuid().ToString("n");
            var timePart = DateTimeOffset.UtcNow.Ticks.ToString();

            return $"{guidPart}-{timePart}{extension}";
        }

        private readonly IPostRepository postRepository;
        private readonly ILanguageRepository languageRepository;
        private readonly ITextTranslatorService textTranslatorService;
        private readonly IDocumentTranslationService documentTranslationService;
        private readonly IStorageService<TranslateAzureBlobStorageContainer> storageService;
        private readonly ITranslatedDocumentNamingStrategy namingStrategy;
    }

    public class DocumentTranslationTarget
    {
        public string BlobName { get; set; }

        public string LanguageCode { get; set; }

        public string LanguageId { get; set; }

        public DocumentTranslationRequestModels.TargetInput TargetInput { get; set; }
    }
}
