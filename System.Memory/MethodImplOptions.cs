using System;
using System.Collections.Generic;
using System.Text;

internal static class MethodImplOptions
{
    public const System.Runtime.CompilerServices.MethodImplOptions AggressiveInlining =
        (System.Runtime.CompilerServices.MethodImplOptions)256;
    public const System.Runtime.CompilerServices.MethodImplOptions NoInlining =
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining;
}