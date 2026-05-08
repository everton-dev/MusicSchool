using MusicSchool.Domain.Curriculum;

namespace MusicSchool.API.Contracts;

public sealed record UpdateStudentCurriculumProgressRequest(StudentCurriculumProgressStatus Status);
