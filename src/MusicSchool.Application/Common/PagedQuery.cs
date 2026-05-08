namespace MusicSchool.Application.Common;

public record PagedQuery(int PageNumber = 1, int PageSize = 20)
{
    public int NormalizedPageNumber => PageNumber < 1 ? 1 : PageNumber;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 20,
        > 100 => 100,
        _ => PageSize
    };

    public int Skip => (NormalizedPageNumber - 1) * NormalizedPageSize;
}
