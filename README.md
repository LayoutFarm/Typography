![Typography, this image was rendered with this library, in subpixel rendering mode](https://user-images.githubusercontent.com/7447159/31848163-cc9e00fe-b655-11e7-8a40-69258e440c7a.png)
===========

Pure C# Font Reader, Glyph Layout and Rendering.
---


While developing the [PixelFarm Rendering library](https://github.com/PaintLab/PixelFarm),
I figured that the way to render a glyph from a font may be useful for other libraries.

So, I spinned off the way to render a glyph from a font to here, the **Typography** library.

The Typography library does NOT need the PixelFarm Rendering library.

![gdiplus_sample1](https://cloud.githubusercontent.com/assets/7447159/24084514/1969489e-0d1e-11e7-8748-965e9e84693b.png)

_Typography project's Solution Explorer View_

As shown in the above screenshot, an example (marked with `(1)`) that uses Typography with WinGdiPlus is provided,

and an example (marked with `(2)`) the uses Typography with a 'mini' snapshot of PixelFarm Rendering library (marked with `(3)`). 

 

Concept
---

 * 1.Load .ttf, .otf, .ttc, .otc, .woff, .woff2 files, with OpenFontReader.
 
 * 2.Rasterize a character to a bitmap with a pure software renderer which has Agg(anti grain geometry) Quality! with 
      our PixelFarm's MiniAgg :) (https://github.com/PaintLab/PixelFarm)
	  
 * Supported platforms: .Net Framework >= 2.0 or .Net Standard >= 1.3

[Showcase](Docs/Showcase.md)
-----------

What are each project used for?
-----------
See => https://github.com/LayoutFarm/Typography/issues/99

License
-----------

[**MIT**](https://opensource.org/licenses/MIT).

But if you use some part of the codebase,
please check each source file's header for the licensing info if available.

Credits
-----------

The project is based on multiple open-sourced projects (listed below) **all using permissive licenses**.

**Font** 

Apache2, 2014-2016, Samuel Carlsson, Big thanks for https://github.com/vidstige/NRasterizer

MIT, 2015, Michael Popoloski, https://github.com/MikePopoloski/SharpFont

The FreeType Project LICENSE (3-clauses BSD style),2003-2016, David Turner, Robert Wilhelm, and Werner Lemberg and others, https://www.freetype.org/

MIT, 2016, Viktor Chlumsky, https://github.com/Chlumsky/msdfgen

Apache2, 2018, Apache/PDFBox Authors,  https://github.com/apache/pdfbox

**Geometry**

BSD, 2002-2005, Maxim Shemanarev, Anti-Grain Geometry - Version 2.4 http://www.antigrain.com

BSD, 2007-2014, Lars Brubaker, agg-sharp, https://github.com/MatterHackers/agg-sharp 

MIT, 2016, Viktor Chlumsky, https://github.com/Chlumsky/msdfgen

BSD, 2009-2010, Poly2Tri Contributors, https://github.com/PaintLab/poly2tri-cs

Apache2, 2016-2017, WinterDev, https://github.com/PaintLab/PixelFarm

**Platforms**

MIT, 2015-2015, Xamarin, Inc., https://github.com/mono/SkiaSharp

MIT, 2006-2009,  Stefanos Apostolopoulos and other Open Tool Kit Contributors, https://github.com/opentk/opentk

MIT, 2013, Antonie Blom, https://github.com/andykorth/Pencil.Gaming

MIT, 2004, 2007, Novell Inc., for System.Drawing 

**Unpack, Zlib, Brotli**

MIT, 2018, SharpZipLib, https://github.com/icsharpcode/SharpZipLib 

MIT, 2009, 2010, 2013-2016 by the Brotli Authors., https://github.com/google/brotli

MIT, 2017, brezza92 (C# port from original code, by hand), https://github.com/brezza92/brotli

MIT, 2019, master131, https://github.com/master131/BrotliSharpLib

**Demo**

MIT, 2017, Zou Wei, https://github.com/zwcloud, see more Zou Wei's GUI works at [here](https://zwcloud.net/#project/imgui) and [here](https://github.com/zwcloud/ImGui)
