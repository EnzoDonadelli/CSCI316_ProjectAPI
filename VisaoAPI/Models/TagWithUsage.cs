namespace VisaoAPI.Models
{
    public class TagWithUsage
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }
}