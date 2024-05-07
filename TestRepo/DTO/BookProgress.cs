using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestRepo.DTO
{
    public class BookProgress
    {
        public int Id { get; set; }
        public int BookId { get; set; }

        // Keep JsonIgnore if you don't want the Book details to appear in responses
        [ForeignKey("BookId")]
        [JsonIgnore]
        public virtual Book Book { get; set; }

        public int AccountId { get; set; }

        // Keep JsonIgnore to prevent Account details from being exposed
        [ForeignKey("AccountId")]
        [JsonIgnore]
        public virtual Account Account { get; set; }

        public int volumesRead { get; set; }
    }

}
