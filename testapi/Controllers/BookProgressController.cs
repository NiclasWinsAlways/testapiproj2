using Microsoft.AspNetCore.Mvc;
using TestRepo.Data;

namespace testapi.Controllers
{
        [Route("api/[controller]")]
        [ApiController]
        public class BookProgressController : ControllerBase
        {
            private readonly DbAccess _dbAccess;

            public BookProgressController(DbAccess dbAccess)
            {
                _dbAccess = dbAccess;
            }



        }
    }