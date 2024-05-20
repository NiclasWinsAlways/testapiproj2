using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestRepo.DTO;

namespace TestRepo.Models
    {
        public class Loan
        {
            public int Id { get; set; }
            public int AccountId { get; set; }
            public int BookId { get; set; }
            public DateTime LoanDate { get; set; }
            public DateTime DueDate { get; set; }
            public bool Returned { get; set; }
            public double Progress { get; set; } // Assuming there's a progress field

            public Book Book { get; set; }
        public Account Account { get; set; } // Ensure this navigation property exists

    }
}



