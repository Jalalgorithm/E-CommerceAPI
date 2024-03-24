using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Backend.Model
{

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(50)]
        public string PhoneNumber { get; set; } = "";
        [MaxLength(50)]

        public string Email { get; set; } = "";

        public string UniqueOrderId { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        [Precision(16,2)]
        public decimal ShippingFee { get; set; }
        [MaxLength(350)]
        public string DeliveryAddress { get; set; } = "";
        [MaxLength(30)]
        public string PaymentStatus { get; set; } = "";
        [MaxLength (30)]
        public string OrderStatus { get; set; } = "";
        [MaxLength(30)]
        public string PaymentMethod { get; set; } = "";

        public User User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
