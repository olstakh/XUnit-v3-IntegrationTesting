using System;

namespace Xunit.V3.IntegrationTesting;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class DependsOnAttribute : Attribute
{
    public string[] Dependencies { get; }

    public DependsOnAttribute(params string[] dependencies)
    {
        Dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
    }
}