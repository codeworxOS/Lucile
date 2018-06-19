using System;
using System.Collections.Generic;
using System.Linq;
using Lucile.Dynamic.Test.Dynamic;
using Lucile.Dynamic.Test.Dynamic.Test;
using Lucile.Dynamic.Text.TransactionProxy;
using Xunit;

namespace Lucile.Dynamic.Text
{
    public class TransactionProxyCreatorTest
    {
        [Fact]
        public void TransactionProxyByteArrayPropertySample()
        {
            var target1 = new NonVirtualBaseProperties
            {
                IntProperty = 123,
                StringProperty = "test",
                ByteArrayProperty = new byte[] { 1, 2, 3 }
            };

            var target2 = new NonVirtualBaseProperties
            {
                IntProperty = 123,
                StringProperty = "test",
                ByteArrayProperty = new byte[] { 1, 2, 3 }
            };

            var proxy = TransactionProxyCreator.Current.CreateProxy<NonVirtualBaseProperties>(target1, target2);
            Assert.True(new byte[] { 1, 2, 3 }.SequenceEqual(proxy.ByteArrayProperty));
            Assert.False((bool)proxy.GetType().GetProperty("ByteArrayProperty_HasMultipleValues").GetValue(proxy));

            target2.ByteArrayProperty = new byte[] { 4, 5, 6 };
            proxy = TransactionProxyCreator.Current.CreateProxy<NonVirtualBaseProperties>(target1, target2);
            Assert.Null(proxy.ByteArrayProperty);
            Assert.True((bool)proxy.GetType().GetProperty("ByteArrayProperty_HasMultipleValues").GetValue(proxy));
        }

        [Fact]
        public void TransactionProxyCreatorCreateSingleProxy()
        {
            var target1 = new TransactionProxyClass
            {
                DecimalProperty = 123,
                StringProperty = "Test"
            };

            var proxy = TransactionProxyCreator.Current.CreateProxy<TransactionProxyClass>(target1);
            var proxy2 = TransactionProxyCreator.Current.CreateProxy<TransactionProxyClass>(target1);
            var proxy3 = TransactionProxyCreator.Current.CreateProxy<TransactionProxyClass>(target1);

            var proxy4 = TransactionProxyCreator.Current.CreateProxy<TestTarget>(new TestTarget());

            Assert.Equal(proxy.GetType(), proxy2.GetType());
            Assert.Equal(proxy2.GetType(), proxy3.GetType());
            Assert.NotEqual(proxy4.GetType(), proxy.GetType());

            Assert.Equal(123, proxy.DecimalProperty);
            Assert.Equal("Test", proxy.StringProperty);
        }

        [Fact]
        public void TransactionProxyCreatorInterfaceBase()
        {
            var target1 = new ColumnDefinition
            {
                AllowGrouping = true,
                FieldName = "bla",
                Visible = true,
                SortIndex = 12
            };

            var target2 = new ColumnDefinition
            {
                AllowGrouping = true,
                FieldName = "bla",
                Visible = true,
                SortIndex = 12
            };

            var proxy = TransactionProxyCreator.Current.CreateProxy<IColumnDefinition>(target1, target2);

            Assert.NotNull(proxy);
        }

        [Fact]
        public void TransactionProxyCreatorTestSample()
        {
            var targets = new List<AvdMask>
            {
                new AvdMask{ Id = 1234, Description = "Asdfasdfa", Name = "asdf" }
            };

            var a = typeof(AvdMask).GetProperty("Id");
            var bla = a.GetMethod;

            var test = TransactionProxyCreator.Current.CreateProxy<AvdMask>(targets);

            Assert.NotNull(test);
        }

        [Fact]
        public void TransactionProxyGetValueEntriesMethod()
        {
            var target1 = new NonVirtualBaseProperties
            {
                IntProperty = 1234,
                StringProperty = "test",
                ByteArrayProperty = new byte[] { 1, 2, 3 }
            };

            var target2 = new NonVirtualBaseProperties
            {
                IntProperty = 123,
                StringProperty = "test",
                ByteArrayProperty = new byte[] { 1, 2, 3 }
            };

            ITransactionProxy proxy = (ITransactionProxy)TransactionProxyCreator.Current.CreateProxy<NonVirtualBaseProperties>(target1, target2);
            ITransactionProxy<NonVirtualBaseProperties> typedProxy = (ITransactionProxy<NonVirtualBaseProperties>)proxy;

            var typedValues = typedProxy.GetValueEntries("IntProperty").ToList();
            var values = proxy.GetValueEntries("IntProperty").ToList();

            Assert.NotNull(values);
            Assert.NotNull(typedValues);

            Assert.Equal(2, values.Count);
            Assert.Equal(2, typedValues.Count);

            typedValues = typedProxy.GetValueEntries("StringProperty").ToList();
            values = proxy.GetValueEntries("StringProperty").ToList();

            Assert.NotNull(values);
            Assert.NotNull(typedValues);

            Assert.Equal(1, values.Count);
            Assert.Equal(1, typedValues.Count);
        }

        [Fact]
        public void TransactionProxyHasMultipleValuesMethod()
        {
            var target1 = new NonVirtualBaseProperties
            {
                IntProperty = 1234,
                StringProperty = "test",
                ByteArrayProperty = new byte[] { 1, 2, 3 }
            };

            var target2 = new NonVirtualBaseProperties
            {
                IntProperty = 123,
                StringProperty = "test",
                ByteArrayProperty = new byte[] { 1, 2, 3 }
            };

            ITransactionProxy proxy = (ITransactionProxy)TransactionProxyCreator.Current.CreateProxy<NonVirtualBaseProperties>(target1, target2);
            Assert.True(proxy.HasMultipleValues("IntProperty"));
            Assert.False(proxy.HasMultipleValues("StringProperty"));
            Assert.False(proxy.HasMultipleValues("ByteArrayProperty"));

            ((dynamic)proxy).IntProperty = 678;

            Assert.False(proxy.HasMultipleValues("IntProperty"));
        }

        [Fact]
        public void TransactionProxyNonExistantPropertyGetValueEntries()
        {
            var target1 = new NonVirtualBaseProperties();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ITransactionProxy proxy = (ITransactionProxy)TransactionProxyCreator.Current.CreateProxy<NonVirtualBaseProperties>(target1);

                var result = proxy.GetValueEntries("WhateverProperty");
            });
        }

        [Fact]
        public void TransactionProxyNonExistantPropertyHasMultipleValues()
        {
            var target1 = new NonVirtualBaseProperties();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ITransactionProxy proxy = (ITransactionProxy)TransactionProxyCreator.Current.CreateProxy<NonVirtualBaseProperties>(target1);

                var result = proxy.HasMultipleValues("WhateverProperty");
            });
        }
    }
}