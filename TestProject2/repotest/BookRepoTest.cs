using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;
using TestRepo.DTO;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;

namespace TestProject2.controllertest
{
    // Test class for Book repository operations
    public class BookRepoTests : IDisposable
    {
        private readonly Dbcontext _context;
        private readonly List<Book> _testBooks;
        private readonly TransactionScope _transactionScope;

        // Constructor to set up the test context and data
        public BookRepoTests()
        {
            // Define connection string for the SQL Server
            var connectionString = "Data Source=(localdb)\\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new Dbcontext(options);

            // Begin a new transaction scope for each test
            _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                     new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                                     TransactionScopeAsyncFlowOption.Enabled);

            // Initialize test data
            _testBooks = new List<Book>
            {
                new Book { Author = "Osamu Dazai", Description = "A profound exploration of personal identity.", Title = "No Longer Human", IsLoaned = false },
                new Book { Author = "Test Author", Description = "A second test book.", Title = "Another Book", IsLoaned = false }
            };

            // Populate the database with test data
            PopulateData();
        }

        // Method to populate the database with test data
        private void PopulateData()
        {
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        // Dispose method to clean up after each test
        public void Dispose()
        {
            _transactionScope.Dispose();
            _context.Dispose();
        }

        // Test to verify that a book can be successfully loaned
        [Fact]
        public void LoanBook_SuccessfullyLoan()
        {
            var book = _testBooks.First();
            var result = LoanBook(book.Id, DateTime.Now.AddDays(15));

            Assert.True(result); // Loan operation successful
            Assert.True(book.IsLoaned);
            Assert.NotNull(book.DueDate);
        }

        // Test to verify that loaning a book fails if the book is already loaned
        [Fact]
        public void LoanBook_FailAlreadyLoaned()
        {
            var book = _testBooks.First();
            book.IsLoaned = true; // Pretend the book is already loaned out
            _context.SaveChanges();

            var result = LoanBook(book.Id, DateTime.Now.AddDays(15));

            Assert.False(result); // Should fail since the book is already loaned
        }

        // Test to verify that deleting a non-existent book fails
        [Fact]
        public void DeleteBook_FailNonexistentBook()
        {
            var result = DeleteBook(-1); // Assuming -1 is a non-existent ID
            Assert.False(result);
        }

        // Test to verify that a new book can be successfully added
        [Fact]
        public void AddBook_SuccessfullyAdded()
        {
            var newBook = new Book { Author = "New Author", Description = "New Book Description", Title = "New Title" };
            AddBook(newBook);

            Assert.Contains(_context.Books, b => b.Title == "New Title");
        }

        // Method to loan a book by setting its IsLoaned status and due date
        public bool LoanBook(int bookId, DateTime dueDate)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null || book.IsLoaned)
            {
                return false; // Book not found or already loaned
            }

            book.IsLoaned = true;
            book.DueDate = dueDate;
            _context.SaveChanges();

            return true; // Loan operation successful
        }

        // Method to delete a book by its ID
        public bool DeleteBook(int bookId)
        {
            var book = _context.Books.Find(bookId);
            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        // Method to add a new book to the database
        public void AddBook(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        }
    }
}
