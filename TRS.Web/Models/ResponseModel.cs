using System.Collections.Generic;

namespace TRS.Web.Models
{
    public class ResponseModel
    {
        public ResponseModel()
        {
            ErrorMessages = new List<string>();
        }

        public bool Succeeded { get; set; }
        public object Data { get; set; }
        public List<string> ErrorMessages { get; set; }

        public static ResponseModel Success() => new ResponseModel { Succeeded = true };
    }
}
