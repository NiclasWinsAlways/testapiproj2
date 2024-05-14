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
        //[Fact]
        //public void ChangeEmail_ShouldUpdateEmail()
        //{
        //    SeedDatabase();
        //    var account = _context.Acc.First();
        //    var updateRequest = new AccountUpdateRequest { NewEmail = "updated@test.com" };

        //    var result = _controller.ChangeEmail(account.Id, updateRequest) as OkObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(200, result.StatusCode);

        //    dynamic response = result.Value;
        //    Assert.NotNull(response);

        //    // Ensure that the 'message' property exists and matches the expected value
        //    Assert.True(((IDictionary<string, object>)response).ContainsKey("message"), "Response object does not contain 'message' property.");
        //    Assert.Equal("Email updated successfully.", (string)response.message);

        //    // Verify update
        //    _context.Entry(account).Reload();
        //    Assert.Equal("updated@test.com", account.Email);
        //}

        //[Fact]
        //public void ChangeEmail_MissingEmail_ReturnsBadRequest()
        //{
        //    var accountId = 1; // Ensure this ID exists; you may need to seed it if not already present

        //    var result = _controller.ChangeEmail(accountId, new AccountUpdateRequest { NewEmail = "" }) as BadRequestObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(400, result.StatusCode);

        //    dynamic response = result.Value;
        //    Assert.NotNull(response);

        //    // Directly access the 'message' property on the dynamic type
        //    string message = response.message;
        //    Assert.Equal("NewEmail is required", message);
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
