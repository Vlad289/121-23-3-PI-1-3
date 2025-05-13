using System;
using System.Collections.Generic;

namespace BLL.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Password { get; set; } // Only used for creation/update, not returned in queries
        public string Role { get; set; } = "Unregistered";
    }
}