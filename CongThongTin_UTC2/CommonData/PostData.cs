using CongThongTin_UTC2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CongThongTin_UTC2.CommonData
{
    public class PostData
    {
        public static List<SelectListItem> getTagList()
        {
            UnitOfWork db = new UnitOfWork(new DBCongThongTin());
            List<SelectListItem> lstTag = db.tagRepository.AllTags()
                .Select(m =>
                new SelectListItem
                {
                    Text = m.TagName,
                    Value = m.TagID.ToString(),
                }
                ).ToList();
            return lstTag;
         
        }
    }
}