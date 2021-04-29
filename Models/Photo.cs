using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public String PhotoId { get; set; }
        public int UserId { get; set; }

        [Required(ErrorMessage = "Le titre est requis")]
        [Display(Name = "Titre")]
        [StringLength(64)]
        public string Title { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        [Display(Name = "Date de création")]
        public DateTime CreationDate { get; set; }

        public float RatingAverage { get; set; }
        public int NbRatings { get; set; }

        [JsonIgnore]
        [Display(Name = "Image")]
        // La validation de la présence d'image est automatisée via le script imageUploader.js
        public string PhotoImageData { get; set; }

        [JsonIgnore]
        private ImageGUIDReference PhotoReference { get; set; }

        public String GetPhotoURL(bool thumbnail = false)
        {
            return PhotoReference.GetURL(PhotoId, thumbnail);
        }
        public void SavePhoto()
        {
            PhotoId = PhotoReference.SaveImage(PhotoImageData, PhotoId);
        }
        public void RemovePhoto()
        {
            PhotoReference.Remove(PhotoId);
        }
        public Photo()
        {
            Id = 0;
            UserId = 1;
            Title = "";
            Description = "";
            CreationDate = DateTime.Now;
            RatingAverage = 0.0f;
            NbRatings = 0;
            PhotoReference = new ImageGUIDReference(@"/Photos/", @"No_image.png");
        }
        public bool IsOwner(User user)
        {
            return user.Id == this.UserId;
        }
        public string GetOwnerName()
        {
            User owner = DB.Users.Get(UserId);
            if (owner != null)
                return owner.FullName();
            return "Unknown";
        }
        public string GetOwnerAvatar()
        {
            User owner = DB.Users.Get(UserId);
            if (owner != null)
                return owner.GetAvatarURL();
            return "Unknown";
        }
    }

    public sealed class Photos : JsonSerializer<Photo>
    {
        public bool TitleExist(string title, int excludedId = 0)
        {
            foreach (Photo photo in ToList())
            {
                if ((photo.Id != excludedId) && string.Equals(title, photo.Title, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        public override int Add(Photo photo)
        {
            if (!TitleExist(photo.Title))
            {
                photo.SavePhoto();
                return base.Add(photo);
            }
            return 0;
        }
        public override bool Update(Photo photo)
        {
            if (!TitleExist(photo.Title, photo.Id))
            {
                photo.SavePhoto();
                return base.Update(photo);
            }
            return false;
        }
       
        public override bool Delete(int Id)
        {
            Photo photoToDelete = Get(Id);
            photoToDelete.RemovePhoto();
            DB.Comments.DeletePhotoComments(Id);
            DB.Ratings.DeletePhotoRatings(Id);
            return base.Delete(Id);
        }
        static public int Next(List<Photo> photos, int id)
        {
            if (photos.Count > 0)
            {
                bool found = false;
                foreach (Photo photo in photos)
                {
                    if (found)
                        return photo.Id;
                    if (photo.Id == id)
                    {
                        found = true;
                    }
                }
                return photos[0].Id;
            }
            return id;
        }
        static public int Previous(List<Photo> photos, int id)
        {
            if (photos.Count > 0)
            {
                int previousId = 0;
                foreach (Photo photo in photos)
                {
                    if (photo.Id == id)
                    {
                        if (previousId != 0)
                            return previousId;
                    }
                    previousId = photo.Id;
                }
                return photos[photos.Count - 1].Id;
            }
            return id;
        }
        public List<User> GetAllOwnersSelectList()
        {
            List<User> owners = new List<User>();
            int currentUserId = 0;
            foreach (Photo photo in DB.Photos.ToList().OrderBy(p => p.UserId).ToList())
            {
                if (photo.UserId != currentUserId)
                {
                    currentUserId = photo.UserId;
                    owners.Add(DB.Users.Get(currentUserId));
                }
            }
            
            return owners.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToList();
        }
    }
}