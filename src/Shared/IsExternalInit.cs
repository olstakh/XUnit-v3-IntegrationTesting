namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit { }
    internal class RequiredMemberAttribute : Attribute { }
    internal class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string name) { }
    }
}

namespace System.Diagnostics.CodeAnalysis
{

    [System.AttributeUsage(System.AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class SetsRequiredMembersAttribute : Attribute
    {
    }
}