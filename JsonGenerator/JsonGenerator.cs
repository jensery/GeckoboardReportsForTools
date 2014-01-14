using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace JsonGenerator
{
    public class FileOperation
    {

        // Delect exist file
        public void DelectExist(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        
        //Add text to filestream
        public void AddText(FileStream fs, string value)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

    }

    public class DateOperation
    {
        //get Date list 
        public List<DateTime> GetDateRange(DateTime StarteDate, DateTime EndDate)
        {
            if (StarteDate > EndDate)
            {
                return null;
            }
            List<DateTime> rv = new List<DateTime>();
            DateTime tmpDate = StarteDate;
            do  
            {
                if (tmpDate.DayOfWeek != DayOfWeek.Saturday && tmpDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    rv.Add(tmpDate);
                }
                tmpDate = tmpDate.AddDays(1);
            } while (tmpDate <= EndDate) ;
            return rv;
        }

        //get week number
        public int GetWeekNumber(DateTime dtPassed)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(dtPassed, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }
        
    }

    public class JsonWriter
        {
        FileOperation fileOp = new FileOperation();  

        public void WriteJson_xAxis(FileStream fs, string type, string subtitle,string title_xAsis )
            {
                string head = "{chart: {renderTo: 'container',    type: '"
                   + type
                      + "'},\n title: { text: null}, \n subtitle: {   text: '"
                      +subtitle
                      +"' },\n";
                string xAxis = "xAxis: { \n labels: { rotation: -45, align: 'right',}, \n minTickInterval:4,\n title: { text: '"
                  + title_xAsis
                  + "'  }, categories: [";

                
                fileOp.AddText(fs, head);
                fileOp.AddText(fs, xAxis);

            }
        public void WriteJson_yAxis_head(FileStream fs, string title, string color)
            {
                string yAxis = ", yAxis: {  title: { text: '"
                + title
                + "'  }	\n	 },\n   tooltip: {\n"
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
                        + "   lineColor: '"
                        +color
                        +"',\n"
                         + "  enableMouseTracking: true,\n"
                        + "   marker:{enabled:false,\n"
                         + "      states:{\n"
                         + "      hover:{enabled:true}\n"
                          + "     }},\n"
                     + "  }\n"
                 + "  },\n"
                + "   series: [";

                fileOp.AddText(fs, yAxis);

            }
        public void WriteJson_yAxis(FileStream fs, string name, string color, string symbol)
            {
                string yAxis = "{\n"
                + " name: '"
                + name
                + "',\n"
                + "linecolor: '"
                + color
                + "',\n"
                 + " marker: {\n"
                  + "   symbol: '"
                + symbol
                + "'\n"
                   + "},\n"
                  + " data: [\n";
                fileOp.AddText(fs, yAxis);
            }


        }



    }



