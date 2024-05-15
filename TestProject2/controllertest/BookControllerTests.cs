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
        private Dbcontext _context;
        private DbAccess _dbAccess;
        private BookController _controller;

        public BookControllerTests()
        {
            InitializeContext();
        }

        private void InitializeContext()
        {
            var connectionString = @"Data Source=(localdb)\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                .Options;

            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);
            _controller = new BookController(_dbAccess);
        }

        public void Dispose()
        {
            _context.Dispose();
            InitializeContext(); // Reinitialize for each test to ensure isolation
        }


        private void SeedDatabase()
        {
            const int maxRetries = 5;
            const int retryDelaySeconds = 3;

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    // Clear all entries to ensure a consistent state
                    for (int i = 0; i < 3; i++)
                    {
                        _context.Books.RemoveRange(_context.Books);
                    }
                    _context.SaveChanges();

                    // Add test data anew
                    var book = new Book { Title = "Sample Book", Author = "Author Test" };
                    _context.Books.Add(book);
                    _context.SaveChanges();

                    break; // Exit the loop if successful
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (retryCount == maxRetries - 1)
                    {
                        throw; // Re-throw the exception if max retries are reached
                    }

                    Console.WriteLine($"Concurrency exception occurred. Retry #{retryCount + 1} of {maxRetries}.");
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(retryDelaySeconds));
                }
            }
        }


        [Fact]
        public void GetAllBooks_ShouldReturnBooksWhenAvailable()
        {
            SeedDatabase();
            var result = _controller.GetAllBooks() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var books = result.Value as List<Book>;
            Assert.True(books.Count > 0);
        }

        [Fact]
        public void GetAllBooks_ShouldReturnNotFoundWhenNoBooksAvailable()
        {
            _context.Books.RemoveRange(_context.Books);
            _context.SaveChanges();

            var result = _controller.GetAllBooks() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No books found.", result.Value);
        }
        // not really needed but keeping it here regardless 
        // i need to cause the database to actually break for thsi one to work

        //[Fact]
        //public void GetAllBooks_ShouldHandleExceptions()
        //{
        //    // To simulate an exception, forcibly throw one when accessing the database
        //    _context.Database.CloseConnection(); // Close the connection to force a failure

        //    var result = _controller.GetAllBooks() as ObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(500, result.StatusCode);
        //    Assert.Contains("An error occurred while retrieving books", result.Value.ToString());
        //}
        [Fact]
        public void GetBookById_BookExists_ReturnsOkWithBook()
        {
            SeedDatabase();
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
            // Use a guaranteed non-existent ID
            var nonExistentId = 9999;

            var result = _controller.GetBookById(nonExistentId) as NotFoundResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void AddBook_WithValidBook_ReturnsCreatedResult()
        {
            var newBook = new Book { Title = "New Book", Author = "New Author" };

            var result = _controller.AddBook(newBook) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
            var book = result.Value as Book;
            Assert.NotNull(book);
            Assert.Equal(newBook.Title, book.Title); // Confirm the book details are correct
        }

        [Fact]
        public void AddBook_WithNullBook_ReturnsBadRequest()
        {
            var result = _controller.AddBook(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Book data is required.", result.Value);
        }

        // also not needed i would need to break the database again for this one to work
        // im not gonna write a simulated failure i have no idea how to
        //[Fact]
        //public void AddBook_WhenDbUpdateFails_ReturnsServerError()
        //{
        //    // To simulate an exception, forcibly throw one when adding the book
        //    _context.Database.CloseConnection(); // Close the connection to force a failure

        //    var newBook = new Book { Title = "Error Book", Author = "Error Author" };

        //    var result = _controller.AddBook(newBook) as ObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(500, result.StatusCode);
        //    Assert.Contains("An error occurred while adding the book", result.Value.ToString());
        //}

        [Fact]
        public void GetBookByTitle_ValidTitle_ReturnsOk()
        {
            SeedDatabase(); // Ensures the database is seeded before each test

            var titleRequest = new TitleRequest { title = "Sample Book" };
            var result = _controller.GetBookByTitle(titleRequest) as OkObjectResult;

            Assert.NotNull(result);  // First check if result is not null
            Assert.Equal(200, result.StatusCode);  // Check if status code is 200 OK
            var book = result.Value as Book;
            Assert.NotNull(book);  // Check if the book itself is not null
            Assert.Equal("Sample Book", book.Title);  // Validate that the book title matches
        }

        [Fact]
        public void GetBookByTitle_InvalidTitle_ReturnsNotFound()
        {
            SeedDatabase();
            var titleRequest = new TitleRequest { title = "Nonexistent Book" };

            var result = _controller.GetBookByTitle(titleRequest) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("No book found with title", result.Value.ToString());
        }

        //[Fact]
        //public void GetBookByTitle_NullOrEmptyTitle_ReturnsBadRequest()
        //{
        //    var titleRequest = new TitleRequest { title = "" }; // Testing with empty title

        //    var result = _controller.GetBookByTitle(titleRequest) as BadRequestObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(400, result.StatusCode);
        //    Assert.Equal("Title is required.", result.Value);
        //}

        //[Fact]
        //public void GetBookByTitle_DbFailure_ReturnsServerError()
        //{
        //    var titleRequest = new TitleRequest { title = "Sample Book" };
        //    _context.Database.CloseConnection(); // Simulate a database failure

        //    var result = _controller.GetBookByTitle(titleRequest) as ObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(500, result.StatusCode);
        //    Assert.Contains("An error occurred", result.Value.ToString());
        //}

        [Fact]
        public void LoanBook_BookExistsAndLoanable_ReturnsOk()
        {
            SeedDatabase();
            int bookId = _context.Books.FirstOrDefault().Id;
            DateTime dueDate = DateTime.Now.AddDays(14);

            var result = _controller.LoanBook(bookId, dueDate) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Book loaned successfully.", result.Value);
        }
        [Fact]
        public void LoanBook_WithInvalidBookId_ReturnsNotFound()
        {
            // Arrange
            int invalidBookId = 999; // Assuming 999 is not a valid ID
            DateTime dueDate = DateTime.Now.AddDays(14);

            // Act
            var result = _controller.LoanBook(invalidBookId, dueDate) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Book not found or already loaned.", result.Value);
        }

        // I DONT KNOW WHY I KEEP ON MAKING THESE I NEED TO MAKE A SIMULATED FAIL FOR THIS TO WORK AND IM NOT GOING TO
        // if it returns errór 500 it works
        //[Fact]
        //public void LoanBook_ThrowsException_ReturnsServerError()
        //{
        //    SeedDatabase();
        //    int bookId = _context.Books.FirstOrDefault().Id;
        //    _context.Database.CloseConnection(); // Force a failure

        //    var result = _controller.LoanBook(bookId, DateTime.Now.AddDays(14)) as ObjectResult;

        //    Assert.NotNull(result);
        //    Assert.Equal(500, result.StatusCode);
        //    Assert.Contains("An error occurred while loaning the book:", result.Value.ToString());
        //}

        [Fact]
        public void CreateVolume_WithValidData_ReturnsSuccess()
        {
            SeedDatabase();
            var bookId = _context.Books.First().Id;

            var result = _controller.CreateVolume(bookId, 1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Volume created successfully.", result.Value);
        }

        [Fact]
        public void CreateVolume_WithInvalidData_ReturnsBadRequest()
        {
            var result = _controller.CreateVolume(0, 0) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid book ID or volume number.", result.Value);
        }

        [Fact]
        public void CreateVolume_WithNonexistentBook_ReturnsNotFound()
        {
            var result = _controller.CreateVolume(999, 1) as NotFoundObjectResult; // Assuming book ID 999 does not exist

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Book not found.", result.Value);
        }
        // I STILL NEED TO CREATE MORE SIMULATIONS FOR THIS ONE TO MAKE IT FAIL IF I WANT TO GET THIS ONE TO WORK WHICH IM NOT GONNA DO

        //[Fact]
        //public void CreateVolume_WhenDbFails_ThrowsException()
        //{
        //    // Assume database is seeded, and then close the connection to simulate a DB failure
        //    SeedDatabase();
        //    _context.Database.CloseConnection(); // Attempt to close the connection to force an error

        //    var bookId = _context.Books.First().Id; // Use a valid book ID

        //    var result = _controller.CreateVolume(bookId, 1) as ObjectResult; // Attempt to create a volume which should fail

        //    Assert.NotNull(result);
        //    Assert.Equal(500, result.StatusCode); // Checking for server error status code
        //    Assert.Contains("Internal server error:", result.Value.ToString()); // Confirm the error message contains expected text
        //}

        [Fact]
        public void DeleteBookById_WithInvalidId_ReturnsBadRequest()
        {
            var result = _controller.DeleteBookById(-1) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid book ID", result.Value.ToString());
        }


        [Fact]
        public void DeleteBookById_WithNonExistentId_ReturnsNotFound()
        {
            // Ensure the database does not contain the book we are looking to delete
            var nonExistentBookId = 999; // Assume 999 is not used
            var result = _controller.DeleteBookById(nonExistentBookId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Book not found", result.Value.ToString());
        }


        [Fact]
        public void DeleteBookById_WithValidId_ReturnsOk()
        {
            SeedDatabase(); // Adds a book to the database
            var bookId = _context.Books.First().Id; // Get the ID of the book just added

            var result = _controller.DeleteBookById(bookId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Book deleted successfully", result.Value.ToString());
        }



    }
}
