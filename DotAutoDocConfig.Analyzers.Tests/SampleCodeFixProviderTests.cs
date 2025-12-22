// using System.Threading.Tasks;
// using Verifier =
//     Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<DotAutoDocConfig.Analyzers.SampleSyntaxAnalyzer,
//         DotAutoDocConfig.Analyzers.SampleCodeFixProvider>;
//
// namespace DotAutoDocConfig.Analyzers.Tests;
//
// public class SampleCodeFixProviderTests
// {
//     [Test]
//     public async Task ClassWithMyCompanyTitle_ReplaceWithCommonKeyword()
//     {
//         const string text = @"
// public class MyCompanyClass
// {
// }
// ";
//
//         const string newText = @"
// public class CommonClass
// {
// }
// ";
//
//         var expected = Verifier.Diagnostic()
//             .WithLocation(2, 14)
//             .WithArguments("MyCompanyClass");
//         await Verifier.VerifyCodeFixAsync(text, expected, newText).ConfigureAwait(false);
//     }
// }
