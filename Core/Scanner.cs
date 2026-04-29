using Runbook.Interfaces;
using Runbook.Models;

namespace Runbook.Core;

// Scans the scripts folder and returns a list of supported scripts
public class Scanner : IScanner
{
    public List<Script> Scan(string folderPath)
    {
        var scripts = new List<Script>();
        _ = Directory.GetFiles(folderPath);

        foreach (var file in Directory.GetFiles(folderPath))
        {
            // Determine script type based on file extension
            var extension = Path.GetExtension(file);
            ScriptType? type = extension switch
            {
                ".sh" => ScriptType.Bash,
                ".csx" => ScriptType.CSharp,
                _ => null,
            };

            // Skip files with unsupported extensions
            if (type is null)
            {
                Console.WriteLine($"Skipping unsupported file: {file}");
                continue;
            }

            // Add the script to the list with its name, path, and type
            scripts.Add(
                new Script
                {
                    Name = Path.GetFileName(file),
                    Path = file,
                    Type = type.Value,
                }
            );
        }

        return scripts;
    }
}
