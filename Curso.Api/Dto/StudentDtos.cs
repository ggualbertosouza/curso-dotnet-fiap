using System.ComponentModel.DataAnnotations;
using Curso.Api.Models;

namespace Curso.Api.Dto;

public class CreateStudentRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class UpdateStudentRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public record StudentResponse(int Id, string Name, string Email)
{
    public static StudentResponse FromStudent(Student student) =>
        new(student.Id, student.Name, student.Email);
}
