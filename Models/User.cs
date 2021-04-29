using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace PhotosManager.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Prénom et nom requis.")]
        [Display(Name ="Prénom et nom")]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required(ErrorMessage = "Mot de passe requis.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }
        public bool Admin { get; set; }
        public string AvatarId { get; set; }

        [JsonIgnore]
        [Display(Name = "Avatar")]
        public string AvatarImageData { get; set; }

        [JsonIgnore]
        private ImageGUIDReference AvatarReference { get; set; }

        [JsonIgnore]
        [Required(ErrorMessage = "not there")]
        [Display(Name = "Nouveau mot de passe")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [JsonIgnore]
        [Display(Name = "Confirmation")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La confirmation ne correspond pas.")]
        public string Confirmation { get; set; }

        public String GetAvatarURL(bool thumbnail = false)
        {
            return AvatarReference.GetURL(AvatarId, thumbnail);
        }
        public void SaveAvatar()
        {
            AvatarId = AvatarReference.SaveImage(AvatarImageData, AvatarId);
        }
        public void RemoveAvatar()
        {
            AvatarReference.Remove(AvatarId);
        }
        public User()
        {
            Admin = false;
            AvatarReference = new ImageGUIDReference(@"/Avatars/", @"no_avatar.png");
            AvatarReference.MaxSize = 512;
            AvatarReference.HasThumbnail = false;
        }
        public bool UserNamesMatch(User user)
        {
            return string.Equals(this.FirstName, user.FirstName) &&
                   string.Equals(this.LastName, user.LastName);
        }
        public bool PassswordMatch(User user)
        {
            return string.Equals(this.Password, user.Password);
        }
        public string FullName()
        {
            return FirstName + " " + LastName;
        }
    }
    public class ChangePassword
    {
        [Display(Name = "Nouveau mot de passe")]
        [Required(ErrorMessage = "Le nouveau mot de passe est requis.")]
        [DataType(DataType.Password)]
        [StringLength(20, ErrorMessage = "Doit contenir au moins {2} caractères", MinimumLength = 6)]
        public string NewPassword { get; set; }
        [Display(Name = "Confirmation")]
        [Required(ErrorMessage = "La confirmation du nouveau mot de passe est requise.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La confirmation ne correspond pas.")]
        public string Confirmation { get; set; }
    }
    public sealed class Users : JsonSerializer<User>
    {
        public User GetUserByName(User user)
        {
            foreach (User u in ToList())
            {
                if (user.UserNamesMatch(u))
                    return u;
            }
            return null;
        }
        public bool UserNameExist(User user, int excludedId = 0)
        {
            foreach (User u in ToList())
            {
                if ((u.Id != excludedId) && user.UserNamesMatch(u))
                {
                    return true;
                }
            }
            return false;
        }
        public override int Add(User user)
        {
            if (!UserNameExist(user))
            {
                user.SaveAvatar();
                return base.Add(user);
            }
            return 0;
        }
        public override bool Update(User user)
        {
            if (!UserNameExist(user, user.Id))
            {
                user.SaveAvatar();
                return base.Update(user);
            }
            return false;
        }

        public override bool Delete(int Id)
        {
            User userToDelete = Get(Id);
            userToDelete.RemoveAvatar();
            DB.Ratings.DeleteUserRatings(Id);
            // DB.Comments.DeletePhotoComments(Id);
            // TODO remove comments and photos
            return base.Delete(Id);
        }
    }
    public static class OnlineUsers
    {
        public static void AddSessionUser(User user)
        {
            if (HttpRuntime.Cache["OnLineUsers"] == null)
            {
                HttpRuntime.Cache["OnLineUsers"] = new List<User>();
            }
            ((List<User>)HttpRuntime.Cache["OnLineUsers"]).Add(user);
            HttpContext.Current.Session["User"] = user;
            HttpContext.Current.Session.Timeout = 90;
            HttpRuntime.Cache["OnLineUsersUpdate"] = DateTime.Now;
        }
        public static void RemoveSessionUser()
        {
            if (HttpRuntime.Cache["OnLineUsers"] != null)
            {
                ((List<User>)HttpRuntime.Cache["OnLineUsers"]).Remove(GetSessionUser());
                HttpContext.Current.Session.Abandon();
                HttpRuntime.Cache["OnLineUsersUpdate"] = DateTime.Now;
            }
        }
        public static User GetSessionUser()
        {
            try
            {
                return (User)HttpContext.Current.Session["User"];
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static DateTime LastUpdate()
        {
            return (DateTime)HttpRuntime.Cache["OnLineUsersUpdate"];
        }

        public static bool NeedUpdate(DateTime date)
        {
            return DateTime.Compare(date, (DateTime)HttpRuntime.Cache["OnLineUsersUpdate"]) < 0;
        }
        public static bool CurrentUserIsAdmin()
        {
            User user = GetSessionUser();
            if (user != null)
                return user.Admin;
            return false;
        }
        public static bool IsOnLine(int userId)
        {
            foreach (User user in (List<User>)HttpRuntime.Cache["OnLineUsers"])
            {
                if (user.Id == userId)
                    return true;
            }
            return false;
        }
    }

}