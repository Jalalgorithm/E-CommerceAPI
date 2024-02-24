namespace Backend.ApiModel.Products
{
    public class GetProductDtoList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public string DisplayImage { get; set; }
        public string Brand { get; set; }

        public string CategoryName { get; set; }
    }
}
