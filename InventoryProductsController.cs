using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GateBoys.Models;
using System.IO;
using IdentitySample.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Mail;
using System.Configuration;
using System.Net.Mime;
using GateBoys.Helpers;

namespace GateBoys.Controllers
{

    public class InventoryProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: InventoryProducts
        //[Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            products.updateStatus();
            ViewBag.low = db.Products.Where(a => a.quantityOnHand < a.minimumStock && a.quantityOnHand > 0 && a.status != "Ordered").ToList().Count;
            ViewBag.outOf = db.Products.Where(a => a.quantityOnHand == 0 && a.status != "Ordered").ToList().Count;
            ViewBag.alert = ViewBag.low + ViewBag.outOf;
            return View(db.Products.ToList());
        }
        public ActionResult Alert()
        {
            ViewBag.low = db.Products.Where(a => a.quantityOnHand < a.minimumStock && a.quantityOnHand > 0 && a.status != "Ordered").ToList().Count;
            ViewBag.outOf = db.Products.Where(a =>  a.quantityOnHand ==0 && a.status != "Ordered").ToList().Count;
            ViewBag.alert = ViewBag.low+ViewBag.outOf;
            return View(db.Products.ToList());
        }
        public ActionResult placeOrder(int? id)
        {
            if(id == null)
            {
                return RedirectToAction("alert");
            }

            InventoryProductList products = new InventoryProductList();

            products.inventory = db.Products.ToList().Where(x => x.SupplierId == id && x.quantityOnHand < x.minimumStock).ToList();
            return View(products);
        }

        [HttpPost]
        public ActionResult placeOrder(InventoryProductList model)
        {

            var selectedProd = model.inventory.Where(x => x.isOrdered == true).ToList();
            string productsOrdered = "";
            string supplierName = "";
            string suplierEmail = "";
            string managerEmail = User.Identity.GetUserName();

            foreach (InventoryProduct v in selectedProd)
            {
                OrderSupply o = new OrderSupply();
                o.supplierEmail = db.Suppliers.ToList().Where(x => x.supplierid == v.SupplierId).FirstOrDefault().supplierEmail;
                o.supplier = db.Suppliers.ToList().Where(x => x.supplierid == v.SupplierId).FirstOrDefault().supplierName;              
                o.suplyNum = db.Suppliers.ToList().Where(x => x.supplierid == v.SupplierId).FirstOrDefault().supplierNumber;
                o.itemQty = db.Products.ToList().Where(x => x.productId == v.productId).FirstOrDefault().quantityToOrder;
                o.totalOrder = (db.Products.ToList().Where(x => x.productId == v.productId).FirstOrDefault().quantityToOrder) * (db.Products.ToList().Where(x => x.productId == v.productId).FirstOrDefault().prevUnitPrice);
                o.dateOrdered = DateTime.Now.ToString();
                o.ProductsList = db.Products.ToList().Where(x => x.productId == v.productId).FirstOrDefault().productName;
                o.status = "Pending";
                o.isOrdered = true;
                o.orderedBy = User.Identity.GetUserName();
                suplierEmail = db.Suppliers.ToList().Where(x => x.supplierid == v.SupplierId).FirstOrDefault().supplierEmail;
                supplierName = db.Suppliers.ToList().Where(x => x.supplierid == v.SupplierId).FirstOrDefault().supplierName;
                productsOrdered += $" { o.ProductsList} ({o.itemQty}), ";
                InventoryProduct pp = db.Products.ToList().Where(x => x.productId == v.productId).FirstOrDefault();
                pp.status = "Ordered";
                db.OrderSupplies.Add(o);
                db.SaveChanges();
            }
            if (suplierEmail != null)
            {
                string msg = $"Hi {supplierName}. We would like to order the following products { productsOrdered } Contact us at gateboys@gmail.com for enquiries.";
                emailhelper.sendMail(suplierEmail, "Placing new order", msg);
                string newMsg = $"Hi you have placed an order to { supplierName} for the following products { productsOrdered } Contact us at gateboys@gmail.com for enquiries.";
                emailhelper.sendMail(managerEmail, "Placed new order", newMsg);

            }
            return View("addProdSuc");
        }

        public ActionResult addProdSuc()
        {
            return View();
        }
        public ActionResult confirmStock(int id)
        {
            OrderSupply o = db.OrderSupplies.ToList().Where(x => x.orderId == id).FirstOrDefault();
            InventoryProduct i = db.Products.ToList().Where(x => x.productName == o.ProductsList).FirstOrDefault();
            i.quantityOnHand = i.quantityOnHand + o.itemQty;
            i.status = "In Stock";
            o.status = "Delivered";
            db.SaveChanges();
            return RedirectToAction("Index", "OrderSupplies");
        }
        
        public ActionResult AlertNotification()
        {
            return View(db.Products.ToList());
        }
        public ActionResult AlertNotification2()
        {
            return View(db.Products.ToList());
        }
        //
        // GET: InventoryProducts/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryProduct inventoryProduct = db.Products.Find(id);
            @ViewBag.Category = db.Category_.FirstOrDefault(a => a.categoryId == inventoryProduct.categoryId).categoryName;
            if (inventoryProduct == null)
            {
                return HttpNotFound();
            }
            return View(inventoryProduct);
        }

        // GET: InventoryProducts/Create
       // [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.SupplierId = new SelectList(db.Suppliers.ToList(), "supplierid", "supplierName");
            return View();
        }

        // POST: InventoryProducts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "productId,brandId,brandName,productName,productDescription,Image,categoryId,SupplierId,quantityOnHand,minimumStock," +
            "unitPrice,DiscountPrice,totalPrice,prevUnitPrice,quantityToOrder,onPromotion,status,isOrdered,onOrderednum,numRatings")] InventoryProduct inventoryProduct, HttpPostedFileBase upload)
        {
            inventoryProduct.totalPrice = 0;
            inventoryProduct.perc = 0;
            inventoryProduct.brandName = db.brands.FirstOrDefault(a => a.brandId == inventoryProduct.brandId).brandName;
            inventoryProduct.Image = uploader.getFileByte(upload);
            if (ModelState.IsValid)
            {
                db.Products.Add(inventoryProduct);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.categoryId = new SelectList(db.Category_, "categoryId", "categoryName", inventoryProduct.categoryId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "supplierid", "supplierName", inventoryProduct.SupplierId);
            return View(inventoryProduct);
        }
        public JsonResult getBrand(int id)
        {
            var brands = db.brands.Where(a => a.categoryId == id).OrderBy(a => a.brandName).ToList();
            return Json(brands, JsonRequestBehavior.AllowGet);
        }

        // GET: InventoryProducts/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryProduct inventoryProduct = db.Products.Find(id);
            if (inventoryProduct == null)
            {
                return HttpNotFound();
            }
           // ViewBag.categoryId = new SelectList(db.Category_, "categoryId", "categoryName", inventoryProduct.categoryId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "supplierid", "supplierName");
            return View(inventoryProduct);
        }

        // POST: InventoryProducts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "productId, brandId, brandName, productName, productDescription, Image,  categoryId, SupplierId, quantityOnHand, minimumStock, unitPrice, DiscountPrice, totalPrice, prevUnitPrice, quantityToOrder, onPromotion, status, isOrdered, onOrderednum, numRatings")] InventoryProduct inventoryProduct, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {

                if(upload!=null)
                {
                    inventoryProduct.Image = uploader.getFileByte(upload);
                }
                db.Entry(inventoryProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
           // ViewBag.categoryId = new SelectList(db.Category_, "categoryId", "categoryName", inventoryProduct.categoryId);

            ViewBag.SupplierId = new SelectList(db.Suppliers, "supplierid", "supplierName", inventoryProduct.SupplierId);
            return View(inventoryProduct);
        }
        
        public ActionResult promo(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryProduct inventoryProduct = db.Products.Find(id);
            if (inventoryProduct == null)
            {
                return HttpNotFound();
            }
            inventoryProduct.onPromotion = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult discount(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryProduct inventoryProduct = db.Products.Find(id);
            if (inventoryProduct == null)
            {
                return HttpNotFound();
            }
            //  ViewBag.categoryId = new SelectList(db.Category_, "categoryId", "categoryName", inventoryProduct.categoryId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "supplierid", "supplierName");
            return View(inventoryProduct);
        }

        // POST: InventoryProducts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult discount([Bind(Include = "productId, brandId, brandName, productName, productDescription, Image,  categoryId, SupplierId, quantityOnHand, minimumStock, unitPrice, DiscountPrice, totalPrice, prevUnitPrice, quantityToOrder, onPromotion, status, isOrdered, onOrderednum, numRatings")] InventoryProduct inventoryProduct, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                if (inventoryProduct.onPromotion == true && inventoryProduct.DiscountPrice > 0)
                {
                    inventoryProduct.DiscountPrice = (inventoryProduct.DiscountPrice / 100) * inventoryProduct.unitPrice;
                    inventoryProduct.unitPrice = inventoryProduct.unitPrice - inventoryProduct.DiscountPrice;
                }
                else if (inventoryProduct.onPromotion == false)
                {
                    inventoryProduct.unitPrice = inventoryProduct.unitPrice + inventoryProduct.DiscountPrice;
                }
                if (upload != null && upload.ContentLength > 0)
                {
                    using (var reader = new System.IO.BinaryReader(upload.InputStream))
                    {
                        inventoryProduct.Image = reader.ReadBytes(upload.ContentLength);
                        if (inventoryProduct.Image == null)
                        {

                            return View(inventoryProduct);
                        }
                    }
                }

                db.Entry(inventoryProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SupplierId = new SelectList(db.Suppliers, "supplierid", "supplierName", inventoryProduct.SupplierId);
            return View(inventoryProduct);
        }


        // GET: InventoryProducts/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryProduct inventoryProduct = db.Products.Find(id);
            if (inventoryProduct == null)
            {
                return HttpNotFound();
            }
            return View(inventoryProduct);
        }

        // POST: InventoryProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InventoryProduct inventoryProduct = db.Products.Find(id);
            db.Products.Remove(inventoryProduct);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult GetPic(string id)
        {
            int myId = Convert.ToInt32(id);
            var fileToRetrieve = db.Products.FirstOrDefault(a => a.productId == myId);
            var ContentType = "image/jpg";
            return File(fileToRetrieve.Image, ContentType);
        }
        // GET: InventoryProducts/Edit/5
        [Authorize(Roles = "Stock Manager")]
        public ActionResult EditOnOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryProduct inventoryProduct = db.Products.Find(id);
            if (inventoryProduct == null)
            {
                return HttpNotFound();
            }
            ViewBag.SupplierId = new SelectList(db.Suppliers, "supplierid", "supplierName", inventoryProduct.SupplierId);
            return View(inventoryProduct);
        }

        // POST: InventoryProducts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOnOrder([Bind(Include = "productId,brandId,brandName,productName,productDescription,Image,categoryId,SupplierId,quantityOnHand,minimumStock,unitPrice,DiscountPrice,totalPrice,prevUnitPrice,quantityToOrder,onPromotion,status,isOrdered,onOrderednum,numRatings")] InventoryProduct inventoryProduct, HttpPostedFileBase upload, string returnUrl)
        {
            /*this helps getting a value from the database before it is edited
             * 
             * var newP = db.Products.Where(x => x.productId == inventoryProduct.productId).Select(x => x.unitPrice).Single();
            decimal p = Convert.ToDecimal(newP);
            decimal totP = p + Convert.ToDecimal(inventoryProduct.unitPrice);
            inventoryProduct.unitPrice = totP;*/

            //inventoryProduct.status = "Processing order";
            if (ModelState.IsValid)
            {

                if (upload != null && upload.ContentLength > 0)
                {
                    inventoryProduct.Image = uploader.getFileByte(upload);
                }

                db.Entry(inventoryProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("placeOrder","InventoryProducts", new { inventoryProduct.SupplierId });
            }
            ViewBag.SupplierId = new SelectList(db.Suppliers, "supplierid", "supplierName", inventoryProduct.SupplierId);
            return View(inventoryProduct);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
