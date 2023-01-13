using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using LibRebuildPDFCMap.Helpers;
using System;

namespace LibRebuildPDFCMap
{
    public class Rebuilder
    {
        public static void Rebuild(string inputFile, string outputFile, RebuilderOptions options = null)
        {
            using (var input = File.OpenRead(inputFile))
            using (var output = File.Create(outputFile))
            {
                Rebuild(input, output, options);
            }
        }

        public static void Rebuild(Stream input, Stream output, RebuilderOptions options = null)
        {
            var reader = new PdfReader(input);
            reader.ConsolidateNamedDestinations();

            Document document = null;
            PdfCopy copy = null;

            Action<string> logger = options.Logger ?? NoLogger;

            foreach (var pageNum in Enumerable.Range(1, reader.NumberOfPages))
            {
                logger($"d: Processing page #{pageNum}");

                if (document == null)
                {
                    document = new Document(reader.GetPageSizeWithRotation(pageNum));
                    copy = new PdfCopy(document, output);
                    copy.CloseStream = false;
                    document.Open();
                }

                var rotation = reader.GetPageRotation(pageNum);
                var pageDict = reader.GetPageN(pageNum);
                pageDict.Put(PdfName.ROTATE, new PdfNumber(rotation));

                if (pageDict.GetAsDict(PdfName.RESOURCES) is PdfDictionary resources)
                {
                    logger($"d: /Resources exists");

                    if (resources.GetAsDict(PdfName.FONT) is PdfDictionary font)
                    {
                        logger($"d: /Font exists");

                        var keys = font.Keys.Cast<PdfName>().ToArray();
                        foreach (var key in keys)
                        {
                            logger($"d: Processing font {key}");

                            if (font.GetAsIndirectObject(key) is PdfIndirectReference fontIndir)
                            {
                                if (reader.GetPdfObject(fontIndir.Number) is PdfDictionary oneFont)
                                {
                                    if (oneFont.GetAsName(PdfName.TYPE) == PdfName.FONT)
                                    {
                                        logger($"d: Font /Type is /Font");

                                        logger($"d: Font /Subtype is {oneFont.GetAsName(PdfName.SUBTYPE)}");

                                        if (oneFont.GetAsName(PdfName.ENCODING)?.CompareTo(new PdfName("Identity-H")) == 0)
                                        {
                                            logger($"d: Font /Encoding is Identity-H");

                                            var force = options?.ForceRebuildCMap ?? false;

                                            if (oneFont.Get(PdfName.TOUNICODE) == null)
                                            {
                                                logger($"d: /ToUnicode is not set");

                                                force = true;
                                            }

                                            if (force)
                                            {
                                                var fontDescs = GetFontDescsOf(oneFont, reader);
                                                if (fontDescs.Any())
                                                {
                                                    logger($"d: /FontDescriptor is found");

                                                    var fontDesc = fontDescs.First();

                                                    if (fontDesc.GetAsIndirectObject(PdfName.FONTFILE2) is PdfIndirectReference fontFile2Ref)
                                                    {
                                                        if (reader.GetPdfObject(fontFile2Ref.Number) is PRStream fontFile2)
                                                        {
                                                            logger($"d: Going to load /FontFile2");

                                                            var ttf = PdfReader.GetStreamBytes(fontFile2);

                                                            logger($"d: {ttf.Length} bytes");

                                                            var ttfStream = new MemoryStream(ttf, false);

                                                            TrueTypeCMapTable12Reader ttfReader = null;

                                                            logger($"d: Extracting CMapGroupings from this font");

                                                            try
                                                            {
                                                                ttfReader = new TrueTypeCMapTable12Reader(ttfStream, logger);
                                                                logger($"d: CMapGroupings are extracted from this font");
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                logger($"w: {ex.Message}");
                                                                logger($"d: {ex}");
                                                            }

                                                            if (ttfReader != null)
                                                            {
                                                                var toUnicodeGenerator = new ToUnicodeGenerator(ttfReader.CMapGroupings);

                                                                var toUnicode = new PdfStream(
                                                                    Encoding.ASCII.GetBytes(toUnicodeGenerator.Content)
                                                                );
                                                                var toUnicodeRef = copy.AddToBody(toUnicode);

                                                                logger($"d: Update /ToUnicode of this font");

                                                                oneFont.Put(PdfName.TOUNICODE, toUnicodeRef.IndirectReference);

                                                                logger($"d: Done");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            logger($"d: Font /Encoding is not Identity-H. This kind of font cannot be processed by LibRebuildPDFCMap");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                PdfImportedPage page = copy.GetImportedPage(reader, pageNum);
                copy.AddPage(page);

                if (pageNum == 1)
                {
                    document.AddTitle("" + reader.Info["Title"]);
                    document.AddSubject("" + reader.Info["Subject"]);
                    document.AddKeywords("" + reader.Info["Keywords"]);
                    document.AddAuthor("" + reader.Info["Author"]);
                    document.AddCreator("" + reader.Info["Creator"]);
                }
            }

            logger($"d: Closing PDFs");

            copy.Close();
            document.Close();
        }

        private static PdfDictionary[] GetFontDescsOf(PdfDictionary oneFont, PdfReader reader)
        {
            if (oneFont.GetAsIndirectObject(PdfName.FONTDESCRIPTOR) is PdfIndirectReference fontDescRef)
            {
                if (reader.GetPdfObject(fontDescRef.Number) is PdfDictionary fontDesc)
                {
                    return new PdfDictionary[] { fontDesc };
                }
            }
            else if (oneFont.GetAsArray(PdfName.DESCENDANTFONTS) is PdfArray descendantFonts)
            {
                foreach (var descendantFontRef in descendantFonts.AsEnumerable().Cast<PdfIndirectReference>())
                {
                    if (reader.GetPdfObject(descendantFontRef.Number) is PdfDictionary descendantFont)
                    {
                        return GetFontDescsOf(descendantFont, reader);
                    }
                }
            }
            return new PdfDictionary[0];
        }

        private static void NoLogger(string message) { }
    }
}