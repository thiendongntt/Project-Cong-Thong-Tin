using CongThongTin_UTC2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CongThongTin_UTC2.Repository
{
    public class TagRepository
    {
        private DBCongThongTin entity;
        public TagRepository(DBCongThongTin context)
        {
            this.entity = context;
        }
        public void AddTag(Tag tag)
        {
            entity.Tags.Add(tag);
        }
        public IQueryable<Tag> AllTags()
        {
            IQueryable<Tag> query = entity.Tags;
            return query.AsQueryable();
        }
        public void DeleteTag(Tag post)
        {
            entity.Tags.Remove(post);
        }
        public void UpdateTag(Tag tag)
        {
            entity.Entry(tag).State = EntityState.Modified;
        }
        public Tag FindByID(int id)
        {
            return entity.Tags.Find(id);
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
    }
}