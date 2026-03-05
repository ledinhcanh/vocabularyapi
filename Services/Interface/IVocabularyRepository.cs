using API.Models.AppConfig;
using API.Models.Request.Vocabulary;

namespace API.Services.Interface
{
    public interface IVocabularyRepository
    {
        Task<AppResponse<object>> GetVocabulariesByTopic(int topicId);
        Task<AppResponse<object>> CreateVocabulary(CreateVocabularyRequest request);
        Task<AppResponse<object>> UpdateVocabulary(UpdateVocabularyRequest request);
        Task<AppResponse<object>> DeleteVocabulary(int id);
        Task<AppResponse<object>> GetWordsToLearn(int? topicId = null);

        Task<AppResponse<object>> SubmitReview(SubmitReviewRequest request);

        // --- Progress Management ---
        Task<AppResponse<object>> GetVocabularyProgress(int? topicId = null);
        Task<AppResponse<object>> ResetProgress(int vocabId);
        Task<AppResponse<object>> ResetTopicProgress(int topicId);
        Task<AppResponse<object>> ResetAllProgress();
        Task<AppResponse<object>> GetLearnedWordsForTest(int? topicId = null);

        // --- Gamification ---
        Task<AppResponse<object>> SubmitXP(int xpAmount);
        Task<AppResponse<object>> GetUserGamificationProfile();
    }
}