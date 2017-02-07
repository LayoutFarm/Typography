![PixelFarm's Typography](https://github.com/LayoutFarm/Typography/blob/master/screenshots/title.png)
===========

Pure C# TrueType and OpenType Font Reader, Glyph Layout and Rendering.

 * 1.Loads .ttf files, with OpenTypeReader (Big thanks go to NOpenType ,https://github.com/vidstige/NRasterizer)
 
 * 2.Rasterizes char to bitmap with pure software renderer + Agg(anti grain geometry) Quality! with 
      our PixelFarm's MiniAgg :) (https://github.com/PaintLab/PixelFarm)
	  
 * .Net >=2.0 

 
License
-----------

Apache2, 2016-2017, WinterDev

Apache2, 2014-2016, Samuel Carlsson, from https://github.com/vidstige/NRasterizer

MIT, 2015, Michael Popoloski, from https://github.com/MikePopoloski/SharpFont

The FreeType Project LICENSE (3-clauses BSD),2003-2016, David Turner, Robert Wilhelm, and Werner Lemberg. from https://www.freetype.org/

BSD, 2009-2010, Poly2Tri Contributors.

BSD, 2002-2005, Maxim Shemanarev (http://www.antigrain.com)Anti-Grain Geometry - Version 2.4,

BSD, 2007-2014, Lars Brubaker, agg-sharp

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

![enable_kerning1](https://cloud.githubusercontent.com/assets/7447159/19415089/7d3ae864-938e-11e6-94b1-4817b327832a.png)

