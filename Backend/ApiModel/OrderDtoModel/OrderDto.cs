using System.ComponentModel.DataAnnotations;

namespace Backend.ApiModel.OrderDtoModel
{
    public class OrderDto
    {
        [Required]
        public string ProductIdentifiers { get; set; }
        [Required]
        [MaxLength(100)]
        public string DeliveryAddress { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
    }
}
