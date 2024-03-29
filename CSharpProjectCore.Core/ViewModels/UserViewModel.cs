﻿using CSharpProjectCore.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpProjectCore.Core.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime? Birthdate { get; set; }
    }
}
