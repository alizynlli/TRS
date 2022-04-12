namespace TRS.Web.ViewModels.ClientUser
{
    public class TaskDetailsViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TaskType { get; set; }
        public string TaskStatus { get; set; }
        public byte TaskStatusConst { get; set; }
        public string ImportanceDegree { get; set; }
        public string CreateDate { get; set; }
        public string UnderConsiderationDate { get; set; }
        public string CompletedDate { get; set; }
        public string ConfirmationDate { get; set; }
    }
}
