using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Arctic_Knowledge_Book_Store.Models;

namespace Arctic_Knowledge_Book_Store.Controllers
{
    public class HomeController : Controller
    {
        BookStoreDBEntities entities = new BookStoreDBEntities();
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Admin_Reg(Admin admin, string confirmPass)
        {
            bool isValidPassword = false;
            if (admin.Password.Equals(confirmPass)) { isValidPassword = true; }
            else { ViewBag.pass = "Please re-enter the same password!"; }
            if (ModelState.IsValid && isValidPassword)
            {
                entities.Admins.Add(admin);
                entities.SaveChanges();
            }
            return View("Admin_Login");
        }

        [HttpPost]
        public ActionResult Customer_Reg(Customer customer, string confirmPass, string username)
        {
            bool isValidPassword = false, isValidUsername = true;
            var checkUsername = entities.Customers.FirstOrDefault(c => c.Username.Equals(username));
            if (checkUsername != null) { ViewBag.user = "Username is not available!"; isValidUsername = false; }
            if (customer.Password.Equals(confirmPass)) { isValidPassword = true; }
            else { ViewBag.pass = "Please re-enter the same password!"; }
            if (ModelState.IsValid && isValidPassword && isValidUsername)
            {
                entities.Customers.Add(customer);
                entities.SaveChanges();
                return View("Customer_Login", customer);
            }
            return View("Customer_Login");
        }

        public ActionResult Admin_Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Admin_Login(Admin admin)
        {
            var checkedQuery = entities.Admins.Where(a => a.ID.Equals(admin.ID) && a.Password.Equals(admin.Password)).Select(ad => ad);
            if (checkedQuery.Count() > 0)
            {
                return RedirectToAction("Admin_View");
            }
            ViewBag.msg = "Enter a valid ID and a Password!";
            return View();

        }
        public ActionResult Customer_Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Customer_Login(Customer customer)
        {
            var checkedQuery = entities.Customers.Where(c => c.Username.Equals(customer.Username) && c.Password.Equals(customer.Password)).Select(cs => cs);
            if (checkedQuery.Count() > 0)
            {
                return RedirectToAction("Index");
            }
            ViewBag.msg = "Enter a valid Username and a Password!";
            return View();
        }
        public ActionResult Admin_View()
        {
            return View(entities.Stocks.ToList());
        }

        public ActionResult Admin_Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Admin_Add([Bind(Exclude = "image")]Book book, string quantity)
        {
            HttpPostedFileBase file = Request.Files["image"];
            book.Image = ConvertToBytes(file);
            entities.Books.Add(book);
            entities.SaveChanges();

            var bookID = entities.Books.FirstOrDefault(b => b.ISBN.Equals(book.ISBN));
            Stock stock = new Stock();
            stock.BookID = bookID.ID;
            stock.Quantity = int.Parse(quantity);
            entities.Stocks.Add(stock);
            entities.SaveChanges();
            return View(book);
        }
        public ActionResult Admin_Edit(int id)
        {
            var details = entities.Stocks.FirstOrDefault(s => s.ID == id);
            return View(details);
        }
        [HttpPost]
        public ActionResult Admin_Edit(int id, Stock stock)
        {
            HttpPostedFileBase file = Request.Files["image"];
            var details = entities.Stocks.FirstOrDefault(s => s.ID == id);
            details.Book.Name = stock.Book.Name;
            details.Book.Author = stock.Book.Author;
            details.Book.Publication = stock.Book.Publication;
            details.Book.ISBN = stock.Book.ISBN;
            details.Book.Price = stock.Book.Price;
            if (file.ContentLength > 0) { details.Book.Image = ConvertToBytes(file); }
            entities.SaveChanges();
            details = null;
            return View(details);
        }
        public ActionResult Admin_Delete(int id)
        {
            var details = entities.Stocks.FirstOrDefault(s => s.ID == id);
            return View(details);
        }
        [HttpPost]
        public ActionResult Admin_Delete(int id, Stock stock)
        {
            var details = entities.Stocks.FirstOrDefault(s => s.ID == id);
            entities.Stocks.Remove(details);
            
            return View(details);
        }
        public ActionResult Cart()
        {
            return View();
        }
        public byte[] ConvertToBytes(HttpPostedFileBase image)
        {
           
            byte[] imageBytes = null;
            
            BinaryReader reader = new BinaryReader(image.InputStream);
            imageBytes = reader.ReadBytes((int)image.ContentLength);
            return imageBytes;
        }
        public ActionResult RetrieveImage(int id)
        {
            byte[] cover = GetImageFromDataBase(id);
            if (cover != null)
            {
                return File(cover, "image/jpg");
            }
            else
            {
                return null;
            }
        }
        public byte[] GetImageFromDataBase(int Id)
        {
            var q = from b in entities.Books where b.ID == Id select b.Image;
            byte[] cover = q.First();
            return cover;
        }
        public ActionResult Sales()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Sales(Sale sale)
        {
            sale.DateOfPurchase = DateTime.Now;
            entities.Sales.Add(sale);
            entities.SaveChanges();
            return View();
        }
    }
}