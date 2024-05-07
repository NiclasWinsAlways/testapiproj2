using Microsoft.AspNetCore.Mvc;
using TestRepo.Data;
using TestRepo.DTO;
using TestRepo.Interface;

namespace testapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolumesController : ControllerBase
    {
        private readonly DbAccess _dbAccess;

        public VolumesController(DbAccess dbAccess)
        {
            _dbAccess = dbAccess;
        }

        [HttpPost("add-volume")]
        public IActionResult CreateVolume([FromQuery] int bookId, [FromQuery] int volNumber)
        {
            if (bookId <= 0 || volNumber <= 0)
            {
                return BadRequest("Invalid book ID or volume number.");
            }

            try
            {
                var result = _dbAccess.CreateVol(bookId, volNumber);  // Ensure that CreateVol no longer expects totalPages
                if (result == "Book doesn't exist")
                {
                    return NotFound("Book not found.");
                }
                return Ok("Volume created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("GetBookVol")]
        public IActionResult GetBookVol(int BookId)
        {
            var VolList = _dbAccess.GetBookVol(BookId);
            if (VolList == null || !VolList.Any())
            {
                return NotFound($"No volumes found for book ID {BookId}.");
            }
            return Ok(VolList);
        }


        [HttpPut("UpdTotalVol")]
        public IActionResult UpdateVolumeCount(int id, int volCount)
        {
            _dbAccess.UpdVolCount(id, volCount);
            return NoContent();
        }

        [HttpPost("UpdPagesRead")]
        public IActionResult UpdPagesRead([FromBody] updpage pagesread)
        {
            _dbAccess.UpdPageCount(pagesread);
            return Ok(pagesread);
        }

        [HttpPost("CreateVolProgress")]
        public IActionResult CreateVolProgress([FromBody] CVolProgress request)
        {
            _dbAccess.CreateVolProgress(request);
            return NoContent();
        }

        [HttpGet("GetVolProgress")]
        public IActionResult GetVolProgress(int accountId, int volId)
        {
            var volProgress = _dbAccess.GetVolProgress(accountId, volId);
            if (volProgress == null)
            {
                return NotFound($"No volume progress found for account ID {accountId} and volume ID {volId}.");
            }
            return Ok(volProgress);
        }

        [HttpPut("UpdTotalPages")]
        public IActionResult UpdTotalPages(int bookId, int totalPages, int volId)
        {
            _dbAccess.UpdTotalPages(bookId, totalPages, volId);
            return NoContent();
        }
    }
}
