using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;
using TestRepo.DTO;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using TestRepo.Interface;
using Microsoft.Data.SqlClient;

namespace TestProject2.repotest
{
    public class VolRepoTests : IDisposable
    {
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;
        private readonly TransactionScope _transactionScope;

        public VolRepoTests()
        {
            var connectionString = "Data Source=(localdb)\\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);

            // Do not create TransactionScope here if it's not needed
            SeedDatabase();  // Seed data for each test ensuring a clean state
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    _context.Database.ExecuteSqlRaw("DELETE FROM Vols;");
                    _context.Database.ExecuteSqlRaw("DELETE FROM Books;");
                    _context.SaveChanges();

                    var books = new List<Book> {
                new Book { Title = "Sample Book", Author = "Author Test" },
                new Book { Title = "Another Sample Book", Author = "Another Author" }
            };
                    _context.Books.AddRange(books);
                    _context.SaveChanges();

                    var volumes = new List<Volume> {
                new Volume { BookId = books[0].Id, VolNumber = 1 },
                new Volume { BookId = books[0].Id, VolNumber = 2 },
                new Volume { BookId = books[1].Id, VolNumber = 1 }
            };
                    _context.Vols.AddRange(volumes);
                    _context.SaveChanges();

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                throw;
            }
        }



        [Fact]
        public void GetBookVol_WithExistingBookId_ReturnsVolumes()
        {
            int retryCount = 0;
            IEnumerable<Volume> result = null;
            int bookId = _context.Books.First().Id; // Initialize bookId outside the retry loop

            while (retryCount < 3)
            {
                try
                {
                    result = _dbAccess.GetBookVol(bookId); // The critical section prone to deadlocks
                    break; // If success, break the loop
                }
                catch (SqlException ex) when (ex.Number == 1205) // SQL Server deadlock victim code
                {
                    retryCount++;
                    Thread.Sleep(100 * retryCount); // Implementing exponential back-off
                }
            }

            // Asserts remain outside the retry loop
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
            Ivol volRequest = new Ivol { BookId = bookId, VolNumber = volNumber };
            var result = _dbAccess.GetNewVol(volRequest);
            Assert.NotNull(result);
            Assert.Equal(bookId, result.BookId);
            Assert.Equal(volNumber, result.VolNumber);
        }

        [Fact]
        public void GetNewVol_WithNonExistentVolume_ReturnsNull()
        {
            int retryCount = 0;
            Volume result = null;

            while (retryCount < 3) // Retry up to 3 times
            {
                try
                {
                    result = _dbAccess.GetNewVol(new Volume { BookId = 999, VolNumber = 999 });
                    break; // Break out of loop if successful
                }
                catch (SqlException ex) when (ex.Number == 1205) // Check if the exception is a deadlock victim
                {
                    retryCount++;
                    if (retryCount >= 3)
                        throw; // Throw the exception if the max retry limit is reached
                }
            }

            Assert.Null(result);
        }


        [Fact]
        public void GetVolProgress_WithValidIds_ReturnsVolProgress()
        {
            // Create and add a new account ensuring all required fields are set, including Password
            var account = new Account { UserName = "testuser", Email = "test@example.com", Name = "Test Account", Password = "SecurePassword123" };
            _context.Acc.Add(account);

            var book = new Book { Title = "New Title", Author = "New Author" };
            _context.Books.Add(book);
            _context.SaveChanges(); // Save changes to generate Ids for account and book

            var volume = new Volume { BookId = book.Id, VolNumber = 1 };
            _context.Vols.Add(volume);
            _context.SaveChanges(); // Save changes to generate Id for volume

            // Create a VolProgress entity with all required fields
            var volProgress = new VolProgress { AccountId = account.Id, volId = volume.VolumeId, BookId = book.Id, pagesRead = 100 };
            _context.volProgress.Add(volProgress);
            _context.SaveChanges(); // Save the VolProgress

            // Test retrieval to validate persistence
            var result = _dbAccess.GetVolProgress(account.Id, volume.VolumeId);

            Assert.NotNull(result);
            Assert.Equal(100, result.pagesRead);
        }



        [Fact]
        public void CreateVolProgress_WithNewData_CreatesVolProgress()
        {
            // Create a new Account, ensuring that all required fields including Password are provided
            var account = new Account { UserName = "testuser", Email = "test@example.com", Name = "Test Account", Password = "SecurePassword123" };
            _context.Acc.Add(account);

            var book = new Book { Title = "Test Book", Author = "Test Author" };
            _context.Books.Add(book);
            _context.SaveChanges(); // This saves both the Account and Book and generates their IDs

            // Assuming volume IDs need to be set manually or are generated by some other logic in the test
            var volume = new Volume { BookId = book.Id, VolNumber = 1 };
            _context.Vols.Add(volume);
            _context.SaveChanges(); // Save to generate the volume's ID

            var volProgressData = new CVolProgress { accountId = account.Id, bookId = book.Id, volId = volume.VolumeId };
            _dbAccess.CreateVolProgress(volProgressData);

            // Verify the creation of the VolProgress
            var result = _context.volProgress.SingleOrDefault(vp => vp.AccountId == account.Id && vp.BookId == book.Id && vp.volId == volume.VolumeId);
            Assert.NotNull(result);
        }




        [Fact]
        public void CreateVol_WithExistingBook_CreatesVolumeAndReturnsSuccess()
        {
            // Add a new book without setting the Id
            var book = new Book { Title = "Test Book", Author = "Test Author" };
            _context.Books.Add(book);
            _context.SaveChanges(); // Save to generate the Id automatically

            // Retrieve the generated book Id
            int generatedBookId = book.Id;

            // Now create a volume for this book
            var result = _dbAccess.CreateVol(generatedBookId, 1);

            // Verify the volume is created successfully
            var volume = _context.Vols.FirstOrDefault(v => v.BookId == generatedBookId && v.VolNumber == 1);
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
