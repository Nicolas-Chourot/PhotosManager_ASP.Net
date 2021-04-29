using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CustomExtensions;
using PhotosManager.Models;

namespace PhotosManager.Controllers
{
    [UserAccess]
    public class PhotosController : Controller
    {
        private int SearchType
        {
            get
            {
                if (Session["SearchType"] == null)
                {
                    Session["SearchType"] = 0;
                }
                return (int)Session["SearchType"];
            }
            set
            {
                Session["SearchType"] = value;
            }
        }
        private string SearchTags
        {
            get
            {
                if (Session["SearchTags"] == null)
                {
                    Session["SearchTags"] = "";
                }
                return (string)Session["SearchTags"];
            }
            set
            {
                Session["SearchTags"] = value;
            }
        }
        private int SearchOwnerId
        {
            get
            {
                if (Session["SearchOwnerId"] == null)
                {
                    Session["SearchOwnerId"] = 0;
                }
                return (int)Session["SearchOwnerId"];
            }
            set
            {
                Session["SearchOwnerId"] = value;
            }
        }
        private bool SortAscending
        {
            get
            {
                if (Session["SortAscending"] == null)
                {
                    Session["SortAscending"] = true;
                }
                return (bool)Session["SortAscending"];
            }
            set
            {
                Session["SortAscending"] = value;
            }
        }
        private int SortType
        {
            get
            {
                if (Session["SortType"] == null)
                {
                    Session["SortType"] = 0;
                }
                return (int)Session["SortType"];
            }
            set
            {
                Session["SortType"] = value;
            }
        }
        private DateTime LastPhotosUpdate
        {
            get
            {
                if (Session["LastPhotosUpdate"] == null)
                {
                    Session["LastPhotosUpdate"] = new DateTime(0);
                }
                return (DateTime)Session["LastPhotosUpdate"];
            }
            set
            {
                Session["LastPhotosUpdate"] = value;
            }
        }
        private DateTime LastOwnersUpdate
        {
            get
            {
                if (Session["LastOwnersUpdate"] == null)
                {
                    Session["LastOwnersUpdate"] = new DateTime(0);
                }
                return (DateTime)Session["LastOwnersUpdate"];
            }
            set
            {
                Session["LastOwnersUpdate"] = value;
            }
        }

        private List<Photo> GetFilteredPhotos()
        {
            LastPhotosUpdate = DB.Photos.LastUpdate();
            List<Photo> photos = null;
            switch (SearchType)
            {
                case 0: /* search by tags*/
                    if (!string.IsNullOrEmpty(SearchTags))
                    {
                        photos = DB.Photos.ToList()
                            .Where(p => (p.Title.ContainsTags(SearchTags) ||
                                         p.Description.ContainsTags(SearchTags)))
                            .ToList();
                    }
                    else
                    {
                        photos = DB.Photos.ToList();
                    }
                    break;
                case 1: /* search by owner user*/
                    if (SearchOwnerId != 0)
                        photos = DB.Photos.ToList().Where(p => p.UserId == SearchOwnerId).ToList();
                    else
                        photos = DB.Photos.ToList();
                    break;
            }

            if (SortAscending)
            {
                switch (SortType)
                {
                    case 0: /*sort by Title*/
                        photos = photos.OrderBy(p => p.Title).ToList();
                        break;
                    case 1: /*sort by Creation date*/
                        photos = photos.OrderBy(p => p.CreationDate).ToList();
                        break;
                    case 2: /*sort Ratings*/
                        photos = photos.OrderBy(p => p.RatingAverage).ThenBy(p => p.NbRatings).ToList();
                        break;
                }
            }
            else
            {
                switch (SortType)
                {
                    case 0: /*sort by Title*/
                        photos = photos.OrderByDescending(p => p.Title).ToList();
                        break;
                    case 1: /*sort by Creation date*/
                        photos = photos.OrderByDescending(p => p.CreationDate).ToList();
                        break;
                    case 2: /*sort Ratings*/
                        photos = photos.OrderByDescending(p => p.RatingAverage).ThenByDescending(p => p.NbRatings).ToList();
                        break;
                }
            }

            return photos;
        }

