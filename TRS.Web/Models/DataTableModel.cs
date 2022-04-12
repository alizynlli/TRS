using System.Collections.Generic;

namespace TRS.Web.Models
{
    public class DataTableModel<TData>
    {
        public string Draw { get; set; }
        public int RecordsFiltered { get; set; }
        public int RecordsTotal { get; set; }
        public List<TData> Data { get; set; }
    }
}
