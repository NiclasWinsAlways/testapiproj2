//using System;
//using Xunit;
//using Microsoft.EntityFrameworkCore;
//using TestRepo.Data;
//using TestRepo.DTO;
//using System.Linq;
//using System.Collections.Generic;
//using System.Transactions;

//namespace TestProject2.repotest
//{
//    public class VolRepoTests : IDisposable
//    {
//        private readonly Dbcontext _context;
//        private readonly List<Volume> _testVolumes;
//        private readonly TransactionScope _transactionScope;

//        public VolRepoTests()
//        {
//            var connectionString = "Data Source=(localdb)\\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
//            var options = new DbContextOptionsBuilder<Dbcontext>()
//                .UseSqlServer(connectionString)
//                .Options;

//            _context = new Dbcontext(options);
//            _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew,
//                                                     new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
//                                                     TransactionScopeAsyncFlowOption.Enabled);

//            _testVolumes = new List<Volume>
//            {
//                new Volume { BookId = 1, VolNumber = 1 },
//                new Volume { BookId = 2, VolNumber = 1 }
//            };
//            PopulateData();
//        }

//        private void PopulateData()
//        {
//            // Create necessary accounts for testing.
//            var testAccounts = new List<Account>
//    {
//        new Account { UserName = "user1", Email = "user1@example.com", Password = "securePassword1", Name = "User One", IsAdmin = false, IsLoggedin = false },
//        new Account { UserName = "user2", Email = "user2@example.com", Password = "securePassword2", Name = "User Two", IsAdmin = false, IsLoggedin = false }
//    };
//            _context.Acc.AddRange(testAccounts);

//            // Ensure books are saved before referencing them in volumes.
//            var testBooks = new List<Book>
//    {
//        new Book { Author = "Author 1", Description = "Desc 1", Title = "Book 1" },
//        new Book { Author = "Author 2", Description = "Desc 2", Title = "Book 2" }
//    };
//            _context.Books.AddRange(testBooks);
//            _context.SaveChanges();

//            _testVolumes = new List<Volume>
//    {
//        new Volume { BookId = 1, VolNumber = 1, totalPages = 300 },
//        new Volume { BookId = 2, VolNumber = 1, totalPages = 250 }
//    };
//            _context.Vols.AddRange(_testVolumes);
//            _context.SaveChanges();
//        }



//        public void Dispose()
//        {
//            _transactionScope.Complete();
//            _transactionScope.Dispose();
//            _context.Dispose();
//        }


//        [Fact]
//        public void GetBookVol_ReturnsVolumes()
//        {
//            var volumes = GetBookVol(1); // Assuming book with ID 1 exists
//            Assert.NotNull(volumes);
//            Assert.NotEmpty(volumes);
//            Assert.All(volumes, v => Assert.Equal(1, v.BookId));
//        }

//        [Fact]
//        public void AddBook_CreatesBookAndInitialVolume()
//        {
//            var newBook = new Book { Author = "New Author", Description = "New Book Description", Title = "New Title" };
//            var accountId = 1; // Assuming account with ID 1 exists
//            AddBook(accountId, newBook);

//            Assert.Contains(_context.Books, b => b.Title == "New Title");

//            // Add a volume after book is confirmed added
//            var newVolume = new Volume { BookId = newBook.BookId, VolNumber = 3, totalPages = 150 };
//            _context.Vols.Add(newVolume);
//            _context.SaveChanges();

//            Assert.Contains(_context.BooksProgress, bp => bp.AccountId == accountId);
//            Assert.True(_context.Vols.Any(v => v.BookId == newBook.BookId && v.VolNumber == 3));
//        }


//        [Fact]
//        public void CreateVol_ReturnsSuccessMessage()
//        {
//            var result = CreateVol(1, 2); // BookId 1 and new VolNumber 2
//            Assert.Equal("Volume created successfully", result);
//            Assert.True(_context.Vols.Any(v => v.BookId == 1 && v.VolNumber == 2)); // Corrected to Vols from Volumes
//        }

//        // Methods under test
//        public List<Volume> GetBookVol(int bookId)
//        {
//            var bookFound = _context.Books.Find(bookId);
//            if (bookFound == null)
//            {
//                return null;
//            }
//            else
//            {
//                return _context.Vols
//                    .Where(b => b.BookId == bookId)
//                    .ToList(); // Corrected to Vols from Volumes
//            }
//        }

//        public void AddBook(int accountId, Book book)
//        {
//            var account = _context.Acc.Find(accountId);
//            if (account == null)
//            {
//                throw new ArgumentException("Account does not exist.", nameof(accountId));
//            }

//            // Ensure Name is also set
//            if (string.IsNullOrEmpty(account.Name))
//            {
//                account.Name = "Default Name"; // Provide a default name if none is provided
//            }

//            _context.Books.Add(book);
//            _context.SaveChanges();

//            var bookProgress = new BookProgress
//            {
//                BookId = book.Id,
//                AccountId = accountId,
//                volumesRead = 0
//            };
//            _context.BooksProgress.Add(bookProgress);
//            _context.SaveChanges();
//        }



//        public string CreateVol(int bookId, int volNumber)
//        {
//            var book = _context.Books.Find(bookId);
//            if (book == null)
//            {
//                return "Book doesn't exist";
//            }

//            var volume = new Volume
//            {
//                BookId = bookId,
//                VolNumber = volNumber
//            };

//            _context.Vols.Add(volume); // Corrected to Vols from Volumes
//            _context.SaveChanges();

//            return "Volume created successfully";
//        }
//    }
//}