        public ActionResult MustRefreshOwners()
        {
            bool mustRefresh = OnlineUsers.NeedUpdate(LastOwnersUpdate);
            return new JsonResult
            {
                Data = new { mustRefresh },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult MustRefreshPhotos()
        {
            bool mustRefresh = DB.Photos.NeedUpdate(LastPhotosUpdate); 
            return new JsonResult
            {
                Data = new { mustRefresh },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult ToogleSort()
        {
            LastPhotosUpdate = new DateTime(0);
            SortAscending = !SortAscending;
            return new JsonResult
            {
                Data = new { SortAscending },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult ToogleSearchType()
        {
            LastPhotosUpdate = new DateTime(0);
            SearchType = (SearchType + 1) % 2;
            return new JsonResult
            {
                Data = new { SearchType },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult ToogleSortType()
        {
            LastPhotosUpdate = new DateTime(0);
            SortType = (SortType + 1) % 3;
            return new JsonResult
            {
                Data = new { SortType },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult SearchByTags(string tags)
        {
            LastPhotosUpdate = new DateTime(0);
            SearchTags = tags.Trim();
            return new JsonResult
            {
                Data = new { SearchTags },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult SearchByOwner(int id)
        {
            SearchOwnerId = id;
            LastPhotosUpdate = new DateTime(0);
            return new JsonResult
            {
                Data = new { SearchOwnerId },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult OwnersList()
        {
            if (OnlineUsers.NeedUpdate(LastOwnersUpdate))
            {
                LastOwnersUpdate = OnlineUsers.LastUpdate();
                List<object> list = new List<object>();
                List<User> owners = DB.Photos.GetAllOwnersSelectList();
                foreach (User owner in owners)
                {
                    list.Add(new
                    {
                        id = owner.Id,
                        name = owner.FullName(),
                        selected = owner.Id == SearchOwnerId,
                        online = OnlineUsers.IsOnLine(owner.Id)
                    });
                }
                return new JsonResult
                {
                    Data = list,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return null;
            }
        }
        public ActionResult PhotosList()
        {
            if (DB.Photos.NeedUpdate(LastPhotosUpdate))
            {
                ViewBag.SortType = SortType;
                return PartialView(GetFilteredPhotos());
            }
            return null; 
        }

        public ActionResult Index()
        {
            LastPhotosUpdate = new DateTime(0);
            LastOwnersUpdate = new DateTime(0);
            ViewBag.SearchType = SearchType;
            ViewBag.SearchOwnerId = SearchOwnerId;
            ViewBag.SortType = SortType;
            ViewBag.SortAscending = SortAscending;
            ViewBag.SearchTags = SearchTags;
            return View();
        }
        public ActionResult Details(int id)
        {
            Photo photo = DB.Photos.Get(id);
            if (photo != null)
            {
                User currentUser = OnlineUsers.GetSessionUser();
                ViewBag.IsOwner = photo.IsOwner(currentUser);
                ViewBag.IsAdmin = currentUser.Admin;
                ViewBag.UserRating = DB.Ratings.UserRating(currentUser.Id, id);
                return View(DB.Photos.Get(id));
            }
            return RedirectToAction("Index");
        }
        public ActionResult Next(int id)
        {
            return RedirectToAction("Details/" + Photos.Next(GetFilteredPhotos(), id));
        }
        public ActionResult Previous(int id)
        {
            return RedirectToAction("Details/" + Photos.Previous(GetFilteredPhotos(), id));
        }

        #region Create
        public ActionResult Create()
        {
            Photo photo = new Photo();
            return View(photo);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Photo photo)
        {
            StringBuilder t = new StringBuilder(photo.Title);
            if (ModelState.IsValid)
            {
                if (!DB.Photos.TitleExist(photo.Title))
                {
                    photo.UserId = OnlineUsers.GetSessionUser().Id;
                    DB.Photos.Add(photo);
                }
                else
                {
                    ModelState.AddModelError("Title", "This title is already used.");
                    return View(photo);
                }
            }
            else
            {
                return View(photo);
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Edit
        public ActionResult Edit(int id)
        {
            Photo photo = DB.Photos.Get(id);
            if (photo != null)
            {
                if (OnlineUsers.CurrentUserIsAdmin() ||
                    photo.IsOwner(OnlineUsers.GetSessionUser()))
                    return View(photo);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Photo photo)
        {
            /*if (!photo.IsOwner(OnlineUsers.GetSessionUser()))
            {
                ModelState.AddModelError("", "Illegal access!!! You are not the owner of this photo.");
                return View(photo);
            }*/
            if (ModelState.IsValid)
            {
                if (!DB.Photos.TitleExist(photo.Title, photo.Id))
                {
                    DB.Photos.Update(photo);
                }
                else
                {
                    ModelState.AddModelError("Title", "This title is already used.");
                    return View(photo);
                }
            }
            else
            {
                return View(photo);
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region Delete
        public ActionResult Delete(int id)
        {
            Photo photo = DB.Photos.Get(id);
            if (photo != null)
            {
                if (OnlineUsers.CurrentUserIsAdmin() ||
                    photo.IsOwner(OnlineUsers.GetSessionUser()))
                    return View(photo);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Photo photo)
        {
            /* if (!photo.IsOwner(OnlineUsers.GetSessionUser()))
             {
                 ModelState.AddModelError("", "Illegal access!!! You are not the owner of this photo.");
                 return View(photo);
             }*/
            DB.Photos.Delete(photo.Id);
            return RedirectToAction("Index");
        }
        #endregion
    }
}