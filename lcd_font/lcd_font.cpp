//-----------------------------------------------------------------------
// Copyright (C) 2002-2004 Maxim Shemanarev (http://www.antigrain.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//-----------------------------------------------------------------------

#include "stdafx.h"
#include "resource.h"
#include <math.h>

#define MAX_LOADSTRING 100

// Global Variables:
HINSTANCE hInst;                                // current instance
TCHAR szTitle[MAX_LOADSTRING];                  // The title bar text
TCHAR szWindowClass[MAX_LOADSTRING];            // The title bar text

// Foward declarations of functions included in this code module:
ATOM                MyRegisterClass(HINSTANCE hInstance);
BOOL                InitInstance(HINSTANCE, int);
LRESULT CALLBACK    WndProc(HWND, UINT, WPARAM, LPARAM);
LRESULT CALLBACK    About(HWND, UINT, WPARAM, LPARAM);

int APIENTRY WinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPSTR     lpCmdLine,
                     int       nCmdShow)
{
    // TODO: Place code here.
    MSG msg;
    HACCEL hAccelTable;

    // Initialize global strings
    LoadString(hInstance, IDS_APP_TITLE, szTitle, MAX_LOADSTRING);
    LoadString(hInstance, IDC_LCD_FONT, szWindowClass, MAX_LOADSTRING);
    MyRegisterClass(hInstance);

    // Perform application initialization:
    if (!InitInstance (hInstance, nCmdShow)) 
    {
        return FALSE;
    }

    hAccelTable = LoadAccelerators(hInstance, (LPCTSTR)IDC_LCD_FONT);

    // Main message loop:
    while (GetMessage(&msg, NULL, 0, 0)) 
    {
        if (!TranslateAccelerator(msg.hwnd, hAccelTable, &msg)) 
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
    }

    return msg.wParam;
}



//
//  FUNCTION: MyRegisterClass()
//
//  PURPOSE: Registers the window class.
//
//  COMMENTS:
//
//    This function and its usage is only necessary if you want this code
//    to be compatible with Win32 systems prior to the 'RegisterClassEx'
//    function that was added to Windows 95. It is important to call this function
//    so that the application will get 'well formed' small icons associated
//    with it.
//
ATOM MyRegisterClass(HINSTANCE hInstance)
{
    WNDCLASSEX wcex;

    wcex.cbSize = sizeof(WNDCLASSEX); 

    wcex.style          = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc    = (WNDPROC)WndProc;
    wcex.cbClsExtra     = 0;
    wcex.cbWndExtra     = 0;
    wcex.hInstance      = hInstance;
    wcex.hIcon          = LoadIcon(hInstance, (LPCTSTR)IDI_LCD_FONT);
    wcex.hCursor        = LoadCursor(NULL, IDC_ARROW);
    wcex.hbrBackground  = (HBRUSH)(COLOR_WINDOW+1);
    wcex.lpszMenuName   = (LPCSTR)IDC_LCD_FONT;
    wcex.lpszClassName  = szWindowClass;
    wcex.hIconSm        = LoadIcon(wcex.hInstance, (LPCTSTR)IDI_SMALL);

    return RegisterClassEx(&wcex);
}

//
//   FUNCTION: InitInstance(HANDLE, int)
//
//   PURPOSE: Saves instance handle and creates main window
//
//   COMMENTS:
//
//        In this function, we save the instance handle in a global variable and
//        create and display the main program window.
//
BOOL InitInstance(HINSTANCE hInstance, int nCmdShow)
{
   HWND hWnd;

   hInst = hInstance; // Store instance handle in our global variable

   hWnd = CreateWindow(szWindowClass, 
                       szTitle, 
                       WS_OVERLAPPEDWINDOW,
                       CW_USEDEFAULT, 
                       CW_USEDEFAULT, 
                       800, 
                       200, 
                       NULL, 
                       NULL, 
                       hInstance, 
                       NULL);

   if (!hWnd)
   {
      return FALSE;
   }

   ShowWindow(hWnd, nCmdShow);
   UpdateWindow(hWnd);

   return TRUE;
}



