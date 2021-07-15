using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CongThongTin_UTC2.ViewModel
{
    public class LoginViewModel
    {

        [MaxLength(20)]
        [Required(ErrorMessage = "Tài khoản không được để trống")]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MaxLength(20)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}