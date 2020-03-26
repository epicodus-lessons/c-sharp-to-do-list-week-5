using Microsoft.AspNetCore.Mvc;
using ToDoList.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

//new using directives
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ToDoList.Controllers
{
  // [Authorize] //Remmove this line!
  public class ItemsController : Controller
  {
    private readonly ToDoListContext _db;
    private readonly UserManager<ApplicationUser> _userManager; 

    public ItemsController(UserManager<ApplicationUser> userManager, ToDoListContext db)
    {
      _userManager = userManager;
      _db = db;
    }

    //we do not want to find the user and their items!
    // public async Task<ActionResult> Index()
    public ActionResult Index()
    {
      // var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      // var currentUser = await _userManager.FindByIdAsync(userId);
      // var userItems = _db.Items.Where(entry => entry.User.Id == currentUser.Id);
      List<Item> userItems = _db.Items.ToList();
      return View(userItems);
    }

    // Add Authorize to the Create() Route
    [Authorize] 
    public ActionResult Create()
    {
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View();
    }

    
    [HttpPost]
    public async Task<ActionResult> Create(Item item, int CategoryId)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      item.User = currentUser;
      _db.Items.Add(item);
      if (CategoryId != 0)
      {
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    // In Details() we need to find the user associated with the item so that in the view, we can show the edit, delete or add category links if the item "belongs" to that user.
    public ActionResult Details(int id)
    {
      var thisItem = _db.Items
          .Include(item => item.Categories)
          .ThenInclude(join => join.Category)
          .FirstOrDefault(item => item.ItemId == id);
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      ViewBag.IsCurrentUser = userId == thisItem.User.Id;
      return View(thisItem);
    }

    // Here we're doing a lot: changing Edit() into an asyn action, finding the user and the item that matches that user id, then doing a safeguard check to see if it is null.
    [Authorize]
    // public ActionResult Edit(int id)
    public async Task<ActionResult> Edit(int id)
    {
       var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      // var thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id); // remove this!
      var thisItem = _db.Items.Where(entry => entry.User.Id == currentUser.Id).FirstOrDefault(items => items.ItemId == id);
      if (thisItem == null)
      {
        return RedirectToAction("Details", new {id = id});
      }
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name"); // we'll keep this!
      return View(thisItem);
    }


    [HttpPost]
    public ActionResult Edit(Item item, int CategoryId)
    {
      if (CategoryId != 0)
      {
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.Entry(item).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    // public ActionResult AddCategory(int id)
    // {
    //   var thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id);
    //   ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
    //   return View(thisItem);
    // }

    [Authorize]
    public async Task<ActionResult> AddCategory(int id)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);

      Item thisItem = _db.Items.Where(entry => entry.User.Id == currentUser.Id).FirstOrDefault(items => items.ItemId == id);
      if (thisItem == null)
      {
        return RedirectToAction("Details", new {id = id});
      }
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View(thisItem);
    }

    [HttpPost]
    public ActionResult AddCategory(Item item, int CategoryId)
    {
      if (CategoryId != 0)
      {
        _db.CategoryItem.Add(new CategoryItem() { CategoryId = CategoryId, ItemId = item.ItemId });
      }
      _db.SaveChanges();
      return RedirectToAction("Index");
    }


    // public ActionResult Delete(int id)
    // {
    //   var thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id);
    //   return View(thisItem);
    // }

    [Authorize]
    public async Task<ActionResult> Delete(int id)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);

      Item thisItem = _db.Items.Where(entry => entry.User.Id == currentUser.Id).FirstOrDefault(items => items.ItemId == id);
      if (thisItem == null)
      {
        return RedirectToAction("Details", new {id = id});
      }
      return View(thisItem);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisItem = _db.Items.FirstOrDefault(items => items.ItemId == id);
      _db.Items.Remove(thisItem);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult DeleteCategory(int joinId)
    {
      var joinEntry = _db.CategoryItem.FirstOrDefault(entry => entry.CategoryItemId == joinId);
      _db.CategoryItem.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
  }
}