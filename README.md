![Typography, this image was rendered with this library, in subpixel rendering mode](https://user-images.githubusercontent.com/7447159/31848163-cc9e00fe-b655-11e7-8a40-69258e440c7a.png)
===========

Pure C# Font Reader, Glyph Layout and Rendering.
---

While developing the [PixelFarm Rendering library](https://github.com/PaintLab/PixelFarm),

I figured that the way to render a glyph from a font may be useful for other libraries.

So, I spinned off the way to render a glyph from a font to here, the **Typography** library.



![typography_thanamas](https://user-images.githubusercontent.com/7447159/44314099-d4357180-a43e-11e8-95c3-56894bfea1e4.png)

_Sov_Thanamas font from https://www.f0nt.com/release/sov_thanamas/_
 
---

Cross Platform
---
The Typography library is **cross-platforms library** and does **NOT** need the PixelFarm Rendering library.

You can use the library to reads font files( .ttf, .otf, .ttc, .otc, .woff, .woff2) and

1) Access all information inside the font. 
2) Layout the font glyphs according to the OpenFont specification.

_The core library does **NOT** provide a glyph rendering implementation_. 
But when you are able to read/access all glyphs and you are able to know
exact position of each glyph=> It is easy to you to render it by your own.

I take some screen snapshots (below) of some projects that use Typography to read each glyph
from a font file then render it by their rendering engine.

![sum2](https://user-images.githubusercontent.com/7447159/78152244-bc6bff00-7463-11ea-847f-138e4ee3c7ff.png) 

_1. [MatterHackers](https://github.com/MatterHackers/MatterControl)/[agg-sharp](https://github.com/MatterHackers/agg-sharp), 2. [CShapMath/Skia-Xamarin Form](https://github.com/verybadcat/CSharpMath), 3. [emoji.wpf/wpf](https://github.com/samhocevar/emoji.wpf),
4. [zwcloud's ImGui/GL,GLES](https://github.com/zwcloud/ImGui)_

---
PixelFarm's Typography
---

Since the core library does not provide glyph rendering implementation, You can learn
how to do it from the example repositories above, or You may learn it from my 
implementation => **PixelFarm.Typography**.

PixelFarm.Typography links the core Typography library to the _PixelFarm_ Rendering library.
You can see , for example, How to implement string drawing, how to implement text-layout services, how to cache glyph shapes. So you can apply this with your own library.


![sum3](https://user-images.githubusercontent.com/7447159/78159669-10c7ac80-746d-11ea-9f22-4aee4d7f3807.png)

_HtmlRenderer on GLES2 surface, text are rendered with the PixelFarm.Typography_


What are each project used for?
-----------
See => https://github.com/LayoutFarm/Typography/issues/99

License
-----------

The project is based on multiple open-sourced projects (listed below) **all using permissive licenses**.

A license for a whole project is [**MIT**](https://opensource.org/licenses/MIT).

But if you use some part of the codebase,
please check each source file's header for the licensing info if available.

 
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

**Unpack, Zlib,Brotli**

MIT, 2018, SharpZipLib, https://github.com/icsharpcode/SharpZipLib 

MIT, 2009, 2010, 2013-2016 by the Brotli Authors., https://github.com/google/brotli

MIT, 2017, brezza92 (C# port from original code, by hand), https://github.com/brezza92/brotli

MIT, 2019, master131, https://github.com/master131/BrotliSharpLib

**Demo**

MIT, 2017, Zou Wei, https://github.com/zwcloud, see more Zou Wei's GUI works at [here](https://zwcloud.net/#project/imgui) and [here](https://github.com/zwcloud/ImGui)
