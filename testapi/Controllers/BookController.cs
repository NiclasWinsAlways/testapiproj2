﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Linq;
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

        [HttpGet("list-all")]
        public IActionResult ListAllBooks()
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

        [HttpPost("add")]
        public IActionResult AddBook([FromBody] Book book) // Removed [FromForm]
        {
            if (book == null)
            {
                return BadRequest("Book data is required.");
            }

            try
            {
                _dbAccess.AddBook(book);
                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding the book: " + ex.Message);
            }
        }


        [HttpPost("GetBookByTitle")]
        public IActionResult GetBookByTitle([FromBody] TitleRequest titleRequest)
        {
            if (titleRequest == null || string.IsNullOrWhiteSpace(titleRequest.title))
            {
                return BadRequest("Title is required.");
            }

            try
            {
                var book = _dbAccess.GetBookByTitle(titleRequest.title);
                if (book == null)
                {
                    return NotFound(new { message = $"No book found with title: {titleRequest.title}" });
                }

                return Ok(book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the book by title: " + ex.Message);
            }
        }

        [HttpPost("loan/{accountId}/{bookId}")]
        public IActionResult LoanBook(int accountId, int bookId, [FromBody] DateTime dueDate)
        {
            try
            {
                bool result = _dbAccess.LoanBook(accountId, bookId, dueDate);

                if (!result)
                {
                    return NotFound("Book not found or already loaned.");
                }

                return Ok("Book loaned successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while loaning the book: " + ex.Message);
            }
        }


        [HttpGet("loans")]
        public IActionResult GetAllLoans()
        {
            try
            {
                var loans = _dbAccess.GetAllLoans();
                if (loans == null || !loans.Any())
                {
                    return NotFound("No loans found.");
                }
                return Ok(loans);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving loans: " + ex.Message);
            }
        }


        [HttpPost("return/{accountId}/{bookId}")]
        public IActionResult ReturnBook(int accountId, int bookId)
        {
            if (accountId <= 0 || bookId <= 0)
            {
                return BadRequest(new { message = "Invalid accountId or bookId." });
            }

            try
            {
                bool result = _dbAccess.ReturnBook(accountId, bookId);

                if (!result)
                {
                    return NotFound(new { message = $"Loan record not found for AccountId: {accountId}, BookId: {bookId} or the book is already returned." });
                }

                return Ok(new { message = "Book returned successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while returning the book: " + ex.Message });
            }
        }




        [HttpGet("list/{accountId}")]
        public IActionResult GetBooksByAccount(int accountId)
        {
            try
            {
                var booksWithProgress = _dbAccess.GetBookList(accountId);
                if (booksWithProgress == null || !booksWithProgress.Any())
                {
                    return NotFound("No books found for this account.");
                }
                return Ok(booksWithProgress);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving books by account: " + ex.Message);
            }
        }

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

        [HttpDelete("delete/{bookId}")]
        public IActionResult DeleteBookById(int bookId)
        {
            if (bookId <= 0)
            {
                return BadRequest("Invalid book ID");
            }

            try
            {
                bool deleted = _dbAccess.DeleteBook(bookId);
                if (deleted)
                {
                    return Ok(new { message = "Book deleted successfully." });
                }
                else
                {
                    return NotFound(new { message = "Book not found." });
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ProblemDetails { Title = "Database error", Detail = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new ProblemDetails { Title = "Invalid operation", Detail = ex.Message });
            }
        }
    }
}
