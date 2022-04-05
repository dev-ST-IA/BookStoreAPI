namespace BulkyBookBackEnd.Models
{
    public class DateRange
    {
        public DateOnly? Start { get; set; } = DateOnly.MinValue;
        public DateOnly? End { get; set; } = DateOnly.MaxValue;
    }
}
