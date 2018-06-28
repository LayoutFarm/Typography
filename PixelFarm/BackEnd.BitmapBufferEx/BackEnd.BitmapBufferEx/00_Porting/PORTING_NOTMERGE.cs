//MIT, 2017-present, WinterDev
//example and test for WritableBitmap (https://github.com/teichgraf/WriteableBitmapEx) on Gdi+

using System;
//for .NET 2.0 
namespace System
{
    public delegate R Func<R>();
    public delegate R Func<T, R>(T t1);
    public delegate R Func<T1, T2, R>(T1 t1, T2 t2);
    public delegate R Func<T1, T2, T3, R>(T1 t1, T2 t2, T3 t3);
}
namespace System.Runtime.InteropServices
{
    public partial class TargetedPatchingOptOutAttribute : Attribute
    {
        public TargetedPatchingOptOutAttribute(string msg) { }
    }
}
namespace System.Runtime.CompilerServices
{
    public partial class ExtensionAttribute : Attribute { }
}