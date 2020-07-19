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

The Typography library is **a cross-platform library** and does **NOT** need the PixelFarm Rendering library.

You can use the library to read font files (.ttf, .otf, .ttc, .otc, .woff, .woff2) and

1) Access [all information inside the font](Typography.OpenFont/Typeface.cs). 
2) Layout the font glyphs according to the [OpenFont specification](http://www.iso.org/iso/home/store/catalogue_ics/catalogue_detail_ics.htm?csnumber=66391).

_The core modules (Typography.OpenFont, Typography.GlyphLayout) do **NOT** provide a glyph rendering implementation_. 
But as you are able to access and read all glyphs, it is easy to render them provided the exact position of each glyph.

Below are some screenshots of projects that use Typography to read each glyph
from font files and render using their rendering engine.

![sum2](https://user-images.githubusercontent.com/7447159/78152244-bc6bff00-7463-11ea-847f-138e4ee3c7ff.png) 

_1. [MatterHackers](https://github.com/MatterHackers/MatterControl)/[agg-sharp](https://github.com/MatterHackers/agg-sharp), 2. [CSharpMath/SkiaSharp, Xamarin.Forms](https://github.com/verybadcat/CSharpMath), 3. [emoji.wpf/wpf](https://github.com/samhocevar/emoji.wpf),
4. [zwcloud's ImGui/GL,GLES](https://github.com/zwcloud/ImGui)_

---
Project arrangement: The purpose of each project
---

The core modules are Typography.OpenFont and Typography.GlyphLayout.
 
**Typography.OpenFont**

- This project is the core and does not depend on other projects.
- This project contains [a font reader](Typography.OpenFont/OpenFontReader.cs) that can read files implementing Open Font Format
  ([ISO/IEC 14496-22:2015](http://www.iso.org/iso/home/store/catalogue_ics/catalogue_detail_ics.htm?csnumber=66391) and [Microsoft OpenType Specification](https://www.microsoft.com/en-us/Typography/OpenTypeSpecification.aspx))
  or Web Open Font Format (either WOFF [1.0](https://www.w3.org/TR/2012/REC-WOFF-20121213/) or [2.0](https://www.w3.org/TR/WOFF2/))
- The OpenType GSUB, GPOS layout mechanism is in here but a more easy-to-use interface is provided in **Typography.GlyphLayout** below.
- No Visual/Graphics Rendering Here


**Typography.GlyphLayout**

_Since the GlyphLayout engine is not stable and quite complex, 
I separated this from the OpenFont core project._

- This project invokes OpenType Layout Engine/Mechanism (esp. GSUB, GPOS) inside Typography.OpenFont
- The engine converts a string to a list of glyph indexes, then substitutes glyphs
   and places them into proper positions with respect to the provided settings,
      eg [Script/Languague Setup](https://github.com/LayoutFarm/Typography/issues/82), 
           or [Advanced GSUB/GPOS on Emoji](https://github.com/LayoutFarm/Typography/issues/18)
- No Visual/Graphics Rendering Here

![sum4](https://user-images.githubusercontent.com/7447159/78161684-09ee6900-7470-11ea-9649-285c38a19079.png)

_1) CoreModules, 2) Typography.One: a more easy-to-use than core module_

See more detail about the 2 modules and others here : https://github.com/LayoutFarm/Typography/issues/99

---
PixelFarm's Typography
---

Since the core library does not provide a glyph rendering implementation, You can learn
how to do it from the example repositories above, or you may learn it from my 
implementation => [**PixelFarm.Typography**](PixelFarm.Typography).

PixelFarm.Typography links the core Typography library to the _PixelFarm_ Rendering library.
You can learn how to implement string drawing, how to implement text-layout services, and how to cache glyph shapes, so you can apply this to your own library.

![sum3](https://user-images.githubusercontent.com/7447159/78159669-10c7ac80-746d-11ea-9f22-4aee4d7f3807.png)

_HtmlRenderer on GLES2 surface, text are rendered with the PixelFarm.Typography_

-----------
License
-----------

The project is based on multiple open-sourced projects (listed below) **all using permissive licenses**.

A license for a whole project is [**MIT**](https://opensource.org/licenses/MIT).

But if you copy source code directly, please check each source file's header for the licensing info if available.

 
**Font** 

Apache2, 2014-2016, Samuel Carlsson, Big thanks for https://github.com/vidstige/NRasterizer

MIT, 2015, Michael Popoloski, https://github.com/MikePopoloski/SharpFont

The FreeType Project LICENSE (3-clauses BSD style),2003-2016, David Turner, Robert Wilhelm, and Werner Lemberg and others, https://www.freetype.org/

Apache2, 2018, Apache/PDFBox Authors,  https://github.com/apache/pdfbox

Apache2, 2020, Adobe Font Development Kit for OpenType (AFDKO), https://github.com/adobe-type-tools/afdko

**Text Processing**

Unicode (BSD style), 2020, _UNICODE, INC_, https://www.unicode.org/license.html

Apache2, 2014 , Muhammad Tayyab Akram, https://sheenbidi.codeplex.com/ , https://github.com/Tehreer


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

MIT, 2020, brezza92 (https://github.com/brezza92), MathML layout engine
