using System;
using System.Web.Mvc;

namespace CorporatePortal.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SessionAuthorizeAttribute : AuthorizeAttribute
    {
        public SessionAuthorizeAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles ?? new string[0]);
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null || httpContext.Session == null ||
                httpContext.Session["LoginId"] == null)
                return false;

            var role = httpContext.Session["Role"] as string;
            if (string.IsNullOrEmpty(role)) return false;
            if (string.IsNullOrEmpty(Roles)) return true;

            foreach (var allowedRole in Roles.Split(','))
                if (string.Equals(role, allowedRole.Trim(), StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            bool authenticated = session != null && session["LoginId"] != null;
            if (!authenticated)
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Login", action = "Login_Pageload" }));
            else
                filterContext.Result = new HttpStatusCodeResult(403, "You are not authorized to access this page.");
        }
    }
}
