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
#pragma warning disable CS1591 // Polyfill — not part of public API
    [System.AttributeUsage(System.AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class SetsRequiredMembersAttribute : Attribute
    {
    }
#pragma warning restore CS1591
}