using System;

namespace EasyCSharp;

class CastFromAttributeBase : Attribute
{

}

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
class CastFromAttribute : CastFromAttributeBase
{
    public CastFromAttribute(Type Type) { }
}

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
class CastFromAttribute<FromType> : CastFromAttributeBase
{
    public CastFromAttribute() { }
}

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
class CastAttribute : Attribute
{
    public CastAttribute() { }
}
