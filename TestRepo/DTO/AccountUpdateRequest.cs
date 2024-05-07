using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRepo.DTO
{
    public class AccountUpdateRequest
    {
        public string? NewUsername { get; set; }
        public string? NewPassword { get; set; }
        public string? NewEmail { get; set; }
    }
}
