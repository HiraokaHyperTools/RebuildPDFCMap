using CommandLine;

namespace RebuildPDFCMap
{
    internal class Program
    {
        [Verb("rebuild", HelpText = "Rebuild PDF")]
        private class RebuildOpt
        {
            [Value(0, Required = true, MetaName = "Input PDF", HelpText = "Input PDF")]
            public string? InputPdf { get; set; }

            [Value(1, Required = true, MetaName = "Output PDF", HelpText = "Output PDF")]
            public string? OutputPdf { get; set; }

            [Option('f', "force", HelpText = "Force rebuild cmap")]
            public bool Force { get; set; }
        }

        [Verb("dummy")]
        private class DummyOpt
        {

        }

        static int Main(string[] args) => Parser.Default.ParseArguments<RebuildOpt, DummyOpt>(args)
            .MapResult<RebuildOpt, DummyOpt, int>(
                DoRebuild,
                DoDummy,
                ex => 1
            );

        private static int DoDummy(DummyOpt arg)
        {
            throw new NotImplementedException();
        }

        private static int DoRebuild(RebuildOpt arg)
        {
            LibRebuildPDFCMap.Rebuilder.Rebuild(
                arg.InputPdf,
                arg.OutputPdf,
                new LibRebuildPDFCMap.RebuilderOptions
                {
                    Logger = Console.Error.WriteLine,
                    ForceRebuildCMap = arg.Force,
                }
            );

            return 0;
        }
    }
}