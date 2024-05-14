//using System;
//using Xunit;
//using Microsoft.EntityFrameworkCore;
//using TestRepo.Data;
//using TestRepo.DTO;
//using System.Linq;
//using System.Collections.Generic;
//using System.Transactions;
//using TestRepo.Interface;

//namespace TestProject2.controllertest
//{
//    public class UpdateTestRepoTests : IDisposable
//    {
//        private readonly DbContextOptions<Dbcontext> _options;
//        private readonly Dbcontext _context;
//        private readonly List<VolProgress> _testProgress;
//        private readonly TransactionScope _transactionScope;

//        public UpdateTestRepoTests()
//        {
//            var connectionString = "Data Source=(localdb)\\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
//            _options = new DbContextOptionsBuilder<Dbcontext>()
//                .UseSqlServer(connectionString)
//                .Options;

//            _context = new Dbcontext(_options);
//            _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew,
//                                                     new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
//                                                     TransactionScopeAsyncFlowOption.Enabled);

//            _testProgress = new List<VolProgress>
//            {
//                new VolProgress { BookId = 1, volId = 1, AccountId = 1, pagesRead = 0 },
//                // Add more test data as needed
//            };
//            PopulateData();
//        }

//        private void PopulateData()
//        {
//            _context.volProgress.AddRange(_testProgress);
//            _context.SaveChanges();
//        }

//        public void Dispose()
//        {
//            _transactionScope.Dispose();
//            _context.Dispose();
//        }

//        [Fact]
//        public void UpdPageCount_SuccessfullyUpdated()
//        {
//            // Arrange
//            var newPage = new UpdPageDTO
//            {
//                BookId = 1,
//                volId = 1,
//                AccountId = 1,
//                PagesRead = 50 // Example values, change as needed
//            };
//            var dbAcc = new DbAcc(_context); // Replace DbAcc with the actual class where UpdPageCount is defined

//            // Act
//            dbAcc.UpdPageCount(newPage);

//            // Assert
//            var updatedBook = _context.volProgress.FirstOrDefault(b => b.BookId == newPage.BookId && b.volId == newPage.volId && b.AccountId == newPage.AccountId);
//            Assert.NotNull(updatedBook);
//            Assert.Equal(newPage.PagesRead, updatedBook.pagesRead);
//        }

//        [Fact]
//        public void UpdTotalPages_SuccessfullyUpdated()
//        {
//            // Arrange
//            var dbAcc = new DbAcc(_context); // Replace DbAcc with the actual class where UpdTotalPages is defined
//            var bookId = 1;
//            var volId = 1;
//            var newPageCount = 100; // Example values, change as needed

//            // Act
//            dbAcc.UpdTotalPages(bookId, newPageCount, volId);

//            // Assert
//            var updatedBook = _context.Vols.FirstOrDefault(b => b.BookId == bookId && b.VolumeId == volId);
//            Assert.NotNull(updatedBook);
//            Assert.Equal(newPageCount, updatedBook.totalPages);
//        }

//        [Fact]
//        public void UpdVolCount_SuccessfullyUpdated()
//        {
//            // Arrange
//            var dbAcc = new DbAcc(_context); // Replace DbAcc with the actual class where UpdVolCount is defined
//            var bookProgressId = 1;
//            var newVolCount = 3; // Example values, change as needed

//            // Act
//            dbAcc.UpdVolCount(bookProgressId, newVolCount);

//            // Assert
//            var updatedBookProgress = _context.BooksProgress.Find(bookProgressId);
//            Assert.NotNull(updatedBookProgress);
//            Assert.Equal(newVolCount, updatedBookProgress.volumesRead);
//        }
//    }
//}
