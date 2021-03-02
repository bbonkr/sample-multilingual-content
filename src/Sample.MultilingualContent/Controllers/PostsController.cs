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

            return StatusCode((int)HttpStatusCode.OK, posts);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<PostModel>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPost(string id, string language = "")
        {
            var post = await repository.GetPost(id, language);

            return StatusCode((int)HttpStatusCode.OK, post);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> InsertAsync(PostSaveRequestModel model)
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

        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<PostDetailModel>), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateAsync(string id, PostSaveRequestModel model)
        {
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

        private readonly IPostRepository repository;
        private readonly ILogger logger;
    }
}
