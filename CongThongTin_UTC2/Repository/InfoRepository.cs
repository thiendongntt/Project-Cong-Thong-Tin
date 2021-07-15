using CongThongTin_UTC2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CongThongTin_UTC2.Repository
{
    public class InfoRepository
    {
        private DBCongThongTin entity;// = new DVCPContext();
        public InfoRepository(DBCongThongTin context)
        {
            this.entity = context;
        }
        
        public WebInfo FindByID(int id = 1)
        {
            WebInfo u = entity.WebInfo.Find(id);
            return u;
        }
       
        public void Update(WebInfo u)
        {
            entity.Entry(u).State = EntityState.Modified;
        }
    }
}