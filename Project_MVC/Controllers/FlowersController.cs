﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using Project_MVC.Models;
using Project_MVC.Services;
using Project_MVC.Utils;

namespace Project_MVC.Controllers
{
    public class FlowersController : Controller
    {
        private ICRUDService<Flower> mySQLFlowerService;
        private ICRUDService<Category> mySQLCategoryService;
        //private ICRUDService<OwnerOfCourse> mySQLOwnerOfCourseService;
        //private IOrderService mySQLOrderService;
        //private ICRUDService<UserProduct> mySQLUserProductService;
        private IUserService userService;
        private IImageService mySQLImageService;
        private IValidateService mySQLValidateService;

        public FlowersController()
        {
            mySQLFlowerService = new MySQLFlowerService();
            mySQLCategoryService = new MySQLCategoryService();
            mySQLImageService = new MySQLImageService();
            //mySQLOwnerOfCourseService = new MySQLOwnerOfCourseService();
            //mySQLOrderService = new MySQLOrderService();
            //mySQLUserProductService = new MySQLUserProductsService();
            userService = new UserService();
            mySQLValidateService = new MySQLValidateService();
        }
        //[Authorize(Roles = Constant.Admin + "," + Constant.Employee)]
        // Cái này là dùng cho AutoComplete
        public ActionResult GetListProductCategories()
        {
            //Console.WriteLine("123");
            //var list = db.ProductCategories.Where(s => s.Status != ProductCategoryStatus.Deleted).ToList();
            var list = mySQLCategoryService.GetList().Where(s => !string.IsNullOrEmpty(s.ParentCode));
            var newlist = list.Select(dep => new
            {
                dep.Code,
                dep.Name
            });
            return new JsonResult()
            {
                Data = newlist,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        private string GetLevelOneCategoryCode(IEnumerable<Flower> flowers)
        {
            var categoryCodes = flowers.GroupBy(x => x.CategoryCode).Select(g => g.First()).Select(s => s.CategoryCode);
            var lstCategories = mySQLCategoryService.GetList().Where(s => categoryCodes.ToList().Contains(s.Code));
            return lstCategories.FirstOrDefault().ParentCode;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RatingFlower(string rating, string currentFlowerCode)
        {
            var rate = Convert.ToDecimal(rating);
            //ModelStateDictionary state = ModelState;
            if (mySQLImageService.Rating(rate, currentFlowerCode))
            {
                return RedirectToAction("DetailCustomers", new { id = currentFlowerCode });
            }

            return RedirectToAction("DetailCustomers", new { id = currentFlowerCode });
        }


        public ActionResult IndexCustomer(string amount, string sortFlower, string levelOneCategoryCode, string categoryCode,
            string searchString, string currentFilter, int? page, string searchFlowerGlobal, string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                var filterThisPage = JsonConvert.DeserializeObject<ThisPage>(filter);
                if (string.IsNullOrEmpty(searchString))
                {
                    currentFilter = filterThisPage.SearchString;
                }
                if (string.IsNullOrEmpty(amount))
                {
                    amount = filterThisPage.Amount;
                }
                if (string.IsNullOrEmpty(sortFlower))
                {
                    sortFlower = filterThisPage.SortFlower;
                }
                if (string.IsNullOrEmpty(searchFlowerGlobal))
                {
                    searchFlowerGlobal = filterThisPage.SearchFlowerGlobal;
                }
            }

            if (string.IsNullOrEmpty(levelOneCategoryCode) && string.IsNullOrEmpty(categoryCode) && string.IsNullOrEmpty(searchFlowerGlobal))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CurrentUserName = userService.GetCurrentUserName();
            
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var flowers = mySQLFlowerService.GetList();

            if (!String.IsNullOrEmpty(levelOneCategoryCode))
            {
                var categories = mySQLCategoryService.GetList().Where(s => s.ParentCode == levelOneCategoryCode);
                flowers = categories.SelectMany(s => s.Flowers).Where(s => s.Status == Flower.FlowerStatus.NotDeleted).ToList();
                ViewBag.Categories = categories;
                //categoryCode = categories.FirstOrDefault().Code;
            }

            if (!String.IsNullOrEmpty(categoryCode))
            {
                flowers = flowers.Where(s => s.CategoryCode == categoryCode);
                var category = mySQLCategoryService.Detail(categoryCode);
                ViewBag.Categories = mySQLCategoryService.GetList().Where(s => s.ParentCode == category.ParentCode);
                //var productCategory = mySQLCategoryService.Detail(categoryCode);
                //var list = productCategory.OwnerOfCourses.ToList();
                //ViewBag.Teachers = list;
            }

            if (!String.IsNullOrEmpty(searchFlowerGlobal))
            {
                flowers = flowers.Where(s => s.Name.ToUpper().Contains(searchFlowerGlobal.ToUpper()) || s.Code.ToUpper().Contains(searchFlowerGlobal.ToUpper()));
            }

            ViewBag.CurrentFilterFlowerGlobal = searchFlowerGlobal;

            if (!String.IsNullOrEmpty(searchString))
            {
                flowers = flowers.Where(s => s.Name.ToUpper().Contains(searchString.ToUpper()) || s.Code.ToUpper().Contains(searchString.ToUpper()));
            }

            if (!String.IsNullOrEmpty(sortFlower))
            {
                switch (sortFlower)
                {
                    case "1":
                        flowers = flowers.OrderBy(s => s.Name);
                        break;
                    case "2":
                        flowers = flowers.OrderBy(s => Utility.NewPrice(s.Price, s.Discount));
                        break;
                    default:
                        break;
                }

                if (string.IsNullOrEmpty(searchFlowerGlobal))
                {
                    levelOneCategoryCode = GetLevelOneCategoryCode(flowers);
                    var lstCategories = mySQLCategoryService.GetList().Where(s => s.ParentCode == levelOneCategoryCode);
                    ViewBag.Categories = lstCategories;
                }
            }

            if (!String.IsNullOrEmpty(amount))
            {
                //var culture = System.Globalization.CultureInfo.GetCultureInfo("vn");
                var nbspace = "\u00A0";
                var text = HttpUtility.HtmlDecode(HttpUtility.HtmlDecode(nbspace));
                var newStrAmount = amount.Replace(",", string.Empty).Replace(" ", string.Empty).Replace(text, string.Empty);
                var arrCharDong = newStrAmount.Where(s => !char.IsDigit(s)).ToArray();
                var amounts = newStrAmount.Replace(arrCharDong.FirstOrDefault(), ',').Split(',');
                amounts = amounts.Skip(1).ToArray();
                var min = Double.Parse(amounts.First());
                var max = Double.Parse(amounts.Last());
                flowers = flowers.Where(s => Utility.NewPrice(s.Price, s.Discount) <= max && Utility.NewPrice(s.Price, s.Discount) >= min);

                if (string.IsNullOrEmpty(searchFlowerGlobal))
                {
                    levelOneCategoryCode = GetLevelOneCategoryCode(flowers);
                    var lstCategories = mySQLCategoryService.GetList().Where(s => s.ParentCode == levelOneCategoryCode);
                    ViewBag.Categories = lstCategories;
                }
            }

            int pageSize = Constant.PageSizeFlowersOnCustomerPage;
            int pageNumber = (page ?? 1);
            ThisPage thisPage = new ThisPage()
            {
                CurrentPage = pageNumber,
                TotalPage = Math.Ceiling((double)flowers.Count() / pageSize),
                ProductCategoryCode = categoryCode,
                LevelOneCategoryCode = levelOneCategoryCode,
                CurrentType = Constant.Customer,
                SearchString = searchString,
                Amount = amount,
                SortFlower = sortFlower,
                SearchFlowerGlobal = searchFlowerGlobal
            };
            ViewBag.Page = thisPage;

            //var flowerCategories = new List<Category>();
            //foreach(var item in flowers)
            //{
            //    var category = mySQLCategoryService.Detail(item.CategoryCode);
            //    flowerCategories.Add(category);
            //}

            // nếu page == null thì lấy giá trị là 1, nếu không thì giá trị là page
            //return View(students.ToList().ToPagedList(pageNumber, pageSize));

            var list = flowers.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList();
            return View(flowers.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList());
        }

        [Authorize(Roles = Constant.Admin + "," + Constant.Employee)]
        // GET: Products
        public ActionResult Index(string productCategoryCode, string sortOrder, string searchString, string currentFilter, int? page, string type, string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                var filterThisPage = JsonConvert.DeserializeObject<ThisPage>(filter);
                currentFilter = filterThisPage.SearchString;
            }

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            // lúc đầu vừa vào thì sortOrder là null, cho nên gán NameSortParm = name_desc
            // Ấn vào link Full name thì lúc đó NameSortParm có giá trị là name_desc, sortOrder trên view được gán = NameSortParm cho nên sortOrder != null
            // và NameSortParm = ""
            // Ấn tiếp vào link Full Name thì sortOrder = "" cho nên NameSortParm = name_desc
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var flowers = mySQLFlowerService.GetList();

            if (!String.IsNullOrEmpty(productCategoryCode))
            {
                flowers = flowers.Where(s => s.CategoryCode == productCategoryCode);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                flowers = flowers.Where(s => s.Name.Contains(searchString) || s.Code.Contains(searchString) 
                || s.Category.Name.Contains(searchString) || s.Category.Code.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    flowers = flowers.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    flowers = flowers.OrderBy(s => s.UpdatedAt);
                    break;
                case "date_desc":
                    flowers = flowers.OrderByDescending(s => s.UpdatedAt);
                    break;
                default:
                    flowers = flowers.OrderBy(s => s.Name);
                    break;
            }

            int pageSize = Constant.PageSize;
            int pageNumber = (page ?? 1);
            ThisPage thisPage = new ThisPage()
            {
                CurrentPage = pageNumber,
                TotalPage = Math.Ceiling((double)flowers.Count() / pageSize),
                FunctionType = type,
                SearchString = searchString
            };
            ViewBag.Page = thisPage;

            // nếu page == null thì lấy giá trị là 1, nếu không thì giá trị là page
            //return View(students.ToList().ToPagedList(pageNumber, pageSize));
            return View(flowers.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList());
        }

        [Authorize(Roles = Constant.Admin + "," + Constant.Employee)]
        // GET: Flowers/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flower flower = mySQLFlowerService.Detail(id);
            if (flower == null || flower.IsDeleted())
            {
                return HttpNotFound();
            }
            return View(flower);
        }

        public ActionResult DetailCustomers(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flower flower = mySQLFlowerService.Detail(id);
            if (flower == null || flower.IsDeleted())
            {
                return Redirect(Request.UrlReferrer.PathAndQuery);
            }
            ViewBag.Flowers = mySQLFlowerService.GetList();
            ViewBag.CurrentUserName = userService.GetCurrentUserName();
            ViewBag.CurrentRating = flower.Rating;
            return View(flower);
        }

        [Authorize(Roles = Constant.Admin + "," + Constant.Employee)]
        // GET: Flowers/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: Flowers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [Authorize(Roles = Constant.Admin + "," + Constant.Employee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Price,Description,CategoryCode,Discount")] Flower flower, string strImageUrl)
        {
            if (mySQLFlowerService.CreateWithImage(flower, ModelState, strImageUrl, null))
            {
                return RedirectToAction("Index");
            }

            return View(flower);
        }

        // GET: Flowers/Edit/5
        [Authorize(Roles = Constant.Admin + "," + Constant.Employee)]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flower flower = mySQLFlowerService.Detail(id);
            if (flower.Category == null)
            {
                flower.CategoryNameAndCode = "";
            }
            else
            {
                flower.CategoryNameAndCode = flower.Category.Code + " - " + flower.Category.Name;
            }
            if (flower == null || flower.IsDeleted())
            {
                return HttpNotFound();
            }
            //ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "Id", "Name", product.ProductCategoryId);
            return View(flower);
        }

        // POST: Flowers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = Constant.Admin + "," + Constant.Employee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Code,Name,Price,Description,CategoryCode,Discount")] Flower flower, string strImageUrl, string editor)
        {
            //ModelStateDictionary state = ModelState;
            if (flower == null || flower.Code == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var existFlower = mySQLFlowerService.Detail(flower.Code);
            if (existFlower == null || existFlower.IsDeleted())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            if (mySQLFlowerService.UpdateWithImage(existFlower, flower, ModelState, strImageUrl))
            {
                return RedirectToAction("Index");
            }

            return View(flower);
        }

        // GET: Flowers/Delete/5
        //public ActionResult Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Flower flower = db.Flowers.Find(id);
        //    if (flower == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(flower);
        //}

        // POST: Flowers/Delete/5
        [Authorize(Roles = Constant.Admin + "," + Constant.Employee)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var existFlower = mySQLFlowerService.Detail(id);
            if (existFlower == null || existFlower.IsDeleted())
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
            if (mySQLFlowerService.Delete(existFlower, ModelState))
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mySQLFlowerService.DisposeDb();
            }
            base.Dispose(disposing);
        }
    }
}
