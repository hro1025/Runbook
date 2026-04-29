using Runbook.Models;

namespace Runbook.Interfaces;

// Defines the contract for scanning a folder and returning a list of scripts
public interface IScanner
{
    public List<Script> Scan(string folderPath);
}
