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
    public class BookRepoTests : IDisposable
    {
        private readonly Dbcontext _context;
        private readonly List<Book> _testBooks;
        private readonly TransactionScope _transactionScope;

        public BookRepoTests()
        {
            var connectionString = "Data Source=(localdb)\\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new Dbcontext(options);
            _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                     new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                                     TransactionScopeAsyncFlowOption.Enabled);

            _testBooks = new List<Book>
            {
                new Book { Author = "No Longer Human", Description = "A profound exploration of personal identity.", Title = "No Longer Human", IsLoaned = false },
                new Book { Author = "Another Book", Description = "A second test book.", Title = "Another Book", IsLoaned = false }
            };
            PopulateData();
        }

        private void PopulateData()
        {
            _context.Books.AddRange(_testBooks);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _transactionScope.Dispose();
            _context.Dispose();
        }

        [Fact]
        public void LoanBook_SuccessfullyLoan()
        {
            var book = _testBooks.First();
            var result = LoanBook(book.Id, DateTime.Now.AddDays(15));

            Assert.True(result); // Loan operation successful
            Assert.True(book.IsLoaned);
            Assert.NotNull(book.DueDate);
        }

        [Fact]
        public void LoanBook_FailAlreadyLoaned()
        {
            var book = _testBooks.First();
            book.IsLoaned = true; // Pretend the book is already loaned out
            _context.SaveChanges();

            var result = LoanBook(book.Id, DateTime.Now.AddDays(15));

            Assert.False(result); // Should fail since the book is already loaned
        }


        [Fact]
        public void DeleteBook_FailNonexistentBook()
        {
            var result = DeleteBook(-1); // Assuming -1 is a non-existent ID
            Assert.False(result);
        }

        [Fact]
        public void AddBook_SuccessfullyAdded()
        {
            var newBook = new Book { Author = "New Author", Description = "New Book Description", Title = "New Title" };
            AddBook(newBook);

            Assert.Contains(_context.Books, b => b.Title == "New Title");
        }
  

        // Methods under test
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
       

        public void AddBook(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        }
    }
}

