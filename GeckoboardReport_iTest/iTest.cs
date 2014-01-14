using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using JsonGenerator;


namespace GeckoboardReport_iTest
{
    class Result
    {
        public DateTime CreateDate { get; set; }
        public String UserName { get; set; }
        public String ApplicationVersion { get; set; }

    }

    class iTest
    {
        static void Main()
        {
            FileOperation fileOp = new FileOperation();
            JsonWriter jsonwriter = new JsonWriter();

            List<Result> resultElement = new List<Result>();

            using (var db = new TCIPEntities())
            {

                db.CommandTimeout = 15 * 60;
                resultElement = (from cip in db.ApplicationCIPs.OrderByDescending(x => x.CreateDate).Take(200000)
                                 where
                                    cip.ApplicationName == "iTest"
                                    && cip.ADSUserName != "polomsky"
                                    && cip.ADSUserName != "weije"
                                    && cip.ADSUserName != "zhangjus"
                                    && cip.ADSUserName != "chenha"
                                    && cip.ADSUserName != "wangmar"
                                    && cip.ADSUserName != "palomag"
                                    && cip.CreateDate > new DateTime(2013, 8, 10)
                                 select new Result
                                 {
                                     UserName = cip.ADSUserName,
                                     CreateDate = cip.CreateDate,
                                     ApplicationVersion = cip.ApplicationVersion
                                 }).ToList();

            }
            //iTest Json File         
            string fileName_iTestUser = @"C:\Users\guj\Dropbox\iTest.json";
            fileOp.DelectExist(fileName_iTestUser);
            using (FileStream fs = File.Create(fileName_iTestUser, 1024))
            {
                jsonwriter.WriteJson_xAxis(fs, "column", "USSCLPDDBSGP002-AATSPRD1", "Month");
                QueryContent(fs, resultElement, 1);
                jsonwriter.WriteJson_yAxis_head(fs, "Month", "#D94600");
                jsonwriter.WriteJson_yAxis(fs, "UserCount", "#D94600", "square");
                QueryContent(fs, resultElement, 2);
                fileOp.AddText(fs, "]}");
            }

            ////iTest Excel
            //string excelName_iTestUser = @"\\installreport\BO\ToolsCIP\iTest.csv";
            //fileOp.DelectExist(excelName_iTestUser);
            //using (FileStream fs1 = File.Create(excelName_iTestUser, 1024))
            //{
            //    fileOp.AddText(fs1, "WeekNum,#Users\r\n");
            //    QueryContent(fs1, resultElement, 3);
            //}

            //iTestVersionTrend Json
            string fileName_iTestVersionTrend = @"C:\Users\guj\Dropbox\iTestVersionTrend.json";
            fileOp.DelectExist(fileName_iTestVersionTrend);
            using (FileStream fs4 = System.IO.File.Create(fileName_iTestVersionTrend, 1024))
            {
                jsonwriter.WriteJson_xAxis(fs4, "spline", "USSCLPDDBSGP002-AATSPRD1", "Date");
                QueryContent_iTestVersionTrend(fs4, resultElement, 1);
                jsonwriter.WriteJson_yAxis_head(fs4, "buildNo.", "UserCount");
                jsonwriter.WriteJson_yAxis(fs4, "build9.5.331", "#D94600", "square");
                QueryContent_iTestVersionTrend(fs4, resultElement, 2);
                fileOp.AddText(fs4, ",");
                jsonwriter.WriteJson_yAxis(fs4, "build9.5.334", "#0080FF", "circle");
                QueryContent_iTestVersionTrend(fs4, resultElement, 3);
                fileOp.AddText(fs4, ",");
                jsonwriter.WriteJson_yAxis(fs4, "build9.6.355", "#EAC100", "diamond");
                QueryContent_iTestVersionTrend(fs4, resultElement, 4);
                fileOp.AddText(fs4, "]}");
            }


            //iTestVersion Excel
            string excelName_iTestVersion = @"\\installreport\BO\ToolsCIP\iTestVersion.csv";
            fileOp.DelectExist(excelName_iTestVersion);
            using (FileStream fs3 = File.Create(excelName_iTestVersion, 1024))
            {
                fileOp.AddText(fs3, "Date,iTestVersion,UserName,\r\n");
                QueryContent_iTestVersion(fs3, resultElement);
            }

        }

