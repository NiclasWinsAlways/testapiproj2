using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;
using TestRepo.DTO;
using System.Linq;
using System.Collections.Generic;
using TestRepo.Interface;
using Microsoft.AspNetCore.Mvc;
using testapi.Controllers;
using Microsoft.EntityFrameworkCore.Storage;
using System.Net;

namespace TestProject2.repotest
{
    public class VolumesControllerTests : IDisposable
    {
        private readonly Dbcontext _context;
        private readonly DbAccess _dbAccess;
        private readonly VolumesController _controller;
        private readonly IDbContextTransaction _transaction;

        public VolumesControllerTests()
        {
            var connectionString = @"Data Source=(localdb)\MSSQLLocalDB; Integrated Security=True; Initial Catalog=MyDatabase; TrustServerCertificate=True;";
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseSqlServer(connectionString)
                .Options;

            _context = new Dbcontext(options);
            _dbAccess = new DbAccess(_context);
            _controller = new VolumesController(_dbAccess);

            _transaction = _context.Database.BeginTransaction();
        }

        public void Dispose()
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            _context.Database.ExecuteSqlRaw("DELETE FROM VolProgress");
            _context.Database.ExecuteSqlRaw("DELETE FROM Vols");
            _context.Database.ExecuteSqlRaw("DELETE FROM Books");
            _context.Database.ExecuteSqlRaw("DELETE FROM Accounts");
            _context.SaveChanges();

            // Accounts
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Accounts ON");
            _context.Acc.AddRange(new List<Account> {
        new Account { Id = 1, UserName = "TestUser", Name = "Test User", Email = "testuser@example.com", Password = "Test123" }
    });
            _context.SaveChanges();
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Accounts OFF");

            // Books
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Books ON");
            _context.Books.AddRange(new List<Book> {
        new Book { Id = 1, Title = "Sample Book", Author = "Author Test" },
        new Book { Id = 2, Title = "Another Sample Book", Author = "Another Author" }
    });
            _context.SaveChanges();
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Books OFF");

            // Volumes
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Vols ON");
            _context.Vols.AddRange(new List<Volume> {
        new Volume { VolumeId = 1, BookId = 1, VolNumber = 1, totalPages = 150 },
        new Volume { VolumeId = 2, BookId = 1, VolNumber = 2, totalPages = 200 },
        new Volume { VolumeId = 3, BookId = 2, VolNumber = 1, totalPages = 100 }
    });
            _context.SaveChanges();
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Vols OFF");

            // VolProgress
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT VolProgress ON");
            _context.volProgress.AddRange(new List<VolProgress> {
        new VolProgress { Id = 1, volId = 1, BookId = 1, AccountId = 1, pagesRead = 50 } // Ensure this BookId exists
    });
            _context.SaveChanges();
            _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT VolProgress OFF");
        }




        [Fact]
        public void GetVolProgress_ValidRequest_ReturnsOk()
        {
            // Arrange
            SeedDatabase();  // Make sure it's called here if not done in the constructor
            int accountId = 1;
            int volId = 1; // Ensure this volId exists and matches a valid BookId

            // Act
            var result = _controller.GetVolProgress(accountId, volId) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.IsType<VolProgress>(result.Value);
        }


        [Fact]
        public void CreateVolume_ValidRequest_ReturnsOk()
        {
            SeedDatabase();  // Ensuring fresh data for each test

            // Arrange
            int bookId = 1;
            int volNumber = 3;

            // Act
            var result = _controller.CreateVolume(bookId, volNumber) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Volume created successfully.", result.Value.ToString());
        }




        //[Fact]
        //public void UpdateTotalPages_ValidRequest_ReturnsNoContent()
        //{
        //    // Arrange
        //    int bookId = 1;
        //    int totalPages = 250;
        //    int volId = 1;

        //    // Act
        //    var result = _controller.UpdateTotalCount(bookId, totalPages, volId);

        //    // Assert
        //    Assert.IsType<NoContentResult>(result);
        //}
        [Fact]
        public void GetBookVol_WithExistingVolumes_ReturnsOkWithVolumes()
        {
            // Arrange
            SeedDatabase();  // Ensuring data is present
            int bookId = 1;  // Ensure this book ID exists in the seeded data and has associated volumes

            // Act
            var result = _controller.GetBookVol(bookId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var volumes = Assert.IsType<List<Volume>>(okResult.Value);
            Assert.Equal(2, volumes.Count); // Check if the count of volumes matches expected
        }

        [Fact]
        public void CreateVolProgress_ValidRequest_ReturnsNoContent()
        {
            SeedDatabase();  // Ensure fresh data is available
            var bookId = _context.Books.First().Id;  // Assuming a book is available

            // Clear any existing VolProgress that might interfere
            _context.volProgress.RemoveRange(_context.volProgress.Where(vp => vp.AccountId == 1 && vp.volId == 1 && vp.BookId == bookId));
            _context.SaveChanges();

            var volProgress = new CVolProgress { accountId = 1, volId = 1, bookId = bookId, PagesRead = 100 };

            // Act
            var result = _controller.CreateVolProgress(volProgress);
            _context.SaveChanges();  // Ensure changes are committed

            // Assert
            Assert.IsType<NoContentResult>(result);
            var addedProgress = _context.volProgress.AsNoTracking().FirstOrDefault(vp => vp.AccountId == 1 && vp.volId == 1 && vp.BookId == bookId);
            Assert.NotNull(addedProgress);
            Assert.Equal(100, addedProgress.pagesRead);  // Check if the pages read value is as expected
        }




        [Fact]
        public void GetVolProgress_VolProgressDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            int accountId = 999;  // Use an ID that does not exist
            int volId = 999;      // Use an ID that does not exist

            // Act
            var result = _controller.GetVolProgress(accountId, volId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result); // Verify that NotFound result is returned
        }

        [Fact]
        public void UpdateVolumeCount_ValidUpdate_ReturnsNoContent()
        {
            // Arrange
            int bookId = 1;  // Assume this is a valid book ID with existing volumes
            int volCount = 250;  // New total volumes count to update

            // Act
            var result = _controller.UpdateVolumeCount(bookId, volCount);

            // Assert
            Assert.IsType<NoContentResult>(result); // Confirm that no content result is returned
        }








    }
}
