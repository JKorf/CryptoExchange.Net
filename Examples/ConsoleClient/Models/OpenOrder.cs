using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient.Models
{
    public class OpenOrder
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityFilled { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public string OrderSide { get; set; }
        public DateTime OrderTime { get; set; }
    }
}
