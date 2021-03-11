using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Sample.MultilingualContent.Models;
using Sample.MultilingualContent.Repositories;
using kr.bbon.AspNetCore.Mvc;

namespace Sample.MultilingualContent.Controllers
{
    [ApiVersion("1.1")]
    [ApiController]
    [Area("api")]
    [Route("[area]/v{version:apiVersion}/[controller]")]
    public class BooksController: ApiControllerBase
    {
        public BooksController(IBookRepository repository, ILoggerFactory loggerFactory)
        {
            this.repository = repository;
            logger = loggerFactory.CreateLogger<BooksController>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseModel<IEnumerable<BookModel>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAllAsync(string language = "", int page = 1, int take = 10)
        {
            try
            {
                var books = await repository.GetAllAsync(language, page, take);

                logger.LogInformation($"{nameof(BooksController)}.{nameof(GetAllAsync)} books.count={books.Count():n0}");

                return StatusCode((int)HttpStatusCode.OK, books);
            }
            catch (Exception ex)
            {
                return StatusCode(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel<BookModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetBookAsync(string id, string language = "")
        {
            try
            {
                var post = await repository.GetBookAsync(id, language);
                logger.LogInformation($"{nameof(BooksController)}.{nameof(GetBookAsync)} book.found={post != null}");
                if (post == null)
                {
                    throw new RecordNotFoundException($"Could not find a book ({id})");
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
        [ProducesResponseType(typeof(ApiResponseModel<BookModel>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> InsertAsync(BookSaveRequestModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var book = await repository.SaveAsync(model);

                    return StatusCode(HttpStatusCode.Created, book);
                }
                else
                {
                    return StatusCode(HttpStatusCode.BadRequest, String.Join(", ", ModelState.Values.Select(x => x.AttemptedValue)));
                }
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
        [ProducesResponseType(typeof(ApiResponseModel<BookModel>), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateAsync(string id, BookSaveRequestModel model)
        {
            try
            {
                if (!id.Equals(model.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidRequestException<IEnumerable<string>>($"Request body is invalid.", new[] { "Book identifier does not match." });
                }

                if (ModelState.IsValid)
                {
                    var book = await repository.SaveAsync(model);

                    return StatusCode(HttpStatusCode.Accepted, book);
                }
                else
                {
                    return StatusCode(HttpStatusCode.BadRequest, String.Join(", ", ModelState.Values.Select(x => x.AttemptedValue)));
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

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ApiResponseModel), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            try
            {
                await repository.DeleteAsync(id);

                return StatusCode(HttpStatusCode.Accepted, $"The book Deleted. ({id})");
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

        private readonly IBookRepository repository;
        private readonly ILogger logger;
    }
}
