![PixelFarm's Typography](https://github.com/LayoutFarm/Typography/blob/master/screenshots/title2.png)
===========

Pure C# TrueType/OpenType/OpenFont Reader, Glyph Layout and Rendering.

 * 1.Loads .ttf files, with OpenFontReader (Big thanks go to NOpenType ,https://github.com/vidstige/NRasterizer)
 
 * 2.Rasterizes char to bitmap with pure software renderer + Agg(anti grain geometry) Quality! with 
      our PixelFarm's MiniAgg :) (https://github.com/PaintLab/PixelFarm)
	  
 * .Net >=2.0 

 
License
-----------

Apache2, 2016-2017, WinterDev

Apache2, 2014-2016, Samuel Carlsson, from https://github.com/vidstige/NRasterizer

MIT, 2015, Michael Popoloski, from https://github.com/MikePopoloski/SharpFont

The FreeType Project LICENSE (3-clauses BSD),2003-2016, David Turner, Robert Wilhelm, and Werner Lemberg. from https://www.freetype.org/

BSD, 2009-2010, Poly2Tri Contributors, from https://github.com/PaintLab/poly2tri-cs

BSD, 2002-2005, Maxim Shemanarev (http://www.antigrain.com)Anti-Grain Geometry - Version 2.4,

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
GSUB: ligature feature

![ligature1](https://cloud.githubusercontent.com/assets/7447159/23093970/f7f879a8-f622-11e6-8539-8cdbcf1026d7.png)





