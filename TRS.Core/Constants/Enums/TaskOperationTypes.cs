using System.ComponentModel;

namespace TRS.Core.Constants.Enums
{
    public enum TaskOperationTypes
    {
        [Description("Yaradıldı")]
        Created = 0,
        [Description("Təhvil alındı")]
        WasTaken = 1,
        [Description("Tamamlandı")]
        Completed = 2,
        [Description("Təsdiqləndi")]
        Confirmed = 3,
        [Description("Təhvil verildi")]
        Transferred = 4
    }
}
