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
    public class BookControllerTests : IDisposable
    {
        private readonly DbContextOptions<Dbcontext> _contextOptions;
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;
        private readonly BookController _controller;

        public BookControllerTests()
        {
            // Configure in-memory database for testing
            _contextOptions = new DbContextOptionsBuilder<Dbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())  // Use unique database name for each test run
                .Options;

            _context = new Dbcontext(_contextOptions);
            _dbAccess = new DbAccess(_context);
            _controller = new BookController(_dbAccess);

            // Seed the in-memory database
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Ensure the database is clean before seeding
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Clear the change tracker to avoid tracking issues
            _context.ChangeTracker.Clear();

            // Add test data
            _context.Books.AddRange(
                new Book { Id = 1, Title = "Sample Book", Author = "Author Test" },
                new Book { Id = 2, Title = "Another Book", Author = "Another Author" }
            );
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

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

        [Fact]
        public void GetBookById_BookDoesNotExist_ReturnsNotFound()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var nonExistentId = 9999;
            var result = _controller.GetBookById(nonExistentId) as NotFoundResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

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

        [Fact]
        public void AddBook_WithNullBook_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.AddBook(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Book data is required.", result.Value);
        }

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

        [Fact]
        public void LoanBook_BookExistsAndLoanable_ReturnsOk()
        {
            SeedDatabase(); // Ensure fresh data for each test
            int bookId = _context.Books.First().Id;
            DateTime dueDate = DateTime.Now.AddDays(14);

            var result = _controller.LoanBook(bookId, dueDate) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Book loaned successfully.", result.Value);
        }

        [Fact]
        public void LoanBook_WithInvalidBookId_ReturnsNotFound()
        {
            SeedDatabase(); // Ensure fresh data for each test
            int invalidBookId = 999;
            DateTime dueDate = DateTime.Now.AddDays(14);

            var result = _controller.LoanBook(invalidBookId, dueDate) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Book not found or already loaned.", result.Value);
        }

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

        [Fact]
        public void CreateVolume_WithInvalidData_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.CreateVolume(0, 0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid book ID or volume number.", result.Value);
        }

        [Fact]
        public void CreateVolume_WithNonexistentBook_ReturnsNotFound()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.CreateVolume(999, 1) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Book not found.", result.Value);
        }

        [Fact]
        public void DeleteBookById_WithInvalidId_ReturnsBadRequest()
        {
            SeedDatabase(); // Ensure fresh data for each test
            var result = _controller.DeleteBookById(-1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid book ID", result.Value.ToString());
        }

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
    }
}
