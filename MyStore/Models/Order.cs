namespace MyStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderDetails { get; set; } // JSON string of cart items
        public System.DateTime OrderDate { get; set; }
    }
}
