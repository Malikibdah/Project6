using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using E_Commerce.Models;

namespace E_Commerce.Controllers
{
    public class UsersController : Controller
    {
        private ecommerceEntities db = new ecommerceEntities();

        // GET: Users
        public ActionResult Index()
        {
            var categories = db.Categories.ToList();
            return View(categories);
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection form)
        {
            var email = form["email"];
            var password = form["password"];

            var user = db.Users.FirstOrDefault(u => u.email == email && u.password == password);
            if (user != null)
            {
                Session["user_id"] = user.id;
                return RedirectToAction("Index", "Users");
            }
            return View();
        }
        public ActionResult Logout()
        {
            Session["user_id"] = null;
            return RedirectToAction("Index", "Users");
        }
        public ActionResult Products(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var products = db.Products.Where(p => p.category_id == id).ToList();

            return View(products);
        }
        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,name,email,password,confpassword")] User user)
        {

            if (ModelState.IsValid && user.password == user.confpassword)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.error = "Passwords do not match!";
                return View(user);
            }


        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,name,email,password")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        public ActionResult Checkout()
        {

            return View();
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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
        public ActionResult ProductDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var productdetailes = db.Products.Find(id);
            return View(productdetailes);
        }
        
        public ActionResult Cart(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user_id = Session["user_id"];
            if (user_id == null)
            {

                return RedirectToAction("Login");
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            var existingCartItem = db.Carts.FirstOrDefault(c => c.product_id == product.id && c.Users_id == (int)user_id);
            if (existingCartItem != null)
            {
                existingCartItem.quantity += 1;
            }
            else
            {

                Cart cart = new Cart()
                {
                    product_id = product.id,
                    Users_id = (int)user_id,
                    quantity = 1
                };
                db.Carts.Add(cart);
            }

            db.SaveChanges();

            return RedirectToAction("Showcart", "Users");
        }
        public ActionResult Showcart()
        {
            var user = Session["user_id"];
            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }
            var cart = db.Carts.Include(c => c.Product).Where(c => c.Users_id == (int)user).ToList();
            return View(cart);
        }

        public ActionResult Addquantity(int? id)
        {
            var user = (int)Session["user_id"];
            var cartItem = db.Carts.FirstOrDefault(m => m.product_id == id);
            if (cartItem == null)
            {
                cartItem = new Cart();
                cartItem.quantity = 1;
                cartItem.Users_id = (int)user;
                cartItem.product_id = id;
                db.Carts.Add(cartItem);
                db.SaveChanges();
                return RedirectToAction("Showcart", "Users");
            }
            cartItem.quantity += 1;
            db.Entry(cartItem).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Showcart", "Users");
        }
        public ActionResult Subquantity(int? id)
        {
            var user = (int)Session["user_id"];
            var cartItem = db.Carts.FirstOrDefault(m => m.product_id == id);

            if (cartItem.quantity > 1)
            {
                cartItem.quantity -= 1;
                db.Entry(cartItem).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Showcart", "Users");
        }
        public ActionResult DeletItem(int? id)
        {
            var user = (int)Session["user_id"];
            var cartItem = db.Carts.FirstOrDefault(m => m.product_id == id);
            db.Carts.Remove(cartItem);
            db.SaveChanges();
            return RedirectToAction("Showcart", "Users");
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Shop()
        {

            return View(db.Products.ToList());
        }
        public ActionResult About()
        {
            return View();
        }


    }
}
