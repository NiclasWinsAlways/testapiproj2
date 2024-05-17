using Microsoft.AspNetCore.Mvc;
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
    // Test class for BookController
    public class BookControllerTests : IDisposable
    {
        private readonly DbContextOptions<Dbcontext> _contextOptions;
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;
        private readonly BookController _controller;

        // Constructor to set up the test context and controller
        public BookControllerTests()
        {
            // Configure in-memory database for testing
            _contextOptions = new DbContextOptionsBuilder<Dbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Use unique database name for each test run
                .Options;

            _context = new Dbcontext(_contextOptions);
            _dbAccess = new DbAccess(_context);
            _controller = new BookController(_dbAccess);

            // Seed the in-memory database with initial data
            SeedDatabase();
        }

        // Method to seed the in-memory database with initial data
        private void SeedDatabase()
        {
            // Ensure the database is clean before seeding
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Clear the change tracker to avoid tracking issues
            _context.ChangeTracker.Clear();

            // Add test data
            _context.Books.AddRange(
                new Book { Id = 1, Title = "Sample Book", Author = "Author Test", IsLoaned = false },
                new Book { Id = 2, Title = "Another Book", Author = "Another Author", IsLoaned = true }
            );
            _context.SaveChanges();
        }

        // Dispose method to clean up after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Test to verify that GetBookByTitle returns the correct book for a valid title
        [Fact]
        public void GetBookByTitle_ValidTitle_ReturnsBook()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var titleRequest = new TitleRequest { title = "Sample Book" };
            var result = _controller.GetBookByTitle(titleRequest) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var book = result.Value as Book;
            Assert.NotNull(book);
            Assert.Equal("Sample Book", book.Title);
            Assert.Equal("Author Test", book.Author);
        }

        // Test to verify that GetAllBooks returns a list of books when available
        [Fact]
        public void GetAllBooks_ShouldReturnBooksWhenAvailable()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.GetAllBooks() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var books = result.Value as List<Book>;
            Assert.True(books.Count > 0);
        }

        // Test to verify that GetAllBooks returns NotFound when no books are available
        [Fact]
        public void GetAllBooks_ShouldReturnNotFoundWhenNoBooksAvailable()
        {
            // Clear all books
            _context.Books.RemoveRange(_context.Books);
            _context.SaveChanges();

            var result = _controller.GetAllBooks();

            Assert.NotNull(result);
            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("No books found.", notFoundResult.Value);
        }

        // Test to verify that GetBookById returns the correct book when it exists
        [Fact]
        public void GetBookById_BookExists_ReturnsOkWithBook()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var existingBook = _context.Books.First();
            var result = _controller.GetBookById(existingBook.Id) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var book = result.Value as Book;
            Assert.NotNull(book);
            Assert.Equal(existingBook.Id, book.Id);
        }

        // Test to verify that GetBookById returns NotFound when the book does not exist
        [Fact]
        public void GetBookById_BookDoesNotExist_ReturnsNotFound()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var nonExistentId = 9999;
            var result = _controller.GetBookById(nonExistentId) as NotFoundResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        // Test to verify that AddBook returns CreatedAtActionResult for a valid book
        [Fact]
        public void AddBook_WithValidBook_ReturnsCreatedResult()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var newBook = new Book { Title = "New Book", Author = "New Author" };
            var result = _controller.AddBook(newBook) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            var book = result.Value as Book;
            Assert.NotNull(book);
            Assert.Equal(newBook.Title, book.Title);
        }

        // Test to verify that AddBook returns BadRequest when the book is null
        [Fact]
        public void AddBook_WithNullBook_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.AddBook(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Book data is required.", result.Value);
        }

        // Test to verify that GetBookByTitle returns NotFound for an invalid title
        [Fact]
        public void GetBookByTitle_InvalidTitle_ReturnsNotFound()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var titleRequest = new TitleRequest { title = "Nonexistent Book" };
            var result = _controller.GetBookByTitle(titleRequest) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No book found with title", result.Value.ToString());
        }

        // Test to verify that LoanBook returns Ok when the book exists and is loanable
        [Fact]
       
        public void LoanBook_BookExistsAndLoanable_ReturnsOk()
        {
            SeedDatabase(); // Ensure fresh data for each test
            int accountId = 1; // Specify a valid accountId
            int bookId = _context.Books.First().Id;
            DateTime dueDate = DateTime.Now.AddDays(14);

            var result = _controller.LoanBook(accountId, bookId, dueDate) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Book loaned successfully.", result.Value);
        }
    



        // Test to verify that CreateVolume returns Ok for valid data
        [Fact]
        public void CreateVolume_WithValidData_ReturnsSuccess()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var bookId = _context.Books.First().Id;

            var result = _controller.CreateVolume(bookId, 1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Volume created successfully.", result.Value);
        }

        // Test to verify that CreateVolume returns BadRequest for invalid data
        [Fact]
        public void CreateVolume_WithInvalidData_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.CreateVolume(0, 0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid book ID or volume number.", result.Value);
        }

        // Test to verify that CreateVolume returns NotFound for a non-existent book
        [Fact]
        public void CreateVolume_WithNonexistentBook_ReturnsNotFound()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.CreateVolume(999, 1) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Book not found.", result.Value);
        }

        // Test to verify that DeleteBookById returns BadRequest for an invalid ID
        [Fact]
        public void DeleteBookById_WithInvalidId_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.DeleteBookById(-1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid book ID", result.Value.ToString());
        }

        // Test to verify that DeleteBookById returns NotFound for a non-existent book ID
        [Fact]
        public void DeleteBookById_WithNonExistentId_ReturnsNotFound()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var nonExistentBookId = 999;
            var result = _controller.DeleteBookById(nonExistentBookId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Book not found", result.Value.ToString());
        }

        // Test to verify that DeleteBookById returns Ok for a valid book ID
        [Fact]
        public void DeleteBookById_WithValidId_ReturnsOk()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var bookId = _context.Books.First().Id;
            var result = _controller.DeleteBookById(bookId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Book deleted successfully", result.Value.ToString());
        }



        // Test to verify that GetBooksByAccount returns NotFound when no books are found
        [Fact]
        public void GetBooksByAccount_NoBooksFound_ReturnsNotFound()
        {
            SeedDatabase(); // Ensure fresh data for each test
            int accountId = 999; // This account should not have any books
            var result = _controller.GetBooksByAccount(accountId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No books found for this account.", result.Value);
        }
    }
}
