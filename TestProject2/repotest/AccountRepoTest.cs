using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;
using TestRepo.DTO;

namespace TestProject2.repotest
{
    public class AccountTests : IDisposable
    {
        // Declare the context and dbAccess objects
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;
        private List<Account> _initialData;

        public AccountTests()
        {
            // Set up the in-memory database options
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Initialize the context and dbAccess with the in-memory database
            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);

            // Seed the in-memory database with initial data
            SeedDatabase();
        }

        // Seed the database with initial data for testing
        private void SeedDatabase()
        {
            // Ensure the database is clean before seeding
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Create initial account data
            var accounts = new List<Account>
            {
                new Account
                {
                    Id = 1,
                    UserName = "TestUser",
                    Password = "initialPassword",
                    Email = "test1@example.com",
                    Name = "Initial Name",
                    IsAdmin = false,
                    IsLoggedin = false
                },
                new Account
                {
                    Id = 2,
                    UserName = "secondUser",
                    Password = "secondPassword",
                    Email = "second@example.com",
                    Name = "Second Name",
                    IsAdmin = false,
                    IsLoggedin = false
                }
            };

            // Add accounts to the context and save changes
            _context.Acc.AddRange(accounts);
            _context.SaveChanges();

            // Store the initial data for use in tests
            _initialData = _context.Acc.ToList();
        }

        // Clean up the database after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        // Test that GetAllAccounts returns all accounts
        public void GetAllAccounts_ReturnsAllAccounts()
        {
            var accounts = _dbAccess.GetAllAccounts();
            Assert.Equal(_initialData.Count, accounts.Count);
        }

        [Fact]
        // Test that GetAccountById returns the correct account
        public void GetAccountById_ReturnsCorrectAccount()
        {
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            var account = _dbAccess.GetAccountById(firstAccount.Id);
            Assert.NotNull(account);

            Assert.Equal(firstAccount.UserName, account.UserName);
        }

        [Fact]
        // Test that UpdateAccount updates an account successfully
        public void UpdateAccount_UpdatesSuccessfully()
        {
            // Arrange: Get the first account and modify its email
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            var account = _context.Acc.AsNoTracking().FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(account);

            account.Email = "updated@example.com";

            // Act: Update the account using dbAccess
            _dbAccess.UpdateAccount(account);

            // Clear the change tracker to avoid conflicts
            _context.ChangeTracker.Clear();

            // Assert: Verify the account was updated
            var updatedAccount = _context.Acc.AsNoTracking().FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal("updated@example.com", updatedAccount.Email);
        }

        [Fact]
        // Test that ChangePassword updates the password successfully
        public void ChangePassword_UpdatesPasswordSuccessfully()
        {
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            // Ensure the account is being tracked
            var account = _context.Acc.FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(account);

            var newPassword = "NewPassword123";
            _dbAccess.ChangePassword(firstAccount.Id, newPassword);

            // Clear the change tracker to avoid conflicts
            _context.ChangeTracker.Clear();

            // Assert: Verify the password was updated
            var updatedAccount = _context.Acc.AsNoTracking().FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal(newPassword, updatedAccount.Password);
        }

        [Fact]
        // Test that ChangeUsername updates the username successfully
        public void ChangeUsername_UpdatesUsernameSuccessfully()
        {
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            var newUsername = "NewUsername";
            _dbAccess.ChangeUsername(firstAccount.Id, newUsername);

            // Clear the change tracker to avoid conflicts
            _context.ChangeTracker.Clear();

            // Assert: Verify the username was updated
            var updatedAccount = _context.Acc.AsNoTracking().FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal(newUsername, updatedAccount.UserName);
        }

        [Fact]
        // Test that ChangeEmail updates the email successfully
        public void ChangeEmail_UpdatesEmailSuccessfully()
        {
            // Arrange: Get the first account and define a new email
            var account = _initialData.FirstOrDefault();
            Assert.NotNull(account);

            var newEmail = "new@example.com";

            // Act: Change the email using dbAccess
            _dbAccess.ChangeEmail(account.Id, newEmail);

            // Clear the change tracker to avoid conflicts
            _context.ChangeTracker.Clear();

            // Assert: Verify the email was updated
            var updatedAccount = _context.Acc.AsNoTracking().SingleOrDefault(a => a.Id == account.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal(newEmail, updatedAccount.Email);
        }

        [Fact]
        // Test that CheckLogin returns null for invalid credentials
        public void CheckLogin_InvalidCredentials_ReturnsNull()
        {
            var result = _dbAccess.CheckLogin("wrongUser", "wrongPassword");
            Assert.Null(result);
        }

        [Fact]
        // Test that CheckLogin returns account info for valid credentials
        public void CheckLogin_ValidCredentials_ReturnsAccountInfo()
        {
            // Arrange: Get the first account
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            // Act: Check login with valid credentials
            var result = _dbAccess.CheckLogin(firstAccount.UserName, firstAccount.Password);

            // Assert: Verify the result is correct
            Assert.NotNull(result);
            if (result != null)
            {
                var accountId = (int)result.GetType().GetProperty("AccountId").GetValue(result, null);
                var isAdmin = (bool)result.GetType().GetProperty("IsAdmin").GetValue(result, null);

                Assert.Equal(firstAccount.Id, accountId);
                Assert.Equal(firstAccount.IsAdmin, isAdmin);

                // Clear the change tracker to ensure fresh data fetch
                _context.ChangeTracker.Clear();

                // Verify the IsLoggedin property
                var loggedInAccount = _context.Acc.AsNoTracking().SingleOrDefault(a => a.Id == firstAccount.Id);
                Assert.NotNull(loggedInAccount);
                Assert.False(loggedInAccount.IsLoggedin);
            }
        }
    }
}
