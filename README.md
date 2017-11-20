![Typography, this img was rendered with this lib, in subpixel rendering mode](https://user-images.githubusercontent.com/7447159/31848163-cc9e00fe-b655-11e7-8a40-69258e440c7a.png)
===========

Pure C# TrueType/OpenType/OpenFont Reader, Glyph Layout and Rendering.
---

During developing the [PixelFarm](https://github.com/PaintLab/PixelFarm),
I think _'How-to-render-a-font-glyph'_ may be useful for other libs.

So, I spin off  _'How-to-render-a-font-glyph'_ part to here,the **Typography**.

The Typography lib dose NOT NEED PixelFarm Rendering lib.

![gdiplus_sample1](https://cloud.githubusercontent.com/assets/7447159/24084514/1969489e-0d1e-11e7-8748-965e9e84693b.png)

_pic 1:  Typography project's Solution Explorer View_

see pic1, I provide the example(1) that uses Typography with WinGdiPlus,

and the example(2) the uses Typography with 'mini' snapshot of PixelFarm Rendering Lib(3). 

Concept
---

 * 1.Loads .ttf files, with OpenFontReader.
 
 * 2.Rasterizes char to bitmap with pure software renderer + Agg(anti grain geometry) Quality! with 
      our PixelFarm's MiniAgg :) (https://github.com/PaintLab/PixelFarm)
	  
 * .Net >=2.0 
 

Screenshots
-----------
Some screenshots of the current master.

 

Enable TrueType Hinting, Tahoma , 8 pts

![enable_truetype_hinting](https://cloud.githubusercontent.com/assets/7447159/21425153/03d4f3c2-c87a-11e6-863e-eb2ba9bc0d61.png)

---
Tahoma, 72 pts (disable TrueType Hinting)

![compare1_tahoma_72pts](https://cloud.githubusercontent.com/assets/7447159/19414301/597e7b82-9372-11e6-81b8-5c8374a7400d.png)

---
Tahoma, 8 pts, (disable TrueType Hinting)

![compare2_tahoma_8pts](https://cloud.githubusercontent.com/assets/7447159/19414345/de616836-9373-11e6-87ac-64076a8d9f1c.png)

---
Tahoma, 11 pts (disable TrueType Hinting)

![compare3_tahoma_11pts](https://cloud.githubusercontent.com/assets/7447159/19414753/bec50254-9381-11e6-8ebb-07b23d84eb90.png)

---
Enable Kerning

![enable_kerning1](https://cloud.githubusercontent.com/assets/7447159/23192688/605f9a9c-f8d7-11e6-9850-92b19fd098bf.png)

---
SubPixel Rendering
 
![lcd_09](https://cloud.githubusercontent.com/assets/7447159/22780526/a0e65712-eef1-11e6-948a-eca8e8158aaa.png)

---
Multi-channel signed distance field (Msdf) Texture (https://github.com/Chlumsky/msdfgen) 

![msdfgen](https://cloud.githubusercontent.com/assets/7447159/22966208/c0c2407c-f393-11e6-8575-250a6939214b.png)

---
Msdf Texture

![msdfgen2](https://cloud.githubusercontent.com/assets/7447159/25565003/7cdde83a-2de9-11e7-9ff8-3740cd37c40a.png)

---
Msdf Texture Sample
![msdf_test_03](https://cloud.githubusercontent.com/assets/7447159/25564423/3686e0dc-2ddd-11e7-97f5-c34ba2d21762.png)
 

---

Android GLES2-based

![gles_android_emu](https://cloud.githubusercontent.com/assets/7447159/24420575/8725debe-141d-11e7-8ff2-0170334fa1f7.png)

_pic 1: GLES2-based android demo, DroidSans.ttf. Each glyph is tesselated to GlyphRun mesh (with C#  Tesselator), and is rendered directly to GLES2 shader._


![gles_android_emu](https://cloud.githubusercontent.com/assets/7447159/24421237/bbd1df9e-141f-11e7-82d7-b22f2e5d9fe0.png)

_pic 2: same technique as pic1, msjh.ttf_, '啊rAbc' , 

please note that baseline of 啊 is not correct

---
**Advance OpenFont Text Shaping**

**1. GSUB :  ligature feature** 
 
![ligature](https://cloud.githubusercontent.com/assets/7447159/23093970/f7f879a8-f622-11e6-8539-8cdbcf1026d7.png)

_pic 1: show GSUB's  glyph ligature, see f-i_

(see more : https://github.com/LayoutFarm/Typography/issues/80#issuecomment-344943446)

---

**2. GPOS**
 
 
![gpos](https://cloud.githubusercontent.com/assets/7447159/23071092/d53c89c2-f55f-11e6-8b6d-a9353345f77c.png)

_pic 2: test with Thai (complex script) glyph that require gpos table_
 
---
**3. GSUB** : ccmp
 


![gsub](https://cloud.githubusercontent.com/assets/7447159/23079342/1efa46c0-f57f-11e6-869e-fc9700037feb.png)

_pic 3: test with Thai glyph (complex script) , shows glyph substitution_

--- 

**4. GSUB -  GPOS** 

![th_glyph](https://cloud.githubusercontent.com/assets/7447159/23125153/f96d8608-f7a2-11e6-921d-d9bb132c179c.png)
 
 ![th_glyph2](https://cloud.githubusercontent.com/assets/7447159/23194740/7b778fd2-f8e2-11e6-9aa1-1d62ad93de06.png)

_pic 4: test with Thai glyph (complex script)_

(see more: https://github.com/LayoutFarm/Typography/issues/82#issue-274483803)

---

**Fun with Emoji !**
--- 

The Emoji and related features are contributed by [@samhocevar](https://github.com/samhocevar)

![typo_symbol](https://user-images.githubusercontent.com/7447159/31386243-515fdace-adf0-11e7-8644-f7c4632d9856.png)

_pic 1: Segoe UI Symbol Normal, on Win7_
 
 
 
![typo_symbol2](https://user-images.githubusercontent.com/7447159/31386781-3a8b85f8-adf2-11e7-8791-c19abe1fee8f.png)

_pic 2: FireFoxEmoji.ttf from https://github.com/mozilla/fxemoji_


![typo_symbol3](https://user-images.githubusercontent.com/245089/31382300-77f4a52c-adb7-11e7-9510-1e07a76f41ab.png)

_pic 3: Segoe UI Emoji Normal, Win 10_


**Advanced Emoji Ligature**

>This is 👩🏾‍👨🏾‍👧🏾‍👶🏾  “Family - Woman: Medium-Dark Skin Tone, Man: Medium-Dark Skin Tone, Girl: Medium-Dark Skin Tone, Baby: Medium-Dark Skin Tone” without `ccmp` ligatures:

>![image](https://user-images.githubusercontent.com/245089/32985721-fb7cef62-ccc0-11e7-8bef-90ee4ae27a04.png)

>And here it is with `ccmp`:
 

![image](https://user-images.githubusercontent.com/7447159/32997655-5a380fbe-cdc5-11e7-87ba-18f51c44e2ef.png)

_pic 4: Say with Emoji?_


(see: https://github.com/LayoutFarm/Typography/issues/18)

(see more sam's work at https://github.com/samhocevar/emoji.wpf)

---


License
-----------
Source code from multiple projects.
I select ONLY PERMISSIVE license.
Here... 

 
**Font** 

Apache2, 2014-2016, Samuel Carlsson, Big thanks for https://github.com/vidstige/NRasterizer

MIT, 2015, Michael Popoloski, https://github.com/MikePopoloski/SharpFont

The FreeType Project LICENSE (3-clauses BSD style),2003-2016, David Turner, Robert Wilhelm, and Werner Lemberg and others, https://www.freetype.org/

MIT, 2016, Viktor Chlumsky, https://github.com/Chlumsky/msdfgen


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

MIT, 2004,2007, Novell Inc., for System.Drawing 

**Demo**

MIT, 2017, Zou Wei, https://github.com/zwcloud, see more Zou Wei's GUI works at ![here](https://zwcloud.net/#project/imgui) and ![here](https://github.com/zwcloud/ImGui)
