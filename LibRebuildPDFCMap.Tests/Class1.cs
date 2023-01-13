using LibRebuildPDFCMap.Helpers;
using NUnit.Framework;

namespace LibRebuildPDFCMap.Tests
{
    public class Class1
    {
        private string AtSamples(string relative) => Path.Combine(
            TestContext.CurrentContext.WorkDirectory, "..", "..", "..", "samples", relative
        );

        [Test]
        public void ToUnicodeGeneratorTest()
        {
            using (var fs = File.OpenRead(AtSamples(@"a.ttf")))
            {
                var reader = new TrueTypeCMapTable12Reader(fs, Console.Error.WriteLine);
                var generator = new ToUnicodeGenerator(reader.CMapGroupings);
                Console.WriteLine();
                Console.WriteLine(generator.Content);
            }
        }

        [Test]
        [TestCase("a.pdf", "aa.pdf")]
        [TestCase(@"X:\seiko\edge.pdf", @"X:\seiko\edge~.pdf")]
        [TestCase(@"X:\seiko\cube.pdf", @"X:\seiko\cube~.pdf")]
        public void RebuilderTest(string inputPath, string outputPath)
        {
            Rebuilder.Rebuild(
                AtSamples(inputPath),
                AtSamples(outputPath),
                new RebuilderOptions
                {
                    ForceRebuildCMap = true,
                    Logger = Console.Error.WriteLine,
                }
            );
        }
    }
}