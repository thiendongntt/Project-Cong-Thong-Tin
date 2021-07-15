using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CongThongTin_UTC2.Models;

namespace CongThongTin_UTC2.Repository
{
    public class CommentRepository
    {
        private DBCongThongTin entity;// = new DVCPContext();
        public CommentRepository(DBCongThongTin context)
        {
            this.entity = context;
        }
        public void AddComment(Comment comment)
        {
            entity.Comments.Add(comment);
        }
        public void DeleteComment(Comment comment)
        {
            entity.Comments.Remove(comment);
        }

        public Comment FindByID(int id)
        {
            Comment c = entity.Comments.Where(s => s.CommentID == id).SingleOrDefault();
            //Comment c = (from s in entity.Comments
            //            where s.userid == userid && s.post_id == post_id
            //            select s).ToList();

            return c;
        }
        public IQueryable<Comment> AllComments()
        {
            IQueryable<Comment> query = entity.Comments;
            return query.AsQueryable();
        }


        public void SaveChanges()
        {
            entity.SaveChanges();
        }
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    entity.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void AddComment(List<Comment> userComment)
        {
            throw new NotImplementedException();
        }
    }
}