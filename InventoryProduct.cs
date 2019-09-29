using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GateBoys.Models;

namespace GateBoys.Models
{
    public class InventoryProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int productId { get; set; }

        [Display(Name = "Brand Id")]
        public int brandId { get; set; }
        
        [Display(Name = "Brand Name")]
        public string brandName { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string productName { get; set; }


        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string productDescription { get; set; }

        public byte[] Image { get; set; }

        [Display(Name ="Category ID")]
        public int categoryId { get; set; }

        public string catId { get; set; }
        //public virtual Category_ Categories { get; set; }

        /// </summary>

        public int SupplierId { get; set; }
        public virtual Supplier Suppliers { get; set; }

        [Required]
        [Display(Name = "Quantity ")]
        public int quantityOnHand { get; set; }

        [Required]
        [Display(Name = "Quantity Ordered")]
        public int quantityToOrder { get; set; }

        [Required]
        [Display(Name = "Unit Price")]
        public decimal unitPrice { get; set; }

        [Display(Name = "Prev Price")]
        public decimal prevUnitPrice { get; set; }

        [Display(Name = "Discount Price")]
        public decimal DiscountPrice { get; set; }

        [Display(Name = "Total Price")]
        public decimal totalPrice { get; set; }

        [Display(Name = "On Promotion")]
        public bool onPromotion { get; set; }

        public string status { get; set; }

        [Display(Name = "Is Ordered")]
        public bool isOrdered { get; set; }

        [Required]
        [Display(Name = "Minimum Stock ")]
        public int minimumStock { get; set; }

        //////////
        public int numRatings { get; set; }
        public int qtySold { get; set; }

        [Display(Name = "Percent")]
        public int perc { get; set; }


    }
    public class InventoryProductList
    {
        public List<InventoryProduct> inventory { get; set; }
    }
}