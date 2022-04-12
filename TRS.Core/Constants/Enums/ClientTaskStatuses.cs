using System.ComponentModel;

namespace TRS.Core.Constants.Enums
{
    public enum ClientTaskStatuses : byte
    {
        [Description("Baxılmayıb")]
        NotSeen = 0,
        [Description("Hazırda baxılır")]
        UnderConsideration = 1,
        [Description("Tamamlanıb")]
        Completed = 2,
        [Description("Təsdiqlənib")]
        Confirmed = 3
    }
}
