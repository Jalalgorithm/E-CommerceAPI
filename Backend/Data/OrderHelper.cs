namespace Backend.Data
{
    public class OrderHelper
    {
        public static decimal ShippingFee { get; } = 5;
        public static Dictionary<string, string> PaymentMethods { get; } = new Dictionary<string, string>()
        {
            {"Cash" , "Cash On Delivery" },
            {"PayStack" , "PayStack" }
        };

        public static List<string> PaymentStatuses { get; } = new()
        {
            "Pending" , "Accepted" , "Canceled"
        };
        public static List<string> OrderStatuses { get; } = new()
        {
            "Created" , "Accepted" , "Canceled" , "Shipped" , "Delivered" , "Returned"
        };


        public static Dictionary<int,int> GetProductDictionary (string productIdentifiers)
        {
            var productDictionary  = new Dictionary<int, int> (); 
            
            if(productIdentifiers.Length > 0)
            {
                string[] productIdArray = productIdentifiers.Split ('-');
                foreach (var productId in productIdArray) 
                {
                    try
                    {
                        int id = int.Parse (productId);

                        if (productDictionary.ContainsKey(id))
                        {
                            productDictionary[id] += 1;
                        }
                        else
                        {
                            productDictionary.Add(id, 1);
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }


            return productDictionary;
        }
    }
}
