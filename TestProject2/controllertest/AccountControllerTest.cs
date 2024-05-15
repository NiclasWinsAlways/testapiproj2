using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using TestRepo.Data;
using TestRepo.DTO;
using testapi.Controllers;
using Microsoft.CSharp.RuntimeBinder;

namespace TestProject2.controllertest
{
    public class AccountControllerTests : IDisposable
    {
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;
        private readonly AccountController _controller;
        private IDbContextTransaction _transaction;

        public AccountControllerTests()
        {
            var connectionString = @"Data Source=(localdb)\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);
            _controller = new AccountController(_dbAccess);

            // Start a transaction for each test
            _transaction = _context.Database.BeginTransaction();
        }

        public void Dispose()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            //ResetIdentity("Accounts"); comment this out when needed
            _context.Dispose();
        }


        private void SeedDatabase()
        {
            if (!_context.Acc.Any())
            {
                var account = new Account { UserName = "test", Password = "test", Email = "test@test.com", Name = "Test User" };
                _context.Acc.Add(account);
                _context.SaveChanges();
            }
        }


        [Fact]
        public void ListAccounts_ShouldReturnNotEmptyResult()
        {
            SeedDatabase(); // Seed the database with test data

            var result = _controller.ListAccounts() as OkObjectResult;

            Assert.NotNull(result);
            var accounts = result.Value as List<Account>;
            Assert.True(accounts.Count > 0);
        }

        [Fact]
        public void CreateAccount_ShouldReturnCreatedResult()
        {
            var newAccount = new Account { UserName = "newuser", Password = "newpass", Email = "newuser@test.com", Name = "New User" };

            var result = _controller.CreateAccount(newAccount) as CreatedResult;

            Assert.NotNull(result);
            Assert.Equal($"api/account/{newAccount.Id}", result.Location);
        }

        [Fact]
        public void GetAccountInfo_ShouldReturnNotFoundForInvalidId()
        {
            SeedDatabase();
            var result = _controller.GetAccountInfo(-1); // Assuming -1 is not a valid ID

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void ChangeUsername_WhenUsernameIsValid_ReturnsOkResult()
        {
            SeedDatabase();
            var updateRequest = new AccountUpdateRequest { NewUsername = "newUsername" };

            var result = _controller.ChangeUsername(1, updateRequest) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void ChangeUsername_WhenUsernameIsInvalid_ReturnsBadRequest()
        {
            SeedDatabase();
            var updateRequest = new AccountUpdateRequest { NewUsername = "" };

            var result = _controller.ChangeUsername(1, updateRequest) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public void ChangePassword_WithValidRequest_ReturnsOk()
        {
            // Seed database with a sample account
            SeedDatabase();
            var accountId = 1;  // Assuming this ID exists
            var request = new AccountUpdateRequest { NewPassword = "newSecurePassword123" };

            var result = _controller.ChangePassword(accountId, request) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Password updated successfully.", result.Value.ToString());
        }

        [Fact]
        public void ChangePassword_WithEmptyPassword_ReturnsBadRequest()
        {
            var accountId = 1;  // Assuming this ID exists
            var request = new AccountUpdateRequest { NewPassword = "" };

            var result = _controller.ChangePassword(accountId, request) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("NewPassword is required", result.Value.ToString());
        }

        [Fact]
        public void ChangeEmail_WithValidRequest_ReturnsOk()
        {
            // Seed database with a sample account
            SeedDatabase();
            var accountId = 1;  // Assuming this ID exists
            var request = new AccountUpdateRequest { NewEmail = "newemail@example.com" };

            var result = _controller.ChangeEmail(accountId, request) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Email updated successfully.", result.Value.ToString());
        }

        [Fact]
        public void ChangeEmail_WithEmptyEmail_ReturnsBadRequest()
        {
            var accountId = 1;  // Assuming this ID exists
            var request = new AccountUpdateRequest { NewEmail = "" };

            var result = _controller.ChangeEmail(accountId, request) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("NewEmail is required", result.Value.ToString());
        }

        [Fact]
        public void DeleteAccount_WhenSuccessful_ReturnsNoContent()
        {
            // Seed database with a sample account
            SeedDatabase();
            var accountId = 1;  // Assuming this ID exists

            var result = _controller.DeleteAccount(accountId) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }
        
        //[Fact]
        //public void DeleteAccount_WhenAccountDoesNotExist_ReturnsNotFound()
        //{
        //    var accountId = 999;  // Assuming this ID does not exist

        //    var result = _controller.DeleteAccount(accountId) as NotFoundObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(404, result.StatusCode);
        //    Assert.Contains("not found", result.Value.ToString());
        //}

        private void ResetIdentity(string tableName)
        {
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT ('{tableName}', RESEED, 0);");
            }
        }
    }
  }
