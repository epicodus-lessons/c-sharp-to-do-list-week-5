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

//////////////////////////////////////////////////////
//////// Authorizing Create, Update and Delete of ItemsController.cs 
//////////////////////////////////////////////////////
//////// 1. Item.cs needs ApplicationUser property
//////// 2. ItemsController.cs has various updates
//////// 3. Views/Items/Details.cshtml has updates
//////// 4. Views/Items/Index.cshtml has updates
//////////////////////////////////////////////////////

namespace ToDoList.Controllers
{
  public class ItemsController : Controller
  {
    private readonly ToDoListContext _db;
    private readonly UserManager<ApplicationUser> _userManager; 

    public ItemsController(UserManager<ApplicationUser> userManager, ToDoListContext db)
    {
      _userManager = userManager;
      _db = db;
    }

    //Index Route updated to find all DB items
    public ActionResult Index()
    {
      List<Item> userItems = _db.Items.ToList();
      return View(userItems);
    }

    //Create Route updated to Add Authorization
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

    // In the Details route we need to find the user associated with the item so that in the view, we can show the edit, delete or add category links if the item "belongs" to that user. Line 75 involves checking if the userId has returned as null, and if it has then IsCurrentUser is set to false, if it has not, then the program evaluates whether userId is equal to thisItem.User.Id.
    public ActionResult Details(int id)
    {
      var thisItem = _db.Items
          .Include(item => item.Categories)
          .ThenInclude(join => join.Category)
          .FirstOrDefault(item => item.ItemId == id);
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      ViewBag.IsCurrentUser = userId != null ? userId == thisItem.User.Id : false;
      return View(thisItem);
    }

    // Edit Route is updated to find the user and the item that matches the user id, then is routed based on that result. 
    [Authorize]
    public async Task<ActionResult> Edit(int id)
    {
       var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      var thisItem = _db.Items.Where(entry => entry.User.Id == currentUser.Id).FirstOrDefault(items => items.ItemId == id);
      if (thisItem == null)
      {
        return RedirectToAction("Details", new {id = id});
      }
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name"); 
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

    // AddCategory is updated to find the user and the item that matches the user id, then is routed based on that result. 
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

    // Delete route is updated to find the user and the item that matches the user id, then is routed based on that result. 
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