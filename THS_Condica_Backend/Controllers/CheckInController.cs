using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json.Linq;
using THS_Condica_Backend;
using THS_Condica_Backend.Models;

namespace THS_Condica_Backend.Controllers
{   [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private UserDbContext _context;
        private UserManager<ApplicationUser> _userManager;

        public CheckInController(UserDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //ADMIN

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("adcondica")]
        public IEnumerable<CheckInDTO> Condica(string userName, string startDate, string endDate)
        {

            var checkIn = (from c in _context.CheckIn
                           join u in _userManager.Users on c.OwnerId equals u.Id
                           select new CheckInDTO
                           {
                               ID = c.ID,
                               FirstEntry = c.FirstEntry,
                               SecondEntry = c.SecondEntry,
                               Day = c.Day,
                               UserName = u.UserName,
                               FullName = u.FirstName + " " + u.LastName
                           }); ; 

            if(!string.IsNullOrEmpty(userName))
            {
                checkIn = checkIn.Where(x => x.UserName.Equals(userName));

            }
            //if empty gets present year
            if (string.IsNullOrEmpty(startDate))
            {
                startDate = DateTime.Now.ToString();
            }        
            if (string.IsNullOrEmpty(endDate))
            {
                endDate = DateTime.Now.ToString();
                //if string is all do nothing
            }
           
                checkIn = checkIn.Where(c => c.Day >= Convert.ToDateTime(startDate)  && c.Day <= Convert.ToDateTime(endDate))
                                  .OrderByDescending(o => o.Day);
            
            return checkIn;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("adtotalmonth")]
        public JObject TotalMonthlyCheckIn(string userName, string startDate, string endDate)
        {
            var checkIn = Condica(userName, startDate, endDate);

            var firstCheckin = checkIn.Select(p => TimeSpan.Parse(p.FirstEntry));
            var secondCheckin = checkIn.Select(p => TimeSpan.Parse(p.SecondEntry));

            TimeSpan firstTotal = TimeSpan.Zero;
            foreach (TimeSpan timeSpan in firstCheckin)
            {
                firstTotal += timeSpan;
            }
            TimeSpan secondTotal = TimeSpan.Zero;
            foreach (TimeSpan timeSpan in secondCheckin)
            {
                secondTotal += timeSpan;
            }
            TimeSpan total = secondTotal - firstTotal;
            string finalValue = ((total.Days * 24 + total.Hours) + ":" + total.Minutes);

            JObject json = new JObject();
            json.Add("mHours", finalValue);

            return json;

        }
        [Authorize(Roles = "Admin")]
        [HttpGet("adtotalyear")]
        public JObject AdTotalYearlyCheckIn([FromQuery]string userName)
        {
            var checkIn = (from c in _context.CheckIn
                           join u in _userManager.Users on c.OwnerId equals u.Id
                           select new CheckInDTO
                           {
                               ID = c.ID,
                               FirstEntry = c.FirstEntry,
                               SecondEntry = c.SecondEntry,
                               Day = c.Day,
                               UserName = u.UserName
                           });

            if (!string.IsNullOrEmpty(userName))
            {
                checkIn = checkIn.Where(x => x.UserName.Equals(userName));

            }

            var year = DateTime.Now.Year.ToString();

            var selectedYear = checkIn.Where(c => c.Day.Year == Int32.Parse(year));

            var firstCheckin = selectedYear.Select(p => TimeSpan.Parse(p.FirstEntry));
            var secondCheckin = selectedYear.Select(p => TimeSpan.Parse(p.SecondEntry));

            TimeSpan firstTotal = TimeSpan.Zero;
            foreach (TimeSpan timeSpan in firstCheckin)
            {
                firstTotal += timeSpan;
            }

            TimeSpan secondTotal = TimeSpan.Zero;
            foreach (TimeSpan timeSpan in secondCheckin)
            {
                secondTotal += timeSpan;
            }
            TimeSpan total = secondTotal - firstTotal;
            string finalValue = ((total.Days * 24 + total.Hours) + ":" + total.Minutes);

            JObject json = new JObject();
            json.Add("mHours", finalValue);

            return json;
        }

        [HttpPost]
        [Route("adduserentry")]
        public async Task<ActionResult<CheckInModel>> AdPostCheckInModel(CheckInDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            string userId = user.Id;

            var checkIn = new CheckInModel
            {
                Day = model.Day,
                FirstEntry = model.FirstEntry,
                SecondEntry = model.SecondEntry,
                OwnerId = userId

            };

            _context.CheckIn.Add(checkIn);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCheckInModel", new { id = checkIn.ID }, checkIn);
        }

        [HttpPut]
        [Route("adupdateentry")]
        public async Task<IActionResult> AdPutCheckInModel([FromBody] CheckInDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            string userId = user.Id;

            var checkIn = new CheckInModel
            {
                ID = model.ID,
                Day = model.Day,
                FirstEntry = model.FirstEntry,
                SecondEntry = model.SecondEntry,
                OwnerId = userId

            };
            
            _context.Entry(checkIn).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CheckInModelExists(model.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // DELETE: api/CheckIn/5
        [HttpDelete]
        [Route("addeleteentry/{id}")]
        public async Task<ActionResult<CheckInModel>> AdDeleteCheckInModel(int id)
        {
            var checkInModel = await _context.CheckIn.FindAsync(id);
            if (checkInModel == null)
            {
                return NotFound();
            }

            _context.CheckIn.Remove(checkInModel);
            await _context.SaveChangesAsync();

            return checkInModel;
        }

        //USERS

        // return total worked hours in this month
        [HttpGet("totalmonth")]
        public JObject TotalMonthlyCheckIn(string year, string month)
        {
            var checkin = GetCheckIn(year, month);

            var firstCheckin = checkin.Select(p => TimeSpan.Parse(p.FirstEntry));
            var secondCheckin = checkin.Select(p => TimeSpan.Parse(p.SecondEntry));

            TimeSpan firstTotal = TimeSpan.Zero;
            foreach (TimeSpan timeSpan in firstCheckin)
            {
                firstTotal += timeSpan;
            }
            TimeSpan secondTotal = TimeSpan.Zero;
            foreach (TimeSpan timeSpan in secondCheckin)
            {
                secondTotal += timeSpan;
            }
            TimeSpan total = secondTotal - firstTotal;
            string finalValue = ((total.Days * 24 + total.Hours) + ":" + total.Minutes);

            JObject json = new JObject();
            json.Add("mHours", finalValue);

            return json;
        }

        // return total worked hours in this year
        [HttpGet("totalyear")]
        public JObject TotalYearlyCheckIn([FromQuery] string year)
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }

            var selectedYear = _context.CheckIn.Where(c => c.OwnerId.Equals(userId) && c.Day.Year == Int32.Parse(year));

            var firstCheckin = selectedYear.Select(p => TimeSpan.Parse(p.FirstEntry));
            var secondCheckin = selectedYear.Select(p => TimeSpan.Parse(p.SecondEntry));

            TimeSpan firstTotal = TimeSpan.Zero;
            foreach (TimeSpan timeSpan in firstCheckin)
            {
                firstTotal += timeSpan;
            }
            TimeSpan secondTotal = TimeSpan.Zero;
            foreach (TimeSpan timeSpan in secondCheckin)
            {
                secondTotal += timeSpan;
            }
            TimeSpan total = secondTotal - firstTotal;
            string finalValue = ((total.Days * 24 + total.Hours) + ":" + total.Minutes);

            JObject json = new JObject();
            json.Add("mHours", finalValue);

            return json;
        }

        // GET: api/CheckIn
        [HttpGet]
        public IEnumerable<CheckInModel> GetCheckIn([FromQuery] string year, string month)
        {   
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            var checkin = _context.CheckIn.Where(c => c.OwnerId.Equals(userId));

            //if empty gets present year
            if (string.IsNullOrEmpty(year))
            {
                year = DateTime.Now.Year.ToString();
            }
            //if empty gets present month
            if (string.IsNullOrEmpty(month))
            {
                month = DateTime.Now.Month.ToString();
            //if string is all do nothing
            }else if (month.Equals("All"))
            {

            }
            //else transform the string into month index
            else
            {
                month = TransformMonth(month).ToString();
            }
            //if month string is all query only the year
            if(month.Equals("All"))
            {
                checkin = checkin.Where(c => c.Day.Year == Int32.Parse(year))
                                 .OrderByDescending(o => o.Day);
            }
            //if month string is legit query year and month
            else
            {
                checkin = checkin.Where(c => c.Day.Year == Int32.Parse(year) && c.Day.Month == Int32.Parse(month))
                                 .OrderByDescending(o => o.Day);
            }
           
            return checkin; //returns only the sepcific user values
        }
        public int TransformMonth(string month)
        {
          string[] months ={
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December"
            };
            if(months.Contains(month))
            {
                return months.IndexOf(month) +1;
            }
            return DateTime.Now.Month;
        }

        // GET: api/CheckIn/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CheckInModel>> GetCheckInModel(int id)
        {
            var checkInModel = await _context.CheckIn.FindAsync(id);

            if (checkInModel == null)
            {
                return NotFound();
            }
            return checkInModel;
        }

        // GET: api/CheckIn/todayEntry
        [HttpGet]
        [Route("todayEntry")]
        public async Task<ActionResult<CheckInModel>> GetTodayEntry()
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            var id = _context.CheckIn.First(u => u.OwnerId == userId && u.Day == DateTime.Today).ID;
            var checkInModel = await _context.CheckIn.FindAsync(id);

            if (checkInModel == null)
            {
                return NotFound();
            }
            return checkInModel;
        }

