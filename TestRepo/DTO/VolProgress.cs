using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestRepo.DTO
{
    public class VolProgress
    {
        [Key]
        public int Id { get; set; }
        public int pagesRead { get; set; }
        [ForeignKey("volId")]
        [JsonIgnore]
        public virtual Volume Volume { get; set; }
        public int volId { get; set; }
        [ForeignKey("BookId")]
        [JsonIgnore]
        public virtual Book book { get; set; }
        public int BookId { get; set; }
        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        [JsonIgnore]
        public virtual Account Account { get; set; }

    }
}