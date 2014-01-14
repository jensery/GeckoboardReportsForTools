using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace GeckobardReport_DTF
{
    class Result
    {
        public DateTime? StartTime {get;set;}
        public DateTime? FinishTime {get; set;}
        public string TotalCases { get; set; }
    }

    class DTFData
    {
        static void Main()
        {

            //DTF Json File
            string fileName_DTF = @"C:\Users\guj\Dropbox\DTF.json";
            DelectExist(fileName_DTF);
            using (FileStream fs = System.IO.File.Create(fileName_DTF, 1024))
            {
                WriteJson_xAxis(fs);
                QueryContent_DTF(fs, 1);
                WriteJson_yAxis(fs);
                QueryContent_DTF(fs, 2);
                AddText(fs, "]}]}");
            }
            //DTF Excel 
            string excelName_DTF = @"\\installreport\BO\ToolsCIP\DTF.csv";
            DelectExist(excelName_DTF);
            using (FileStream fs1 = File.Create(excelName_DTF, 1024))
            {
                AddText(fs1, "JobID,LeadTime,ImageStart,ImageEnd,#Cases\r\n");
                QueryContent_DTF(fs1, 3);
            }
            
        }
        private static void QueryContent_DTF(FileStream fs, int k)
        {

            DateTime dt = DateTime.Now.Subtract(TimeSpan.FromDays(30)); 
            List<string> jobIDs = new List<string>();
            //while (true)
            //{
                try
                {

                    using (var db = new aat_dtfEntities())
                    {
                        jobIDs = (from jb in db.jobs
                                  where jb.StartTime > dt && jb.Status == "complete" & (jb.Submitter == "fosterc" || jb.Submitter == "gaomi")
                                  select jb).OrderBy(x => x.StartTime).Select(j => j.JobID).Distinct().ToList();
                    }

                    //break;

                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(5000);
                }
            //}



            List<Result> time = new List<Result>();
                
            foreach (var jobID in jobIDs)
            {
                //while (true)
                //{
                    try
                    {
                        using (var db2 = new aat_dtfEntities())
                      {
                            time = (from Jb in db2.jobs
                                        where Jb.JobID == jobID
                                        select new Result{StartTime = Jb.StartTime, FinishTime = Jb.FinishTime, TotalCases = Jb.TotalCases })
                                    .ToList();
                     }

                    //    break;
                    }
                    catch (Exception e)
                    {
                        System.Threading.Thread.Sleep(5000);
                    }
                //}


                DateTime minStartTime = DateTime.MaxValue;
                DateTime maxEndTime = DateTime.MinValue;
                int casesCount = 0;
                foreach (var element in time)
                {
                    casesCount = Int32.Parse(element.TotalCases.Substring(element.TotalCases.IndexOf("/") + 1)) + casesCount;
                    if (element.StartTime < minStartTime)
                        minStartTime = element.StartTime.Value;

                    if (element.FinishTime > maxEndTime)
                        maxEndTime = element.FinishTime.Value;
                }


                TimeSpan ts = maxEndTime - minStartTime;
                int ts1 = ts.Days * 24 + ts.Hours + ts.Minutes / 60;


                switch (k)
                {
                    case 1:
                        AddText(fs, "\"");
                        AddText(fs, jobID.Substring(0, jobID.LastIndexOf("@")));
                        AddText(fs, "\",");
                        break;
                    case 2:
                        AddText(fs, ts1.ToString());
                        AddText(fs, ",");
                        break;
                    case 3:
                        string csv = string.Format("{0},{1},{2},{3},{4}", jobID, ts1, minStartTime, maxEndTime, casesCount);
                        AddText(fs, csv);
                        AddText(fs, "\r\n");
                        break;
                }
            }

           
        }

        private static void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }
        private static void DelectExist(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        private static void WriteJson_xAxis(FileStream fs)
        {
            string title = "chart: {renderTo: 'container',    type: 'line' },\n title: { text: null}, \n subtitle: {   text: 'Oresqlserver01.AAT.DTF' },\n";
            string xAxis = "xAxis: { \n labels: { rotation: -45, size: 10, align: 'right',      }, \n minTickInterval:4,\n title: { text: 'build'  }, categories: [";


            AddText(fs, "{");
            AddText(fs, title);
            AddText(fs, xAxis);

        }
        private static void WriteJson_yAxis(FileStream fs)
        {
            string yAxis = "]}, yAxis: {  title: { text: 'LeadTime(hrs)'  }	\n	 },\n   tooltip: {\n"
               + "enabled: true, \n"
                  + "formatter: function() {\n"
                       + "return '<b>'+ this.series.name +'</b><br/>'+\n"
                        + "   this.x +': '+ this.y;\n"
                   + "}\n"
              + "},\n"
               + "plotOptions: {\n"
                   + "line: {\n"
                      + " dataLabels: {\n"
                          + " enabled: false,\n"
                     + "  },"
                    + "   lineColor: '#D94600',\n"
                     + "  enableMouseTracking: true,\n"
                    + "   marker:{enabled:false,\n"
                     + "      states:{\n"
                     + "      hover:{enabled:true}\n"
                      + "     }},\n"
                 + "  }\n"
             + "  },\n"
            + "   series: [{\n"
              + "     name: 'JobID',\n"
            + "       color:'#D94600',\n"
             + "      data: [";
            AddText(fs, yAxis);
        }

    }
}
