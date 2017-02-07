using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Lucile.Json
{
    public class JsonConverterAttribute : Attribute, IDynamicMetaObjectProvider
    {
        private IDynamicMetaObjectProvider _dynamicWrapper;

        public JsonConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
            _dynamicWrapper = new DynamicObjectWrapper(this);
        }

        public JsonConverterAttribute(Type converterType, params object[] converterParameters)
            : this(converterType)
        {
            ConverterParameters = converterParameters;
        }

        public object[] ConverterParameters { get; private set; }

        public Type ConverterType { get; private set; }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new DelegatingMetaObject(_dynamicWrapper, parameter, BindingRestrictions.GetTypeRestriction(parameter, this.GetType()), this);
        }

        private class DynamicObjectWrapper : DynamicObject
        {
            private readonly JsonConverterAttribute _att;

            public DynamicObjectWrapper(JsonConverterAttribute att)
            {
                _att = att;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return base.GetDynamicMemberNames();
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (binder.Name == "ConverterType")
                {
                    result = _att.ConverterType;
                    return true;
                }
                else if (binder.Name == "ConverterParameters")
                {
                    result = _att.ConverterParameters;
                    return true;
                }

                return base.TryGetMember(binder, out result);
            }
        }
    }
}