//---------------------------------
// A simple helper class to create font, select object
// and properly destroy it.
class lcd_font
{
public:
    ~lcd_font()
    {
        ::SelectObject(m_hdc, m_old_font);
        ::DeleteObject(m_font);
    }

    lcd_font(HDC hdc, const char* typeface, int height, bool bold=false, bool italic=false) :
        m_hdc(hdc),
        m_font(::CreateFont(height,               // height of font
                            0,                    // average character width
                            0,                    // angle of escapement
                            0,                    // base-line orientation angle
                            bold ? 700 : 400,     // font weight
                            italic,               // italic attribute option
                            FALSE,                // underline attribute option
                            FALSE,                // strikeout attribute option
                            ANSI_CHARSET,         // character set identifier
                            OUT_DEFAULT_PRECIS,   // output precision
                            CLIP_DEFAULT_PRECIS,  // clipping precision
                            ANTIALIASED_QUALITY,  // output quality
                            FF_DONTCARE,          // pitch and family
                            typeface)),           // typeface name
        m_old_font(::SelectObject(m_hdc, m_font))
    {
    }

private:
    HDC     m_hdc;
    HFONT   m_font;
    HGDIOBJ m_old_font;
};


// Possible formats for GetGlyphOutline() and corresponding 
// numbers of levels of gray.
//---------------------------------
struct ggo_gray2 { enum { num_levels = 5,  format = GGO_GRAY2_BITMAP }; };
struct ggo_gray4 { enum { num_levels = 17, format = GGO_GRAY4_BITMAP }; };
struct ggo_gray8 { enum { num_levels = 65, format = GGO_GRAY8_BITMAP }; };


// Sub-pixel energy distribution lookup table.
// See description by Steve Gibson: http://grc.com/cttech.htm
// The class automatically normalizes the coefficients
// in such a way that primary + 2*secondary + 3*tertiary = 1.0
// Also, the input values are in range of 0...NumLevels, output ones
// are 0...255
//---------------------------------
template<class GgoFormat> class lcd_distribution_lut
{
public:
    lcd_distribution_lut(double prim, double second, double tert)
    {
        double norm = (255.0 / (GgoFormat::num_levels - 1)) / (prim + second*2 + tert*2);
        prim   *= norm;
        second *= norm;
        tert   *= norm;
        for(unsigned i = 0; i < GgoFormat::num_levels; i++)
        {
            m_primary[i]   = (unsigned char)floor(prim   * i);
            m_secondary[i] = (unsigned char)floor(second * i);
            m_tertiary[i]  = (unsigned char)floor(tert   * i);
        }
    }

    unsigned primary(unsigned v)   const { return m_primary[v];   }
    unsigned secondary(unsigned v) const { return m_secondary[v]; }
    unsigned tertiary(unsigned v)  const { return m_tertiary[v];  }

    static unsigned ggo_format()
    {
        return GgoFormat::format;
    }

private:
    unsigned char m_primary[GgoFormat::num_levels];
    unsigned char m_secondary[GgoFormat::num_levels];
    unsigned char m_tertiary[GgoFormat::num_levels];
};



// This function prepares the alpha-channel information 
// for the glyph averaging the values in accordance with 
// the method suggested by Steve Gibson. The function
// extends the width by 4 extra pixels, 2 at the beginning 
// and 2 at the end. Also, it doesn't align the new width 
// to 4 bytes, that is, the output gm.gmBlackBoxX is the 
// actual width of the array.
//---------------------------------
template<class LutType>
void prepare_lcd_glyph(const LutType& lut, 
                       const unsigned char* gbuf1, 
                       const GLYPHMETRICS& gm, 
                       unsigned char* gbuf2, 
                       GLYPHMETRICS* gm2)
{
    unsigned src_stride = (gm.gmBlackBoxX + 3) / 4 * 4;
    unsigned dst_width  = src_stride + 4;
    memset(gbuf2, 0, dst_width * gm.gmBlackBoxY);

    for(unsigned y = 0; y < gm.gmBlackBoxY; ++y)
    {
        const unsigned char* src_ptr = gbuf1 + src_stride * y;
        unsigned char* dst_ptr = gbuf2 + dst_width * y;
        unsigned x;
        for(x = 0; x < gm.gmBlackBoxX; ++x)
        {
            unsigned v = *src_ptr++;
            dst_ptr[0] += lut.tertiary(v);
            dst_ptr[1] += lut.secondary(v);
            dst_ptr[2] += lut.primary(v);
            dst_ptr[3] += lut.secondary(v);
            dst_ptr[4] += lut.tertiary(v);
            ++dst_ptr;
        }
    }
    gm2->gmBlackBoxX = dst_width;
}


