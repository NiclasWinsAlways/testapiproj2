//using System.Collections.Generic;
//using Xunit;
//using Microsoft.EntityFrameworkCore;
//using TestRepo.Data;
//using TestRepo.DTO;
//using System.Linq;

//namespace TestProject2.controllertest
//{
//    // Class to set up a shared context for all the tests in this file
//    public class BookControllerTestsFixture
//    {
//        public static Dbcontext Context { get; } = new Dbcontext(new DbContextOptionsBuilder<Dbcontext>()
//            .UseInMemoryDatabase(databaseName: "OurDummyDb").Options);

//        // Populate the database with sample data when the fixture is created
//        static BookControllerTestsFixture()
//        {
//            PopulateData();
//        }

//        private static void PopulateData()
//        {
//            var b1 = new Book { Id = 1, Author = "No Longer Human", Description = "test" };
//            var b2 = new Book { Id = 2, Author = "Another Book", Description = "another test" };
//            Context.Books.AddRange(b1, b2);
//            Context.SaveChanges();
//        }
//    }

//    // Class for the test cases
//    public class BookControllerTests
//    {
//        // Initialize the context with the shared context from the fixture
//        private readonly Dbcontext _context;

//        public BookControllerTests()
//        {
//            _context = BookControllerTestsFixture.Context;
//        }

//        #region Test cases

//        // Test case: GetAllBooks_ReturnAll
//        // Verify that the GetAllBooks method returns all the books in the database
//        [Fact]
//        public void GetAllBooks_ReturnAll()
//        {
//            // Act
//            var books = _context.Books.ToList();

//            // Assert
//            Assert.Equal(2, books.Count);
//            Assert.Contains(books, b => b.Author == "No Longer Human");
//            Assert.Contains(books, b => b.Author == "Another Book");
//        }

//        // Test case: GetBookById_ReturnsCorrectBook
//        // Verify that the GetBookById method returns the correct book for a given ID
//        [Fact]
//        public void GetBookById_ReturnsCorrectBook()
//        {
//            // Arrange
//            // Initialize the DbAccess class with the context
//            var dbAcc = new DbAccess(_context);

//            // Act
//            // Get the book with ID = 2
//            var bookId = 2;
//            var book = dbAcc.GetBookById(bookId);

//            // Assert
//            // Verify that the book is not null
//            Assert.NotNull(book);
//            // Verify that the book ID matches the given ID
//            Assert.Equal(bookId, book.Id);
//            // Verify that the book author matches the expected value
//            Assert.Equal("Another Book", book.Author);
//            // Verify that the book description matches the expected value
//            Assert.Equal("another test", book.Description);
//        }

//        #endregion
//    }
//}