        // PUT: api/CheckIn/secondEntry
        [HttpPut]
        [Route("secondEntry")]
        public async Task<IActionResult> PutCheckInModel([FromBody] CheckInModel checkInModel)
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            var id = _context.CheckIn.Where(u => u.OwnerId == userId)
                                     .FirstOrDefault(i => i.Day == DateTime.Today).ID;

            if (id != checkInModel.ID)
            {
                return BadRequest();
            }

            //_context.Entry(checkInModel).State = EntityState.Modified;
            var dbModel = await _context.CheckIn.FindAsync(id);
            dbModel.ID = checkInModel.ID;
            dbModel.SecondEntry = checkInModel.SecondEntry;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CheckInModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // POST: api/CheckIn
        [HttpPost]
        [Route("firstEntry")]
        public async Task<ActionResult<CheckInModel>> PostCheckInModel(CheckInModel checkInModel)
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;

            checkInModel.OwnerId = userId;
            checkInModel.Day = DateTime.Today;
            checkInModel.SecondEntry = checkInModel.FirstEntry;
            _context.CheckIn.Add(checkInModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCheckInModel", new { id = checkInModel.ID }, checkInModel);
        }

        // DELETE: api/CheckIn/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CheckInModel>> DeleteCheckInModel(int id)
        {
            var checkInModel = await _context.CheckIn.FindAsync(id);
            if (checkInModel == null)
            {
                return NotFound();
            }

            _context.CheckIn.Remove(checkInModel);
            await _context.SaveChangesAsync();

            return checkInModel;
        }

        private bool CheckInModelExists(int id)
        {
            return _context.CheckIn.Any(e => e.ID == id);
        }
    }
}