// Color struct
//---------------------------------
struct rgba
{
    rgba() : r(0), g(0), b(0), a(255) {}
    rgba(unsigned char r_, unsigned char g_, unsigned char b_, unsigned char a_=255) : 
        r(r_), g(g_), b(b_), a(a_) {}
    unsigned char r,g,b,a;
};



// Blend one span into the R-G-B 24 bit frame buffer
// For the B-G-R byte order or for 32-bit buffers modify
// this function accordingly. The general idea is 'span' 
// contains alpha values for individual color channels in the 
// R-G-B order, so, for the B-G-R order you will have to 
// choose values from the 'span' array differently
//---------------------------------
void blend_lcd_span(int x, 
                    int y, 
                    const unsigned char* span, 
                    int width, 
                    const rgba& color, 
                    unsigned char* rgb24_buf, 
                    unsigned rgb24_stride)
{
    unsigned char* p = rgb24_buf + rgb24_stride * y + x;
    unsigned char rgb[3] = { color.r, color.g, color.b };
    int i = x % 3;
    do
    {
        int a0 = int(*span++) * color.a;
		auto existingColor = *p;
        //*p++ = (unsigned char)((((rgb[i++] - *p) * a0) + (*p << 16)) >> 16);
		*p++ = (unsigned char)((((rgb[i++] - existingColor) * a0) + (existingColor << 16)) >> 16);
		 
        if(i > 2) i = 0;
    }
    while(--width);
}



// Blend one rectangular glyph
//---------------------------------
void blend_lcd_glyph(const unsigned char* gbuf, 
                     int x, 
                     int y, 
                     const rgba& color,
                     const GLYPHMETRICS& gm, 
                     unsigned char* rgb24_buf, 
                     unsigned rgb24_stride)
{

    for(unsigned i = 0; i < gm.gmBlackBoxY; i++)
    {
        blend_lcd_span(x + gm.gmptGlyphOrigin.x, 
                       y + gm.gmptGlyphOrigin.y - i, 
                       gbuf + gm.gmBlackBoxX * i, 
                       gm.gmBlackBoxX, 
                       color, 
                       rgb24_buf, 
                       rgb24_stride);
    }
}



// Draw a text string in the frame buffer
//---------------------------------
template<class LutType, class CharT>
void draw_lcd_text(HDC hdc, 
                   const LutType& lut,
                   int x, 
                   int y, 
                   const rgba& color, 
                   const CharT* str, 
                   unsigned char* rgb24, 
                   unsigned stride)
{
    // Create an affine matrix with 3x horizontal scaling.
    // 3x means that we interpret each pixel in the resulting glyph
    // in such a way that it corresponds to the sublixel 
    // (red, green, or blue), but not to the whole pixel.
    //----------------------------------
    MAT2 scale3h;
    memset(&scale3h, 0, sizeof(MAT2));
    scale3h.eM11.value = 3;
    scale3h.eM22.value = 1;

    // Allocate buffers for glyphs
    // In reality use some smarter strategy to detect
    // the size of the buffer
    unsigned gbuf_size = 16*1024; 
    unsigned char* gbuf1 = new unsigned char [gbuf_size];
    unsigned char* gbuf2 = new unsigned char [gbuf_size];

    while(*str)
    {
        GLYPHMETRICS gm;
        int total_size = GetGlyphOutline(hdc,
                                         *str,
                                         lut.ggo_format(),
                                         &gm,
                                         gbuf_size,
                                         (void*)gbuf1,
                                         &scale3h);
        if(total_size >= 0)
        {
            prepare_lcd_glyph(lut, gbuf1, gm, gbuf2, &gm);
            blend_lcd_glyph(gbuf2, x, y, color, gm, rgb24, stride);
        }
        else
        {
            // GetGlyphOutline() fails when being called for
            // GGO_GRAY8_BITMAP and white space (stupid Microsoft).
            // It doesn't even initialize the glyph metrics
            // structure. So, we have to query the metrics
            // separately (basically we need gmCellIncX).
            total_size = GetGlyphOutline(hdc,
                                         *str,
                                         GGO_METRICS,
                                         &gm,
                                         gbuf_size,
                                         (void*)gbuf1,
                                         &scale3h);
        }

        x += gm.gmCellIncX;
        ++str;
    }
    delete [] gbuf2;
    delete [] gbuf1;
}



