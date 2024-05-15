using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TestRepo.Interface;

namespace TestRepo.DTO
{
    public class Volume : Ivol
    {
        public int VolumeId { get; set; }
        public int VolNumber { get; set; }
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        [JsonIgnore]
        public virtual Book Book { get; set; }

        public int totalPages { get; set; }

    }

}