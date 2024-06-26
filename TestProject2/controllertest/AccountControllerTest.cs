﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using TestRepo.Data;
using TestRepo.DTO;
using testapi.Controllers;

namespace TestProject2.controllertest
{
    // Test class for AccountController
    public class AccountControllerTests : IDisposable
    {
        private readonly DbContextOptions<Dbcontext> _contextOptions;
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;
        private readonly AccountController _controller;

        // Constructor to set up the test context and controller
        public AccountControllerTests()
        {
            // Configure in-memory database for testing
            _contextOptions = new DbContextOptionsBuilder<Dbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Use unique database name for each test run
                .Options;

            _context = new Dbcontext(_contextOptions);
            _dbAccess = new DbAccess(_context);
            _controller = new AccountController(_dbAccess);

            // Seed the in-memory database with initial data
            SeedDatabase();
        }

        // Method to seed the in-memory database with initial data
        private void SeedDatabase()
        {
            // Clear existing data and reset the change tracker
            _context.ChangeTracker.Clear();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed data
            var account = new Account { UserName = "test", Password = "test", Email = "test@test.com", Name = "Test User" };
            _context.Acc.Add(account);
            _context.SaveChanges();
        }

        // Dispose method to clean up after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Test to verify that ListAccounts returns a non-empty result
        [Fact]
        public void ListAccounts_ShouldReturnNotEmptyResult()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.ListAccounts() as OkObjectResult;

            Assert.NotNull(result);
            var accounts = result.Value as List<Account>;
            Assert.NotNull(accounts);
            Assert.True(accounts.Count > 0);
        }

        // Test to verify that CreateAccount returns a CreatedResult
        [Fact]
        public void CreateAccount_ShouldReturnCreatedResult()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var newAccount = new Account { UserName = "newuser", Password = "newpass", Email = "newuser@test.com", Name = "New User" };

            var result = _controller.CreateAccount(newAccount) as CreatedResult;

            Assert.NotNull(result);
            Assert.Equal($"api/account/{newAccount.Id}", result.Location);
        }

        // Test to verify that GetAccountInfo returns NotFound for an invalid ID
        [Fact]
        public void GetAccountInfo_ShouldReturnNotFoundForInvalidId()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var result = _controller.GetAccountInfo(-1); // Assuming -1 is not a valid ID

            Assert.IsType<NotFoundResult>(result);
        }

        // Test to verify that ChangeUsername returns OkResult for a valid username
        [Fact]
        public void ChangeUsername_WhenUsernameIsValid_ReturnsOkResult()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var updateRequest = new AccountUpdateRequest { NewUsername = "newUsername" };

            var account = _context.Acc.FirstOrDefault();
            Assert.NotNull(account);

            var result = _controller.ChangeUsername(account.Id, updateRequest) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // Test to verify that ChangeUsername returns BadRequest for an invalid username
        [Fact]
        public void ChangeUsername_WhenUsernameIsInvalid_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var updateRequest = new AccountUpdateRequest { NewUsername = "" };

            var account = _context.Acc.FirstOrDefault();
            Assert.NotNull(account);

            var result = _controller.ChangeUsername(account.Id, updateRequest) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        // Test to verify that ChangePassword returns OkResult for a valid request
        [Fact]
        public void ChangePassword_WithValidRequest_ReturnsOk()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var request = new AccountUpdateRequest { NewPassword = "newSecurePassword123" };

            var account = _context.Acc.FirstOrDefault();
            Assert.NotNull(account);

            var result = _controller.ChangePassword(account.Id, request) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Password updated successfully.", result.Value.ToString());
        }

        // Test to verify that ChangePassword returns BadRequest for an empty password
        [Fact]
        public void ChangePassword_WithEmptyPassword_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var request = new AccountUpdateRequest { NewPassword = "" };

            var account = _context.Acc.FirstOrDefault();
            Assert.NotNull(account);

            var result = _controller.ChangePassword(account.Id, request) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("NewPassword is required", result.Value.ToString());
        }

        // Test to verify that ChangeEmail returns OkResult for a valid request
        [Fact]
        public void ChangeEmail_WithValidRequest_ReturnsOk()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var request = new AccountUpdateRequest { NewEmail = "newemail@example.com" };

            var account = _context.Acc.FirstOrDefault();
            Assert.NotNull(account);

            var result = _controller.ChangeEmail(account.Id, request) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Email updated successfully.", result.Value.ToString());
        }

        // Test to verify that ChangeEmail returns BadRequest for an empty email
        [Fact]
        public void ChangeEmail_WithEmptyEmail_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var request = new AccountUpdateRequest { NewEmail = "" };

            var account = _context.Acc.FirstOrDefault();
            Assert.NotNull(account);

            var result = _controller.ChangeEmail(account.Id, request) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("NewEmail is required", result.Value.ToString());
        }

        // Test to verify that DeleteAccount returns NoContent when successful
        [Fact]
        public void DeleteAccount_WhenSuccessful_ReturnsNoContent()
        {
            SeedDatabase(); // Ensure fresh data for each test

            var account = _context.Acc.FirstOrDefault();
            Assert.NotNull(account);

            var result = _controller.DeleteAccount(account.Id) as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(204, result.StatusCode);
        }

        // Uncomment and complete the test to verify that DeleteAccount returns NotFound for a non-existent account
        //[Fact]
        //public void DeleteAccount_WhenAccountDoesNotExist_ReturnsNotFound()
        //{
        //    var accountId = 999;  // Assuming this ID does not exist

        //    var result = _controller.DeleteAccount(accountId) as NotFoundObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(404, result.StatusCode);
        //    Assert.Contains("not found", result.Value.ToString());
        //}
    }
}
