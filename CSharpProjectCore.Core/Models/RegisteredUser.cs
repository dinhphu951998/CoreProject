using CSharpProjectCore.Core.Constant;
using CSharpProjectCore.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpProjectCore.Core.Models
{
    public class RegisteredUser : BaseViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Role { get; set; } = RolesEnum.MEMBER.ToString();
    }
}
