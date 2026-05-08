using MusicSchool.Domain.Common;

namespace MusicSchool.Domain.Teachers;

public sealed class TeacherInstrument
{
    private TeacherInstrument()
    {
    }

    internal TeacherInstrument(TeacherId teacherId, InstrumentId instrumentId)
    {
        TeacherId = teacherId;
        InstrumentId = instrumentId;
    }

    public TeacherId TeacherId { get; private set; }

    public InstrumentId InstrumentId { get; private set; }
}
