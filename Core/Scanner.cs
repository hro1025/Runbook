using Runbook.Interfaces;
using Runbook.Models;

namespace Runbook.Core;

public class Scanner : IScanner
{
    public List<Script> Scan(string folderPath)
    {
        var scripts = new List<Script>();
        var files = Directory.GetFiles(folderPath);

        foreach (var file in Directory.GetFiles(folderPath))
        {
            var extension = Path.GetExtension(file);
            ScriptType? type = extension switch
            {
                ".sh" => ScriptType.Bash,
                ".csx" => ScriptType.CSharp,
                _ => null,
            };
            if (type is null)
            {
                Console.WriteLine($"Skipping unsupported file: {file}");
                continue;
            }

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
