﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Project_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Project_MVC.Controllers
{
    [Authorize(Roles = Constant.Admin)]
    [CustomAuthorization]
    public class AppRolesController : Controller
    {
        private MyDbContext _db;
        //public MyDbContext DbContext
        //{
        //    get { return _db ?? HttpContext.GetOwinContext().Get<MyDbContext>(); }
        //    set { _db = value; }
        //}
        private RoleManager<AppRole> roleManager;

        public AppRolesController()
        {
            _db = new MyDbContext();
            var roleStore = new RoleStore<AppRole>(_db);
            roleManager = new RoleManager<AppRole>(roleStore);
        }

        //[Authorize(Roles = "Admin")]

        public ActionResult Index()
        {
            return View(_db.IdentityRoles.ToList());
        }

        public JsonResult GetListRoles()
        {
            return Json(_db.IdentityRoles.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        //[Authorize(Roles = "Admin")]
        //[Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Description")] AppRole role)
        {
            role.CreatedAt = DateTime.Now;
            if (!roleManager.RoleExists(role.Name))
            {
                roleManager.Create(role);
            }
            else
            {
                ModelState.AddModelError("Name", "Role already exists");
                return View(role);
            }
            return RedirectToAction("Index");
        }
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppRole appRole = _db.IdentityRoles.Find(id);
            if (appRole == null)
            {
                return HttpNotFound();
            }
            return View(appRole);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var existRole = _db.IdentityRoles.Find(id);
            if (existRole == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            if (ModelState.IsValid)
            {
                roleManager.Delete(existRole);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public string DeleteRoleAngular(string id)
        {
            if (id == null)
            {
                return "Id not found!";
            }
            var existRole = _db.IdentityRoles.Find(id);
            //if (role == null)
            //{
            //    return "Role not found!";
            //}
            //var existRole = _db.IdentityRoles.Find(role.Id);
            if (existRole == null)
            {
                return "Role not found!";
            }
            if (ModelState.IsValid)
            {
                roleManager.Delete(existRole);
            }
            return "Role Successfully Deleted!";
        }
    }
}