using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Sample.MultilingualContent.Models;

namespace Sample.MultilingualContent.Controllers
{
    public abstract class ApiControllerBase :Controller
    {
        public override ObjectResult StatusCode([ActionResultStatusCode] int statusCode, [ActionResultObjectValue] object value)
        {
            return base.StatusCode(statusCode, ApiResponseModelFactory.Create(statusCode, string.Empty, value));
        }

        public ObjectResult StatusCode<T>([ActionResultStatusCode] HttpStatusCode statusCode, string message, [ActionResultObjectValue] T value)
        {
            return base.StatusCode((int)statusCode, ApiResponseModelFactory.Create(statusCode, message, value));
        }

        public ObjectResult StatusCode<T>([ActionResultStatusCode] HttpStatusCode statusCode, [ActionResultObjectValue] T value)
        {
            return base.StatusCode((int)statusCode, ApiResponseModelFactory.Create(statusCode, string.Empty, value));
        }
    }
}
