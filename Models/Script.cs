namespace Runbook.Models
{
    public class Script
    {
        public string? Name { get; set; }

        public string? Path { get; set; }

        public ScriptType Type { get; set; }
        public bool Loop { get; set; }
        public bool Background { get; set; }
    }
}
