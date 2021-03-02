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
    public class LanguagesController : ApiControllerBase
    {
        public LanguagesController(ILanguageRepository repository, ILoggerFactory loggerFactory)
        {
            this.repository = repository;
            logger = loggerFactory.CreateLogger<LanguagesController>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<LanguageModel>>), 200)]
        public async Task<IActionResult> GetAllAsync()
        {
            var languages = await repository.GetAllAsync();

            return StatusCode(HttpStatusCode.OK, languages);
        }

        private readonly ILanguageRepository repository;
        private readonly ILogger logger;
    }
}
