using MusicSchool.Domain.Curriculum;

namespace MusicSchool.Application.Curriculum;

public sealed record UpdateStudentCurriculumProgressCommand(
    Guid StudentId,
    Guid CurriculumNodeId,
    StudentCurriculumProgressStatus Status);
