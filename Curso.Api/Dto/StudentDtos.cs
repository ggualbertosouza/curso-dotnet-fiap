using System.ComponentModel.DataAnnotations;
using Curso.Api.Controllers;

namespace Curso.Api.Dto;

public record CreateStudentRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string Name,

    [property: Required]
    [property: EmailAddress]
    string Email
);

public record UpdateStudentRequest(
    [property: Required]
    [property: StringLength(100, MinimumLength = 2)]
    string Name,

    [property: Required]
    [property: EmailAddress]
    string Email
);

public record StudentResponse(int Id, string Name, string Email)
{
    public static StudentResponse FromStudent(Student student) =>
        new(student.Id, student.Name, student.Email);
}
