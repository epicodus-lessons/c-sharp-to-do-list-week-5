using System.Collections.Generic;

namespace ToDoList.Models
{
//////////////////////////////////////////////////////
//////// Authorizing Create, Update and Delete Routes of ItemsController.cs
//////////////////////////////////////////////////////
//////// 1. Item.cs needs ApplicationUser property
//////// 2. ItemsController.cs has various updates
//////// 3. Views/Items/Details.cshtml has updates
//////// 4. Views/Items/Index.cshtml has updates
//////////////////////////////////////////////////////

    public class Item
    {
        public Item()
        {
            this.Categories = new HashSet<CategoryItem>();
        }

        public int ItemId { get; set; }
        public string Description { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<CategoryItem> Categories { get;}
    }
}