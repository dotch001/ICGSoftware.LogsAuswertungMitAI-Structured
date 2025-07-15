using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICGSoftware.Library.Logging
{
    internal class ApplicationSettingsClass
    {
        public required string outputFolderForLogs { get; set; }
        public required string logFileName { get; set; }

    }
}
