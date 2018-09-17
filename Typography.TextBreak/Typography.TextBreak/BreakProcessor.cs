using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Typography.TextBreak
{
    public class BreakAtProcessor : ICollection<BreakAtInfo>
    {
        readonly Action<BreakAtInfo> _action;

        public BreakAtProcessor(Action<BreakAtInfo> action) => _action = action;

        public void Add(BreakAtInfo item) => _action(item);

        bool ICollection<BreakAtInfo>.IsReadOnly =>
            throw new NotImplementedException();
        int ICollection<BreakAtInfo>.Count =>
            throw new NotImplementedException();
        void ICollection<BreakAtInfo>.Clear() =>
            throw new NotImplementedException();
        bool ICollection<BreakAtInfo>.Contains(BreakAtInfo item) =>
            throw new NotImplementedException();
        void ICollection<BreakAtInfo>.CopyTo(BreakAtInfo[] array, int arrayIndex) =>
            throw new NotImplementedException();
        IEnumerator<BreakAtInfo> IEnumerable<BreakAtInfo>.GetEnumerator() =>
            throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() =>
            throw new NotImplementedException();
        bool ICollection<BreakAtInfo>.Remove(BreakAtInfo item) =>
            throw new NotImplementedException();
    }
    public class BreakSpanProcessor : ICollection<BreakAtInfo>
    {
        BreakAtInfo lastBreakAt = BreakAtInfo.Empty;
        readonly Action<BreakSpan> _action;

        public BreakSpanProcessor(Action<BreakSpan> action) => _action = action;

        public void Add(BreakAtInfo item)
        {
            _action(new BreakSpan
            {
                startAt = lastBreakAt.breakAt,
                len = (ushort)(item.breakAt - lastBreakAt.breakAt),
                wordKind = item.wordKind
            });
            lastBreakAt = item;
        }

        bool ICollection<BreakAtInfo>.IsReadOnly =>
            throw new NotImplementedException();
        int ICollection<BreakAtInfo>.Count =>
            throw new NotImplementedException();
        void ICollection<BreakAtInfo>.Clear() =>
            throw new NotImplementedException();
        bool ICollection<BreakAtInfo>.Contains(BreakAtInfo item) =>
            throw new NotImplementedException();
        void ICollection<BreakAtInfo>.CopyTo(BreakAtInfo[] array, int arrayIndex) =>
            throw new NotImplementedException();
        IEnumerator<BreakAtInfo> IEnumerable<BreakAtInfo>.GetEnumerator() =>
            throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() =>
            throw new NotImplementedException();
        bool ICollection<BreakAtInfo>.Remove(BreakAtInfo item) =>
            throw new NotImplementedException();
    }
}
