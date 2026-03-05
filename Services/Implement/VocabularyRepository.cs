using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Request.Vocabulary;
using API.Services.Interface;
using API.Models.Database.Identities;

namespace API.Services.Implement
{
    public class VocabularyRepository : IVocabularyRepository
    {
        private readonly ApiDBContext _context;
        private readonly int _userId;
        private readonly VocabContentService _vocabContentService;
        public VocabularyRepository(ApiDBContext context, IHttpContextAccessor httpContextAccessor, VocabContentService vocabContentService)
        {
            _context = context;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out _userId);
            _vocabContentService = vocabContentService;
        }

        #region CRUD Basic
        public async Task<AppResponse<object>> GetVocabulariesByTopic(int topicId)
        {
            var list = await _context.Vocabularies
                .Where(x => x.TopicId == topicId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return new AppResponse<object> { IsSuccess = true, Data = list };
        }

        public async Task<AppResponse<object>> CreateVocabulary(CreateVocabularyRequest request)
        {
            try
            {
                // Kiểm tra xem từ đã tồn tại trong Topic chưa để tránh trùng lặp
                var existingWord = await _context.Vocabularies
                    .AnyAsync(x => x.TopicId == request.TopicId && x.Word == request.Word);
                if (existingWord)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Từ này đã có trong chủ đề rồi!" };
                }

                // --- BẮT ĐẦU ĐOẠN TỰ ĐỘNG HÓA (AUTO-FILL) ---
                string finalPhonetic = request.Phonetic;
                string finalAudio = request.AudioUrl;
                string finalExample = request.ExampleSentence;

                // Nếu thiếu thông tin, gọi "AI Service" để lấy
                if (string.IsNullOrEmpty(finalPhonetic) || string.IsNullOrEmpty(finalAudio) || string.IsNullOrEmpty(finalExample) || string.IsNullOrEmpty(request.ImageUrl))
                {
                    // Gọi service lấy data tự động
                    var autoData = await _vocabContentService.GetWordInfoAsync(request.Word);

                    // Chỉ điền vào nếu user để trống
                    if (string.IsNullOrEmpty(finalPhonetic)) finalPhonetic = autoData.phonetic;
                    if (string.IsNullOrEmpty(finalAudio)) finalAudio = autoData.audioUrl;
                    if (string.IsNullOrEmpty(finalExample)) finalExample = autoData.example;
                    if (string.IsNullOrEmpty(request.ImageUrl)) request.ImageUrl = autoData.imageUrl;
                }
                // ---------------------------------------------

                var vocab = new Vocabulary
                {
                    TopicId = request.TopicId,
                    Word = request.Word,
                    Meaning = request.Meaning, // Nghĩa tiếng Việt thì User nên tự nhập cho chuẩn ngữ cảnh

                    Phonetic = finalPhonetic,       // Data tự động
                    AudioUrl = finalAudio,          // Data tự động
                    ExampleSentence = finalExample, // Data tự động

                    ImageUrl = request.ImageUrl,
                    CreatedDate = DateTime.Now
                };

                _context.Vocabularies.Add(vocab);
                await _context.SaveChangesAsync();

                return new AppResponse<object> { IsSuccess = true, Message = "Thêm từ thành công", Data = vocab };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> UpdateVocabulary(UpdateVocabularyRequest request)
        {
            try
            {
                var vocab = await _context.Vocabularies.FindAsync(request.Id);
                if (vocab == null) 
                    return new AppResponse<object> { IsSuccess = false, Message = "Không tìm thấy từ vựng" };

                var existingWord = await _context.Vocabularies
                    .AnyAsync(x => x.Id != request.Id && x.TopicId == request.TopicId && x.Word == request.Word);
                if (existingWord)
                {
                    return new AppResponse<object> { IsSuccess = false, Message = "Từ này đã có trong chủ đề rồi!" };
                }

                vocab.TopicId = request.TopicId;
                vocab.Word = request.Word;
                vocab.Meaning = request.Meaning;
                vocab.Phonetic = request.Phonetic;
                vocab.AudioUrl = request.AudioUrl;
                vocab.ExampleSentence = request.ExampleSentence;
                vocab.ImageUrl = request.ImageUrl;

                _context.Vocabularies.Update(vocab);
                await _context.SaveChangesAsync();

                return new AppResponse<object> { IsSuccess = true, Message = "Cập nhật từ thành công", Data = vocab };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> DeleteVocabulary(int id)
        {
            var item = await _context.Vocabularies.FindAsync(id);
            if (item == null) return new AppResponse<object> { IsSuccess = false, Message = "Không tìm thấy" };

            _context.Vocabularies.Remove(item);
            await _context.SaveChangesAsync();
            return new AppResponse<object> { IsSuccess = true, Message = "Đã xóa" };
        }
        #endregion

        #region SRS Algorithm (Thông minh)

        // Lấy bài học: Ưu tiên từ Cần Ôn (Due) -> Sau đó lấy thêm từ Mới (New)
        public async Task<AppResponse<object>> GetWordsToLearn(int? topicId = null)
        {
            try
            {
                // 1. Lấy danh sách cần ôn tập (Review Queue)
                // Sử dụng cú pháp LINQ Query để Join thủ công
                var reviewsQuery = from p in _context.LearningProgresses
                                     join v in _context.Vocabularies on p.VocabId equals v.Id // Nối bảng tại đây
                                     where p.UserId == _userId && p.NextReviewDate <= DateTime.Now
                                     select new { p, v };

                if (topicId.HasValue)
                {
                    reviewsQuery = reviewsQuery.Where(x => x.v.TopicId == topicId.Value);
                }

                var reviews = await reviewsQuery.Select(x => new
                                     {
                                         Vocab = x.v, // Lấy toàn bộ object Vocabulary
                                         ProgressId = x.p.Id,
                                         Type = "Review",
                                         EaseFactor = x.p.EaseFactor,
                                         Repetitions = x.p.Repetitions
                                     })
                                     .Take(30)
                                     .ToListAsync();

                // 2. Nếu ít bài ôn quá, lấy thêm từ mới (New Queue)
                if (reviews.Count < 30)
                {
                    // Lấy danh sách ID các từ đã học
                    var learnedVocabIds = _context.LearningProgresses
                        .Where(p => p.UserId == _userId)
                        .Select(p => p.VocabId);

                    var newWordsQuery = _context.Vocabularies
                        .Where(v => !learnedVocabIds.Contains(v.Id));

                    if (topicId.HasValue)
                    {
                        newWordsQuery = newWordsQuery.Where(v => v.TopicId == topicId.Value);
                    }

                    var newWords = await newWordsQuery
                        .Take(30 - reviews.Count)
                        .Select(v => new
                        {
                            Vocab = v,
                            ProgressId = (long)0,
                            Type = "New",
                            EaseFactor = (double?)2.5, // Ép kiểu cho khớp data type
                            Repetitions = (int?)0
                        })
                        .ToListAsync();

                    reviews.AddRange(newWords);
                }

                // Trộn ngẫu nhiên danh sách trước khi trả về
                var result = reviews.OrderBy(x => Guid.NewGuid()).ToList();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        // Logic cập nhật thuật toán SuperMemo-2
        public async Task<AppResponse<object>> SubmitReview(SubmitReviewRequest request)
        {
            try
            {
                // 1. Tìm progress cũ
                var progress = await _context.LearningProgresses
                    .FirstOrDefaultAsync(p => p.UserId == _userId && p.VocabId == request.VocabId);

                // 2. Nếu chưa học bao giờ -> Tạo mới
                if (progress == null)
                {
                    progress = new LearningProgress
                    {
                        UserId = _userId,
                        VocabId = request.VocabId,
                        // Khởi tạo giá trị mặc định để tránh null
                        Box = 0,
                        EaseFactor = 2.5,
                        Repetitions = 0,
                        IntervalDays = 0,
                        IsMastered = false,
                        LastReviewDate = DateTime.Now,
                        NextReviewDate = DateTime.Now // Để học ngay
                    };
                    _context.LearningProgresses.Add(progress);
                }

                // --- XỬ LÝ LOGIC NULLABLE (Quan trọng) ---
                // Lấy giá trị ra biến thường (non-nullable) để tính toán cho dễ
                int currentReps = progress.Repetitions ?? 0;
                double currentEF = progress.EaseFactor ?? 2.5;
                int currentInterval = progress.IntervalDays ?? 0;

                // --- CORE ALGORITHM (SM-2) ---
                if (request.Quality >= 3) // Nếu nhớ (3, 4, 5)
                {
                    if (currentReps == 0)
                    {
                        currentInterval = 1;
                    }
                    else if (currentReps == 1)
                    {
                        currentInterval = 6;
                    }
                    else
                    {
                        // Fix lỗi convert: Tính toán bằng double rồi ép về int
                        double nextIntervalRaw = (double)currentInterval * currentEF;
                        currentInterval = (int)Math.Round(nextIntervalRaw);
                    }

                    currentReps++;

                    // Công thức chỉnh EaseFactor
                    // EF' = EF + (0.1 - (5-q) * (0.08 + (5-q)*0.02))
                    double q = request.Quality;
                    currentEF = currentEF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
                }
                else // Nếu quên (0, 1, 2)
                {
                    currentReps = 0;
                    currentInterval = 1; // Reset về học lại ngày mai
                }

                // Chặn biên dưới EF không nhỏ hơn 1.3 (để không bị lặp lại quá nhiều)
                if (currentEF < 1.3) currentEF = 1.3;

                // --- CẬP NHẬT NGƯỢC LẠI VÀO MODEL ---
                progress.Repetitions = currentReps;
                progress.EaseFactor = currentEF;
                progress.IntervalDays = currentInterval;

                // Tính ngày review tiếp theo
                progress.LastReviewDate = DateTime.Now;
                progress.NextReviewDate = DateTime.Now.AddDays(currentInterval);

                // --- LƯU DB ---
                // Nếu ID > 0 nghĩa là bản ghi cũ -> Update
                // Nếu ID == 0 nghĩa là bản ghi mới -> Add (EF tự tracking, nhưng gọi Update cũng không sao)
                if (progress.Id > 0)
                {
                    _context.LearningProgresses.Update(progress);
                }

                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = $"Đã lưu. Gặp lại sau {currentInterval} ngày."
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }
        #endregion

        #region Progress Management & Testing
        public async Task<AppResponse<object>> GetVocabularyProgress(int? topicId = null)
        {
            try
            {
                var query = from v in _context.Vocabularies
                            join p in _context.LearningProgresses
                                on new { VocabId = v.Id, UserId = _userId } 
                                equals new { p.VocabId, p.UserId } into vp
                            from p in vp.DefaultIfEmpty()
                            select new { v, p };

                if (topicId.HasValue)
                {
                    query = query.Where(x => x.v.TopicId == topicId.Value);
                }

                var progressList = await query
                    .OrderByDescending(x => x.v.Id)
                    .Select(x => new
                    {
                        Vocab = x.v,
                        Progress = x.p
                    }).ToListAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Data = progressList
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> ResetProgress(int vocabId)
        {
            try
            {
                var progress = await _context.LearningProgresses
                    .FirstOrDefaultAsync(p => p.VocabId == vocabId && p.UserId == _userId);

                if (progress != null)
                {
                    _context.LearningProgresses.Remove(progress);
                    await _context.SaveChangesAsync();
                }

                return new AppResponse<object> { IsSuccess = true, Message = "Đã xoá tiến độ học của từ này" };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> ResetTopicProgress(int topicId)
        {
            try
            {
                var vocabIdsInTopic = await _context.Vocabularies
                    .Where(v => v.TopicId == topicId)
                    .Select(v => v.Id)
                    .ToListAsync();

                var progressesToRemove = await _context.LearningProgresses
                    .Where(p => p.UserId == _userId && vocabIdsInTopic.Contains(p.VocabId))
                    .ToListAsync();

                if (progressesToRemove.Any())
                {
                    _context.LearningProgresses.RemoveRange(progressesToRemove);
                    await _context.SaveChangesAsync();
                }

                return new AppResponse<object> { IsSuccess = true, Message = "Đã xoá toàn bộ tiến độ của chủ đề" };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> ResetAllProgress()
        {
            try
            {
                var progressesToRemove = await _context.LearningProgresses
                    .Where(p => p.UserId == _userId)
                    .ToListAsync();

                if (progressesToRemove.Any())
                {
                    _context.LearningProgresses.RemoveRange(progressesToRemove);
                    await _context.SaveChangesAsync();
                }

                return new AppResponse<object> { IsSuccess = true, Message = "Đã xoá toàn bộ tiến độ của tất cả từ vựng" };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> GetLearnedWordsForTest(int? topicId = null)
        {
            try
            {
                var learnedWordsQuery = from p in _context.LearningProgresses
                                        join v in _context.Vocabularies on p.VocabId equals v.Id
                                        where p.UserId == _userId
                                        select new
                                        {
                                            Vocab = v,
                                            ProgressId = p.Id,
                                            Type = "Test",
                                            EaseFactor = p.EaseFactor,
                                            Repetitions = p.Repetitions
                                        };

                if (topicId.HasValue)
                {
                    learnedWordsQuery = learnedWordsQuery.Where(x => x.Vocab.TopicId == topicId.Value);
                }

                var learnedWords = await learnedWordsQuery
                    .Take(50) // Giới hạn bài test 50 từ/lần
                    .ToListAsync();

                // Trộn ngẫu nhiên
                var result = learnedWords.OrderBy(x => Guid.NewGuid()).ToList();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }
        #endregion

        #region Gamification
        public async Task<AppResponse<object>> SubmitXP(int xpAmount)
        {
            try
            {
                var user = await _context.Users.FindAsync(_userId);
                if (user == null) return new AppResponse<object> { IsSuccess = false, Message = "User not found" };

                // Cộng XP
                user.XP += xpAmount;

                // Tính toán Level hiện tại (Ví dụ: mỗi 100 XP = 1 Level)
                user.Level = (user.XP / 100) + 1;

                // Xử lý chuỗi ngày học (Streak)
                var today = DateTime.Today;
                var lastStudy = user.LastStudyDate?.Date;

                if (lastStudy == null)
                {
                    // Ngày học đầu tiên
                    user.StreakCount = 1;
                }
                else if (lastStudy == today.AddDays(-1))
                {
                    // Học liên tiếp ngày hôm qua -> hôm nay
                    user.StreakCount += 1;
                }
                else if (lastStudy < today.AddDays(-1))
                {
                    // Mất chuỗi (cách > 1 ngày) -> Reset về 1
                    user.StreakCount = 1;
                }
                // Nếu lastStudy == today thì đã tính streak hôm nay rồi, không cộng thêm nữa

                user.LastStudyDate = DateTime.Now;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Message = $"Đã cộng {xpAmount} XP!",
                    Data = new
                    {
                        xp = user.XP,
                        level = user.Level,
                        streak = user.StreakCount
                    }
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<AppResponse<object>> GetUserGamificationProfile()
        {
            try
            {
                var user = await _context.Users.FindAsync(_userId);
                if (user == null) return new AppResponse<object> { IsSuccess = false, Message = "User not found" };

                // Kiểm tra xem hôm nay đã mất chuỗi chưa (để hiển thị UI)
                var today = DateTime.Today;
                var lastStudy = user.LastStudyDate?.Date;
                bool isStreakLost = lastStudy < today.AddDays(-1);
                
                // Nếu đã mất chuỗi, update luôn về 0 để UI hiện đúng
                if (isStreakLost && user.StreakCount > 0)
                {
                    user.StreakCount = 0;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }

                return new AppResponse<object>
                {
                    IsSuccess = true,
                    Data = new
                    {
                        xp = user.XP,
                        level = user.Level,
                        streak = user.StreakCount,
                        lastStudyDate = user.LastStudyDate
                    }
                };
            }
            catch (Exception ex)
            {
                return new AppResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }
        #endregion
    }
}