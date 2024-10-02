using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dtos
{
    public class OrderResponseDto
    {
        public Guid OrderId { get; set; } // Unique identifier for the order
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderDto Order { get; set; }
        public ProductDto Product { get; set; }
    }


}
