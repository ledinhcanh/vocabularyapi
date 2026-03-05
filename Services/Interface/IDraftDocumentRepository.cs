using API.Models.AppConfig; 
using API.Models.Request.DraftDocument;

namespace API.Services.Interface
{
    public interface IDraftDocumentRepository
    {
        Task<AppPagingResponse<object>> GetListDraftDocument(GetDraftDocumentRequest request);
        Task<AppResponse<object>> GetDraftDocumentDetail(int DraftDocumentId);
        Task<AppResponse<object>> CommentDraftDocument(DraftDocumentCommentRequest request);
    }
}
