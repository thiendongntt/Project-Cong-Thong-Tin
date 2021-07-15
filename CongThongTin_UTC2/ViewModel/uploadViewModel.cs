using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CongThongTin_UTC2.ViewModel
{
    public class uploadViewModel
    {
        [Required]
        public int id { get; set; }
        public string title { get; set; }
        //[Required]
        //public List<HttpPostedFileBase> File { get; set; }
    }
}