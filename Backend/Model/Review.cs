using Microsoft.EntityFrameworkCore;

namespace Backend.Model
{
    [Index("Email" , IsUnique =true)]
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Comment { get; set; }
        public string Email { get; set; }
        [Precision(16,2)]
        public decimal Rating { get; set; }



        public Product Product { get; set; }

    }
}
