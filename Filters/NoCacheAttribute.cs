using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace schoool_Management.Filters
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var response = context.HttpContext.Response;

            response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "0";

            base.OnResultExecuting(context);
        }
    }
}
