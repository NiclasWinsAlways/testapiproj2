using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestRepo.Data;
using TestRepo.DTO;
using TestRepo.Interface;

namespace testapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly DbAccess _dbAccess;

        // Constructor to inject the DbAccess service
        public BookController(DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }


        [HttpGet("all")]
        public IActionResult GetAllBooks()
        {
            try
            {
                var books = _dbAccess.GetAllBooks();
                if (books.Any())
                {
                    return Ok(books);
                }
                else
                {
                    return NotFound("No books found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving books: " + ex.Message);
            }
        }

        // GET method to retrieve a book by ID
        [HttpGet("{id}")]
        public IActionResult GetBookById(int id)
        {
            var book = _dbAccess.GetBookById(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }


        // POST method to add a new book
        [HttpPost("add")]
        public IActionResult AddBook([FromBody] Book book)
        {
            // Directly call to add the book in the database without an account ID
            _dbAccess.AddBook(book);
            return Created($"api/book/{book.Id}", book); // Return a '201 Created' response with the book object
        }

        [HttpPost("GetBookByTitle")]
        public IActionResult GetBookByTitle([FromBody] TitleRequest titleRequest)
        {
            if (titleRequest == null || string.IsNullOrWhiteSpace(titleRequest.title))
            {
                return BadRequest("Title is required.");
            }

            var book = _dbAccess.GetBookByTitle(titleRequest.title);
            if (book == null)
            {
                return NotFound(new { message = $"No book found with title: {titleRequest.title}" });
            }

            return Ok(book);
        }


        // POST method to add progress for a specific book
        // {bookId} is passed as a URL segment
        [HttpPost("{bookId}/progress")]
        public IActionResult AddBookProgress(int bookId, [FromBody] BookProgressDto bookProgressDto)
        {
            // Verify if the book with the given ID exists by checking for any associated volumes
            var volumes = _dbAccess.GetBookVol(bookId);
            if (volumes == null || !volumes.Any())
            {
                return NotFound($"No book found with ID {bookId}");
            }

            // Map DTO to the domain model
            var bookProgress = new BookProgress
            {
                BookId = bookId,
                AccountId = bookProgressDto.AccountId,  // Assuming AccountId is provided through some means
                volumesRead = bookProgressDto.volumesRead
            };

            try
            {
                _dbAccess.AddBookProgress(bookProgress);
                return CreatedAtAction(nameof(GetBookProgress), new { bookId = bookId, progressId = bookProgress.Id }, bookProgress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding book progress: " + ex.Message);
            }
        }

        // GET method to retrieve specific book progress using bookId and progressId
        [HttpGet("{bookId}/progress/{progressId}")]
        public IActionResult GetBookProgress(int bookId, int progressId)
        {
            var progress = _dbAccess.GetBookProgressById(progressId);
            if (progress == null || progress.BookId != bookId)
            {
                return NotFound("Progress not found or does not belong to the specified book.");
            }

            return Ok(progress);
        }

        // GET method to retrieve all books associated with an accountId
        [HttpGet("list/{accountId}")]
        public IActionResult GetBooksByAccount(int accountId)
        {
            var booksWithProgress = _dbAccess.GetBookList(accountId);
            return Ok(booksWithProgress);
        }

        // POST method to create a volume for a book, parameters are passed via query string
        [HttpPost("add-volume")]
        public IActionResult CreateVolume([FromQuery] int bookId, [FromQuery] int volNumber)
        {
            if (bookId <= 0 || volNumber <= 0)
            {
                return BadRequest("Invalid book ID or volume number.");
            }

            try
            {
                var result = _dbAccess.CreateVol(bookId, volNumber);
                if (result == "Book doesn't exist")
                {
                    return NotFound("Book not found.");
                }
                return Ok("Volume created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

        }

        // DELETE: api/Book/delete/{bookId}
        [HttpDelete("delete/{bookId}")]
        public IActionResult DeleteBookById(int bookId)
        {
            if (bookId <= 0)
            {
                return BadRequest("Invalid book ID");
            }

            var deleted = _dbAccess.DeleteBook(bookId);
            if (deleted)
            {
                return Ok(new { message = "Book deleted successfully." });
            }
            else
            {
                return NotFound(new { message = "Book not found." });
            }
        }

    }
}