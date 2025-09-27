namespace MyStore.Models
{
    // DTO (Data Transfer Object) for receiving order data from the frontend
    public class OrderDto
    {
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public List<CartItemDto> CartItems { get; set; }
    }
}
