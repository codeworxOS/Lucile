using System;
using Lucile.Data.Metadata;

namespace Lucile.Core.Test.Metadata
{
    internal class ArticleNameValueAccessor : IEntityValueAccessor
    {
        public Action<object, object> CreateAddItemDelegate(INavigationProperty navigationProperty)
        {
            throw new NotImplementedException();
        }

        public Func<object, object> CreateGetValueDelegate(string propertyName)
        {
            throw new NotImplementedException();
        }

        public Action<object, object> CreateRemoveItemDelegate(INavigationProperty navigationProperty)
        {
            throw new NotImplementedException();
        }

        public Action<object, object> CreateSetValueDelegate(string propertyName)
        {
            throw new NotImplementedException();
        }

        public Func<object, object, bool> CreatMatchForeignKeyDelegate(INavigationProperty navigationProperty)
        {
            throw new NotImplementedException();
        }

        public object GetPrimaryKey(object entity)
        {
            throw new NotImplementedException();
        }

        public Type GetPrimaryKeyType()
        {
            throw new NotImplementedException();
        }

        public bool IsOfType(object value)
        {
            throw new NotImplementedException();
        }

        public bool KeyEquals(object leftEntity, object rightEntity)
        {
            throw new NotImplementedException();
        }
    }
}