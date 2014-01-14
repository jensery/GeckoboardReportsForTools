using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using JsonGenerator;

namespace ECSUser
{
    class Result
    {
        public String JobID { get; set; }
        public DateTime? CreateDate { get; set; }
        public String UserName { get; set; }
        public String JobType { get; set; }
        public DateTime MFGT_CreateDate { get; set; }

    }
    class DTFUserCount
    {
        static void Main()
        {
            List<Result> resultElement = new List<Result>();
            List<Result> resultElement2 = new List<Result>();
            List<Result> resultElement_JobType = new List<Result>();
            FileOperation fileOp = new FileOperation();
            JsonWriter jsonwriter = new JsonWriter();         
            

            using (var db = new MFGTEntities())
            {
                db.CommandTimeout = 15 * 60;
                resultElement = (from jb in db.Jobs
                where jb.CreatedDate > new DateTime(2013,12,11)
                                    && jb.JobType == "Custom"
                                    && jb.Creator != "polomsky"
                                    && jb.Creator != "weije"
                                    && jb.Creator != "zhangjus"
                                    && jb.Creator != "chenha"
                                    && jb.Creator != "wangmar"
                                    && jb.Creator != "palomag"
                select new Result { JobID = jb.JobID, 
                    CreateDate = jb.CreatedDate, 
                    UserName = jb.Creator }).ToList();  
            }
            //Get the number of users for different JobType by month
            using (var db = new MFGTEntities())
            {
                db.CommandTimeout = 15 * 60;
                resultElement_JobType = (from jb in db.Jobs
                where jb.CreatedDate > new DateTime(2013,1,1)
                                    && jb.Creator != "polomsky"
                                    && jb.Creator != "weije"
                                    && jb.Creator != "zhangjus"
                                    && jb.Creator != "chenha"
                                    && jb.Creator != "wangmar"
                                    && jb.Creator != "palomag"
                 select new Result
                                    {
                                        JobID = jb.JobID,
                                        MFGT_CreateDate = jb.CreatedDate,
                                        UserName = jb.Creator,
                                        JobType = jb.JobType
                                    }).ToList();  


            }
            using (var db2 = new AAT_DTF_ecsEntities2())
            {
                resultElement2 = (from jb2 in db2.jobs
                                  where jb2.StartTime > new DateTime(2013, 12, 11)
                                    && jb2.Submitter != "polomsky"
                                    && jb2.Submitter != "weije"
                                    && jb2.Submitter != "zhangjus"
                                    && jb2.Submitter != "chenha"
                                    && jb2.Submitter != "wangmar"
                                    && jb2.Submitter != "palomag"
                                  select new Result
                                  {
                                      JobID = jb2.JobID,
                                      CreateDate = jb2.StartTime,
                                      UserName = jb2.Submitter
                                  }).ToList();
            }
            
            //DTF4 vs. DTF5 user count 
            string fileName = @"C:\Users\guj\Dropbox\DTFUserCount.json";
            fileOp.DelectExist(fileName);
            using (FileStream fs = File.Create(fileName, 1024))
            {
                jsonwriter.WriteJson_xAxis(fs, "spline", "MFGT&AAT_DTF_ecs", "Date");
                QueryContent(fs, resultElement,resultElement2, 1);
                jsonwriter.WriteJson_yAxis_head(fs, "UserCount", "EAC100");
                jsonwriter.WriteJson_yAxis(fs, "DTF4", "#EAC100", "diamond");
                QueryContent(fs, resultElement, resultElement2,2);
                fileOp.AddText(fs, ",");
                jsonwriter.WriteJson_yAxis(fs, "DTF5", "#0080FF", "circle");
                QueryContent(fs, resultElement,resultElement2, 3);
                fileOp.AddText(fs, "]}");
            }
            //Custom vs. official vs. codecoverage user count by month
            string fileName_JobType = @"C:\Users\guj\Dropbox\DTFJobTypeUserCount.json";
            fileOp.DelectExist(fileName_JobType);
            using (FileStream fs3 = File.Create(fileName_JobType, 1024))
            {
                jsonwriter.WriteJson_xAxis(fs3, "spline", "MFGT", "Month");
                QueryContent_JobType(fs3,  resultElement_JobType, 1);
                jsonwriter.WriteJson_yAxis_head(fs3, "UserCount", "EAC100");
                jsonwriter.WriteJson_yAxis(fs3, "Custom", "#EAC100", "diamond");
                QueryContent_JobType(fs3,  resultElement_JobType, 2);
                fileOp.AddText(fs3, ",");
                jsonwriter.WriteJson_yAxis(fs3, "Official", "#336600", "circle");
                QueryContent_JobType(fs3, resultElement_JobType, 3); 
                fileOp.AddText(fs3, ",");                
                jsonwriter.WriteJson_yAxis(fs3, "CodeCoverage", "#ff6600", "square");
                QueryContent_JobType(fs3,  resultElement_JobType, 4);
                fileOp.AddText(fs3, "]}");
            }
        }
       
