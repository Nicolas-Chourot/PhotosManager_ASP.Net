using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public sealed class DB
    {
        private static readonly DB instance = new DB();
        public static Photos Photos { get; set; }
        public static Users Users { get; set; }
        public static Comments Comments { get; set; }
        public static Ratings Ratings { get; set; }
        public DB()
        {
            Photos = new Photos();
            Users = new Users();
            Comments = new Comments();
            Ratings = new Ratings();

            Photos.Init(HttpContext.Current.Server.MapPath("~/App_Data/Photos.json"));
            Users.Init(HttpContext.Current.Server.MapPath("~/App_Data/Users.json"));
            Comments.Init(HttpContext.Current.Server.MapPath("~/App_Data/Comments.json"));
            Ratings.Init(HttpContext.Current.Server.MapPath("~/App_Data/Ratings.json"));
        }
        public static DB Instance
        {
            get { return instance; }
        }
    }
}