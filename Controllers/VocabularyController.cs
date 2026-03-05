using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Services.Interface;
using API.Models.Request.Vocabulary;
using API.Models.AppConfig;

namespace API.Controllers
{
    [Route("api/vocabularies")]
    [ApiController]
    [Authorize]
    public class VocabularyController : AppControllerBase
    {
        private readonly IVocabularyRepository _vocabRepo;

        public VocabularyController(IVocabularyRepository vocabRepo)
        {
            _vocabRepo = vocabRepo;
        }

        // API cho Admin: Tạo từ mới
        [HttpPost("create")]
        // [Authorize(Roles = "Admin")] // Bỏ comment nếu muốn chặn User thường
        public async Task<IActionResult> Create([FromBody] CreateVocabularyRequest request)
        {
            var res = await _vocabRepo.CreateVocabulary(request);
            return Ok(res);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateVocabularyRequest request)
        {
            var res = await _vocabRepo.UpdateVocabulary(request);
            return Ok(res);
        }

        // API lấy danh sách từ theo Topic (để xem list)
        [HttpGet("topic/{topicId}")]
        public async Task<IActionResult> GetByTopic(int topicId)
        {
            var res = await _vocabRepo.GetVocabulariesByTopic(topicId);
            return Ok(res);
        }

        // API xóa từ
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _vocabRepo.DeleteVocabulary(id);
            return Ok(res);
        }

        // --- API HỌC TẬP (QUAN TRỌNG) ---

        // 1. Vào màn hình học -> Gọi cái này lấy list thẻ bài
        [HttpGet("study")]
        public async Task<IActionResult> Study()
        {
            var res = await _vocabRepo.GetWordsToLearn();
            return Ok(res);
        }

        [HttpGet("study/topic/{topicId}")]
        public async Task<IActionResult> StudyByTopic(int topicId)
        {
            var res = await _vocabRepo.GetWordsToLearn(topicId);
            return Ok(res);
        }

        // 2. Học xong 1 thẻ -> Gọi cái này để lưu kết quả
        [HttpPost("study/submit-review")]
        public async Task<IActionResult> SubmitReview(SubmitReviewRequest request)
        {
            var res = await _vocabRepo.SubmitReview(request);
            if (!res.IsSuccess) return BadRequest(res);
            return Ok(res);
        }

        // --- PROGRESS MANAGEMENT & TESTING ---
        [HttpGet("progress")]
        public async Task<IActionResult> GetAllVocabularyProgress()
        {
            var res = await _vocabRepo.GetVocabularyProgress(null);
            return Ok(res);
        }

        [HttpGet("progress/topic/{topicId}")]
        public async Task<IActionResult> GetVocabularyProgress(int topicId)
        {
            var res = await _vocabRepo.GetVocabularyProgress(topicId);
            return Ok(res);
        }

        [HttpDelete("progress/{vocabId}")]
        public async Task<IActionResult> ResetProgress(int vocabId)
        {
            var res = await _vocabRepo.ResetProgress(vocabId);
            return res.IsSuccess ? Ok(res) : BadRequest(res);
        }

        [HttpDelete("progress/all")]
        public async Task<IActionResult> ResetAllProgress()
        {
            var res = await _vocabRepo.ResetAllProgress();
            return res.IsSuccess ? Ok(res) : BadRequest(res);
        }

        [HttpDelete("progress/topic/{topicId}")]
        public async Task<IActionResult> ResetTopicProgress(int topicId)
        {
            var res = await _vocabRepo.ResetTopicProgress(topicId);
            return res.IsSuccess ? Ok(res) : BadRequest(res);
        }

        [HttpGet("study/learned-test")]
        public async Task<IActionResult> GetAllLearnedWordsForTest()
        {
            var res = await _vocabRepo.GetLearnedWordsForTest(null);
            return Ok(res);
        }

        [HttpGet("study/learned-test/topic/{topicId}")]
        public async Task<IActionResult> GetLearnedWordsForTest(int topicId)
        {
            var res = await _vocabRepo.GetLearnedWordsForTest(topicId);
            return Ok(res);
        }

        // --- GAMIFICATION ---
        [HttpPost("gamification/submit-xp/{xpAmount}")]
        public async Task<IActionResult> SubmitXP(int xpAmount)
        {
            var res = await _vocabRepo.SubmitXP(xpAmount);
            if (!res.IsSuccess) return BadRequest(res);
            return Ok(res);
        }

        [HttpGet("gamification/profile")]
        public async Task<IActionResult> GetGamificationProfile()
        {
            var res = await _vocabRepo.GetUserGamificationProfile();
            if (!res.IsSuccess) return BadRequest(res);
            return Ok(res);
        }
    }
}