using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    public class CommentsController : Controller
    {
        private DateTime LastUpdate
        {
            get
            {
                if (Session["LastCommentsUpdate"] == null)
                {
                    Session["LastCommentsUpdate"] = DateTime.Now;
                }
                return (DateTime)Session["LastCommentsUpdate"];
            }
            set
            {
                Session["LastCommentsUpdate"] = value;
            }
        }
        public ActionResult MustRefresh()
        {
            bool mustRefresh = DB.Comments.NeedUpdate(LastUpdate);
            return new JsonResult
            {
                Data = new { mustRefresh },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        [HttpPost]
        public ActionResult Create(Comment comment)
        {
            bool ok = false;
            if (!string.IsNullOrEmpty(comment.Text))
            {
                ok = true;
                comment.UserId = OnlineUsers.GetSessionUser().Id;
                comment.CreationDate = DateTime.Now;
                DB.Comments.Add(comment);
            }
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
            DB.Comments.Delete(id);
            return new JsonResult
            {
                Data = new { ok },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult CommentsList(int photoId)
        {
            LastUpdate = DB.Comments.LastUpdate();
            return PartialView(DB.Comments.ToListView(photoId));
        }
    }
}