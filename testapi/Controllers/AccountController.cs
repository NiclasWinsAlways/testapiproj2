using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TestRepo.Data;
using TestRepo.DTO;
namespace testapi.Controllers
{
    public class EmailChangeRequest
    {
        public string NewEmail { get; set; }
    }
    public class PasswordChangeRequest
    {
        public string NewPassword { get; set; }
    }
    public class UsernameChangeRequest
    {
        public string NewUsername { get; set; }
    }
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DbAccess _dbAccess;

        public AccountController(DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }

        [HttpGet("list")]
        public IActionResult ListAccounts()
        {
            var accounts = _dbAccess.GetAllAccounts(); // Ensure you have a method to get all accounts
            if (accounts == null || !accounts.Any())
                return NotFound("No accounts found.");

            return Ok(accounts);
        }

        [HttpPost("create")]
        public IActionResult CreateAccount([FromBody] Account account)
        {
            _dbAccess.CreateAcc(account);
            return Created($"api/account/{account.Id}", account);
        }

        //GET: api/Accounts/GetAccInfo/1

        [HttpGet("GetAccInfo/{accountId}")]
        public IActionResult GetAccountInfo(int accountId)
        {
            var account = _dbAccess.GetAccInfo(accountId);
            if (account == null) return NotFound();
            return Ok(account);
        }

        // POST: api/Accounts/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var result = _dbAccess.CheckLogin(loginDto.Username, loginDto.Password);
            if (result == null)
            {
                return Unauthorized();
            }
            return Ok(result); // This will return both AccountId and IsAdmin
        }

        // FIX THIS SO I CAN UPDATE EMAIL AND PASSWORD + USERNAME LATER NO GET ID AND UPDATEACCOUNT IN DBACC
        [HttpPut("{accountId}/changeUsername")]
        public IActionResult ChangeUsername(int accountId, [FromBody] AccountUpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.NewUsername))
                return BadRequest(new { message = "NewUsername is required" });

            try
            {
                _dbAccess.ChangeUsername(accountId, request.NewUsername);
                return Ok(new { message = "Username updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpPut("{accountId}/changePassword")]
        public IActionResult ChangePassword(int accountId, [FromBody] AccountUpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.NewPassword))
                return BadRequest(new { message = "NewPassword is required" });

            try
            {
                _dbAccess.ChangePassword(accountId, request.NewPassword);
                return Ok(new { message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }



        [HttpPut("{accountId}/changeEmail")]
        public IActionResult ChangeEmail(int accountId, [FromBody] AccountUpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.NewEmail))
                return BadRequest(new { message = "NewEmail is required" });

            try
            {
                _dbAccess.ChangeEmail(accountId, request.NewEmail);
                return Ok(new { message = "Email updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        [HttpDelete("{accountId}")]
        public IActionResult DeleteAccount(int accountId)
        {
            try
            {
                _dbAccess.DeleteAcc(accountId);
                return NoContent();
            }
            catch (KeyNotFoundException knfe)
            {
                return NotFound(knfe.Message);
            }
            catch (Exception ex)
            {
                // Log the detailed exception
                return StatusCode(500, "Internal server error: " + ex.Message);
            }


            //altered version for password hashing
            #region
            //public object CheckLogin(string userName, string password)
            //{
            //    var account = _dbContext.Acc
            //        .SingleOrDefault(a => a.UserName == userName);

            //    if (account != null && VerifyPassword(account, password))
            //    {
            //        account.IsLoggedin = true;
            //        _dbContext.SaveChanges();

            //        return new { AccountId = account.Id, IsAdmin = account.IsAdmin };
            //    }

            //    return null;
            //}

            //public Account GetAccInfo(int accountId)
            //{
            //    return _dbContext.Acc.SingleOrDefault(b => b.Id == accountId);
            //}

            //public void CreateAcc(Account newAccount)
            //{
            //    if (newAccount == null)
            //        throw new ArgumentNullException(nameof(newAccount));

            //    if (string.IsNullOrEmpty(newAccount.Password))
            //        throw new ArgumentException("Password cannot be null or empty", nameof(newAccount));

            //    // Generate salt and hash the password
            //    string salt = GenerateSalt();
            //    newAccount.Salt = salt;
            //    newAccount.PasswordHash = HashPassword(newAccount.Password, salt);

            //    // Clear the plain text password before storing the account object
            //    newAccount.Password = newAccount.PasswordHash;

            //    _dbContext.Acc.Add(newAccount);
            //    _dbContext.SaveChanges();
            //}

            //public void DeleteAcc(int accountId)
            //{
            //    var account = _dbContext.Acc.SingleOrDefault(b => b.Id == accountId);
            //    if (account != null)
            //    {
            //        _dbContext.Acc.Remove(account);
            //        _dbContext.SaveChanges();
            //    }
            //}

            //public void ChangePassword(int accountId, string newPassword)
            //{
            //    var account = _dbContext.Acc.SingleOrDefault(a => a.Id == accountId);
            //    if (account != null)
            //    {
            //        account.Salt = GenerateSalt();
            //        account.PasswordHash = HashPassword(newPassword, account.Salt);
            //        _dbContext.SaveChanges();
            //    }
            //}

            //public void ChangeUsername(int accountId, string newUsername)
            //{
            //    var account = _dbContext.Acc.SingleOrDefault(a => a.Id == accountId);
            //    if (account != null)
            //    {
            //        account.UserName = newUsername;
            //        _dbContext.SaveChanges();
            //    }
            //}

            //public void ChangeEmail(int accountId, string newEmail)
            //{
            //    var account = _dbContext.Acc.SingleOrDefault(a => a.Id == accountId);
            //    if (account != null)
            //    {
            //        account.Email = newEmail;
            //        _dbContext.SaveChanges();
            //    }
            //}

            //public void UpdateAccount(Account account)
            //{
            //    _dbContext.Entry(account).State = EntityState.Modified;
            //    _dbContext.SaveChanges();
            //}

            //public Account GetAccountById(int accountId)
            //{
            //    return _dbContext.Acc.FirstOrDefault(a => a.Id == accountId);
            //}

            //public List<Account> GetAllAccounts()
            //{
            //    return _dbContext.Acc.ToList();
            //}

            //private string GenerateSalt()
            //{
            //    using (var rng = RandomNumberGenerator.Create())
            //    {
            //        byte[] saltBytes = new byte[16];
            //        rng.GetBytes(saltBytes);
            //        return Convert.ToBase64String(saltBytes);
            //    }
            //}

            //private string HashPassword(string password, string salt)
            //{
            //    using (var sha256 = SHA256.Create())
            //    {
            //        byte[] combinedBytes = Encoding.UTF8.GetBytes(password + salt);
            //        byte[] hashBytes = sha256.ComputeHash(combinedBytes);
            //        return Convert.ToBase64String(hashBytes);
            //    }
            //}

            //private bool VerifyPassword(Account account, string password)
            //{
            //    string hash = HashPassword(password, account.Salt);
            //    return hash == account.PasswordHash;
            //}

            #endregion
        }

    }
}


