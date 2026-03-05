using API.Models.AppConfig;
using API.Models.Request.Question;

namespace API.Services.Interface
{
    public interface IQuestionContentRepository
    {
        public Task<AppPagingResponse<object>> GetListQuestions(GetQuestionRequest request);
        public Task<AppResponse<object>> GetCategories();
        public Task<AppResponse<object>> GetQuestionDetail(int QuestionId);
        public Task<AppResponse<object>> GetQuestionRelated(int QuestionId);
        public Task<AppResponse<object>> PushQuestionContent(PushQuestionContentRequest request);
        public Task<AppResponse<object>> VoteQuestionContent(VoteQuestionContentRequest request);
    }
}
