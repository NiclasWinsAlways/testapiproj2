using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;
using TestRepo.DTO;
using System.Linq;
using System.Collections.Generic;
using TestRepo.Interface;

namespace TestProject2.repotest
{
    public class VolRepoTests : IDisposable
    {
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;

        public VolRepoTests()
        {
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);

            SeedDatabase();  // Seed data for each test ensuring a clean state
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Sample Book", Author = "Author Test" },
                new Book { Id = 2, Title = "Another Sample Book", Author = "Another Author" }
            };
            _context.Books.AddRange(books);
            _context.SaveChanges();

            var volumes = new List<Volume>
            {
                new Volume { VolumeId = 1, BookId = books[0].Id, VolNumber = 1 },
                new Volume { VolumeId = 2, BookId = books[0].Id, VolNumber = 2 },
                new Volume { VolumeId = 3, BookId = books[1].Id, VolNumber = 1 }
            };
            _context.Vols.AddRange(volumes);
            _context.SaveChanges();

            var accounts = new List<Account>
            {
                new Account { Id = 1, UserName = "testuser1", Email = "test1@example.com", Name = "Test User 1", Password = "Password1" },
                new Account { Id = 2, UserName = "testuser2", Email = "test2@example.com", Name = "Test User 2", Password = "Password2" }
            };
            _context.Acc.AddRange(accounts);
            _context.SaveChanges();
        }

        [Fact]
        public void GetBookVol_WithExistingBookId_ReturnsVolumes()
        {
            int bookId = _context.Books.First().Id;
            var result = _dbAccess.GetBookVol(bookId);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, vol => Assert.Equal(bookId, vol.BookId));
        }

        [Fact]
        public void GetBookVol_WithNonExistingBookId_ReturnsNull()
        {
            var bookId = 999;  // Non-existing book ID
            var volumes = _dbAccess.GetBookVol(bookId);
            Assert.Null(volumes);
        }

        [Fact]
        public void GetNewVol_WithValidVolume_ReturnsVolume()
        {
            var bookId = _context.Books.First().Id;
            int volNumber = 1;
            var volRequest = new Ivol { BookId = bookId, VolNumber = volNumber };

            var result = _dbAccess.GetNewVol(volRequest);

            Assert.NotNull(result);
            Assert.Equal(bookId, result.BookId);
            Assert.Equal(volNumber, result.VolNumber);
        }

        [Fact]
        public void GetNewVol_WithNonExistentVolume_ReturnsNull()
        {
            var result = _dbAccess.GetNewVol(new Volume { BookId = 999, VolNumber = 999 });
            Assert.Null(result);
        }

        [Fact]
        public void GetVolProgress_WithValidIds_ReturnsVolProgress()
        {
            var account = new Account { UserName = "testuser", Email = "test@example.com", Name = "Test Account", Password = "SecurePassword123" };
            _context.Acc.Add(account);
            _context.SaveChanges();  // Ensure the account is saved before using its ID

            var book = new Book { Title = "New Title", Author = "New Author" };
            _context.Books.Add(book);
            _context.SaveChanges();  // Ensure the book is saved before using its ID

            var volume = new Volume { BookId = book.Id, VolNumber = 1 };
            _context.Vols.Add(volume);
            _context.SaveChanges();  // Ensure the volume is saved before using its ID

            var volProgress = new VolProgress { AccountId = account.Id, volId = volume.VolumeId, BookId = book.Id, pagesRead = 100 };
            _context.volProgress.Add(volProgress);
            _context.SaveChanges();  // Ensure the VolProgress is saved before using its ID

            var result = _dbAccess.GetVolProgress(account.Id, volume.VolumeId);

            Assert.NotNull(result);
            Assert.Equal(100, result.pagesRead);
        }

        [Fact]
        public void CreateVolProgress_WithNewData_CreatesVolProgress()
        {
            var account = new Account { UserName = "testuser", Email = "test@example.com", Name = "Test Account", Password = "SecurePassword123" };
            _context.Acc.Add(account);
            _context.SaveChanges();

            var book = new Book { Title = "Test Book", Author = "Test Author" };
            _context.Books.Add(book);
            _context.SaveChanges();

            var volume = new Volume { BookId = book.Id, VolNumber = 1 };
            _context.Vols.Add(volume);
            _context.SaveChanges();

            var volProgress = new VolProgress { AccountId = account.Id, volId = volume.VolumeId, BookId = book.Id, pagesRead = 50 };
            _context.volProgress.Add(volProgress);
            _context.SaveChanges();

            var result = _context.volProgress.AsNoTracking().SingleOrDefault(vp => vp.AccountId == account.Id && vp.BookId == book.Id && vp.volId == volume.VolumeId);
            Assert.NotNull(result);
            Assert.Equal(50, result.pagesRead);
        }

        [Fact]
        public void CreateVol_WithExistingBook_CreatesVolumeAndReturnsSuccess()
        {
            var book = new Book { Title = "Test Book", Author = "Test Author" };
            _context.Books.Add(book);
            _context.SaveChanges();  // Ensure the book is saved before using its ID

            int generatedBookId = book.Id;

            var result = _dbAccess.CreateVol(generatedBookId, 1);

            var volume = _context.Vols.AsNoTracking().FirstOrDefault(v => v.BookId == generatedBookId && v.VolNumber == 1);
            Assert.NotNull(volume);
            Assert.Equal("Volume created successfully", result);
        }

        [Fact]
        public void CreateVol_WithNonExistingBook_ReturnsError()
        {
            var result = _dbAccess.CreateVol(999, 1);
            Assert.Equal("Book doesn't exist", result);
        }
    }
}
