using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CsvHelper;
using System.IO;
using System.Collections.Generic;
using System.Formats.Asn1;

namespace BlockOut.Scheduler
{
    public class Creator
    {
        public class SchedulingInfo
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime Date { get; set; }
            public string Status { get; set; }
        }


        public class CsvImporter
        {
            public List<SchedulingInfo> ImportCsv(string filePath)
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<SchedulingInfo>();
                    return new List<SchedulingInfo>(records);
                }
            }
        }


    }
}