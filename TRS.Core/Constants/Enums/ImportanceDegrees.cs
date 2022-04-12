using System.ComponentModel;

namespace TRS.Core.Constants.Enums
{
    public enum ImportanceDegrees : byte
    {
        [Description("Təcili")]
        Urgent = 0,
        [Description("Gün içində")]
        DuringTheDay = 1,
        [Description("3 gün içində")]
        WithinThreeDays = 2,
        [Description("Həftə içində")]
        DuringTheWeek = 3
    }
}
