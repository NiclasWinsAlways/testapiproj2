using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;
using TestRepo.DTO;
using Microsoft.EntityFrameworkCore.Storage;

namespace TestProject2.repotest
{
    public class AccountTests : IDisposable
    {
        private readonly DbAccess _dbAccess;
        private readonly Dbcontext _context; // Ensure the context class name matches your actual DbContext class.
        private List<Account> _initialData;  // This must be declared at the class level
        private readonly IDbContextTransaction _transaction;

        public AccountTests()
        {
            var connectionString = "Data Source=(localdb)\\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
            // Use DbContextOptions specific to your Dbcontext class.
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer(connectionString)
                .Options;

            // Initialize your specific DbContext with the correct options.
            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);

            _transaction = _context.Database.BeginTransaction();  // Start transaction
            _initialData = new List<Account>();  // Initialize the list here
            PrepareInitialData();
        }


        private void PrepareInitialData()
        {
            if (!_context.Acc.Any())  // Check if the database is empty
            {
                var account = new Account
                {
                    UserName = "initialUser",
                    Password = "initialPassword",
                    Email = "initial@example.com",
                    Name = "Initial Name",
                    IsAdmin = false,
                    IsLoggedin = false
                };
                _context.Acc.Add(account);
                _context.SaveChanges();
            }
            _initialData = _context.Acc.ToList();  // Always refresh _initialData from current DB state
        }



        public void Dispose()
        {
            _transaction.Rollback();
            _transaction.Dispose();
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
            if (firstAccount == null)
            {
                Assert.Fail("No accounts available for testing.");
                return;
            }

            var account = _dbAccess.GetAccountById(firstAccount.Id);
            Assert.NotNull(account);
            // Ensure the expected username is correctly set to match the actual username in the database
            Assert.Equal(firstAccount.UserName, account.UserName); // Adjusted to dynamically match the first account's username
        }

        [Fact]
        public void UpdateAccount_UpdatesSuccessfully()
        {
            var firstAccount = _initialData.FirstOrDefault();
            if (firstAccount == null)
            {
                Assert.Fail("No account found for testing.");
                return;
            }

            // Retrieve the account with no tracking to avoid conflicts
            var account = _context.Acc.AsNoTracking().FirstOrDefault(a => a.Id == firstAccount.Id);
            if (account == null)
            {
                Assert.Fail("No account found with ID " + firstAccount.Id);
                return;
            }

            // Modify the account details
            account.Email = "updated@example.com";

            // Clear the DbContext local cache before updating
            _context.ChangeTracker.Clear(); // This ensures no tracked entities are conflicting

            // Update the account
            _context.Acc.Update(account);
            _context.SaveChanges();

            // Verify the update
            var updatedAccount = _context.Acc.AsNoTracking().FirstOrDefault(a => a.Id == firstAccount.Id);
            Assert.Equal("updated@example.com", updatedAccount.Email);
        }

        [Fact]
        public void ChangePassword_UpdatesPasswordSuccessfully()
        {
            var firstAccount = _initialData.FirstOrDefault();
            if (firstAccount == null)
            {
                Assert.Fail("No accounts available for testing.");
                return;
            }

            var account = _context.Acc.FirstOrDefault(a => a.Id == firstAccount.Id);
            if (account == null)
            {
                Assert.Fail("No account found with ID " + firstAccount.Id);
                return;
            }

            var newPassword = "NewPassword123";
            account.Password = newPassword;
            _context.SaveChanges();

            var updatedAccount = _context.Acc.AsNoTracking().FirstOrDefault(a => a.Id == account.Id);
            Assert.Equal(newPassword, updatedAccount.Password);
        }
        [Fact]
        public void ChangeUsername_UpdatesUsernameSuccessfully()
        {
            var firstAccount = _initialData.FirstOrDefault();
            if (firstAccount == null)
            {
                Assert.Fail("No accounts available for testing.");
                return;
            }

            var newUsername = "NewUsername";
            _dbAccess.ChangeUsername(firstAccount.Id, newUsername);

            var updatedAccount = _context.Acc.Find(firstAccount.Id);
            Assert.Equal(newUsername, updatedAccount.UserName);
        }

        [Fact]
        public void ChangeEmail_UpdatesEmailSuccessfully()
        {
            var account = _context.Acc.Find(_initialData.First().Id); // Use the first available account ID
            if (account == null)
            {
                Assert.Fail("No account found with expected ID.");
                return;
            }

            var newEmail = "new@example.com";
            _dbAccess.ChangeEmail(account.Id, newEmail);

            var updatedAccount = _context.Acc.Find(account.Id);
            Assert.Equal(newEmail, updatedAccount.Email);
        }

        //[Fact]
        //public void CheckLogin_ValidCredentials_ReturnsAccountInfo()
        //{
        //    var firstAccount = _initialData.FirstOrDefault();
        //    Assert.NotNull(firstAccount); // Ensure there is an account to test

        //    var result = _dbAccess.CheckLogin(firstAccount.UserName, firstAccount.Password);
        //    Assert.NotNull(result); // Ensure a result is returned

        //    // Using reflection to check property existence before accessing
        //    var resultProperties = result.GetType().GetProperties().Select(p => p.Name).ToList();

        //    // Manually check for the properties and assert with a message if not found
        //    Assert.True(resultProperties.Contains("IsAdmin"), "Result object does not contain 'IsAdmin' property.");
        //    Assert.True(resultProperties.Contains("AccountId"), "Result object does not contain 'AccountId' property.");

        //    // Safely casting to dynamic after confirming properties exist
        //    dynamic loginResult = result;
        //    Assert.False(loginResult.IsAdmin);
        //    Assert.Equal(firstAccount.Id, loginResult.AccountId);
        //}



        [Fact]
        public void CheckLogin_InvalidCredentials_ReturnsNull()
        {
            var result = _dbAccess.CheckLogin("wrongUser", "wrongPassword");
            Assert.Null(result);
        }
    }
}
