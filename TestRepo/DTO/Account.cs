using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestRepo.DTO
{
    [Table("Accounts")]  // Explicitly specifying the table name for EF Core
    public class Account
    {
        [Key]  // Explicitly marking Id as the primary key if not yet configured
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        //public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<BookProgress>? Books { get; set; }

        [NotMapped]
        [JsonIgnore]
        public bool IsLoggedin { get; set; }

    }
}