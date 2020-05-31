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

![basic MathML rendering](https://user-images.githubusercontent.com/7447159/83210237-86f24200-a184-11ea-9d10-ba407a2915ef.PNG)
 

## Dependencies Projects

[Typography](https://github.com/LayoutFarm/Typography)

## Reference Projects
Thanks these projects for inspire me about layout concept.

MIT, 2020, [CSharpMath](https://github.com/verybadcat/CSharpMath)

MIT, 2020, [Wpf-Math](https://github.com/ForNeVeR/wpf-math)
