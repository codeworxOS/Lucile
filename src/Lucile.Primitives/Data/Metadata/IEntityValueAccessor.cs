using System;

namespace Lucile.Data.Metadata
{
    public interface IEntityValueAccessor
    {
        bool IsOfType(object value);

        Type GetPrimaryKeyType();

        object GetPrimaryKey(object entity);

        bool KeyEquals(object leftEntity, object rightEntity);

        Func<object, object> CreateGetValueDelegate(string propertyName);

        Action<object, object> CreateSetValueDelegate(string propertyName);

        Func<object, object, bool> CreatMatchForeignKeyDelegate(INavigationProperty navigationProperty);

        Action<object, object> CreateRemoveItemDelegate(INavigationProperty navigationProperty);

        Action<object, object> CreateAddItemDelegate(INavigationProperty navigationProperty);
    }
}
