using System.Collections.Generic;

namespace himchistka.Models
{
    public sealed class AppState
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Order> Orders { get; set; } = new List<Order>();

        public int NextUserId { get; set; } = 1;
        public int NextProductId { get; set; } = 1;
        public int NextOrderId { get; set; } = 1;
    }
}
