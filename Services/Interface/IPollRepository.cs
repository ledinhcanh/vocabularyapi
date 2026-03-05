using API.Models.AppConfig;
using API.Models.Request.Poll;

namespace API.Services.Interface
{
    public interface IPollRepository
    {
        Task<AppResponse<object?>> GetDefaultPoll();
        Task<AppResponse<object?>> PostPollVote(PollVoteRequest request);
    }
}
