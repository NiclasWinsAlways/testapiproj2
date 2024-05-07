using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

//FIX PASSWORD HASHING POTENTIAL ISSUES LATER
namespace TestRepo.Data
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            string newHashedPassword = HashPassword(password);
            return hashedPassword == newHashedPassword;
        }
    }
}