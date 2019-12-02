using CSharpProjectCore.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpProjectCore.Core.Models
{
    public class AccessTokenResponse
    {
        public UserViewModel User { get; set; }
        public string[] Roles { get; set; }
        public string AccessToken { get; set; }
    }
}
