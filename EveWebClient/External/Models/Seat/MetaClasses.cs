using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveWebClient.External.Models.Seat
{
    public class Meta
    {
        public int current_page { get; set; }
        public int from { get; set; }
        public int last_page { get; set; }
        public string path { get; set; }
        public int per_page { get; set; }
        public int to { get; set; }
        public int total { get; set; }
    }

    public class Links
    {
        public string first { get; set; }
        public string last { get; set; }
        public object prev { get; set; }
        public string next { get; set; }
    }
}
