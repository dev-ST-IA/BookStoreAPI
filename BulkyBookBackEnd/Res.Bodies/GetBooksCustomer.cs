namespace BulkyBookBackEnd.Res.Bodies
{
    public class GetBooksCustomer
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public float Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public string Description { get; set; }
        public int Units { get; set; }
        public string ImageUrl { get; set; }
        public string Publisher { get; set; }
        public double Rating { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
