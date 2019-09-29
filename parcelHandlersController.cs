using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GateBoys.Helpers;
using GateBoys.Models;
using IdentitySample.Models;

namespace GateBoys.Controllers
{
    public class parcelHandlersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: parcelHandlers
        public ActionResult Index()
        {
            return View(db.parcelHandlers.ToList());
        }

        // GET: parcelHandlers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            parcelHandler parcelHandler = db.parcelHandlers.Find(id);
            if (parcelHandler == null)
            {
                return HttpNotFound();
            }
            return View(parcelHandler);
        }

        // GET: parcelHandlers/Create
        public ActionResult Create(string email)
        {
            if (email == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.employeeEmail = email;
            return View();
        }

        // POST: parcelHandlers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,employeeName,employeeMidName,employeeSurname,employeeIdNumber,employeeEmail,employeePhonenumber" +
            ",employeeProfilePic,employeeCv,employeeIdDocument,employeeQualification,country,street_number,route,administrative_area_level_1" +
            ",locality,postal_code,fullAdress,isStillEmployeed,reasonLeft,addedByEmail,dateRegistered")] parcelHandler parcelHandler, HttpPostedFileBase profPic, HttpPostedFileBase qualification, HttpPostedFileBase idDoc, HttpPostedFileBase cvDoc)
        {
            parcelHandler.employeeCv = uploader.getFileByte(cvDoc);
            parcelHandler.employeeIdDocument = uploader.getFileByte(idDoc);
            parcelHandler.employeeProfilePic = uploader.getFileByte(profPic);
            parcelHandler.dateRegistered = DateTime.Now.ToString();
            parcelHandler.isStillEmployeed = true;
            if (qualification != null)
            {
                parcelHandler.employeeQualification = uploader.getFileByte(qualification);
            }
            if (ModelState.IsValid)
            {
                db.parcelHandlers.Add(parcelHandler);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(parcelHandler);
        }

        // GET: parcelHandlers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            parcelHandler parcelHandler = db.parcelHandlers.Find(id);
            if (parcelHandler == null)
            {
                return HttpNotFound();
            }
            return View(parcelHandler);
        }

        // POST: parcelHandlers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,employeeName,employeeMidName,employeeSurname,employeeIdNumber,employeeEmail,employeePhonenumber,employeeProfilePic,employeeCv,employeeIdDocument,employeeQualification,country,street_number,route,administrative_area_level_1,locality,postal_code,fullAdress,isStillEmployeed,reasonLeft,addedByEmail,dateRegistered")] parcelHandler parcelHandler)
        {
            if (ModelState.IsValid)
            {
                db.Entry(parcelHandler).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(parcelHandler);
        }

        // GET: parcelHandlers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            parcelHandler parcelHandler = db.parcelHandlers.Find(id);
            if (parcelHandler == null)
            {
                return HttpNotFound();
            }
            return View(parcelHandler);
        }

        // POST: parcelHandlers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            parcelHandler parcelHandler = db.parcelHandlers.Find(id);
            db.parcelHandlers.Remove(parcelHandler);
            db.SaveChanges();
            return RedirectToAction("Index");
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
