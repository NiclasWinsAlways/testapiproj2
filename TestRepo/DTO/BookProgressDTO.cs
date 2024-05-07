using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRepo.DTO
{
    public class BookProgressDto
    {
        public int Id { get; set; }
        public int BookId { get; set; } // Include if you want to expose BookId
        public int AccountId { get; set; }
        public int volumesRead { get; set; }
        // Optionally include other properties you want to expose
    }
}
