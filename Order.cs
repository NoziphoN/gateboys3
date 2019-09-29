using GateBoys.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GateBoys.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public int OrderProgress { get; set; }
        public int OrderQuantity { get; set; }
        public decimal TotalOrderCost { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public string DeliveryAddress { get; set; }
        public int Id { get; set; }
        public string username { get; set; }
        public string orderedItems { get; set; }
        public int orderedQty { get; set; }
        public string Cell { get; set; }
        [Display(Name = "Driver")]
        public string AssignedDriver { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

        public Order()
        {
            this.OrderDate = DateTime.Now;
            this.OrderProgress = 1;
        }
    }
}