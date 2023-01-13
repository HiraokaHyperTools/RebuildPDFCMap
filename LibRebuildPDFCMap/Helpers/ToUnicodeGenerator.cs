using LibRebuildPDFCMap.Properties;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;

namespace LibRebuildPDFCMap.Helpers
{
    internal class ToUnicodeGenerator
    {
        private class Pair
        {
            public string From { get; set; }
            public string To { get; set; }
        }

        public ToUnicodeGenerator(TrueTypeCMapTable12Reader.CMapGrouping[] groupings)
        {
            var items = groupings
            .SelectMany(Expand)
            .ToArray();

            Content = Template.Parse(Resources.ToUnicodeTemplate).Render(
                new
                {
                    Num = items.Length,
                    Items = items,
                }
            );
        }

        private IEnumerable<Pair> Expand(TrueTypeCMapTable12Reader.CMapGrouping grouping)
        {
            var from = grouping.StartGlyphID;
            var to = grouping.StartCharCode;
            while (true)
            {
                yield return new Pair
                {
                    From = from.ToString("X4"),
                    To = to.ToString("X4"),
                };

                if (to == grouping.EndCharCode || 0xFFFFU < to)
                {
                    break;
                }

                ++from;
                ++to;
            }
        }

        public string Content { get; }
    }
}
