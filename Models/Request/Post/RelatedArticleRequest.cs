using System.ComponentModel.DataAnnotations;

namespace API.Models.Request.Post
{
    public class RelatedArticleRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "ArticleID must be greater than zero")]
        public int ArticleID { set; get; }      
        [Range(1, 50, ErrorMessage = "Count must be between 1 and 50")]        
        public int Count { set; get; }
    }
}
