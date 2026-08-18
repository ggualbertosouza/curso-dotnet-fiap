using Microsoft.AspNetCore.Mvc;
using Curso.Api.Dto;

namespace Curso.Api.Controllers;

public record Student(int Id, string Name, string Email);

[ApiController]
[Route("[controller]")]
public class StudentController : ControllerBase
{
    private static readonly List<Student> _students = new();
    private static int _nextId = 1;

    [HttpGet]
    public ActionResult<IEnumerable<StudentResponse>> Get()
    {
        var response = _students.Select(StudentResponse.FromStudent);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public ActionResult<StudentResponse> GetById(int id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);

        if (student is null)
            return NotFound();

        return Ok(StudentResponse.FromStudent(student));
    }

    [HttpPost]
    public ActionResult<StudentResponse> Post([FromBody] CreateStudentRequest request)
    {
        var student = new Student(_nextId++, request.Name, request.Email);
        _students.Add(student);

        var response = StudentResponse.FromStudent(student);

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, response);
    }

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] UpdateStudentRequest request)
    {
        var index = _students.FindIndex(s => s.Id == id);

        if (index == -1)
            return NotFound();

        _students[index] = new Student(id, request.Name, request.Email);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);

        if (student is null)
            return NotFound();

        _students.Remove(student);

        return NoContent();
    }
}
