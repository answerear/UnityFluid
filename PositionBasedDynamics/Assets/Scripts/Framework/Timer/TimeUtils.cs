using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Framework
{
    public class TimeUtils : Singleton<TimeUtils>
    {
        private DateTime ZERO = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private long mSvrTimeDiff = 0;

        public long UpdateSvrTime(long svrTimeStamp)
        {
            mSvrTimeDiff = svrTimeStamp - (int)GetSecondsSinceEpoch();
            return mSvrTimeDiff;
        }

        public long GetSvrTime()
        {
            return mSvrTimeDiff + (int)GetSecondsSinceEpoch();
        }

        // 是否已过期
        public bool IsExpired(long svrEndTime)
        {
            long svrTimeNow = GetSvrTime();
            return svrTimeNow >= svrEndTime;
        }

        // 判断当前时间是否在开始和结束时间之间
        public bool NowInTimeSection(long svrBegTime, long svrEndTime)
        {
            long svrTimeNow = GetSvrTime();
            return svrBegTime <= svrTimeNow && svrTimeNow <= svrEndTime;
        }

        // 根据字符串转换成距离当地时区的UTC时间，如中国为距1970-01-01 00:08:00的秒数
        // 时间格式为 yyyy-MM-dd hh:mm:ss
        public long ConvertTime(string strDateTime)
        {
            if(strDateTime == null)
            {
                return 0;
            }

            DateTime time;
            bool ret = DateTime.TryParseExact(strDateTime, "yyyy-MM-dd HH:mm:ss", 
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out time);
            if(ret)
            {
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(ZERO); // 当地时区
                TimeSpan ts = time - startTime;
                return Convert.ToInt64(ts.TotalSeconds);
            }
            else
            {
                Log.Error("ConvertTime ", strDateTime, " format error !");
                return 0;
            }

        }

        // 判断“偏移”时间是否在开始和结束时间之间

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTimeBeg"> string like 1970-01-01 00:00:00,为null时，表示没有开始时间 </param>
        /// <param name="dataTimeEnd"> string like 1970-01-01 00:00:00,为null时，表示没有结束时间 </param>
        /// <param name="deltaDay">偏移天数</param>
        /// <returns></returns>
        public bool NowInSection(string dataTimeBeg, string dataTimeEnd, int deltaDay=0)
        {
            long timeNow = GetSvrTime();
            long timeDst = timeNow + deltaDay * 86400;  // 推迟deltaDay天
            long timeBeg = ConvertTime(dataTimeBeg);
            long timeEnd = ConvertTime(dataTimeEnd);

            bool valid = false;
            if(dataTimeBeg == null)
            {
                if(dataTimeEnd == null)
                { // 没有开始和结束时间
                    valid = true;
                }
                else
                {// 没有开始时间，有结束时间
                    valid = timeDst <= timeEnd;
                }
            }
            else
            {
                if (dataTimeEnd == null)
                { // 有开始时间，没有结束时间
                    valid = timeBeg <= timeDst;
                }
                else
                {// 有开始时间，有结束时间
                    valid = timeBeg <= timeDst && timeDst <= timeEnd;
                }
            }

            return valid;
        }

        // 判断当前时间是否在指定的时间段之间
        // timeSection 格式"13:30-16:40"，有可能为null
        public bool NowInDailySection(string timeSection)
        {
            if (timeSection == null)
                return true;

            long timeNow = GetSvrTime();

            // 正则表达式解出4个int值
            var regex = new System.Text.RegularExpressions.Regex("\\d+");
            var regexMatches = regex.Matches(timeSection);
            var results = new System.Collections.Generic.List<int>();
            foreach (Match match in regexMatches)
                results.Add(Int32.Parse(match.Value));
            if (results.Count < 4)
            {
                Log.Error("NowInDailySection ", timeSection, " format error !");
                return true;
            }

            DateTime dateNow = MakeDateTime(timeNow);
            DateTime dateBeg = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, results[0], results[1], 0, DateTimeKind.Utc);
            DateTime dateEnd = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, results[2], results[3], 0, DateTimeKind.Utc);

            return dateBeg <= dateNow && dateNow <= dateEnd;
        }

        // 获取当前时间到结束时间剩余的秒数
        public long GetLeftSec(long svrEndTime)
        {
            long svrTimeNow = GetSvrTime();
            return svrEndTime - svrTimeNow;
        }

        public void GetLeftTime(long svrEndTime, out int day, out int hour, out int min, out int sec)
        {
            long leftSec = GetLeftSec(svrEndTime);
            int leftMin = (int)(leftSec / 60);
            day = leftMin / (60 * 24);
            hour = leftMin % (60 * 24) / 60;
            min = leftMin % (60 * 24) % 60;
            sec = (int)leftSec % 60;
        }

        public void Test()
        {
            long time0 = ConvertTime("1970-01-01 00:00:00");
            Debug.Assert(time0 == 0);
            long time1 = ConvertTime("1970-01-01 10:00:00");
            Debug.Assert(time1 == 60 * 60);

            bool inTime0 = NowInDailySection("00:30-23:40");
            Debug.Assert(inTime0);
            bool inTime1 = NowInDailySection("00:30-00:40");
            Debug.Assert(!inTime1);
            bool inTime2 = NowInDailySection("07:30-20:55");
            Debug.Assert(inTime2);

            long svrTime = GetSvrTime();
            long timeLeft = GetLeftSec(svrTime);
            Debug.Assert(timeLeft == 0);

            Debug.Assert(NowInSection("1970-01-01 00:00:00", "2022-01-01 00:00:00", 0));
            Debug.Assert(NowInSection("2020-10-23 07:00:00", "2020-10-23 23:00:00", 0));
            Debug.Assert(NowInSection("2020-10-23 09:00:00", "2020-10-23 13:00:00", 0));
        }

        public string FormatDateTime(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string FormatDate(DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd");
        }

        public string FormateTime(DateTime dateTime, bool ms = false)
        {
            return ms ? dateTime.ToString("HH:mm:ss.fff") : dateTime.ToString("HH:mm:ss");
        }

        public string FormatDateTime(long datetime, bool ms = false)
        {
            DateTime time = DateTime.FromBinary(datetime);
            return ms ? time.ToString("yyyy-MM-dd HH:mm:ss.fff") : time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public DateTime MakeDateTime(long timestamp, bool ms = false)
        {
            DateTime time = TimeZone.CurrentTimeZone.ToLocalTime(ZERO);
            return ms ? time.AddMilliseconds(timestamp) : time.AddSeconds(timestamp);
        }

        // 该函数返回年、月、日，并且用点号分隔开
        public string FormatDateTime2(long timestamp)
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            time = time.AddSeconds(timestamp).ToLocalTime();

            return time.ToString("yyyy.MM.dd");
        }

        public long Ticks()
        {
            return DateTime.Now.Ticks;
        }

        // 本地时间，中国为距离1970-01-01 08:00:00的毫秒数
        public long GetMillisecondsSinceEpoch()
        {
            TimeSpan ts = DateTime.UtcNow - ZERO;
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        // 本地时间，中国为距离1970-01-01 08:00:00的秒数
        public double GetSecondsSinceEpoch()
        {
            TimeSpan ts = DateTime.UtcNow - ZERO;
            return ts.TotalSeconds;
        }

        public string GetMSecsSinceEpoch()
        {
            TimeSpan ts = DateTime.UtcNow - ZERO;
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }
    }
}
