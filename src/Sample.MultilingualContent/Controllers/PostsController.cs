using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Sample.MultilingualContent.Models;
using Sample.MultilingualContent.Repositories;

namespace Sample.MultilingualContent.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Area("api")]
    [Route("[area]/v{version:apiVersion}/[controller]")]
    public class PostsController : ApiControllerBase
    {
        public PostsController(IPostRepository repository, ILoggerFactory loggerFactory)
        {
            this.repository = repository;
            logger = loggerFactory.CreateLogger<LanguagesController>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<PostModel>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllAsync(string language = "", int page = 1, int take = 10)
        {
            var posts = await repository.GetAllAsync(language, page, take);

            logger.LogInformation($"{nameof(PostsController)}.{nameof(GetAllAsync)} posts.count={posts.Count():n0}");

            return StatusCode((int)HttpStatusCode.OK, posts);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<PostModel>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPost(string id, string language = "")
        {
            try
            {
                var post = await repository.GetPost(id, language);
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

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> InsertAsync(PostSaveRequestModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var post = await repository.SaveAsync(model);

                    return StatusCode(HttpStatusCode.Created, post);
                }
                else
                {
                    return StatusCode(HttpStatusCode.BadRequest, String.Join(", ", ModelState.Values.Select(x => x.AttemptedValue)));
                }
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

        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAsync(string id, PostSaveRequestModel model)
        {
            try
            {
                if (!id.Equals(model.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidRequestException($"Request body is invalid.");
                }

                if (ModelState.IsValid)
                {
                    var post = await repository.SaveAsync(model);

                    return StatusCode(HttpStatusCode.Accepted, post);
                }
                else
                {
                    return StatusCode(HttpStatusCode.BadRequest, String.Join(", ", ModelState.Values.Select(x => x.AttemptedValue)));
                }

            }
            catch(InvalidRequestException ex)
            {
                return StatusCode(HttpStatusCode.BadRequest, ex.Message);
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

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                await repository.DeleteAsync(id);

                return StatusCode(HttpStatusCode.Accepted, $"The post Deleted. ({id})");
            }
            catch (InvalidRequestException ex)
            {
                return StatusCode(HttpStatusCode.BadRequest, ex.Message);
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

        private readonly IPostRepository repository;
        private readonly ILogger logger;
    }
}
