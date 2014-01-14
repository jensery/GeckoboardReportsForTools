using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace GeckoboardReport_VEE
{
    class VEEData
    {
        public static void Main()
        {
//VEE Json File         
		    string fileName_VEE = @"C:\Users\guj\Dropbox\VEE.json";
            DelectExist(fileName_VEE); 
            using (FileStream fs = System.IO.File.Create(fileName_VEE, 1024)) 
            {
                WriteJson_xAxis(fs);
                QueryContent(fs, 1);
                WriteJson_yAxis(fs);
                QueryContent(fs, 2);
                AddText(fs, "]}]}");
		    }
//VEE Excel
            string excelName_VEE = @"\\installreport\BO\ToolsCIP\VEE.csv";
            DelectExist(excelName_VEE);
		    using (FileStream fs1 = File.Create(excelName_VEE, 1024)) 
            {
                AddText(fs1, "BuildNumber,LeadTime,#Error,#Image,BuildPost,ImageStart,ImageEnd\r\n");
			    QueryContent(fs1,3);
		    }



          }
         
        private static void DelectExist(string fileName)
        {
            if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
        }
        //Add text to filestream
        private static void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }
        
        //write json file
        private static void WriteJson_xAxis(FileStream fs)
    {
 	            string title ="chart: {renderTo: 'container',    type: 'line' },\n title: { text: null}, \n subtitle: {   text: 'shamsdimg01.VEEDataTest5' },\n";
		        string xAxis =  "xAxis: { \n labels: { rotation: -45,      align: 'right',      }, \n minTickInterval:4,\n title: { text: 'build'  }, categories: [";
               
             
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
              + "     name: 'Build Number',\n"
            + "       color:'#D94600',\n"
             + "      data: [";
            AddText(fs, yAxis);
        }

                
		//query lead time
        

        private static void QueryContent(FileStream fs, int k)
	    {

            DateTime dt = DateTime.Now.AddDays(-30);
            var db = new veedatatest5Entities1();

            var jbs = (from jb in db.jobs
                              select new
                              {
                                 start_time = jb.start_time,
                                 buildname = jb.buildname
                              }).ToList();

            var buildnames = (from jb in jbs
                              where jb.start_time.DateTime > dt 
                              select jb.buildname.Substring(jb.buildname.IndexOf(";") + 1))
            .Distinct().ToList();


            foreach (var buildName in buildnames)
            {
                var postTime = (from post in db.productbuilds
                                where post.build_label == buildName && !post.build_name.EndsWith("srv")
                                select post.createtime);
                DateTime minCreateTime = DateTime.MaxValue;
                foreach (var timeElement in postTime)
                {
                    if (timeElement.Value < minCreateTime)
                        minCreateTime = timeElement.Value;
                }

                var time = (from jb in db.jobs
                            where jb.buildname.EndsWith(buildName)
                            select new { jb.start_time, jb.end_time, jb.current_status_id });

                int i = 0;
                int j = 0;

                DateTime minStartTime = DateTime.MaxValue;
                DateTime maxEndTime = DateTime.MinValue;
                foreach (var element in time)
                {
                    j++;
                    if (element.current_status_id != 6)
                        i++;

                    if (element.start_time.DateTime < minStartTime)
                        minStartTime = element.start_time.DateTime;

                    if (element.current_status_id == 6 && element.end_time.DateTime > maxEndTime)
                        maxEndTime = element.end_time.DateTime;
                }
                //Console.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", buildName, ts.Days*24+ts.Hours+ts.Minutes/60-8,i, j,minStartTime, minCreateTime));    
                TimeSpan ts = maxEndTime - minStartTime;
                int ts1 = ts.Days * 24 + ts.Hours + ts.Minutes / 60 - 8;

                switch (k)
                {
                    case 1:
                        AddText(fs, "\"");
                        AddText(fs, buildName);
                        AddText(fs, "\",");
                        break;
                    case 2:
                        AddText(fs, ts1.ToString());
                        AddText(fs, ",");
                        break;
                    case 3:
                        string csv = string.Format("{0},{1},{2},{3},{4},{5},{6}", buildName, ts1, i, j, minCreateTime,minStartTime, maxEndTime);
                        AddText(fs, csv);
                        AddText(fs, "\r\n");
                        break;
                }
		
		
		
	}
	
	 
	
	}

        
    }

}
