namespace BulkyBookBackEnd.Models
{
    public class DateRange
    {
        public DateTime? Start { get; set; } = DateTime.MinValue;
        public DateTime? End { get; set; } = DateTime.MaxValue;
    }
}
