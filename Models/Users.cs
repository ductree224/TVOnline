﻿using Microsoft.AspNetCore.Identity;

namespace TVOnline.Models {
    public class Users : IdentityUser{
        public string FullName{ get; set; }
        public string? City { get; set; }
    }
}
