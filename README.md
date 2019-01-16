![Typography, this image was rendered with this library, in subpixel rendering mode](https://user-images.githubusercontent.com/7447159/31848163-cc9e00fe-b655-11e7-8a40-69258e440c7a.png)
===========

Pure C# TrueType/OpenType/OpenFont Reader, Glyph Layout and Rendering.
---

During developing the [PixelFarm Rendering library](https://github.com/PaintLab/PixelFarm),
I think _'How-to-render-a-font-glyph'_ may be useful for other libraries.

So, I spinned off the _'How-to-render-a-font-glyph'_ part to here, the **Typography** library.

The Typography lib does NOT need the PixelFarm Rendering library.

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
 

What are each project used for?
-----------
See => https://github.com/LayoutFarm/Typography/issues/99


Screenshots
-----------
Some screenshots of the current master.

Enable TrueType Hinting, Tahoma, 8 pts

![enable_truetype_hinting](https://cloud.githubusercontent.com/assets/7447159/21425153/03d4f3c2-c87a-11e6-863e-eb2ba9bc0d61.png)

---
Tahoma, 72 pts (TrueType Hinting: disabled)

![compare1_tahoma_72pts](https://cloud.githubusercontent.com/assets/7447159/19414301/597e7b82-9372-11e6-81b8-5c8374a7400d.png)

---
Tahoma, 8 pts, (TrueType Hinting: disabled)

![compare2_tahoma_8pts](https://cloud.githubusercontent.com/assets/7447159/19414345/de616836-9373-11e6-87ac-64076a8d9f1c.png)

---
Tahoma, 11 pts (TrueType Hinting: disabled)

![compare3_tahoma_11pts](https://cloud.githubusercontent.com/assets/7447159/19414753/bec50254-9381-11e6-8ebb-07b23d84eb90.png)

---
Kerning: Enabled

![enable_kerning1](https://cloud.githubusercontent.com/assets/7447159/23192688/605f9a9c-f8d7-11e6-9850-92b19fd098bf.png)

---
Subpixel Rendering
 
![lcd_09](https://cloud.githubusercontent.com/assets/7447159/22780526/a0e65712-eef1-11e6-948a-eca8e8158aaa.png)

_Tahoma, grey-scale vs lcd-effect subpixel rendering_

![typography_thanamas](https://user-images.githubusercontent.com/7447159/44314099-d4357180-a43e-11e8-95c3-56894bfea1e4.png)

_lcd-effect subpixel rendering, Sov_Tahanamas font from https://www.f0nt.com/release/sov_thanamas/_

---
A Multi-channel signed distance field (Msdf) Texture (https://github.com/Chlumsky/msdfgen) 

![msdfgen](https://cloud.githubusercontent.com/assets/7447159/22966208/c0c2407c-f393-11e6-8575-250a6939214b.png)

---
Another Msdf Texture

![msdfgen2](https://cloud.githubusercontent.com/assets/7447159/25565003/7cdde83a-2de9-11e7-9ff8-3740cd37c40a.png)

---
A Msdf Texture Sample
![msdf_test_03](https://cloud.githubusercontent.com/assets/7447159/25564423/3686e0dc-2ddd-11e7-97f5-c34ba2d21762.png)
 

---

An Android GLES2-based demo

![gles_android_emu](https://cloud.githubusercontent.com/assets/7447159/24420575/8725debe-141d-11e7-8ff2-0170334fa1f7.png)

_pic 1: GLES2-based android demo, DroidSans.ttf. Each glyph is tesselated to GlyphRun mesh (with C#  Tesselator), and is rendered directly to GLES2 shader._


![gles_android_emu](https://cloud.githubusercontent.com/assets/7447159/24421237/bbd1df9e-141f-11e7-82d7-b22f2e5d9fe0.png)

_pic 2: The same technique as pic1, msjh.ttf_, '啊rAbc' , 

Please note that baseline of 啊 is not correct

---
**Advanced OpenFont Text Shaping**

**1. GSUB :  Ligature features** 
 
![ligature](https://cloud.githubusercontent.com/assets/7447159/23093970/f7f879a8-f622-11e6-8539-8cdbcf1026d7.png)

_pic 1: Showing GSUB's glyph ligature, see f-i_

(see more : https://github.com/LayoutFarm/Typography/issues/80#issuecomment-344943446)

---

**2. GPOS**
 
 
![gpos](https://cloud.githubusercontent.com/assets/7447159/23071092/d53c89c2-f55f-11e6-8b6d-a9353345f77c.png)

_pic 2: Testing with Thai (complex script) glyph that require gpos table_
 
---

**3. GSUB** : ccmp

![gsub](https://cloud.githubusercontent.com/assets/7447159/23079342/1efa46c0-f57f-11e6-869e-fc9700037feb.png)

_pic 3: Testing with Thai glyphs (complex script), showing glyph substitution_

--- 

**4. GSUB and GPOS** 

![th_glyph](https://cloud.githubusercontent.com/assets/7447159/23125153/f96d8608-f7a2-11e6-921d-d9bb132c179c.png)
 
![th_glyph2](https://cloud.githubusercontent.com/assets/7447159/23194740/7b778fd2-f8e2-11e6-9aa1-1d62ad93de06.png)

_pic 4: testing with Thai glyph (complex script)_


![th_sarabun](https://user-images.githubusercontent.com/7447159/50389175-61743400-0759-11e9-94c1-00586919a443.png)

_pic 5: complex GSUB-GPOS, Sarabun font from https://github.com/cadsondemak/Sarabun_


(see more: https://github.com/LayoutFarm/Typography/issues/82#issue-274483803)

---

**Fun with Emojis!**
--- 

Emojis and related features were contributed by [@samhocevar](https://github.com/samhocevar).

![typo_symbol](https://user-images.githubusercontent.com/7447159/31386243-515fdace-adf0-11e7-8644-f7c4632d9856.png)

_pic 1: Segoe UI Symbol Normal, on Windows 7_
 
![typo_symbol2](https://user-images.githubusercontent.com/7447159/31386781-3a8b85f8-adf2-11e7-8791-c19abe1fee8f.png)

_pic 2: FireFoxEmoji.ttf from https://github.com/mozilla/fxemoji_

![typo_symbol3](https://user-images.githubusercontent.com/245089/31382300-77f4a52c-adb7-11e7-9510-1e07a76f41ab.png)

_pic 3: Segoe UI Emoji Normal, on Windows 10_


**Advanced Emoji Ligatures**

>This is 👩🏾‍👨🏾‍👧🏾‍👶🏾  “Family - Woman: Medium-Dark Skin Tone, Man: Medium-Dark Skin Tone, Girl: Medium-Dark Skin Tone, Baby: Medium-Dark Skin Tone” without `ccmp` ligatures:

>![image](https://user-images.githubusercontent.com/245089/32985721-fb7cef62-ccc0-11e7-8bef-90ee4ae27a04.png)

>And here it is with `ccmp`:
 

![image](https://user-images.githubusercontent.com/7447159/32997655-5a380fbe-cdc5-11e7-87ba-18f51c44e2ef.png)

_pic 4: Say with Emoji?_


(see: https://github.com/LayoutFarm/Typography/issues/18)

(see more sam's work at https://github.com/samhocevar/emoji.wpf)

---

**CFF Font and MathTable**
--- 

Typography can read [CFF](https://docs.microsoft.com/en-us/typography/opentype/spec/cff) font(.otf) 

In this version, the results are not hinted by CFF instructions, so they are not pixel-perfect.

![latin_long_compare](https://user-images.githubusercontent.com/7447159/39213045-b3b020b4-483a-11e8-9174-9e2a1abda2cf.png)

_pic 1: compare with Win7's NotePad, latin-modern-math-regular.otf, 18 pts_

Further more=> Typography can read all advanced [**MathTable**](https://docs.microsoft.com/en-us/typography/opentype/spec/math). 

See its action here => [CSharpMath](https://github.com/verybadcat/CSharpMath/issues/1#issuecomment-393211266)


---
**Web Open Font Format (WOFF1 and WOFF2)**
--- 
![woff2](https://user-images.githubusercontent.com/7447159/51227867-c867c000-1988-11e9-92df-fb6badfe628f.png)

_Roboto-Regular.woff2, Typography reads and restores woff2_

see https://github.com/LayoutFarm/Typography/issues/27



---
The HtmlRenderer example!
---

This is a snapshot of Html drawboard from (https://github.com/LayoutFarm/HtmlRenderer).
The glyphs are generated/layouted with _Typography_, and rendered with PixelFarm (https://github.com/PaintLab/PixelFarm).

![html_renderer_with_selection2](https://user-images.githubusercontent.com/7447159/49267623-fc952900-f48d-11e8-8ac8-03269c571c2c.png)

_pic 1: HtmlRenderer on GLES2 surface, text are renderered with the Typography_


Also, please note the text selection on the Html Surface. 

---

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
