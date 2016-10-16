PixelFarm's FontRasterizer
===========

Simple and clean TrueType font renderer written purely in c#.

 * 1.Loads .ttf files, with OpenTypeReader (former NRasterizer,https://github.com/vidstige/NRasterizer)
 
 * 2.Rasterizes char to bitmap with pure software renderer + Agg(anti grain geometry) Quality! with 
      our PixelFarm's MiniAgg :) (https://github.com/LayoutFarm/PixelFarm)
	  
 * .Net >=2.0 
 
License
-----------
Apache2, 2014-2016, Samuel Carlsson, WinterDev

some code are in FreeType License (http://git.savannah.gnu.org/cgit/freetype/freetype2.git/tree/docs/FTL.TXT)

Screenshots
-----------
Some screenshots of the current master.

![Screenshot](screenshots/3.png "Screenshot 3") 

Tahoma, 72 pts
![compare1_tahoma_72pts](https://cloud.githubusercontent.com/assets/7447159/19414301/597e7b82-9372-11e6-81b8-5c8374a7400d.png)

Tahoma, 8 pts
![compare2_tahoma_8pts](https://cloud.githubusercontent.com/assets/7447159/19414345/de616836-9373-11e6-87ac-64076a8d9f1c.png)

Tahoma, 11 pts
![compare3_tahoma_11pts](https://cloud.githubusercontent.com/assets/7447159/19414753/bec50254-9381-11e6-8ebb-07b23d84eb90.png)
