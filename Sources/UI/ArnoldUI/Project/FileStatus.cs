using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Project
{
    public class FileStatus
    {
        public string FileName { get; set; }
        public bool IsSaveNeeded { get; set; }

        public bool IsFileOpen => FileName != null;
    }

    public class FileStatusChangedArgs : EventArgs
    {
        public FileStatus FileStatus { get; }

        public FileStatusChangedArgs(FileStatus fileStatus)
        {
            FileStatus = fileStatus;
        }
    }

}
