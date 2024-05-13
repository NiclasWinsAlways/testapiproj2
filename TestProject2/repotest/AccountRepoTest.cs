using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;  // Ensure this namespace contains your DbAccess and custom Dbcontext classes.
using TestRepo.DTO;   // Ensure this namespace contains your Account DTO.

namespace TestProject2.repotest
{
    public class AccountTests : IDisposable  // Implement IDisposable for cleanup
    {
        private readonly DbAccess _dbAccess;
        private readonly Dbcontext _context;
        private int _initialCount; // Store initial count for cleanup

        public AccountTests()
        {
            var connectionString = "Server=localhost; Database=MyBookDB_Test; Integrated Security=True; TrustServerCertificate=True;"; // Use a separate test database
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);

            _initialCount = _context.Acc.Count(); // Store initial count
        }

        public void Dispose()
        {
            // Clean up - remove accounts added during tests
            var accountsToRemove = _context.Acc.Skip(_initialCount).ToList();
            _context.Acc.RemoveRange(accountsToRemove);
            _context.SaveChanges();
            _context.Dispose();
        }

        [Fact]
        public void CreateAcc_AddsAccountSuccessfully()
        {
            // Arrange
            var newAccount = new Account
            {
                UserName = "NewUser",
                Email = "newuser@example.com",
                Password = "password123",
                Name = "Test User" // Adding the required 'Name' field
            };

            // Act
            _dbAccess.CreateAcc(newAccount);

            // Assert
            var accountInDb = _context.Acc.FirstOrDefault(a => a.Email == "newuser@example.com");
            Assert.NotNull(accountInDb);
            Assert.Equal("NewUser", accountInDb.UserName);
            Assert.Equal("newuser@example.com", accountInDb.Email);
            Assert.Equal("password123", accountInDb.Password);
            Assert.Equal("Test User", accountInDb.Name); // Verify the 'Name' field is correctly set
        }

        [Fact]
        public void DeleteAcc_RemovesAccountCorrectly()
        {
            // Arrange - first add an account to delete
            var newAccount = new Account
            {
                UserName = "TestUser",
                Email = "test@example.com",
                Password = "testPass",
                Name = "Test Name"  // Ensure this required field is provided
            };

            _context.Acc.Add(newAccount);
            _context.SaveChanges();

            // Act - delete the account
            _dbAccess.DeleteAcc(newAccount.Id);

            // Assert - verify the account is no longer in the database
            var accountInDb = _context.Acc.FirstOrDefault(a => a.Id == newAccount.Id);
            Assert.Null(accountInDb);
        }

        [Fact]
        public void GetAllAccounts_ReturnsAllAccounts()
        {
            // Arrange
            var account1 = new Account { UserName = "User1", Email = "user1@example.com", Password = "pass1", Name = "Name1" };
            var account2 = new Account { UserName = "User2", Email = "user2@example.com", Password = "pass2", Name = "Name2" };
            _context.Acc.Add(account1);
            _context.Acc.Add(account2);
            _context.SaveChanges();

            // Act
            var accounts = _dbAccess.GetAllAccounts();

            // Assert
            Assert.Equal(_initialCount + 2, accounts.Count); // Adjust assertion to consider initial count
            Assert.Contains(accounts, a => a.Email == "user1@example.com");
            Assert.Contains(accounts, a => a.Email == "user2@example.com");
        }
    }
}
