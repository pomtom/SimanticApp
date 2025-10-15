using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Semantic.WebApp.Plugins
{
    public class PersonalInfo
    {
        [KernelFunction("get_my_information")]
        [Description("call this when my information is needed including name, address, location or birthdate")]
        [return: Description("returns my name, address, location and birthdate formatted as JSON")]
        public Info GetInfo() => new Info();
    }

    public class Info
    {
        public string Name { get => "Semantic WebApp User"; }
        public DateTime Birthdate { get => new DateTime(1990, 1, 1); }
        public string Address { get => "Dallas, TX"; }
    }
}