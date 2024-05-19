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


            //altered version for password hashing
            #region
            //[HttpGet("list")]
            //public IActionResult ListAccounts()
            //{
            //    var accounts = _dbAccess.GetAllAccounts();
            //    if (accounts == null || !accounts.Any())
            //        return NotFound("No accounts found.");

            //    return Ok(accounts);
            //}

            //[HttpPost("create")]
            //public IActionResult CreateAccount([FromBody] Account account)
            //{
            //    if (string.IsNullOrEmpty(account.Password))
            //        return BadRequest(new { message = "Password is required" });

            //    _dbAccess.CreateAcc(account);
            //    return Created($"api/account/{account.Id}", new { account.Id });
            //}

            //[HttpGet("GetAccInfo/{accountId}")]
            //public IActionResult GetAccountInfo(int accountId)
            //{
            //    var account = _dbAccess.GetAccInfo(accountId);
            //    if (account == null) return NotFound();
            //    return Ok(account);
            //}

            //[HttpPost("login")]
            //public IActionResult Login([FromBody] LoginDto loginDto)
            //{
            //    var result = _dbAccess.CheckLogin(loginDto.Username, loginDto.Password);
            //    if (result == null)
            //    {
            //        return Unauthorized();
            //    }
            //    return Ok(result); // This will return both AccountId and IsAdmin
            //}

            //[HttpPut("{accountId}/changeUsername")]
            //public IActionResult ChangeUsername(int accountId, [FromBody] UsernameChangeRequest request)
            //{
            //    if (string.IsNullOrEmpty(request.NewUsername))
            //        return BadRequest(new { message = "NewUsername is required" });

            //    try
            //    {
            //        _dbAccess.ChangeUsername(accountId, request.NewUsername);
            //        return Ok(new { message = "Username updated successfully." });
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            //    }
            //}

            //[HttpPut("{accountId}/changePassword")]
            //public IActionResult ChangePassword(int accountId, [FromBody] PasswordChangeRequest request)
            //{
            //    if (string.IsNullOrEmpty(request.NewPassword))
            //        return BadRequest(new { message = "NewPassword is required" });

            //    try
            //    {
            //        _dbAccess.ChangePassword(accountId, request.NewPassword);
            //        return Ok(new { message = "Password updated successfully." });
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            //    }
            //}

            //[HttpPut("{accountId}/changeEmail")]
            //public IActionResult ChangeEmail(int accountId, [FromBody] EmailChangeRequest request)
            //{
            //    if (string.IsNullOrEmpty(request.NewEmail))
            //        return BadRequest(new { message = "NewEmail is required" });

            //    try
            //    {
            //        _dbAccess.ChangeEmail(accountId, request.NewEmail);
            //        return Ok(new { message = "Email updated successfully." });
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            //    }
            //}

            //[HttpDelete("{accountId}")]
            //public IActionResult DeleteAccount(int accountId)
            //{
            //    try
            //    {
            //        _dbAccess.DeleteAcc(accountId);
            //        return NoContent();
            //    }
            //    catch (KeyNotFoundException knfe)
            //    {
            //        return NotFound(knfe.Message);
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, "Internal server error: " + ex.Message);
            //    }
                #endregion
            }

    }
}


