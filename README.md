![Typography, this img was rendered with this lib, in subpixel rendering mode](https://github.com/LayoutFarm/Typography/blob/master/screenshots/title2.png)
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

 
License
-----------

Apache2, 2016-2017, WinterDev

Apache2, 2014-2016, Samuel Carlsson, Big thanks for https://github.com/vidstige/NRasterizer

MIT, 2015, Michael Popoloski, from https://github.com/MikePopoloski/SharpFont

The FreeType Project LICENSE (3-clauses BSD),2003-2016, David Turner, Robert Wilhelm, and Werner Lemberg. from https://www.freetype.org/

BSD, 2009-2010, Poly2Tri Contributors, from https://github.com/PaintLab/poly2tri-cs

BSD, 2002-2005, Maxim Shemanarev, Anti-Grain Geometry - Version 2.4 from http://www.antigrain.com

BSD, 2007-2014, Lars Brubaker, agg-sharp, from  https://github.com/MatterHackers/agg-sharp 

MIT, 2016, Viktor Chlumsky, from https://github.com/Chlumsky/msdfgen

Screenshots
-----------
Some screenshots of the current master.

![Screenshot](screenshots/3.png "Screenshot 3") 

---

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

![msdfgen2](https://cloud.githubusercontent.com/assets/7447159/23061146/423cd040-f533-11e6-9f1a-a7fc3d60a14a.png)

---
**Advance OpenFont Text Shaping**

**1. GSUB :  ligature feature** 
 
![ligature](https://cloud.githubusercontent.com/assets/7447159/23093970/f7f879a8-f622-11e6-8539-8cdbcf1026d7.png)

_pic 1: show GSUB's  glyph ligature, see f-i_

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





