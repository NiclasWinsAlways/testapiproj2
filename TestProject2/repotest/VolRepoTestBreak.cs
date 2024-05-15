//using System;
//using Xunit;
//using Microsoft.EntityFrameworkCore;
//using TestRepo.Data;
//using TestRepo.DTO;
//using System.Linq;
//using System.Collections.Generic;
//using System.Transactions;
//using TestRepo.Interface;

//namespace TestProject2.repotest
//{
//    public class VolRepoTests : IDisposable
//    {
//        private readonly Dbcontext _context;
//        private readonly DbAccess _dbAccess;

//        public VolRepoTests()
//        {
//            var connectionString = "Data Source=(localdb)\\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
//            var options = new DbContextOptionsBuilder<Dbcontext>()
//                .UseSqlServer(connectionString)
//                .Options;

//            _context = new Dbcontext(options);
//            _dbAccess = new DbAccess(_context);
//            EnsureDatabaseIsClean(); // Ensure a clean state before each test
//        }

//        public void Dispose()
//        {
//            _context.Dispose();
//        }

//        private void EnsureDatabaseIsClean()
//        {
//            _context.Database.EnsureDeleted();
//            _context.Database.EnsureCreated();
//            SeedDatabase(); // Seed database after ensuring it's clean
//        }

//        private void SeedDatabase()
//        {
//            var books = new List<Book>
//            {
//                new Book { Title = "Sample Book", Author = "Author Test" },
//                new Book { Title = "Another Sample Book", Author = "Another Author" }
//            };
//            _context.Books.AddRange(books);
//            _context.SaveChanges();

//            var volumes = new List<Volume>
//            {
//                new Volume { BookId = books[0].Id, VolNumber = 1 },
//                new Volume { BookId = books[0].Id, VolNumber = 2 },
//                new Volume { BookId = books[1].Id, VolNumber = 1 }
//            };
//            _context.Vols.AddRange(volumes);
//            _context.SaveChanges();
//        }

//        [Fact]
//        public void GetBookVol_WithExistingBookId_ReturnsVolumes()
//        {
//            var bookId = _context.Books.First().Id;
//            var result = _dbAccess.GetBookVol(bookId);
//            Assert.NotNull(result);
//            Assert.NotEmpty(result);
//            Assert.All(result, vol => Assert.Equal(bookId, vol.BookId));
//        }

//        [Fact]
//        public void GetBookVol_WithNonExistingBookId_ReturnsNull()
//        {
//            var result = _dbAccess.GetBookVol(999); // Using an ID that does not exist
//            Assert.Null(result);
//        }

//        [Fact]
//        public void GetNewVol_WithValidVolume_ReturnsVolume()
//        {
//            var bookId = _context.Books.First().Id;
//            var volRequest = new Ivol { BookId = bookId, VolNumber = 1 };
//            var result = _dbAccess.GetNewVol(volRequest);
//            Assert.NotNull(result);
//            Assert.Equal(bookId, result.BookId);
//            Assert.Equal(1, result.VolNumber);
//        }

//        [Fact]
//        public void GetNewVol_WithNonExistentVolume_ReturnsNull()
//        {
//            var result = _dbAccess.GetNewVol(new Volume { BookId = 999, VolNumber = 999 });
//            Assert.Null(result);
//        }

//        [Fact]
//        public void GetVolProgress_WithValidIds_ReturnsVolProgress()
//        {
//            var account = new Account { UserName = "Test User", Email = "test@example.com" };
//            var book = new Book { Title = "Test Book", Author = "Test Author" };
//            _context.Acc.Add(account);
//            _context.Books.Add(book);
//            _context.SaveChanges();

//            var volProgress = new VolProgress { AccountId = account.Id, volId = 1, BookId = book.Id, pagesRead = 100 };
//            _context.volProgress.Add(volProgress);
//            _context.SaveChanges();

//            var result = _dbAccess.GetVolProgress(account.Id, 1);
//            Assert.NotNull(result);
//            Assert.Equal(100, result.pagesRead);
//        }

//        [Fact]
//        public void CreateVolProgress_WithNewData_CreatesVolProgress()
//        {
//            var account = new Account { UserName = "Test User", Email = "test@example.com" };
//            var book = new Book { Title = "Test Book", Author = "Test Author" };
//            _context.Acc.Add(account);
//            _context.Books.Add(book);
//            _context.SaveChanges();

//            var volProgressData = new CVolProgress { accountId = account.Id, bookId = book.Id, volId = 1 };
//            _dbAccess.CreateVolProgress(volProgressData);

//            var result = _context.volProgress.FirstOrDefault(vp => vp.AccountId == account.Id && vp.BookId == book.Id && vp.volId == 1);
//            Assert.NotNull(result);
//        }

//        [Fact]
//        public void CreateVol_WithExistingBook_CreatesVolumeAndReturnsSuccess()
//        {
//            var book = new Book { Title = "Existing Book", Author = "Existing Author" };
//            _context.Books.Add(book);
//            _context.SaveChanges();

//            var result = _dbAccess.CreateVol(book.Id, 1);
//            Assert.Equal("Volume created successfully", result);
//        }

//        [Fact]
//        public void CreateVol_WithNonExistingBook_ReturnsError()
//        {
//            var result = _dbAccess.CreateVol(999, 1);
//            Assert.Equal("Book doesn't exist", result);
//        }
//    }
//}
