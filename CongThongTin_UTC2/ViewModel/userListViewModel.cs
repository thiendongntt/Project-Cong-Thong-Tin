using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CongThongTin_UTC2.ViewModel
{
    public class userListViewModel
    {
        public int userid { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [StringLength(20)]
        public string username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string password { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Compare("password",ErrorMessage = "Bạn nhập mật khẩu không trùng nhau!")]
        public string repassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên đầy đủ")]
        [StringLength(30)]
        public string fullname { get; set; }

        [StringLength(20)]
        public string userrole { get; set; }
    }
}