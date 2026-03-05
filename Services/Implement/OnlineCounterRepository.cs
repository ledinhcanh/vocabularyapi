using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using API.Models.AppConfig;
using API.Models.Database.Context;
using API.Models.Database.Identities;
using API.Models.Handle;
using API.Services.Interface;

namespace API.Services.Implement
{

    public class OnlineCounterRepository : IOnlineCounterRepository
    {

        private readonly GovHniContext _context;
        private readonly int _siteId;
        public OnlineCounterRepository(GovHniContext GovHniContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._context = GovHniContext;
            int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(AppClaimType.SiteId)?.Value, out this._siteId); 
        }
        public async Task<AppResponse<object>> GetOnlineCounter()
        {
            var CurrentDate = DateTime.Now.Date;
            var NextCurrentDate = CurrentDate.AddDays(1);
            var FirstDayOfWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            var FirstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);

            var Current = await (
                    from o in _context.OnlineCounters
                    where o.SiteId == _siteId
                    && o.DateCounter >= CurrentDate
                    && o.DateCounter < NextCurrentDate
                    group o by o.CounterId into oGroup
                    select new
                    {
                        CurrentOnline = oGroup.Sum(x => x.CurrentOnline),
                        CounterNumber = oGroup.Sum(x => x.CounterNumber),
                    }
                ).FirstOrDefaultAsync();
            var CounterInWeek = await _context.OnlineCounters.Where(x => x.SiteId == _siteId && x.DateCounter >= FirstDayOfWeek && x.DateCounter < NextCurrentDate).SumAsync(x => x.CounterNumber);
            var CounterInMonth = await _context.OnlineCounters.Where(x => x.SiteId == _siteId && x.DateCounter >= FirstDayOfMonth && x.DateCounter < NextCurrentDate).SumAsync(x => x.CounterNumber);
            var CounterAll = await _context.OnlineCounters.Where(x => x.SiteId == _siteId).SumAsync(x => x.CounterNumber);

            return new AppResponse<object>()
            {
                Data = new
                {
                    CurrentOnline = Current == null ? 0 : Current.CurrentOnline,
                    CounterNumber = Current == null ? 0 : Current.CounterNumber,
                    CounterInWeek = CounterInWeek == null ? 0 : CounterInWeek,
                    CounterInMonth = CounterInMonth == null ? 0 : CounterInMonth,
                    CounterAll = CounterAll == null ? 0 : CounterAll,
                },
                IsSuccess = true,
                Message = "Thành công"
            };
        }

        public async Task<AppResponse<object>> PushOnlineCounter()
        {
            var Now = DateTime.Now.Date;
            var OnlineCounter = await _context.OnlineCounters.Where(x => x.DateCounter == Now && x.SiteId == _siteId).FirstOrDefaultAsync();
            if (OnlineCounter != null)
            {
                OnlineCounter.CounterNumber ??= 0;
                OnlineCounter.CounterNumber++;
            }
            else
            {
                OnlineCounter = new OnlineCounter()
                {
                    CounterNumber = 1,
                    SiteId = _siteId,
                    CurrentOnline = 1,
                    DateCounter = Now,
                };
                await _context.OnlineCounters.AddAsync(OnlineCounter);
            }
            await _context.SaveChangesAsync();
            return new AppResponse<object>()
            {
                Message = "Thành công",
                Data = OnlineCounter,
                IsSuccess = true
            };
        }
    }
}
