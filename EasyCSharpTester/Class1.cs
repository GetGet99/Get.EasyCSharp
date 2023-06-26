using Get.EasyCSharp;

namespace EasyCSharpTester
{
    public partial class Class1
    {
        [OptionalParameter(nameof(Hello), "\"Hello Substitude Parameter\"")]
        [OptionalParameter(nameof(GG), 1 + 1)]
        public void Proc(string Hello, int GG)
        {

        }
        [AutoEventProperty]
        string? test;
    }
}