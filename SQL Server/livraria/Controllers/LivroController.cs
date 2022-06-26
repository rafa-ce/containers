using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using livraria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace livraria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivroController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LivroController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var livros = await _context.Livros.ToListAsync();
            return Ok(livros);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var livro = await _context.Livros.FirstOrDefaultAsync(b => b.Id == id);
            return Ok(livro);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Livro livro)
        {
            try
            {
                _context.Livros.Add(livro);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { Id = livro.Id }, livro);
            }
            catch(Exception)
            {
                return StatusCode(500);
            }
        }
    }
}