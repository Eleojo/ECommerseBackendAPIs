using Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dtos
{
    public class StatusChangeDto
    {
        public OrderStatusEnum OldStatus { get; set; }
        public OrderStatusEnum NewStatus { get; set;}
    }
}
