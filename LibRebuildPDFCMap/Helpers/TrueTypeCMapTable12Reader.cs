using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static iTextSharp.text.pdf.PdfSigGenericPKCS;

namespace LibRebuildPDFCMap.Helpers
{
    internal class TrueTypeCMapTable12Reader
    {
        public CMapGrouping[] CMapGroupings { get; }

        public class Tab
        {
            public uint Tag { get; set; }
            public string TagName { get; set; }
            public long Offset { get; set; }
            public int Length { get; set; }

            public override string ToString() => $"{TagName} {Offset} {Length}";
        }

        public class CMapTab
        {
            public ushort PlatformID { get; set; }
            public ushort EncodingID { get; set; }
            public int Offset { get; set; }
        }

        public class CMapGrouping
        {
            public uint StartCharCode { get; set; }
            public uint EndCharCode { get; set; }
            public uint StartGlyphID { get; set; }
        }

        public TrueTypeCMapTable12Reader(Stream stream, Action<string> logger)
        {
            var big = new BigBinaryReader(stream);
            var little = new BinaryReader(stream);

            var magic = big.ReadUInt32();
            if (magic != 65536)
            {
                throw new InvalidDataException($"True type magic number {magic} has to be 65536");
            }

            Tab[] tabs;

            {
                var numTabs = big.ReadUInt16();
                big.ReadUInt16();
                big.ReadUInt16();
                big.ReadUInt16();

                tabs = Enumerable.Range(0, numTabs)
                    .Select(
                        _ =>
                        {
                            var tag = little.ReadUInt32();
                            big.ReadUInt32();
                            var offset = big.ReadInt32();
                            var length = big.ReadInt32();

                            return new Tab
                            {
                                Tag = tag,
                                TagName = Encoding.ASCII.GetString(BitConverter.GetBytes(tag)),
                                Offset = offset,
                                Length = length,
                            };
                        }
                    )
                    .ToArray();
            }

            var cmapOffset = tabs.Single(it => it.TagName == "cmap").Offset;
            stream.Position = cmapOffset;

            CMapTab[] cmapTabs;

            {
                var version = big.ReadUInt16();
                if (version != 0)
                {
                    throw new InvalidDataException($"cmap table version {version} has to be 0");
                }
                var numTabs = big.ReadUInt16();

                cmapTabs = Enumerable.Range(0, numTabs)
                    .Select(
                        _ =>
                        {
                            var platformId = big.ReadUInt16();
                            var encodingId = big.ReadUInt16();
                            var offset = big.ReadInt32();

                            return new CMapTab
                            {
                                PlatformID = platformId,
                                EncodingID = encodingId,
                                Offset = offset,
                            };
                        }
                    )
                    .ToArray();
            }

            var cmapTab = cmapTabs.SingleOrDefault(it => it.PlatformID == 3 && it.EncodingID == 10);
            if (cmapTab == null)
            {
                throw new InvalidDataException($"cmap table PlatformID 3 EncodingID 10 is not found");
            }

            stream.Position = cmapOffset + cmapTab.Offset;

            CMapGrouping[] cmapGroupings;

            {
                var format = big.ReadUInt16();
                if (format != 12)
                {
                    throw new InvalidDataException();
                }
                big.ReadUInt16();
                var length = big.ReadInt32();
                var lang = big.ReadInt32();
                var nGroups = big.ReadInt32();

                cmapGroupings = Enumerable.Range(0, nGroups)
                    .Select(
                        _ =>
                        {
                            var startCharCode = big.ReadUInt32();
                            var endCharCode = big.ReadUInt32();
                            var startGlyphID = big.ReadUInt32();

                            return new CMapGrouping
                            {
                                StartCharCode = startCharCode,
                                EndCharCode = endCharCode,
                                StartGlyphID = startGlyphID,
                            };
                        }
                    )
                    .ToArray();
            }

            CMapGroupings = cmapGroupings;
        }
    }
}
