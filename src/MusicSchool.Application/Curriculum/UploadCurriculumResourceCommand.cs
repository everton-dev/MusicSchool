namespace MusicSchool.Application.Curriculum;

public sealed record UploadCurriculumResourceCommand(
    Guid CurriculumNodeId,
    Guid UploadedByTeacherId,
    string FileName,
    string ContentType,
    Stream Content);
