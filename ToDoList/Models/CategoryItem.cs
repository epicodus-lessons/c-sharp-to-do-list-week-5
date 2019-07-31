namespace ToDoList.Models
{
  public class CategoryItem
    {       
        public int CategoryItemId { get; set; }
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public Item Item { get; set; }
        public Category Category { get; set; }
    }
}