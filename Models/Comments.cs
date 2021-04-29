using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public DateTime CreationDate { get; set; }
        public string Text { get; set; }
        public Comment()
        {
            Id = 0;
            PhotoId = 0;
            CreationDate = DateTime.Now;
        }

        public CommentView ToView()
        {
            CommentView commentView = new CommentView();
            commentView.Id = Id;
            commentView.CreationDate = CreationDate;
            commentView.Text = Text;
            commentView.OwnerName = DB.Users.Get(UserId).FullName();
            User currentUser = OnlineUsers.GetSessionUser();
            commentView.AvatarUrl = DB.Users.Get(UserId).GetAvatarURL();
            Photo photo = DB.Photos.Get(PhotoId);
            if (currentUser != null)
                commentView.CanDelete = (currentUser.Id == UserId || currentUser.Admin || photo.IsOwner(currentUser));
            else
                commentView.CanDelete = false;
            return commentView;
        }
    }

    public class CommentView
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string OwnerName { get; set; }
        public string AvatarUrl { get; set; }
        public string Text { get; set; }
        public bool CanDelete { get; set; }
    }

    public class Comments : JsonSerializer<Comment>
    {
        public List<CommentView> ToListView(int photoId)
        {
            List<CommentView> commentsViewList = new List<CommentView>();
            foreach (Comment comment in ToList().Where(c => c.PhotoId == photoId))
            {
                commentsViewList.Add(comment.ToView());
            }
            return commentsViewList.OrderByDescending(c => c.CreationDate).ToList();
        }

        public void DeletePhotoComments(int photoId)
        {
            List<Comment> commentsToDelete = ToList().Where(c => c.PhotoId == photoId).ToList();
            foreach (Comment comment in commentsToDelete)
            {
                Delete(comment.Id);
            }
        }
        public void DeleteUserComments(int userId)
        {
            List<Comment> commentsToDelete = ToList().Where(c => c.UserId == userId).ToList();
            foreach (Comment comment in commentsToDelete)
            {
                Delete(comment.Id);
            }
        }
    }
}