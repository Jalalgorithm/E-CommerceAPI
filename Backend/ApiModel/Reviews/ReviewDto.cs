using Microsoft.EntityFrameworkCore;

namespace Backend.ApiModel.Reviews
{
    public class ReviewAddDto
    {
        public string Comment { get; set; }
        public string Email { get; set; }
        public decimal Rating { get; set; }
    }
}
