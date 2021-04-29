using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public int Value { get; set; }
    }

    public class Ratings : JsonSerializer<Rating>
    {
        public int UserRating(int userId, int photoId)
        {
            Rating rating = ToList().Where(r => r.PhotoId == photoId && r.UserId == userId).FirstOrDefault();
            if (rating != null)
                return rating.Value;
            return 0;
        }

        private void UpdatePhotoRating(int photoId)
        {
            Photo photoToUpdate = DB.Photos.Get(photoId);
            if (photoToUpdate != null)
            {
                List<Rating> photoRatings = ToList().Where(r => r.PhotoId == photoId).ToList();
                float sum = 0;
                foreach (Rating photoRating in photoRatings)
                {
                    sum += photoRating.Value;
                }
                photoToUpdate.NbRatings = photoRatings.Count;
                photoToUpdate.RatingAverage = (float)sum / photoToUpdate.NbRatings;
                DB.Photos.Update(photoToUpdate);
            }
        }

        public override int Add(Rating rating)
        {
            int result = 0;
            Photo photoToUpdate = DB.Photos.Get(rating.PhotoId);
            if (photoToUpdate != null)
            {
                if (UserRating(rating.UserId, rating.PhotoId) == 0)
                {
                    result = base.Add(rating);
                }
                else
                {
                    Rating ratingToUpdate = ToList().Where(r => r.PhotoId == rating.PhotoId && r.UserId == rating.UserId).FirstOrDefault();
                    if (ratingToUpdate != null)
                    {
                        rating.Id = ratingToUpdate.Id;
                        Update(rating);
                    }
                }
                UpdatePhotoRating(rating.PhotoId);
            }
            return result;
        }
        public void DeletePhotoRatings(int photoId)
        {
            List<Rating> ratingsToDelete = ToList().Where(c => c.PhotoId == photoId).ToList();
            foreach (Rating rating in ratingsToDelete)
            {
                Delete(rating.Id);
            }
            UpdatePhotoRating(photoId);
        }
        public void DeleteUserRatings(int userId)
        {
            List<Rating> ratingsToDelete = ToList().Where(c => c.UserId == userId).ToList();
            foreach (Rating rating in ratingsToDelete)
            {
                Delete(rating.Id);
                UpdatePhotoRating(rating.PhotoId);
            }
        }
    }
}