using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;
using TestRepo.DTO;
using System.Linq;
#region
//ARRANGE VARIABLE CREATION ETC / WHAT DO WE WANT TO TEST 
//ACT - CALL METHOD
//ASSERT VERIFY I GOT THE RIGHT CALL BACK
#endregion
namespace TestProject2.controllertest
{
    // Class to set up a shared context for all the tests in this file
    public class BookRepoTestsFixture
    {
        public static Dbcontext Context { get; private set; }

        static BookRepoTestsFixture()
        {
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer("Server=localhost; Database=MyBookDB_Test; Trusted_Connection=True; TrustServerCertificate=True;") // Example connection string
                .Options;

            Context = new Dbcontext(options);
            PopulateData();
        }

        private static void PopulateData()
        {
            // Ensure database is clean before populating
            Context.Database.EnsureCreated();

            var b1 = new Book { Id = 1, Author = "No Longer Human", Description = "test" };
            var b2 = new Book { Id = 2, Author = "Another Book", Description = "another test" };
            Context.Books.AddRange(b1, b2);
            Context.SaveChanges();
        }
    }

    // Class for the test cases
    public class BookRepoTests : IDisposable
    {
        private readonly Dbcontext _context;

        public BookRepoTests()
        {
            _context = BookRepoTestsFixture.Context;
        }

        public void Dispose()
        {
            // Clean up test data to avoid side effects between tests
            // Since we're using a separate test database, we don't need to delete anything
        }

        #region Test cases

        // Test case: GetAllBooks_ReturnAll
        [Fact]
        public void GetAllBooks_ReturnAll()
        {
            // Act
            var books = _context.Books.ToList();

            // Assert
            Assert.Equal(2, books.Count);
            Assert.Contains(books, b => b.Author == "No Longer Human");
            Assert.Contains(books, b => b.Author == "Another Book");
        }

        // Test case: GetBookById_ReturnsCorrectBook
        [Fact]
        public void GetBookById_ReturnsCorrectBook()
        {
            // Arrange
            var dbAcc = new DbAccess(_context);

            // Act
            var bookId = 2;
            var book = dbAcc.GetBookById(bookId);

            // Assert
            Assert.NotNull(book);
            Assert.Equal(bookId, book.Id);
            Assert.Equal("Another Book", book.Author);
            Assert.Equal("another test", book.Description);
        }

        #endregion
    }
}


