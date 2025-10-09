# Usage

All test classes are part of a collection from xunit framework perspective. There are 3 possibilities here:

1. Test class is not decorated with any collection-related attribute<br><br>
When a test class is not decorated with `[Collection]` attribute - a collection will be created on the fly, based on [CollectionBehavior](https://api.xunit.net/v3/3.0.0/Xunit.CollectionBehavior.html), defined in [CollectionBehaviorAttribute](https://api.xunit.net/v3/3.0.0/Xunit.CollectionBehaviorAttribute.html) on an assembly level.
By default - `CollectionPerClass` is used, meaning all such not-decorated classes will have their own collection. Otherwise all these classes are going into one single collection.<br><br>
2. Test class has `[Collection("collection name")]` attribute defined, but there's no corresponding collection definition with the same name<br><br>
Think of it as a collection with empty collection definition and a given name<br><br>
3. Test class has `[Collection("collection name")]` attribute defined and there is corresponding collection definition with the same name (or directly `[Collection(typeof(myCollectionDefinition))]`)<br><br>
User-defined collection definition class. Can have fixtures if needed.<br><br>

Let's first break down scenario three and walk backwards:

To order collections - apply `[DependsOnCollections]` attribute to a collection definition. Pass in any types to the attribute's constructor that you want to depend on.

For example:

```csharp
   [CollectionDefinition(Name = nameof(CollectionA))]
   public class CollectionA;

   [CollectionDefinition(Name = nameof(CollectionB))]
   public class CollectionB;

   [CollectionDefinition(Name = nameof(CollectionC))]
   public class CollectionC;
```

if we want `CollectionC` to depend on the other two (meaning all tests that belong to `CollectionC` should run only if all tests from the other two collections were successful) - we can indicate that with

```csharp
   [DependsOnCollections(typeof(CollectionA), typeof(CollectionB))]
   [CollectionDefinition(Name = nameof(CollectionC))]
   public class CollectionC;
```
You can also specify `typeof` of a concrete test class, instead of collection definition. Then at runtime it would be translated into getting the actual collection this test class is part of and taking dependency on that.

Now, if you don't have a collection definition, there are two options to declare dependencies
1. Create a an empty collection definition (like in the above example) and declare dependencies like was described
2. Decorate a test class with `[DependsOnClasses(...)]` attribute, which will lead to source-generated empty collection definition with all the dependencies declared

Ordering collections is achieved by having the following assembly level attribute
```csharp
[assembly: TestCollectionOrderer(typeof(Xunit.v3.IntegrationTesting.DependencyAwareTestCollectionOrderer))]
```
It is added by default via `<UseDependencyAwareTestCollectionOrderer>` msbuild property which is set to true. If you have your own test case orderer - you can set it to false but then the order won't be guaranteed.

Note: since by default collections are executed in parallel - ordering them only makes sense when parallelization is disabled.
This can be done on collection definition level (passing `DisableParallelization = true` to `CollectionDefinition` attribute) or assembly level (passing `DisableTestParallelization = true` to `CollectionBehavior` assembly-level attribute)
