namespace Runbook.Models;

// Represents a script with its name, file path, and type (Bash or CSharp)
public class Script
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    public ScriptType Type { get; set; }
}
