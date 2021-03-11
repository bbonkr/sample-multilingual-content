using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sample.MultilingualContent.Models;

using kr.bbon.AspNetCore.Mvc;
using Sample.MultilingualContent.Domains;

namespace Sample.MultilingualContent.Controllers
{
    [ApiVersion("1.1")]
    [ApiController]
    [Area("api")]
    [Route("[area]/v{version:apiVersion}/[controller]")]
    public class PostsController : ApiControllerBase
    {
        public PostsController(IPostsDomain postsDomain, ILoggerFactory loggerFactory)
        {
            this.postsDomain = postsDomain;
            logger = loggerFactory.CreateLogger<LanguagesController>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<PostModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllAsync(string language = "", int page = 1, int take = 10)
        {
            try
            {
                var posts = await postsDomain.GetPostsAsync(language, page, take);

                logger.LogInformation($"{nameof(PostsController)}.{nameof(GetAllAsync)} posts.count={posts.Count():n0}");

                return StatusCode((int)HttpStatusCode.OK, posts);
            }
            catch (Exception ex)
            {
                return StatusCode(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<PostModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPost(string id, string language = "")
        {
            try
            {
                var post = await postsDomain.FindByIdAsync(id, language);
                logger.LogInformation($"{nameof(PostsController)}.{nameof(GetPost)} post.found={post != null}");
                if (post == null)
                {
                    throw new RecordNotFoundException($"Could not find a post ({id})");
                }

                return StatusCode((int)HttpStatusCode.OK, post);
            }
            catch (RecordNotFoundException ex)
            {
                return StatusCode(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [ApiVersion("1.0")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InsertAsync(PostSaveRequestModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var post = await postsDomain.SaveUsingTextTranslationAsync(model);

                    return StatusCode(HttpStatusCode.Created, post);
                }
                else
                {
                    throw new InvalidRequestException<IEnumerable<string>>("Request body is invalid.", ModelState.Values.Select(x => x.AttemptedValue));
                }
            }
            catch(OptionsValidationException ex)
            {
                return StatusCode(HttpStatusCode.Forbidden, ex.Message, ex.Failures);               
            }
            catch(InvalidRequestException ex)
            {
                return StatusCode(HttpStatusCode.Forbidden, ex.Message, ex.GetDetails());
            }
            catch(SomethingWrongException ex)
            {
                var exceptionDetails = ex.GetDetails();
                return StatusCode(HttpStatusCode.Forbidden, ex.Message, exceptionDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [ApiVersion("1.0")]
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateAsync(string id, PostSaveRequestModel model)
        {
            try
            {
                if (!id.Equals(model.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidRequestException<IEnumerable<string>>($"Request body is invalid.", new[] { "Post identifier does not match." });
                }

                if (ModelState.IsValid)
                {
                    var post = await postsDomain.SaveUsingTextTranslationAsync(model);

                    return StatusCode(HttpStatusCode.Accepted, post);
                }
                else
                {
                    throw new InvalidRequestException<IEnumerable<string>>("Request body is invalid.", ModelState.Values.Select(x => x.AttemptedValue));
                }

            }
            catch(InvalidRequestException ex)
            {
                return StatusCode(HttpStatusCode.BadRequest, ex.Message, ex.GetDetails());
            }
            catch (RecordNotFoundException ex)
            {
                return StatusCode(HttpStatusCode.NotFound, ex.Message);
            }
            catch (SomethingWrongException ex)
            {
                var exceptionDetails = ex.GetDetails();
                return StatusCode(HttpStatusCode.Forbidden, ex.Message, exceptionDetails);
            }
            catch (Exception ex)
            {

                return StatusCode(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                await postsDomain.DeleteAsync(id);

                return StatusCode(HttpStatusCode.Accepted, $"The post Deleted. ({id})");
            }
            catch (InvalidRequestException ex)
            {
                return StatusCode(HttpStatusCode.BadRequest, ex.Message, ex.GetDetails());
            }
            catch (RecordNotFoundException ex)
            {
                return StatusCode(HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {

                return StatusCode(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InsertUseDocumentTranslationAsync(PostSaveRequestModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var post = await postsDomain.SaveUsingDocumentTranslationAsync(model);

                    return StatusCode(HttpStatusCode.Created, post);
                }
                else
                {
                    throw new InvalidRequestException<IEnumerable<string>>("Request body is invalid.", ModelState.Values.Select(x => x.AttemptedValue));
                }
            }
            catch (OptionsValidationException ex)
            {
                return StatusCode(HttpStatusCode.Forbidden, ex.Message, ex.Failures);
            }
            catch (InvalidRequestException ex)
            {
                return StatusCode(HttpStatusCode.Forbidden, ex.Message, ex.GetDetails());
            }
            catch (SomethingWrongException ex)
            {
                var exceptionDetails = ex.GetDetails();
                return StatusCode(HttpStatusCode.Forbidden, ex.Message, exceptionDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateUseDocumentTranslationAsync(string id, PostSaveRequestModel model)
        {
            try
            {
                if (!id.Equals(model.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidRequestException<IEnumerable<string>>($"Request body is invalid.", new[] { "Post identifier does not match." });
                }

                if (ModelState.IsValid)
                {
                    var post = await postsDomain.SaveUsingDocumentTranslationAsync(model);

                    return StatusCode(HttpStatusCode.Accepted, post);
                }
                else
                {
                    throw new InvalidRequestException<IEnumerable<string>>("Request body is invalid.", ModelState.Values.Select(x => x.AttemptedValue));
                }

            }
            catch (InvalidRequestException ex)
            {
                return StatusCode(HttpStatusCode.BadRequest, ex.Message, ex.GetDetails());
            }
            catch (RecordNotFoundException ex)
            {
                return StatusCode(HttpStatusCode.NotFound, ex.Message);
            }
            catch (SomethingWrongException ex)
            {
                var exceptionDetails = ex.GetDetails();
                return StatusCode(HttpStatusCode.Forbidden, ex.Message, exceptionDetails);
            }
            catch (Exception ex)
            {

                return StatusCode(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private readonly IPostsDomain postsDomain;
        private readonly ILogger logger;
    }
}
