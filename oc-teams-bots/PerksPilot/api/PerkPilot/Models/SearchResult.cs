using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerkPilot.Models
{
    public class SearchResult
    {
        public string Content { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string FileExtension { get; set; }
        public string ContentType { get; set; }
        public string HighlightedText { get; set; }
        public string OpenAISummary { get; set; }

    }
}
