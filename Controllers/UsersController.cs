using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    public class UsersController : Controller
    {
        #region Login
        public ActionResult Login()
        {
            Session["IsAdmin"] = false;
            return View();
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            string[] name = user.FirstName.Split(' ');
            user.FirstName = name[0];
            if (name.Length > 1)
                user.LastName = name[1];
            User foundUser = DB.Users.GetUserByName(user);

            if (foundUser != null)
            {
                if (!foundUser.PassswordMatch(user))
                {
                    ModelState.AddModelError("Password", "Mot de passe incorrete");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError("FirstName", "Prénom ou nom incorrecte.");
                return View();
            }
            OnlineUsers.AddSessionUser(foundUser);
            return RedirectToAction("Index", "Photos");
        }
        #endregion

        #region change password
        [UserAccess]
        public ActionResult ChangeProfil()
        {
            User currentUser = (User)Session["User"];
            currentUser.NewPassword = null;
            currentUser.Confirmation = null;
            return View(currentUser);
        }

        [HttpPost]
        [UserAccess]
        public ActionResult ChangeProfil(User user)
        {
            User currentUser = (User)Session["User"];
            if (!string.IsNullOrEmpty(user.NewPassword) &&
                (user.NewPassword != "not_change_1234567890"))
            {
                currentUser.Password = user.NewPassword;
            }
            currentUser.AvatarImageData = user.AvatarImageData;

            DB.Users.Update(currentUser);
            return RedirectToAction("Index", "Photos");
        }

        #endregion

        #region Logout
        public ActionResult Logout()
        {
            OnlineUsers.RemoveSessionUser();
            return RedirectToAction("Login", "Users");
        }
        #endregion
    }
}