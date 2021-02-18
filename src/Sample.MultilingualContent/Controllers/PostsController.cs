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
    [ApiController]
    [Route("[controller]")]
    public class PostsController : ApiControllerBase
    {
        public PostsController(IPostRepository repository, ILoggerFactory loggerFactory)
        {
            this.repository = repository;
            logger = loggerFactory.CreateLogger<LanguagesController>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<PostModel>>), 200)]
        public async Task<IActionResult> GetAllAsync(string language)
        {
            var posts = await repository.GetAllAsync(language);

            return StatusCode((int)HttpStatusCode.OK, posts);
        }

        private readonly IPostRepository repository;
        private readonly ILogger logger;
    }
}
