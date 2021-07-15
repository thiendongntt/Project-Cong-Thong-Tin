namespace CongThongTin_UTC2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Comment
    {
        public int CommentID { get; set; }

        public int? userid { get; set; }

        public int? post_id { get; set; }

        [StringLength(4000)]
        public string CommentText { get; set; }

        public int? status { get; set; }

        public DateTime? CommentDate { get; set; }

        public virtual Post Post { get; set; }

        public virtual User User { get; set; }
    }
}
