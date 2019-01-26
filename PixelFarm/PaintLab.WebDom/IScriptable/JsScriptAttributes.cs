//Apache2, 2014-present, WinterDev

using System;
namespace LayoutFarm.Scripting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class JsTypeAttribute : Attribute
    {
        public JsTypeAttribute()
        {
        }
        public JsTypeAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; private set; }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class JsMethodAttribute : Attribute
    {
        public JsMethodAttribute()
        {
        }
        public JsMethodAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class JsPropertyAttribute : Attribute
    {
        public JsPropertyAttribute()
        {
        }
        public JsPropertyAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; private set; }
    }


    namespace Internal
    {
        //this for internal used only
        public class JsExtendedMapAttribute : Attribute
        {
            public JsExtendedMapAttribute(int scriptMemberId)
            {
                this.MemberId = scriptMemberId;
            }
            public int MemberId
            {
                get;
                private set;
            }
        }
        public class JsExtendedDataAttribute : Attribute
        {
            public JsExtendedDataAttribute(byte[] data)
            {
                this.Data = data;
            }
            public byte[] Data
            {
                get;
                private set;
            }
        }
    }
}
