using EasyCSharp;

namespace EasyCSharpTester
{
    public partial class Class1
    {
        [Property(
            SetVisibility = Visibility.Protected,
            OnChanged = nameof(Proc)
        )]
        int _TestProperty2;

        [AutoNotifyProperty(GetVisibility = Visibility.Private)]
        bool _Test2;

        public void Proc()
        {
            TestProperty2 = 1;
            //Test2.
        }
    }
}