        private static void QueryContent(FileStream fs, List<Result> resultElement, List<Result> resultElement2, int k)
        {

            FileOperation fileOp = new FileOperation();
            DateOperation dateOp = new DateOperation(); 

            DateTime StartDate = new DateTime(2013, 12, 11);
            DateTime EndDate = DateTime.Now.AddDays(-1);
            var db_ECS = new AAT_DTF_ecsEntities2();
              foreach (DateTime date in dateOp.GetDateRange(StartDate, EndDate))
                {
                    var jobs_DTF = (from dtf in resultElement
                                select dtf.JobID).ToList();
                    var jobs_ECS = (from ecs in resultElement2
                               select ecs.JobID).ToList();
                  
                  int userInDTF5 = (from esc in resultElement2
                        where
                         jobs_DTF.Contains(esc.JobID)                                 //   ecs.JobID belongs to custom job 
                         && esc.CreateDate < date.AddDays(1)
                         && esc.CreateDate > date
                        select esc.UserName).Distinct().Count(); 
                     
                    int userInDTF4 = (from dtf in resultElement
                                      where !jobs_ECS.Contains(dtf.JobID)             //   dtf.JobID != ecs.JobID                                        
                                      && dtf.CreateDate < date.AddDays(1)
                                         && dtf.CreateDate > date
                                      select dtf.UserName).Distinct().Count(); 
                  
                  switch (k)
                    {
                        case 1:
                            fileOp.AddText(fs, "\"");
                            fileOp.AddText(fs, date.ToShortDateString().ToString());
                            fileOp.AddText(fs, "\",");
                            break;
                        case 2:
                            fileOp.AddText(fs, userInDTF4.ToString());
                            fileOp.AddText(fs, ",");
                            break;
                        case 3:
                            fileOp.AddText(fs, userInDTF5.ToString());
                            fileOp.AddText(fs, ",");
                            break;
                        
                    }
              }
              fileOp.AddText(fs, "]}"); 

        }

        private static void QueryContent_JobType(FileStream fs, List<Result> resultElement, int k)
        {

            FileOperation fileOp = new FileOperation();
            DateOperation dateOp = new DateOperation();
            DateTime startTime = new DateTime(2013,1,1);
            
            do
            {
                int cusotomCount = (from item in resultElement
                                    where item.JobType == "Custom"
                                    && item.MFGT_CreateDate > startTime
                                    && item.MFGT_CreateDate < startTime.AddMonths(1)
                                    select item.UserName).Distinct().Count();

                int officialCount = (from item in resultElement
                                     where item.JobType == "Official"
                                     && item.MFGT_CreateDate > startTime
                                     && item.MFGT_CreateDate < startTime.AddMonths(1)
                                     select item.UserName).Distinct().Count();
                int icCount = (from item in resultElement
                               where item.JobType == "CodeCoverage"
                               && item.MFGT_CreateDate > startTime
                               && item.MFGT_CreateDate < startTime.AddMonths(1)
                               select item.UserName).Distinct().Count();
                switch (k)
                {
                    case 1:
                        fileOp.AddText(fs, "\"");
                        fileOp.AddText(fs, startTime.Month.ToString());
                        fileOp.AddText(fs, "\",");
                        break;
                    case 2: //custom
                        fileOp.AddText(fs, cusotomCount.ToString());
                        fileOp.AddText(fs, ",");
                        break;
                    case 3: //official
                        fileOp.AddText(fs, officialCount.ToString());
                        fileOp.AddText(fs, ",");
                        break;
                    case 4: //codecoverage
                        fileOp.AddText(fs, icCount.ToString());
                        fileOp.AddText(fs, ",");
                        break;
                        
                }
                startTime = startTime.AddMonths(1);
            } while (startTime <= DateTime.Now);

          fileOp.AddText(fs, "]}");

        }





    }
    }

