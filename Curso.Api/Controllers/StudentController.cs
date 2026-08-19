using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Curso.Api.Data;
using Curso.Api.Dto;
using Curso.Api.Models;

namespace Curso.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentResponse>>> Get()
    {
        var students = await _context.Students.ToListAsync();
        var response = students.Select(StudentResponse.FromStudent);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentResponse>> GetById(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student is null)
            return NotFound();

        return Ok(StudentResponse.FromStudent(student));
    }

    [HttpPost]
    public async Task<ActionResult<StudentResponse>> Post([FromBody] CreateStudentRequest request)
    {
        var student = new Student { Name = request.Name, Email = request.Email };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var response = StudentResponse.FromStudent(student);

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateStudentRequest request)
    {
        var student = await _context.Students.FindAsync(id);

        if (student is null)
            return NotFound();

        student.Name = request.Name;
        student.Email = request.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student is null)
            return NotFound();

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
