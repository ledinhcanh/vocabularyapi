using Azure.Core;
using API.Models.AppConfig;
using API.Models.AppConfig.ModelSetting;
using API.Models.Database.Identities;
using API.Models.Request.Topic;

namespace API.Services.Interface
{
    public interface ITopicRepository
    {
        public Task<AppResponse<object>> GetTopics(string? keyword = null);
        public Task<AppResponse<object>> GetTopicById(int id);
        public Task<AppResponse<object>> CreateTopic(CreateTopicRequest request);
        public Task<AppResponse<object>> UpdateTopic(UpdateTopicRequest request);
        public Task<AppResponse<object>> DeleteTopic(int id);
    }
}