// Swap Blue and Red, that is convert RGB->BGR or BGR->RGB
//---------------------------------
void swap_rb(unsigned char* buf, unsigned width, unsigned height, unsigned stride)
{
    unsigned x, y;
    for(y = 0; y < height; ++y)
    {
        unsigned char* p = buf + stride * y;
        for(x = 0; x < width; ++x)
        {
            unsigned char v = p[0];
            p[0] = p[2];
            p[2] = v;
            p += 3;
        }
    }
}





//
//  FUNCTION: WndProc(HWND, unsigned, WORD, LONG)
//
//  PURPOSE:  Processes messages for the main window.
//
//  WM_COMMAND  - process the application menu
//  WM_PAINT    - Paint the main window
//  WM_DESTROY  - post a quit message and return
//
//
LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    int wmId, wmEvent;
    PAINTSTRUCT ps;
    HDC hdc;
    TCHAR szHello[MAX_LOADSTRING];
    LoadString(hInst, IDS_HELLO, szHello, MAX_LOADSTRING);

    switch (message) 
    {
        case WM_COMMAND:
            wmId    = LOWORD(wParam); 
            wmEvent = HIWORD(wParam); 
            // Parse the menu selections:
            switch (wmId)
            {
                case IDM_ABOUT:
                   DialogBox(hInst, (LPCTSTR)IDD_ABOUTBOX, hWnd, (DLGPROC)About);
                   break;
                case IDM_EXIT:
                   DestroyWindow(hWnd);
                   break;
                default:
                   return DefWindowProc(hWnd, message, wParam, lParam);
            }
            break;
        case WM_PAINT:
            {
                hdc = BeginPaint(hWnd, &ps);
                RECT rt;
                GetClientRect(hWnd, &rt);

                int width = rt.right - rt.left;
                int height = rt.bottom - rt.top;

                //Create compatible DC and a bitmap to render the image 
                //--------------------------------------
                BITMAPINFO bmp_info; 
                bmp_info.bmiHeader.biSize = sizeof(BITMAPINFOHEADER); 
                bmp_info.bmiHeader.biWidth = width; 
                bmp_info.bmiHeader.biHeight = height; 
                bmp_info.bmiHeader.biPlanes = 1; 
                bmp_info.bmiHeader.biBitCount = 24;
                bmp_info.bmiHeader.biCompression = BI_RGB; 
                bmp_info.bmiHeader.biSizeImage = 0; 
                bmp_info.bmiHeader.biXPelsPerMeter = 0; 
                bmp_info.bmiHeader.biYPelsPerMeter = 0; 
                bmp_info.bmiHeader.biClrUsed = 0; 
                bmp_info.bmiHeader.biClrImportant = 0; 

                HDC mem_dc = ::CreateCompatibleDC(hdc); 

                void* buf = 0; 

                HBITMAP bmp = ::CreateDIBSection( 
                    mem_dc, 
                    &bmp_info, 
                    DIB_RGB_COLORS, 
                    &buf, 
                    0, 
                    0 
                ); 

                HBITMAP temp = (HBITMAP)::SelectObject(mem_dc, bmp);

                // Calculate image stride and size 
                //---------------------------------
                unsigned char* rgb24  = (unsigned char*)buf;
                unsigned rgb24_stride = (width * 3 + 3) / 4 * 4;
                unsigned rgb24_size   = height * rgb24_stride;

                // Clear the image
                //---------------------------------
                memset(rgb24, 255, rgb24_size);


                // Create the energy distribution lookup table.
                // See description by Steve Gibson: http://grc.com/cttech.htm
                // The class automatically normalizes the coefficients
                // in such a way that primary + 2*secondary + 3*tertiary = 1.0
                // Also, the input values are in range of 0...64, output ones
                // are 0...255.
                // 
                // Try to play with different coefficients for the primary,
                // secondary, and tertiary distribution weights.
                // Steve Gibson recommends 1/3, 2/9, and 1/9, but it produces 
                // too blur edges. It's better to increase the weight of the 
                // primary and secondary pixel, then the text looks much crisper 
                // with inconsiderably increased "color fringing".
                //---------------------------------
                //lcd_distribution_lut<ggo_gray8> lut(1.0/3.0, 2.0/9.0, 1.0/9.0);
                lcd_distribution_lut<ggo_gray8> lut(0.5, 0.25, 0.125);


                // Use a separate block to make sure the font will be created, 
                // used with current DC and destroyed correctly (we need to 
                // SelectObject(prev_font) before destroying)
                //---------------------------------
                {
                    // Draw text
                    //---------------------------------
                    lcd_font fnt(hdc, "Arial", -20, false, true);
                    draw_lcd_text(hdc, 
                                  lut, 
                                  50 * 3,    // X-positioning is also sub-pixel!
                                  100, 
                                  rgba(0,30,40, 230),
                                  "Hello World! Welcome to the perfectly LCD-optimized text rendering!", 
                                  rgb24, rgb24_stride);
                }


                {
                    // Draw "Copyright"
                    //---------------------------------
                    lcd_font fnt(hdc, "Arial", -12, false, false);
                    draw_lcd_text(hdc, lut, 
                                  120 * 3,  // X-positioning is also sub-pixel!
                                  80, 
                                  rgba(50,20,0, 220),
                                  L"\xA9 Maxim Shemanarev http://antigrain.com", 
                                  rgb24, rgb24_stride);
                }


                {
                    // Draw the big "o"
                    //---------------------------------
                    lcd_font fnt(hdc, "Arial", -100, false, false);
                    draw_lcd_text(hdc, lut, 
                                  50 * 3,    // X-positioning is also sub-pixel!
                                  10, 
                                  rgba(0,0,0),
                                  "O", 
                                  rgb24, rgb24_stride);
                }


                // The drawing method assumes the R-G-B byte order,
                // so that we have to change it to the native Windows one 
                // (B-G-R) to obtain the correct result.
                //-------------------------------------------------
                swap_rb(rgb24, width, height, rgb24_stride);


                // Display the image. If the image is B-G-R-A (32-bits per pixel)
                // one can use AlphaBlend instead of BitBlt. In case of AlphaBlend
                // one also should clear the image with zero alpha, i.e. rgba8(0,0,0,0)
                //-------------------------------------------------
                ::BitBlt(
                  hdc,  
                  rt.left,      
                  rt.top,      
                  width,  
                  height, 
                  mem_dc,
                  0,
                  0,     
                  SRCCOPY
                );

                // Free resources 
                ::SelectObject(mem_dc, temp); 
                ::DeleteObject(bmp); 
                ::DeleteObject(mem_dc);

                EndPaint(hWnd, &ps);
            }
            break;

        case WM_ERASEBKGND:
            break;

        case WM_DESTROY:
            PostQuitMessage(0);
            break;
        default:
            return DefWindowProc(hWnd, message, wParam, lParam);
   }
   return 0;
}

// Mesage handler for about box.
LRESULT CALLBACK About(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
        case WM_INITDIALOG:
                return TRUE;

        case WM_COMMAND:
            if (LOWORD(wParam) == IDOK || LOWORD(wParam) == IDCANCEL) 
            {
                EndDialog(hDlg, LOWORD(wParam));
                return TRUE;
            }
            break;
    }
    return FALSE;
}
