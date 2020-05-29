# MathLayout

## MathML Layout Engine
This C# project does develop for rendering simple Mathematical Markup Language ([MathML3](https://www.w3.org/TR/MathML3))

### Example

    <html>
    <body>
    <math>
        <mrow>
            <mi>x</mi>
            <mo>=</mo>
            <mfrac>
                <mrow>
                    <mrow>
                        <mo>-</mo>
                        <mi>b</mi>
                    </mrow>
                    <mo>&#xB1;<!--PLUS-MINUS SIGN--></mo>
                    <msqrt>
                        <mrow>
                            <msup>
                                <mi>b</mi>
                                <mn>2</mn>
                            </msup>
                            <mo>-</mo>
                            <mrow>
                                <mn>4</mn>
                                <mo>&#x2062;<!--INVISIBLE TIMES--></mo>
                                <mi>a</mi>
                                <mo>&#x2062;<!--INVISIBLE TIMES--></mo>
                                <mi>c</mi>
                            </mrow>
                        </mrow>
                    </msqrt>
                </mrow>
                <mrow>
                    <mn>2</mn>
                    <mo>&#x2062;<!--INVISIBLE TIMES--></mo>
                    <mi>a</mi>
                </mrow>
            </mfrac>
        </mrow>
    </math>
    </body>
    </html>
    
Basic MathML Rendering

![basic MathML rendering](./Example_Pictures/Sample1.png)

## Dependencies Projects
[PixelFarm](https://github.com/PaintLab/PixelFarm)

[Typography](https://github.com/LayoutFarm/Typography)

## Reference Projects
Thanks these projects for inspire me about layout concept.

[CSharpMath](https://github.com/verybadcat/CSharpMath)

[Wpf-Math](https://github.com/ForNeVeR/wpf-math)
