using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CongThongTin_UTC2.ViewModel
{
    public class ListTagViewModel
    {
        public int TagID { get; set; }

        [StringLength(50)]
        public string TagName { get; set; }
        public int PostCount { get; set; }
    }
}