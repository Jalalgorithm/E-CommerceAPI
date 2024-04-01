namespace Backend.ApiModel.OrderDtoModel
{
    public class OrderViewDto
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string FullName { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
