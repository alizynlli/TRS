namespace TRS.Web.ViewModels.ClientUser
{
    public class ClientTaskLineViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public byte TaskStatusInt { get; set; }
        public string TaskStatus { get; set; }
        public string ImportanceDegree { get; set; }
        public string TaskType { get; set; }
    }
}
