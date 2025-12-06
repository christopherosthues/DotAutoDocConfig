using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using DotAutoDocConfig.SourceGenerator.Tests.Utils;
using Microsoft.CodeAnalysis.CSharp;

namespace DotAutoDocConfig.SourceGenerator.Tests;

public class SourceGeneratorWithAdditionalFilesTests
{
    private const string DddRegistryText = @"User
Document
Customer";

    [Test]
    public async Task GenerateClassesBasedOnDDDRegistry()
    {
        // Create an instance of the source generator.
        SourceGeneratorWithAdditionalFiles generator = new();

        // Source generators should be tested using 'GeneratorDriver'.
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Add the additional file separately from the compilation.
        driver = driver.AddAdditionalTexts(
            ImmutableArray.Create<AdditionalText>(
                new TestAdditionalFile("./DDD.UbiquitousLanguageRegistry.txt", DddRegistryText))
        );

        // To run generators, we can use an empty compilation.
        CSharpCompilation compilation = CSharpCompilation.Create(nameof(SourceGeneratorWithAdditionalFilesTests));

        // Run generators. Don't forget to use the new compilation rather than the previous one.
        driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation newCompilation, out _);

        // Retrieve all files in the compilation.
        string[] generatedFiles = newCompilation.SyntaxTrees
            .Select(t => Path.GetFileName(t.FilePath))
            .ToArray();

        // In this case, it is enough to check the file name.
        await Assert.That(generatedFiles)
            .Contains("User.g.cs").And
            .Contains("Document.g.cs").And
            .Contains("Customer.g.cs");
    }
}
