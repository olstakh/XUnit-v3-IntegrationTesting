# Xunit.v3.IntegrationTesting Rules

|Id|Category|Description|Severity|Is enabled|Code fix|
|--|--------|-----------|:------:|:--------:|:------:|
|[XIT0001](Rules/XIT0001.md)|Usage|Class-level `TestCaseOrderer` should be `DependencyAwareTestCaseOrderer`|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0002](Rules/XIT0002.md)|Usage|Assembly-level `TestCaseOrderer` should be `DependencyAwareTestCaseOrderer`|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0003](Rules/XIT0003.md)|Usage|Project is missing class-level and assembly-level `TestCaseOrderer` attribute|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0004](Rules/XIT0004.md)|Usage|`FactDependsOn` has a dependency on a test method that doesn't exist|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0006](Rules/XIT0006.md)|Usage|Assembly is missing `TestFramework` attribute|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0007](Rules/XIT0007.md)|Usage|Assembly `TestFramework` attribute does not extend `DependencyAwareFramework`|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0008](Rules/XIT0008.md)|Usage|`[Fact]`/`[Theory]` should be `[FactDependsOn]`/`[TheoryDependsOn]` in collections with dependencies|<span title='Warning'>⚠️</span>|✔️|✔️|
|[XIT0009](Rules/XIT0009.md)|Usage|`DependsOnCollections` applied to a class that is not a collection definition|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0010](Rules/XIT0010.md)|Usage|`CollectionDefinition` with `DependsOnCollections` must have `DisableParallelization = true`|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0011](Rules/XIT0011.md)|Usage|`DependsOnCollections` requires assembly-level `TestCollectionOrderer`|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0012](Rules/XIT0012.md)|Usage|Assembly-level `TestCollectionOrderer` should be `DependencyAwareTestCollectionOrderer`|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0013](Rules/XIT0013.md)|Usage|`DependsOnClasses` dependency type already belongs to a collection|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0014](Rules/XIT0014.md)|Usage|`DependsOnClasses` dependency type is not part of a named collection|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0015](Rules/XIT0015.md)|Usage|Method has multiple `DependsOn` attributes|<span title='Warning'>⚠️</span>|✔️|❌|
|[XIT0016](Rules/XIT0016.md)|Usage|`DependsOn` attribute combined with another `IFactAttribute`|<span title='Warning'>⚠️</span>|✔️|❌|
