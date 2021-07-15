using CongThongTin_UTC2.CommonData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CongThongTin_UTC2.ViewModel
{
    public class SearchViewModel
    {
        public string title { get; set; }
        //public Dynasty? Dynasty { get; set; }
        //public string tags { get; set; }
        public List<SelectListItem> post_tag { get; set; } = PostData.getTagList();
    }
}