using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRepo.DTO
{
    public class LoanDTO
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool Returned { get; set; }
        public double Progress { get; set; }
    }
}