        //query lead time
        private static void QueryContent(FileStream fs, List<Result> resultElement, int k)
        {
            FileOperation fileOp = new FileOperation();

            var monthCount = ((from item in resultElement
                               select new
                               {
                                   Month = item.CreateDate.Month,
                                   UserCount = item.UserName
                               })
                              .GroupBy(x => x.Month)
                              .Select(g => new { Month = g.Key, UserCount = g.Distinct().Count() }
                                )).ToList();

            foreach (var monthUser in monthCount)
            {

                switch (k)
                {
                    case 1:
                        fileOp.AddText(fs, "\"");
                        fileOp.AddText(fs, monthUser.Month.ToString());
                        fileOp.AddText(fs, "\",");
                        break;
                    case 2:
                        fileOp.AddText(fs, monthUser.UserCount.ToString());
                        fileOp.AddText(fs, ",");
                        break;
                    case 3:
                        string csv = string.Format("{0},{1}", monthUser.Month.ToString(), monthUser.UserCount.ToString());
                        fileOp.AddText(fs, csv);
                        fileOp.AddText(fs, "\r\n");
                        break;
                }



            }


            fileOp.AddText(fs, "]}");


        }
        private static void QueryContent_iTestVersionTrend(FileStream fs4, List<Result> resultElement, int q)
        {

            DateOperation dateOp = new DateOperation();
            FileOperation fileOp = new FileOperation();

            DateTime StartDate = new DateTime(2013, 8, 13);
            DateTime EndDate = DateTime.Now.AddDays(-1);
            foreach (DateTime date in dateOp.GetDateRange(StartDate, EndDate))
            {

                int userCount_331 = (from rst in resultElement
                                     where rst.ApplicationVersion.Contains("9.5.331")
                                     && rst.CreateDate < date.AddDays(1)
                                     && rst.CreateDate > date
                                     select rst.UserName).Distinct().Count();
                int userCount_334 = (from rst in resultElement
                                     where rst.ApplicationVersion.Contains("9.5.334")
                                     && rst.CreateDate < date.AddDays(1)
                                     && rst.CreateDate > date
                                     select rst.UserName).Distinct().Count();
                int userCount_355 = (from rst in resultElement
                                     where rst.ApplicationVersion.Contains("9.6.355")
                                     && rst.CreateDate < date.AddDays(1)
                                     && rst.CreateDate > date
                                     select rst.UserName).Distinct().Count();

                switch (q)
                {
                    case 1:
                        fileOp.AddText(fs4, "\"");
                        fileOp.AddText(fs4, date.ToShortDateString().ToString());
                        fileOp.AddText(fs4, "\",");
                        break;
                    case 2:
                        fileOp.AddText(fs4, userCount_331.ToString());
                        fileOp.AddText(fs4, ",");
                        break;
                    case 3:
                        fileOp.AddText(fs4, userCount_334.ToString());
                        fileOp.AddText(fs4, ",");
                        break;
                    case 4:
                        fileOp.AddText(fs4, userCount_355.ToString());
                        fileOp.AddText(fs4, ",");
                        break;
                }
            }
            fileOp.AddText(fs4, "]}");
        }
        //Export to excel row data
        private static void QueryContent_iTestVersion(FileStream fs3, List<Result> resultElement)
        {

            FileOperation fileOp = new FileOperation();
            var recentData = (from item in resultElement
                                where item.CreateDate > DateTime.Now.AddDays(-7)
                                && item.UserName !="iTools"
                                select new Result{
                                   CreateDate = item.CreateDate,
                                   ApplicationVersion = item.ApplicationVersion,
                                   UserName = item.UserName
                                }).OrderBy(x=>x.ApplicationVersion).Distinct().ToList();
            foreach (var iDate in recentData)
            {
                string csv = string.Format("{0},{1},{2}", iDate.CreateDate.ToString(), iDate.ApplicationVersion.ToString(), iDate.UserName.ToString());
                fileOp.AddText(fs3, csv);
                fileOp.AddText(fs3, "\r\n");
                
            }


        }

    }



}      
        
            
 

    
