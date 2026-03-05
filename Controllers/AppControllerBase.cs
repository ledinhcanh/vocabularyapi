using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class AppControllerBase : ControllerBase
    {
        protected bool ClearCache()
        {
            var clearcache = HttpContext.Request.Query["clearcache"];
            return clearcache.Count > 0 && clearcache.Any(x => x == "1");
        }
    }
}
