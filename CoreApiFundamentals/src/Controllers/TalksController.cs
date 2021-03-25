using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/talks")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IGenericRepository _generic;
        private readonly ITalkRepository _talk;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator, IGenericRepository generic, ITalkRepository talk)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
            _generic = generic;
            _talk = talk;
        }

        [HttpGet]
        public async Task<ActionResult<Talk[]>> Get(string moniker)
        {
            try
            {
                return await _talk.GetTalksByMonikerAsync(moniker);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Talk>> Get(string moniker, int id, bool includeSpeakers = false)
        {
            try
            {
                var talk = await _talk.GetTalkByMonikerAsync(moniker, id, includeSpeakers);
                return talk;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Talk>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp does not exists");

                var talk = _mapper.Map<Talk>(model);

                await _generic.AddTalk(model, moniker);

                var url = _linkGenerator.GetPathByAction(HttpContext,
                    "Get",
                    values: new { moniker, id = talk.TalkId });

                return Created(url, _mapper.Map<TalkModel>(talk));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");

            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Talk>> Put(TalkModel model, string moniker, int id)
        {
            try
            {
                var talk = await _talk.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return BadRequest("Couldn't find the talk");

                var ok = await _generic.UpdateTalk(model, moniker, id);

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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = _talk.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return NotFound("Failed to find the talk to delete");
                await _generic.DeleteTalk(moniker, id);

                if (await _generic.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to delete talk");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}