using System.Web.Http;

namespace MeloManager.Api.Controllers
{
    public class DefaultController : ApiController
    {
        public object Get()
        {
            return new { Api = "MeloManager.Api", Version = "1" };
        }
    }
}