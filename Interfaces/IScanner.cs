using Runbook.Models;

namespace Runbook.Interfaces
{
    public interface IScanner
    {
        public List<Script> Scan(string folderPath);
    }
}
