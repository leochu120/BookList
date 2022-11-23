using BookList.Data;
using BookList.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Runtime.Remoting;
using System.Drawing.Printing;

namespace BookList.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _db;

        public string[] list = new string[10];

        public BookController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Book> objBookList = _db.Books;
            return View(objBookList);
        }

        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Search(SearchCriteria obj)
        {
            string baseURL = "";

            if (obj.Title == null && obj.Author == null)
            {
                return View();
            }
            else if (obj.Title == null)
            {
                baseURL = $"https://www.googleapis.com/books/v1/volumes?q=inauthor:{obj.Author}/";
            }
            else if (obj.Author == null)
            {
                baseURL = $"https://www.googleapis.com/books/v1/volumes?q=intitle:{obj.Title}/";
            }
            else baseURL = $"https://www.googleapis.com/books/v1/volumes?q=intitle:{obj.Title}+inauthor:{obj.Author}/";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(baseURL))
                    {
                        using (HttpContent content = res.Content)
                        {
                            string[] print = new string[10];
                            string data = await content.ReadAsStringAsync();
                            if (data != null)
                            {
                                var dataObj = JObject.Parse(data);
                                for (int i = 0; i < 10; i++)
                                {
                                    if (dataObj["items"][i]["volumeInfo"]["title"] != null)
                                    {
                                        print[i] = dataObj["items"][i]["volumeInfo"]["title"].ToString();
                                    }
                                }
                                //var print = dataObj["items"][0]["volumeInfo"]["title"];
                                list = print;
                                Console.WriteLine(print);
                            }
                            else
                            {
                                //If data is null log it into console.
                                Console.WriteLine("Data is null!");
                            }
                        }
                    }
                }
                //Catch any exceptions and log it into the console.
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            return View("~/Views/Book/Result.cshtml", list);

        }

        public IActionResult Create()
        {
            string title = (string) ControllerContext.RouteData.Values["id"];
            Book book = new Book();
            book.Title = title;
            _db.Books.Add(book);
            _db.SaveChanges();
            TempData["success"] = "Book added successfully";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var bookFromDb = _db.Books.Find(id);

            if (bookFromDb == null)
            {
                return NotFound();
            }
            return View(bookFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Book obj)
        {
            if (ModelState.IsValid)
            {
                _db.Books.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Book updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var bookFromDb = _db.Books.Find(id);

            if (bookFromDb == null)
            {
                return NotFound();
            }
            return View(bookFromDb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.Books.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            _db.Books.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Book deleted successfully";
            return RedirectToAction("Index");
        }
    }
}