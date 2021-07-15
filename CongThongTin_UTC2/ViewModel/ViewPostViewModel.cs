using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CongThongTin_UTC2.ViewModel
{
    public class ViewPostViewModel
    {
        public int post_id { get; set; }
        public string firstTag { get; set; }
        public int? userid { get; set; }

        [Required]
        [StringLength(200)]
        public string post_title { get; set; }

        [Required]
        [StringLength(500)]
        public string post_teaser { get; set; }

        [StringLength(500)]
        public string post_review { get; set; }

        
        public string post_content { get; set; }
        public string post_slug { get; set; }

        public int post_type { get; set; }

        [StringLength(200)]
        public string post_tag { get; set; }

        public DateTime? create_date { get; set; }

        public DateTime? edit_date { get; set; }

 
        public int ViewCount { get; set; }

        public int Rated { get; set; }

        [StringLength(200)]
        public string AvatarImage { get; set; }

        public bool status { get; set; }
        //public string firstTag { get; set; }
        public List<TagList> tagLists { get; set; }

        public List<CommentList> commentList { get; set; }
    }
    public class TagList
    {
        public int id { get; set; }
        public string slug { get; set; }
        public string name { get; set; }
    }
    public class CommentList
    {
        public int CommentID { get; set; }
        public int? userid { get; set; }

        public int? post_id { get; set; }

        [StringLength(4000)]
        public string CommentText { get; set; }

        public int? status { get; set; }
        public DateTime? CommentDate { get; set; }
    }
}