using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
        }

    }
}


//check up on this later this goes into dbacc
//public void ChangePassword(int accountId, string newPassword)
//{
//    // Check if the account exists
//    var account = _dbContext.Acc.Find(accountId);
//    if (account == null)
//    {
//        throw new ArgumentException("Account does not exist.", nameof(accountId));
//    }

//    // Hash the new password
//    string hashedPassword = PasswordHasher.HashPassword(newPassword);

//    // Update the password in the database
//    account.PasswordHash = hashedPassword;
//    _dbContext.Acc.Update(account);
//    _dbContext.SaveChanges();
//}

//and this
//    To update the CheckLogin method to work with hashed passwords, you can modify it to use the VerifyHashedPassword method from the PasswordHasher class.

//Here's an example of how you can modify the CheckLogin method:

//csharp
//Edit
//Full Screen
//Copy code
//public Account CheckLogin(string userName, string password)
//{
//    // Check if the account exists
//    var account = _dbContext.Acc.SingleOrDefault(a => a.UserName == userName);
//    if (account == null)
//    {
//        return null;
//    }

//    // Verify the hashed password
//    if (PasswordHasher.VerifyHashedPassword(account.PasswordHash, password))
//    {
//        return account;
//    }

//    return null;
//}