﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend.Model
{
    [Index("Email" , IsUnique =true)]
    public class PasswordReset
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Email { get; set; } = "";
        public string Token { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
