using API.Models.AppConfig;
using API.Models.Request.Document;

namespace API.Services.Interface
{
    public interface IDocumentRepository
    {
        Task<AppPagingResponse<object>> GetListDocument(GetDocumentsRequest request);
        Task<AppResponse<object>> GetDocumentDetail(int DocumentId);
        Task<AppResponse<object>> GetDocumentRelated(int DocumentId);
        Task<AppResponse<object>> GetCommonCatalog();
    }
}
