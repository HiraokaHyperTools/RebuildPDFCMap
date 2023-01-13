# RebuildPDFCMap

[![Nuget](https://img.shields.io/nuget/v/HiraokaHyperTools.LibRebuildPDFCMap)](https://github.com/HiraokaHyperTools/RebuildPDFCMap)

Try to generate `/ToUnicode` from embedded TrueType fonts.

![pdfxp1](https://user-images.githubusercontent.com/5955540/212269636-14be909d-62b1-4600-bc2d-1b20e565c173.png)

## Target PDFs

This will work for each font having `/Subtype`:

- `/Type0` (`/Subtype /CIDFontType2` in children of `/DescendantFonts`)

And also, for each font having `/Encoding`:

- `/Identity-H`

## LibRebuildPDFCMap

Example:

```cs
  LibRebuildPDFCMap.Rebuilder.Rebuild(
    inputPdfPath,
    outputPdfPath,
    new RebuilderOptions
    {
      ForceRebuildCMap = true,
      Logger = Console.Error.WriteLine,
    }
  );
```

## RebuildPDFCMap command line

```
RebuildPDFCMap.exe rebuild --help

RebuildPDFCMap 1.0.0
Copyright (C) 2023 RebuildPDFCMap

  -f, --force            Force rebuild cmap

  --help                 Display this help screen.

  --version              Display version information.

  Input PDF (pos. 0)     Required. Input PDF

  Output PDF (pos. 1)    Required. Output PDF
```
