using MusicSchool.Domain.Curriculum;

namespace MusicSchool.Application.Curriculum;

public static class CurriculumMapper
{
    public static CurriculumNodeDto ToDto(this CurriculumNode node)
    {
        return new CurriculumNodeDto(
            node.Id.Value,
            node.TenantId.Value,
            node.InstrumentId.Value,
            node.ParentNodeId?.Value,
            node.Title,
            node.Type,
            node.SortOrder,
            node.ResourceFileName,
            node.ResourceFileType,
            node.ResourceContentType,
            node.ResourceUploadedByTeacherId?.Value,
            node.ResourceUploadedOnUtc);
    }

    public static StudentCurriculumProgressDto ToDto(this StudentCurriculumProgress progress)
    {
        return new StudentCurriculumProgressDto(
            progress.Id.Value,
            progress.TenantId.Value,
            progress.StudentId.Value,
            progress.CurriculumNodeId.Value,
            progress.Status,
            progress.UpdatedOnUtc,
            progress.CompletedOnUtc);
    }
}
