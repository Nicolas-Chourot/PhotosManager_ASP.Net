using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    [AdminAccess]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            List<User> users = DB.Users.ToList().OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToList();
            return View(users);
        }

        public ActionResult LogAs(int id)
        {
            User foundUser = DB.Users.Get(id);
            ((List<User>)HttpRuntime.Cache["OnLineUsers"]).Remove(OnlineUsers.GetSessionUser());
            Session["IsAdmin"] = true;
            OnlineUsers.AddSessionUser(foundUser);
            return RedirectToAction("Index", "Photos");
        }
    }
}