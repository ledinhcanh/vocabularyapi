namespace API.Models.Request.Category
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
    }

    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        public int CategoryId { get; set; }
    }
}