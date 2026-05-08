using MusicSchool.Application.Common;

namespace MusicSchool.Application.Families;

public sealed record ListFamilyGroupsQuery(int PageNumber = 1, int PageSize = 20) : PagedQuery(PageNumber, PageSize);
