using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    internal static class SR
    {
        public const string NotSupported_CannotCallGetHashCodeOnSpan =
            @"GetHashCode() on Span and ReadOnlySpan is not supported.";
        public const string NotSupported_CannotCallEqualsOnSpan =
            @"Equals() on Span and ReadOnlySpan is not supported. Use operator == instead.";
        public const string Argument_DestinationTooShort =
            @"Destination is too short.";
        public const string Argument_InvalidTypeWithPointersNotSupported =
            @"Cannot use type '{0}'. Only value types without pointers or references are supported.";
    }
}
