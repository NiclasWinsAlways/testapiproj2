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
        // Declare the context and controller objects
        private readonly Dbcontext _context;
        private readonly VolumesController _controller;

        public VolumesControllerTests()
        {
            // Set up the in-memory database options
            var options = new DbContextOptionsBuilder<Dbcontext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_VolumesControllerTests")
                .Options;

            // Initialize the context and controller with the in-memory database
            _context = new Dbcontext(options);
            var dbAccess = new DbAccess(_context);
            _controller = new VolumesController(dbAccess);

            // Seed the in-memory database with initial data
            SeedDatabase();
        }

        // Clean up the database after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Seed the database with initial data for testing
        private void SeedDatabase()
        {
            // Ensure the database is clean before seeding
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Create initial account data
            _context.Acc.AddRange(new List<Account>
            {
                new Account { Id = 1, UserName = "TestUser", Name = "Test User", Email = "testuser@example.com", Password = "Test123" }
            });

            // Create initial book data
            _context.Books.AddRange(new List<Book>
            {
                new Book { Id = 1, Title = "Sample Book", Author = "Author Test" },
                new Book { Id = 2, Title = "Another Sample Book", Author = "Another Author" }
            });

            // Create initial volume data
            _context.Vols.AddRange(new List<Volume>
            {
                new Volume { VolumeId = 1, BookId = 1, VolNumber = 1, totalPages = 150 },
                new Volume { VolumeId = 2, BookId = 1, VolNumber = 2, totalPages = 200 },
                new Volume { VolumeId = 3, BookId = 2, VolNumber = 1, totalPages = 100 }
            });

            // Create initial volume progress data
            _context.volProgress.AddRange(new List<VolProgress>
            {
                new VolProgress { Id = 1, volId = 1, BookId = 1, AccountId = 1, pagesRead = 50 }
            });

            // Save all changes to the context
            _context.SaveChanges();
        }

        [Fact]
        // Test that GetVolProgress returns Ok for a valid request
        public void GetVolProgress_ValidRequest_ReturnsOk()
        {
            // Arrange: Set valid account and volume IDs
            int accountId = 1;
            int volId = 1;

            // Act: Call the GetVolProgress method
            var result = _controller.GetVolProgress(accountId, volId) as ObjectResult;

            // Assert: Verify the result is Ok and contains the expected VolProgress
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.IsType<VolProgress>(result.Value);
        }

        [Fact]
        // Test that CreateVolume returns Ok for a valid request
        public void CreateVolume_ValidRequest_ReturnsOk()
        {
            // Arrange: Set valid book ID and volume number
            int bookId = 1;
            int volNumber = 3;

            // Act: Call the CreateVolume method
            var result = _controller.CreateVolume(bookId, volNumber) as ObjectResult;

            // Assert: Verify the result is Ok and the volume creation message is correct
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Volume created successfully.", result.Value.ToString());
        }

        [Fact]
        // Test that GetBookVol returns Ok with volumes for an existing book ID
        public void GetBookVol_WithExistingVolumes_ReturnsOkWithVolumes()
        {
            // Arrange: Set a valid book ID
            int bookId = 1;

            // Act: Call the GetBookVol method
            var result = _controller.GetBookVol(bookId);

            // Assert: Verify the result is Ok and contains the expected volumes
            var okResult = Assert.IsType<OkObjectResult>(result);
            var volumes = Assert.IsType<List<Volume>>(okResult.Value);
            Assert.Equal(2, volumes.Count);
        }

        [Fact]
        // Test that CreateVolProgress returns NoContent for a valid request
        public void CreateVolProgress_ValidRequest_ReturnsNoContent()
        {
            // Arrange: Create a valid volume progress request
            var volProgress = new CVolProgress { accountId = 1, volId = 1, bookId = 1, PagesRead = 100 };

            // Act: Call the CreateVolProgress method
            var result = _controller.CreateVolProgress(volProgress);

            // Assert: Verify the result is NoContent
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        // Test that GetVolProgress returns NotFound for a non-existing volume progress
        public void GetVolProgress_VolProgressDoesNotExist_ReturnsNotFound()
        {
            // Arrange: Set non-existing account and volume IDs
            int accountId = 999;
            int volId = 999;

            // Act: Call the GetVolProgress method
            var result = _controller.GetVolProgress(accountId, volId);

            // Assert: Verify the result is NotFound
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Test that UpdateVolumeCount returns NoContent for a valid update
        public void UpdateVolumeCount_ValidUpdate_ReturnsNoContent()
        {
            // Arrange: Set valid book ID and volume count
            int bookId = 1;
            int volCount = 250;

            // Act: Call the UpdateVolumeCount method
            var result = _controller.UpdateVolumeCount(bookId, volCount);

            // Assert: Verify the result is NoContent
            Assert.IsType<NoContentResult>(result);
        }
    }
}
