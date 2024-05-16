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
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;
        private List<Account> _initialData;

        public AccountTests()
        {
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);

            SeedDatabase();  // Initialize the database
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

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

            _context.Acc.AddRange(accounts);
            _context.SaveChanges();

            _initialData = _context.Acc.ToList();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void GetAllAccounts_ReturnsAllAccounts()
        {
            var accounts = _dbAccess.GetAllAccounts();
            Assert.Equal(_initialData.Count, accounts.Count);
        }

        [Fact]
        public void GetAccountById_ReturnsCorrectAccount()
        {
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            var account = _dbAccess.GetAccountById(firstAccount.Id);
            Assert.NotNull(account);

            Assert.Equal(firstAccount.UserName, account.UserName);
        }

        [Fact]
        public void UpdateAccount_UpdatesSuccessfully()
        {
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            var account = _context.Acc.FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(account);

            account.Email = "updated@example.com";

            _context.ChangeTracker.Clear(); // Clear the change tracker to avoid conflicts

            _dbAccess.UpdateAccount(account);  // Update account using DbAccess

            var updatedAccount = _context.Acc.FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal("updated@example.com", updatedAccount.Email);
        }

        [Fact]
        public void ChangePassword_UpdatesPasswordSuccessfully()
        {
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            var account = _context.Acc.FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(account);

            var newPassword = "NewPassword123";
            _dbAccess.ChangePassword(firstAccount.Id, newPassword);

            _context.ChangeTracker.Clear(); // Clear the change tracker to avoid conflicts
            var updatedAccount = _context.Acc.FirstOrDefault(a => a.Id == account.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal(newPassword, updatedAccount.Password);
        }

        [Fact]
        public void ChangeUsername_UpdatesUsernameSuccessfully()
        {
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            var newUsername = "NewUsername";
            _dbAccess.ChangeUsername(firstAccount.Id, newUsername);

            _context.ChangeTracker.Clear(); // Clear the change tracker to avoid conflicts
            var updatedAccount = _context.Acc.FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal(newUsername, updatedAccount.UserName);
        }

        [Fact]
        public void ChangeEmail_UpdatesEmailSuccessfully()
        {
            var account = _initialData.FirstOrDefault();
            Assert.NotNull(account);

            var newEmail = "new@example.com";
            _dbAccess.ChangeEmail(account.Id, newEmail);

            _context.ChangeTracker.Clear(); // Clear the change tracker to avoid conflicts
            var updatedAccount = _context.Acc.FirstOrDefault(a => a.Id == account.Id);
            Assert.NotNull(updatedAccount);
            Assert.Equal(newEmail, updatedAccount.Email);
        }

        [Fact]
        public void CheckLogin_InvalidCredentials_ReturnsNull()
        {
            var result = _dbAccess.CheckLogin("wrongUser", "wrongPassword");
            Assert.Null(result);
        }

        [Fact]
        public void CheckLogin_ValidCredentials_ReturnsAccountInfo()
        {
            var firstAccount = _initialData.FirstOrDefault();
            Assert.NotNull(firstAccount);

            var result = _dbAccess.CheckLogin(firstAccount.UserName, firstAccount.Password);
            Assert.NotNull(result);

            if (result != null)
            {
                var accountId = (int)result.GetType().GetProperty("AccountId").GetValue(result, null);
                var isAdmin = (bool)result.GetType().GetProperty("IsAdmin").GetValue(result, null);

                Assert.Equal(firstAccount.Id, accountId);
                Assert.Equal(firstAccount.IsAdmin, isAdmin);

                var loggedInAccount = _context.Acc.FirstOrDefault(a => a.Id == firstAccount.Id);
                Assert.NotNull(loggedInAccount);
                Assert.True(loggedInAccount.IsLoggedin);
            }
        }
    }
}
