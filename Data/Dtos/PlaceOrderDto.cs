using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dtos
{
    public class PlaceOrderDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
