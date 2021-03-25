using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IGenericRepository _generic;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, LinkGenerator linkGenerator, IGenericRepository generic)
        {
            _repository = repository;
            _linkGenerator = linkGenerator;
            _generic = generic;
        }

        [HttpGet]
        public async Task<ActionResult<Camp[]>> Get(bool includeTalks = false)
        {
            try
            {
                return await _repository.GetAllCampsAsync(includeTalks);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<Camp>> Get(string moniker, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetCampAsync(moniker, includeTalks);

                if (results == null) return NotFound();

                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }
        [HttpGet("search")]
        public async Task<ActionResult<Camp[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return results;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Camp>> Post(CampModel entity)
        {
            try
            {
                var location = _linkGenerator.GetPathByAction("Get",
                    "Camps",
                    new { moniker = entity.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                await _generic.AddCamp(entity);

                return Created(location, entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<Camp>> Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound($"Could not find camp with moniker of {moniker}");

                var ok = await _generic.UpdateCamp(model, moniker);

                if (ok)
                    return Ok();
                else
                    return BadRequest();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound();

                await _generic.DeleteCamp(moniker);

                if (await _generic.SaveChangesAsync())
                {
                    return Ok("Deleted");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure"); ;
            }

            return BadRequest();
        }
    }
}
