using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibRebuildPDFCMap.Helpers
{
    internal static class PdfArrayExtensions
    {
        public static IEnumerable<PdfObject> AsEnumerable(this PdfArray pdfArray)
        {
            for (int x = 0, cx = pdfArray.Length; x < cx; x++)
            {
                yield return pdfArray[x];
            }
        }
    }
}
