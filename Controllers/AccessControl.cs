using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    public class AdminAccess : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            User sessionUser = OnlineUsers.GetSessionUser();
            if (sessionUser != null)
                if (sessionUser.Admin || (bool)HttpContext.Current.Session["IsAdmin"])
                    return true;
            httpContext.Response.Redirect("~/Users/Login");
            return false;
        }
    }

    public class UserAccess : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (OnlineUsers.GetSessionUser() != null)
                return true;

            httpContext.Response.Redirect("~/Users/Login");
            return false;
        }
    }
}