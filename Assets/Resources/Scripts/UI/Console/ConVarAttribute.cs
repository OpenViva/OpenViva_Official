using System;

namespace Viva.console
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ConVarAttribute : Attribute
    {
        public string Command { get; }
        public string Description { get; }
        public string[] Arguments { get; }

        public ConVarAttribute(string command, string description = "", params string[] arguments)
        {
            Command = command;
            Description = description;
            Arguments = arguments;
        }
    }
}