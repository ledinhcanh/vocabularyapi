using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Request.Poll;
using API.Services.Interface;

namespace API.Services.Implement
{
    public class PollRepository : IPollRepository
    {
        private readonly GovHniContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _siteId;
        private readonly string? _webDomain;
        private readonly string? _pollSettingKey;
        public PollRepository(GovHniContext GovHniContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            this._configuration = configuration;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId);
            this._webDomain = httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.WebDomain)?.Value;
            this._pollSettingKey = configuration.GetValue<string>("AppSettings:PollSettingKey");

        }

        public async Task<AppResponse<object?>> GetDefaultPoll()
        {
            var setting = await _context.AppMobileSettings.FindAsync(this._pollSettingKey, this._siteId);
            if (setting == null || string.IsNullOrEmpty(setting.KeySettingValue))
            {
                return new AppResponse<object?>()
                {
                    IsSuccess = false,
                    Message = "Không có dữ liệu"
                };
            }
            Poll pollDefault = System.Text.Json.JsonSerializer.Deserialize<Poll>(setting.KeySettingValue);
            if (pollDefault == null)
            {
                return new AppResponse<object?>()
                {
                    IsSuccess = false,
                    Message = "Không có dữ liệu"
                };
            }
            var Now = DateTime.Now.Date;
            var Poll = await _context.Polls.Where(x => x.PollId == pollDefault.PollId && x.SiteId == _siteId && x.Approved == true && Now >= x.StartDate && x.EndDate >= Now).Select(x => new
            {
                x.PollId,
                x.PollName,
                x.PollDescription,
                x.PollType,
            }).FirstOrDefaultAsync();
            if (Poll == null)
            {
                return new AppResponse<object?>()
                {
                    IsSuccess = false,
                    Message = "Không có dữ liệu"
                };
            }
            var Items = await _context.PollItems.Where(x => x.PollId == pollDefault.PollId).OrderBy(x => x.OrderInList).ToListAsync();
            var Results = new
            {
                Poll.PollId,
                Poll.PollName,
                Poll.PollType,
                Poll.PollDescription,
                Items
            };

            return new AppResponse<object?>()
            {
                Message = "Thành công",
                IsSuccess = true,
                Data = Results
            };

        }

        public async Task<AppResponse<object?>> PostPollVote(PollVoteRequest request)
        {
            var Now = DateTime.Now.Date;
            var PollItems = await _context.PollItems.Where(x => x.PollItemId ==
            request.PollItemId
            && x.Poll.Approved == true
            && x.Poll.SiteId == _siteId
            && x.Poll.Approved == true
            && Now >= x.Poll.StartDate
            && x.Poll.EndDate >= Now).FirstOrDefaultAsync();
            if (PollItems == null)
            {
                return new AppResponse<object?>()
                {
                    Data = null,
                    Message = "Không tìm thấy dữ liệu hoặc quá thời gian thao tác",
                    IsSuccess = false
                };
            }
            if (PollItems.HasContent == true && string.IsNullOrEmpty(request.VoteContent))
            {
                return new AppResponse<object?>()
                {
                    Data = null,
                    Message = "Thông tin lựa chọn không được để trống",
                    IsSuccess = false
                };
            }
            PollItems.Vote ??= 0;
            PollItems.Vote++;
            if (PollItems.HasContent == true && !string.IsNullOrEmpty(request.VoteContent)){
                PollVote pollVote = new PollVote()
                {
                    DateVote = DateTime.Now,
                    Email = request.Email,
                    VoteContent = request.VoteContent,
                    PollItemId = request.PollItemId,
                };
                await _context.PollVotes.AddAsync(pollVote);
            }
            await _context.SaveChangesAsync();
            return new AppResponse<object?>()
            {
                Data = PollItems,
                Message = "Thành công",
                IsSuccess = true
            };
        }
    }
}
