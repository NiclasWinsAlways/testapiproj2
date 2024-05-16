using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestRepo.Data;
using TestRepo.DTO;
using TestRepo.Interface;
using testapi.Controllers;
using Xunit;

namespace TestProject2.repotest
{
    public class VolumesControllerTests : IDisposable
    {
        private readonly Dbcontext _context;
        private readonly VolumesController _controller;

        public VolumesControllerTests()
        {
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new Dbcontext(options);
            var dbAccess = new DbAccess(_context);
            _controller = new VolumesController(dbAccess);

            SeedDatabase();
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

            _context.Acc.AddRange(new List<Account>
            {
                new Account { Id = 1, UserName = "TestUser", Name = "Test User", Email = "testuser@example.com", Password = "Test123" }
            });

            _context.Books.AddRange(new List<Book>
            {
                new Book { Id = 1, Title = "Sample Book", Author = "Author Test" },
                new Book { Id = 2, Title = "Another Sample Book", Author = "Another Author" }
            });

            _context.Vols.AddRange(new List<Volume>
            {
                new Volume { VolumeId = 1, BookId = 1, VolNumber = 1, totalPages = 150 },
                new Volume { VolumeId = 2, BookId = 1, VolNumber = 2, totalPages = 200 },
                new Volume { VolumeId = 3, BookId = 2, VolNumber = 1, totalPages = 100 }
            });

            _context.volProgress.AddRange(new List<VolProgress>
            {
                new VolProgress { Id = 1, volId = 1, BookId = 1, AccountId = 1, pagesRead = 50 }
            });

            _context.SaveChanges();
        }

        [Fact]
        public void GetVolProgress_ValidRequest_ReturnsOk()
        {
            // Arrange
            int accountId = 1;
            int volId = 1;

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

        [Fact]
        public void GetBookVol_WithExistingVolumes_ReturnsOkWithVolumes()
        {
            // Arrange
            int bookId = 1;

            // Act
            var result = _controller.GetBookVol(bookId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var volumes = Assert.IsType<List<Volume>>(okResult.Value);
            Assert.Equal(2, volumes.Count);
        }

        [Fact]
        public void CreateVolProgress_ValidRequest_ReturnsNoContent()
        {
            // Arrange
            var volProgress = new CVolProgress { accountId = 1, volId = 1, bookId = 1, PagesRead = 100 };

            // Act
            var result = _controller.CreateVolProgress(volProgress);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void GetVolProgress_VolProgressDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            int accountId = 999;
            int volId = 999;

            // Act
            var result = _controller.GetVolProgress(accountId, volId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void UpdateVolumeCount_ValidUpdate_ReturnsNoContent()
        {
            // Arrange
            int bookId = 1;
            int volCount = 250;

            // Act
            var result = _controller.UpdateVolumeCount(bookId, volCount);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
