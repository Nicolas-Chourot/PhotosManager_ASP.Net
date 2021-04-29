using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    public class RatingsController : Controller
    {
        private DateTime LastUpdate
        {
            get
            {
                if (Session["LastRatingsUpdate"] == null)
                {
                    Session["LastRatingsUpdate"] = DateTime.Now;
                }
                return (DateTime)Session["LastRatingsUpdate"];
            }
            set
            {
                Session["LastRatingsUpdate"] = value;
            }
        }
        public ActionResult MustRefresh()
        {
            bool mustRefresh = DB.Ratings.NeedUpdate(LastUpdate);
            return new JsonResult
            {
                Data = new { mustRefresh },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        [HttpPost]
        public ActionResult Create(Rating rating)
        {
            bool ok = true;
            rating.UserId = OnlineUsers.GetSessionUser().Id;
            DB.Ratings.Add(rating);

            return new JsonResult
            {
                Data = new { ok },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            bool ok = false;
            DB.Ratings.Delete(id);
            return new JsonResult
            {
                Data = new { ok },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult RatingsAverage(int photoId)
        {   
            Photo photo = DB.Photos.Get(photoId);
            if (photo != null) {
                LastUpdate = DB.Ratings.LastUpdate();
                ViewBag.Average = photo.RatingAverage;
                ViewBag.AverageText = photo.RatingAverage.ToString("0.0") + " (" + photo.NbRatings + ")";
                return PartialView();
            }
            return null;
        }

        [HttpPost]
        public ActionResult Change(Rating rating)
        {
            bool ok = true;
            rating.UserId = OnlineUsers.GetSessionUser().Id;
            DB.Ratings.Add(rating);
            return new JsonResult
            {
                Data = new { ok },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }

}