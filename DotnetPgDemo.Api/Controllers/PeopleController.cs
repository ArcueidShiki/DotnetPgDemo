using DotnetPgDemo.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotnetPgDemo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PeopleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePerson(Person person)
        {
            try
            {
                _context.People.Add(person);
                await _context.SaveChangesAsync();
                return CreatedAtRoute("GetPerson", new { id = person.Id }, person);// 201 Created
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CetPerson()
        {
            try
            {
                var people = await _context.People.ToListAsync();
                return Ok(people);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "GetPerson")]
        public async Task<IActionResult> GetPerson(int id)
        {
            try
            {
                var person = await _context.People.FindAsync(id);
                if (person is null)
                {
                    return NotFound($"Person with Id={id} not found.");
                }
                return Ok(person);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePerson(int id, [FromBody] Person person)
        {
            try
            {
                if (id != person.Id)
                {
                    return BadRequest($"Id in URL ({id}) does not match Id in body ({person.Id}).");
                }
                bool personExists = _context.People.Any(p => p.Id == id);
                if (!personExists)
                {
                    return NotFound($"Person with Id={id} not found.");
                }
                _context.People.Update(person);
                await _context.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            try
            {
                Person? person = await _context.People.FindAsync(id);
                if (person is null)
                {
                    return NotFound($"Person with Id={id} not found.");
                }
                _context.People.Remove(person);
                await _context.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
