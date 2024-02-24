using Microsoft.EntityFrameworkCore;

namespace Backend.ApiModel.Reviews
{
    public class ReviewGetDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Comment { get; set; }
        public string Email { get; set; }
        public decimal Rating { get; set; }
    }
}
