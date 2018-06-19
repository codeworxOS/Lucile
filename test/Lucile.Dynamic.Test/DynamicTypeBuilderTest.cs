using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Lucile;
using Lucile.Dynamic;
using Lucile.Dynamic.Convention;
using Lucile.Dynamic.Interceptor;
using Lucile.Dynamic.Methods;
using Lucile.Dynamic.Test.Dynamic;
using Lucile.Dynamic.Test.Proxy;
using Xunit;

namespace Lucile.Dynamic.Test
{
    public class DynamicTypeBuilderTest
    {
        [Fact]
        public void DynamicTypeBuilderPropertyNewNonNotificationOverride()
        {
            var dtb = new DynamicTypeBuilder<NonVirtualBaseProperties>();
            dtb.AddMember(new DynamicProperty("StringProperty", typeof(string)));
            dtb.AddMember(new DynamicProperty("IntProperty", typeof(int)));
            dtb.AddMember(new DynamicProperty("ByteArrayProperty", typeof(byte[])));
            dtb.AddMember(new DynamicProperty("BlaProperty", typeof(string)));

            dtb.AddConvention(new NotifyPropertyChangedConvention());

            var type = dtb.GeneratedType;
            var instance = Activator.CreateInstance(type);
            Assert.NotNull(instance);

            string changedProperty = null;
            ((INotifyPropertyChanged)instance).PropertyChanged += (s, ea) => changedProperty = ea.PropertyName;

            type.GetProperties().First(p => p.Name == "StringProperty" && p.DeclaringType == type).SetValue(instance, "Bla");
            Assert.Equal("StringProperty", changedProperty);

            changedProperty = null;
            ((NonVirtualBaseProperties)instance).StringProperty = "Bla2";
            Assert.Null(changedProperty);

            changedProperty = null;
            type.GetProperties().First(p => p.Name == "IntProperty" && p.DeclaringType == type).SetValue(instance, 123);
            Assert.Equal("IntProperty", changedProperty);

            changedProperty = null;
            ((NonVirtualBaseProperties)instance).IntProperty = 456;
            Assert.Null(changedProperty);

            changedProperty = null;
            type.GetProperties().First(p => p.Name == "ByteArrayProperty" && p.DeclaringType == type).SetValue(instance, new byte[] { 1, 2, 3 });
            Assert.Equal("ByteArrayProperty", changedProperty);

            changedProperty = null;
            ((NonVirtualBaseProperties)instance).ByteArrayProperty = new byte[] { 4, 5, 6 };
            Assert.Null(changedProperty);

            changedProperty = null;
            type.GetProperty("BlaProperty").SetValue(instance, "asdf");
            Assert.Equal("BlaProperty", changedProperty);
        }

        [Fact]
        public void DynamicTypeBuilderPropertyNewNotificationOverride()
        {
            var dtb = new DynamicTypeBuilder<NonVirtualNotificationBaseProperties>();
            dtb.AddMember(new DynamicProperty("StringProperty", typeof(string)));
            dtb.AddMember(new DynamicProperty("IntProperty", typeof(int)));
            dtb.AddMember(new DynamicProperty("ByteArrayProperty", typeof(byte[])));
            dtb.AddMember(new DynamicProperty("BlaProperty", typeof(string)));

            dtb.AddConvention(new NotifyPropertyChangedConvention());

            var type = dtb.GeneratedType;
            var instance = Activator.CreateInstance(type);
            Assert.NotNull(instance);

            string changedProperty = null;
            ((INotifyPropertyChanged)instance).PropertyChanged += (s, ea) => changedProperty = ea.PropertyName;

            type.GetProperties().First(p => p.Name == "StringProperty" && p.DeclaringType == type).SetValue(instance, "Bla");
            Assert.Equal("StringProperty", changedProperty);

            changedProperty = null;
            ((NonVirtualNotificationBaseProperties)instance).StringProperty = "Bla2";
            Assert.Equal("StringProperty", changedProperty);

            changedProperty = null;
            type.GetProperties().First(p => p.Name == "IntProperty" && p.DeclaringType == type).SetValue(instance, 123);
            Assert.Equal("IntProperty", changedProperty);

            changedProperty = null;
            ((NonVirtualNotificationBaseProperties)instance).IntProperty = 456;
            Assert.Equal("IntProperty", changedProperty);

            changedProperty = null;
            type.GetProperties().First(p => p.Name == "ByteArrayProperty" && p.DeclaringType == type).SetValue(instance, new byte[] { 1, 2, 3 });
            Assert.Equal("ByteArrayProperty", changedProperty);

            changedProperty = null;
            ((NonVirtualNotificationBaseProperties)instance).ByteArrayProperty = new byte[] { 4, 5, 6 };
            Assert.Equal("ByteArrayProperty", changedProperty);

            changedProperty = null;
            type.GetProperties().First(p => p.Name == "BlaProperty" && p.DeclaringType == type).SetValue(instance, "test");
            Assert.Equal("BlaProperty", changedProperty);
        }

        [Fact]
        public void DynamicTypeBuilderSimplePropertyTest()
        {
            var props = new[] { "Key", "Category", "de", "en", "fr", "es", "ru", "zh", "ar" };

            var members = props.Select(p => new DynamicProperty(p, typeof(string)));

            var dtb = new DynamicTypeBuilder(members);
            dtb.AddConvention(new NotifyPropertyChangedConvention());

            var instance = Activator.CreateInstance(dtb.GeneratedType);

            var host = (IDynamicPropertyHost)instance;

            host.SetValue("Key", "123");

            host.SetValue("Category", "Category");
            host.SetValue("de", "loc_de");
            host.SetValue("en", "loc_en");
            host.SetValue("fr", "loc_fr");
            host.SetValue("ru", "loc_ru");
            host.SetValue("es", "loc_es");
            host.SetValue("zh", "loc_zh");
            host.SetValue("ar", "loc_ar");

            Assert.Equal("loc_de", host.GetValue("de"));
            Assert.Equal("loc_en", host.GetValue("en"));
            Assert.Equal("loc_fr", host.GetValue("fr"));
            Assert.Equal("loc_ru", host.GetValue("ru"));
            Assert.Equal("loc_es", host.GetValue("es"));
            Assert.Equal("loc_zh", host.GetValue("zh"));
            Assert.Equal("loc_ar", host.GetValue("ar"));
        }

        [Fact]
        public void DynamicTypeBuilderTransactionalProxyHasMultipleValues()
        {
            var target1 = new TransactionProxyClass
            {
                DecimalProperty = 123,
                IntProperty = 456,
                StringProperty = "Test",
                Targets = new Collection<TestTarget> { new TestTarget() }
            };

            var target2 = new TransactionProxyClass
            {
                DecimalProperty = 123,
                IntProperty = 457,
                StringProperty = "Test",
                Targets = new Collection<TestTarget> { new TestTarget() }
            };

            var target3 = new TransactionProxyClass
            {
                DecimalProperty = 123,
                IntProperty = 458,
                StringProperty = "Test3",
                Targets = new Collection<TestTarget> { new TestTarget() }
            };

            var dtb = new DynamicTypeBuilder<TransactionProxyClass>();
            dtb.AddConvention(new TransactionProxyConvention(typeof(TransactionProxyClass), true));
            if (!dtb.Conventions.OfType<NotifyPropertyChangedConvention>().Any())
            {
                dtb.AddConvention(new NotifyPropertyChangedConvention());
            }

            var proxy = Activator.CreateInstance(dtb.GeneratedType) as TransactionProxyClass;
            ((ITransactionProxy<TransactionProxyClass>)proxy).SetTargets(new[] { target1, target2, target3 });

            var type = proxy.GetType();

            Assert.False((bool)type.GetProperty("DecimalProperty_HasMultipleValues").GetValue(proxy));
            Assert.True((bool)type.GetProperty("IntProperty_HasMultipleValues").GetValue(proxy));
            Assert.True((bool)type.GetProperty("StringProperty_HasMultipleValues").GetValue(proxy));
            Assert.True((bool)type.GetProperty("Targets_HasMultipleValues").GetValue(proxy));
        }

        [Fact]
        public void DynamicTypeBuilderTransactionalProxyInterfaceTest()
        {
            var dtb = new DynamicTypeBuilder<TransactionProxyClass>();
            dtb.AddConvention(new TransactionProxyConvention(typeof(TransactionProxyClass), true));
            if (!dtb.Conventions.OfType<NotifyPropertyChangedConvention>().Any())
            {
                dtb.AddConvention(new NotifyPropertyChangedConvention());
            }

            var proxy = Activator.CreateInstance(dtb.GeneratedType) as TransactionProxyClass;
            Assert.True(proxy is ICommitable);
            Assert.True(proxy is ITransactionProxy);
            Assert.True(proxy is INotifyPropertyChanged);
            Assert.True(proxy is ITransactionProxy<TransactionProxyClass>);
        }

        [Fact]
        public void DynamicTypeBuilderTransactionalProxySetTarget()
        {
            var target1 = new TransactionProxyClass();
            var target2 = new TransactionProxyClass();

            var dtb = new DynamicTypeBuilder<TransactionProxyClass>();
            dtb.AddConvention(new TransactionProxyConvention(typeof(TransactionProxyClass), true));
            if (!dtb.Conventions.OfType<NotifyPropertyChangedConvention>().Any())
            {
                dtb.AddConvention(new NotifyPropertyChangedConvention());
            }

            var proxy = Activator.CreateInstance(dtb.GeneratedType) as ITransactionProxy;

            proxy.SetTargets(new[] { target1 });
            Assert.Single(proxy.Targets);
            Assert.Equal(target1, proxy.Targets.First());

            proxy.SetTargets(new[] { target2 });
            Assert.Single(proxy.Targets);
            Assert.Equal(target2, proxy.Targets.First());
        }

        [Fact]
        public void DynamicTypeBuilderTransactionalProxySetTypedTarget()
        {
            var target1 = new TransactionProxyClass();
            var target2 = new TransactionProxyClass();

            var dtb = new DynamicTypeBuilder<TransactionProxyClass>();
            dtb.AddConvention(new TransactionProxyConvention(typeof(TransactionProxyClass), true));
            if (!dtb.Conventions.OfType<NotifyPropertyChangedConvention>().Any())
            {
                dtb.AddConvention(new NotifyPropertyChangedConvention());
            }

            var proxy = Activator.CreateInstance(dtb.GeneratedType) as ITransactionProxy<TransactionProxyClass>;

            proxy.SetTargets(new[] { target1 });
            Assert.Single(proxy.Targets);
            Assert.Equal(target1, proxy.Targets.First());

            proxy.SetTargets(new[] { target2 });
            Assert.Single(proxy.Targets);
            Assert.Equal(target2, proxy.Targets.First());
        }

        [Fact]
        public void DynamicTypeBuilderTransactionalProxySingleCommit()
        {
            var target1 = new TransactionProxyClass
            {
                DecimalProperty = 123,
                IntProperty = 456,
                StringProperty = "Test",
                Targets = new Collection<TestTarget> { new TestTarget() }
            };

            var dtb = new DynamicTypeBuilder<TransactionProxyClass>();
            dtb.AddConvention(new TransactionProxyConvention(typeof(TransactionProxyClass), true));
            if (!dtb.Conventions.OfType<NotifyPropertyChangedConvention>().Any())
            {
                dtb.AddConvention(new NotifyPropertyChangedConvention());
            }

            var proxy = Activator.CreateInstance(dtb.GeneratedType) as TransactionProxyClass;
            ((ITransactionProxy<TransactionProxyClass>)proxy).SetTargets(new[] { target1 });

            Assert.Equal(123, proxy.DecimalProperty);
            Assert.Equal(456, proxy.IntProperty);
            Assert.Equal("Test", proxy.StringProperty);
            Assert.Single(proxy.Targets);
            Assert.Equal(target1.Targets.First(), proxy.Targets.First());

            proxy.DecimalProperty = 1234;
            proxy.IntProperty = 5678;
            proxy.StringProperty = "Test2";
            proxy.Targets.Add(new TestTarget());

            Assert.Equal(123, target1.DecimalProperty);
            Assert.Equal(456, target1.IntProperty);
            Assert.Equal("Test", target1.StringProperty);
            Assert.Single(target1.Targets);

            ((ICommitable)proxy).Commit();

            Assert.Equal(1234, target1.DecimalProperty);
            Assert.Equal(5678, target1.IntProperty);
            Assert.Equal("Test2", target1.StringProperty);
            Assert.Equal(2, target1.Targets.Count());
        }

        [Fact]
        public void TestAddedDynamicPropertyNotification()
        {
            DynamicTypeBuilder<NotificationObject> builder = new DynamicTypeBuilder<NotificationObject>(
            new DynamicMember[] {
                            new DynamicProperty("StringProperty", typeof(string)),
                            new DynamicProperty("DecimalProperty", typeof(decimal)),
                            new DynamicProperty("NullableDecimalProperty", typeof(decimal?)),
                            new DynamicProperty("DateTimeProperty", typeof(DateTime)),
                            new DynamicProperty("TestClassProperty", typeof(TestClass))
                        }
            );

            builder.AddConvention(new NotifyPropertyChangedConvention());

            var instance = Activator.CreateInstance(builder.GeneratedType);

            Assert.True(typeof(INotifyPropertyChanged).IsInstanceOfType(instance));

            List<string> propertyNames = new List<string>();

            ((INotifyPropertyChanged)instance).PropertyChanged += (s, ea) => { propertyNames.Add(ea.PropertyName); };

            var raise = builder.GeneratedType.GetMethod("RaisePropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(raise);
            raise.Invoke(instance, new object[] { "IntProperty" });

            Assert.Single(propertyNames);
            Assert.Equal("IntProperty", propertyNames[0]);

            ((IDynamicPropertyHost)instance).SetValue("StringProperty", "TestString");

            Assert.Equal(2, propertyNames.Count);
            Assert.Equal("StringProperty", propertyNames[1]);

            ((IDynamicPropertyHost)instance).SetValue("NullableDecimalProperty", 4.3m);

            Assert.Equal(3, propertyNames.Count);
            Assert.Equal("NullableDecimalProperty", propertyNames[2]);
        }

        [Fact(Skip = "Currently Assemblies are loades as Run not as CollectAndRun... .NET Bug. need to verify if aleady fixed")]
        public void TestCollectableAssemblies()
        {
            DynamicTypeBuilder<Object> builder = new DynamicTypeBuilder<object>(new DynamicProperty[] { });
            WeakReference reference = new WeakReference(builder.GeneratedType);

            builder = null;

            Assert.True(reference.IsAlive);

            GC.Collect();
            var res = GC.WaitForFullGCComplete();
            GC.WaitForPendingFinalizers();

            Assert.False(reference.IsAlive);
        }

        [Fact]
        public void TestDynamicObjectGetSetValueMethod()
        {
            DynamicTypeBuilder<DynamicObjectBase> builder = new DynamicTypeBuilder<DynamicObjectBase>(
                new DynamicMember[] {
                                new DynamicProperty("StringProperty", typeof(string)),
                                new DynamicProperty("DecimalProperty", typeof(decimal)),
                                new DynamicProperty("NullableDecimalProperty", typeof(decimal?)),
                                new DynamicProperty("DateTimeProperty", typeof(DateTime)),
                                new DynamicProperty("TestClassProperty", typeof(TestClass))
                                ,new SetValueMethod()
                                ,new GetValueMethod()
                            }
                );

            var conn = new TestClass();

            var instance = Activator.CreateInstance(builder.GeneratedType) as DynamicObjectBase;

            instance.SetValue("StringProperty", "TestString");
            instance.SetValue("DecimalProperty", 1.2m);
            instance.SetValue("NullableDecimalProperty", 1.3m);
            instance.SetValue("DateTimeProperty", DateTime.Today);
            instance.SetValue("TestClassProperty", conn);

            Assert.Null(instance.GetValue("NonExistingProperty"));

            Assert.Equal("TestString", instance.GetValue("StringProperty"));
            Assert.Equal(1.2m, instance.GetValue("DecimalProperty"));
            Assert.Equal(1.3m, instance.GetValue("NullableDecimalProperty"));
            Assert.Equal(DateTime.Today, instance.GetValue("DateTimeProperty"));
            Assert.Equal(conn, instance.GetValue("TestClassProperty"));
        }

        [Fact]
        public void TestDynamicPropertyNotification()
        {
            DynamicTypeBuilder builder = new DynamicTypeBuilder(
            new DynamicMember[] {
                            new DynamicProperty("StringProperty", typeof(string)),
                            new DynamicProperty("DecimalProperty", typeof(decimal)),
                            new DynamicProperty("NullableDecimalProperty", typeof(decimal?)),
                            new DynamicProperty("DateTimeProperty", typeof(DateTime)),
                            new DynamicProperty("TestClassProperty", typeof(TestClass))
                        }
            );

            builder.AddConvention(new NotifyPropertyChangedConvention());

            var instance = Activator.CreateInstance(builder.GeneratedType);

            Assert.True(typeof(INotifyPropertyChanged).IsInstanceOfType(instance));

            List<string> propertyNames = new List<string>();

            ((INotifyPropertyChanged)instance).PropertyChanged += (s, ea) => { propertyNames.Add(ea.PropertyName); };

            var raise = builder.GeneratedType.GetMethod("RaisePropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(raise);
            raise.Invoke(instance, new object[] { "IntProperty" });

            Assert.Single(propertyNames);
            Assert.Equal("IntProperty", propertyNames[0]);

            ((IDynamicPropertyHost)instance).SetValue("StringProperty", "TestString");

            Assert.Equal(2, propertyNames.Count);
            Assert.Equal("StringProperty", propertyNames[1]);

            ((IDynamicPropertyHost)instance).SetValue("NullableDecimalProperty", 4.3m);

            Assert.Equal(3, propertyNames.Count);
            Assert.Equal("NullableDecimalProperty", propertyNames[2]);
        }

        [Fact]
        public void TestDynamicVoid()
        {
            DynamicTypeBuilder<DynamicVoidTestClass> builder = new DynamicTypeBuilder<DynamicVoidTestClass>(
                  new DynamicMember[] {
                        new DynamicProperty("Property1", typeof(string)),
                        new DynamicVoid("TestVoid",typeof(string)),
                        new DynamicVoid("TestVoid2",typeof(TestClass)),
                        new DynamicVoid("TestVoid3")
                }
              );

            builder.AddInterceptor(new ImplementInterfaceInterceptor<ITestVoidInterface>());

            var type = builder.GeneratedType;

            var implementation = Activator.CreateInstance(type) as DynamicVoidTestClass;

            string randomSring = new Random().Next().ToString();
            string testValue = null;

            bool secondCalled = false;

            bool testClassCalled = false;

            Assert.NotNull(type);
            Assert.NotNull(type.GetMethod("TestVoid"));
            Assert.Equal(typeof(void), type.GetMethod("TestVoid").ReturnType);
            Assert.Single(type.GetMethod("TestVoid").GetParameters());

            implementation.Void<ITestVoidInterface, string>(p => p.TestVoid).Subscribe(p => testValue = p);
            implementation.Void<ITestVoidInterface, string>(p => p.TestVoid).Subscribe(p => secondCalled = true);

            implementation.Void<ITestVoidInterface, TestClass>(p => p.TestVoid2).Subscribe(p => testClassCalled = true);

            type.GetMethod("TestVoid").Invoke(implementation, new[] { randomSring });
            type.GetMethod("TestVoid2").Invoke(implementation, new[] { new TestClass() });
            type.GetMethod("TestVoid3").Invoke(implementation, new object[] { });

            Assert.Equal(randomSring, testValue);
            Assert.True(secondCalled);
            Assert.True(testClassCalled);
        }

        [Fact]
        public void TestDynmaicEventForObject()
        {
            DynamicTypeBuilder<object> builder = new DynamicTypeBuilder<object>(
                new DynamicMember[] {
                        new DynamicEvent("PropertyChanged", typeof(PropertyChangedEventHandler)),
                }
            );

            var type = builder.GeneratedType;

            var instance = Activator.CreateInstance(type);

            var eventInfo = type.GetEvent("PropertyChanged");

            Assert.NotNull(eventInfo);

            bool wasCalled = false;

            var handler = new PropertyChangedEventHandler((s, ea) => wasCalled = true);

            eventInfo.AddEventHandler(instance, handler);

            FireEvent<PropertyChangedEventArgs>(instance, "val_PropertyChanged", new PropertyChangedEventArgs("Test"));
            Assert.True(wasCalled);
            wasCalled = false;

            eventInfo.RemoveEventHandler(instance, handler);

            FireEvent<PropertyChangedEventArgs>(instance, "val_PropertyChanged", new PropertyChangedEventArgs("Test"));

            Assert.False(wasCalled);
        }

        [Fact]
        public void TestDynmaicPropertiesForObject()
        {
            DynamicTypeBuilder<object> builder = new DynamicTypeBuilder<object>(
                    new DynamicProperty[] {
                        new DynamicProperty("StringProperty", typeof(string)),
                        new DynamicProperty("DecimalProperty", typeof(decimal)),
                        new DynamicProperty("NullableDecimalProperty", typeof(decimal?)),
                        new DynamicProperty("DateTimeProperty", typeof(DateTime)),
                        new DynamicProperty("TestClassProperty", typeof(TestClass))
                    }
                );

            Assert.NotNull(builder.GeneratedType);

            var instance = Activator.CreateInstance(builder.GeneratedType);

            Assert.True(builder.GeneratedType.IsInstanceOfType(instance));

            var stringProperty = builder.GeneratedType.GetProperty("StringProperty");
            var decimalProperty = builder.GeneratedType.GetProperty("DecimalProperty");
            var nullableDecimalProperty = builder.GeneratedType.GetProperty("NullableDecimalProperty");
            var dateTimeProperty = builder.GeneratedType.GetProperty("DateTimeProperty");
            var TestClassProperty = builder.GeneratedType.GetProperty("TestClassProperty");

            Assert.NotNull(stringProperty);
            Assert.NotNull(decimalProperty);
            Assert.NotNull(nullableDecimalProperty);
            Assert.NotNull(dateTimeProperty);
            Assert.NotNull(TestClassProperty);

            Assert.Equal(typeof(string), stringProperty.PropertyType);
            Assert.Equal(typeof(decimal), decimalProperty.PropertyType);
            Assert.Equal(typeof(decimal?), nullableDecimalProperty.PropertyType);
            Assert.Equal(typeof(DateTime), dateTimeProperty.PropertyType);
            Assert.Equal(typeof(TestClass), TestClassProperty.PropertyType);

            var connection = new TestClass();

            stringProperty.SetValue(instance, "Teststring", null);
            decimalProperty.SetValue(instance, 0.1m, null);
            nullableDecimalProperty.SetValue(instance, 0.2m, null);
            dateTimeProperty.SetValue(instance, new DateTime(2012, 1, 1), null);
            TestClassProperty.SetValue(instance, connection, null);

            Assert.Equal("Teststring", stringProperty.GetValue(instance, null));
            Assert.Equal(0.1m, decimalProperty.GetValue(instance, null));
            Assert.Equal(0.2m, nullableDecimalProperty.GetValue(instance, null));
            Assert.Equal(new DateTime(2012, 1, 1), dateTimeProperty.GetValue(instance, null));
            Assert.Equal(connection, TestClassProperty.GetValue(instance, null));
        }

        [Fact]
        public void TestObjectGetSetValueMethod()
        {
            DynamicTypeBuilder builder = new DynamicTypeBuilder(
                new DynamicMember[] {
                                new DynamicProperty("StringProperty", typeof(string)),
                                new DynamicProperty("DecimalProperty", typeof(decimal)),
                                new DynamicProperty("NullableDecimalProperty", typeof(decimal?)),
                                new DynamicProperty("DateTimeProperty", typeof(DateTime)),
                                new DynamicProperty("TestClassProperty", typeof(TestClass))
                            }
                );

            var conn = new TestClass();

            var instance = Activator.CreateInstance(builder.GeneratedType) as IDynamicPropertyHost;

            instance.SetValue("StringProperty", "TestString");
            instance.SetValue("DecimalProperty", 1.2m);
            instance.SetValue("NullableDecimalProperty", 1.3m);
            instance.SetValue("DateTimeProperty", DateTime.Today);
            instance.SetValue("TestClassProperty", conn);

            Assert.Null(instance.GetValue("NonExistingProperty"));

            Assert.Equal("TestString", instance.GetValue("StringProperty"));
            Assert.Equal(1.2m, instance.GetValue("DecimalProperty"));
            Assert.Equal(1.3m, instance.GetValue("NullableDecimalProperty"));
            Assert.Equal(DateTime.Today, instance.GetValue("DateTimeProperty"));
            Assert.Equal(conn, instance.GetValue("TestClassProperty"));
        }

        [Fact]
        public void TestPropertyOverride()
        {
            DynamicTypeBuilder<PropertyOverrideTestClass> builder1 = new DynamicTypeBuilder<PropertyOverrideTestClass>(
                new DynamicMember[] {
                        new DynamicProperty("Property1", typeof(string)),
                        new DynamicProperty("Property2", typeof(string)),
            }
            );
            Assert.NotNull(builder1.GeneratedType);

            var exceptionHasBeenThrown = false;

            try
            {
                DynamicTypeBuilder<PropertyOverrideTestClass> builder = new DynamicTypeBuilder<PropertyOverrideTestClass>(
                    new DynamicMember[] {
                        new DynamicProperty("Property1", typeof(int)),
                }
                );
                Assert.NotNull(builder.GeneratedType);
            }
            catch (MemberTypeMissmatchException)
            {
                exceptionHasBeenThrown = true;
            }
            Assert.True(exceptionHasBeenThrown);

            DynamicTypeBuilder<PropertyOverrideTestClass> finalBuilder = new DynamicTypeBuilder<PropertyOverrideTestClass>(
                new DynamicMember[] {
                                    new DynamicProperty("Property1", typeof(string)),
                            }
            );

            var type = finalBuilder.GeneratedType;
            var instance = Activator.CreateInstance(type);

            var field = instance.GetType().GetField("val_Property1", BindingFlags.NonPublic | BindingFlags.Instance);
            var baseField = typeof(PropertyOverrideTestClass).GetField("val_Property1", BindingFlags.NonPublic | BindingFlags.Instance);
            var property = instance.GetType().GetProperty("Property1", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotEqual(field, baseField);

            property.SetValue(instance, "StringPropertyTest", null);
            Assert.Null(field);
            Assert.Equal("StringPropertyTest", baseField.GetValue(instance));
        }

        protected void FireEvent<T>(object instance, string handler, T eventArgs) where T : EventArgs
        {
            MulticastDelegate eventDelagate =
                  (MulticastDelegate)instance.GetType().GetField(handler,
                   System.Reflection.BindingFlags.Instance |
                   System.Reflection.BindingFlags.NonPublic).GetValue(instance);

            if (eventDelagate != null)
            {
                Delegate[] delegates = eventDelagate.GetInvocationList();

                foreach (Delegate dlg in delegates)
                {
                    dlg.Method.Invoke(dlg.Target, new object[] { this, eventArgs });
                }
            }
        }

        public class DynamicVoidTestClass : DynamicObjectBase
        {
            public override object GetValue(string memberName)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(string memberName, object value)
            {
                throw new NotImplementedException();
            }
        }

        public class PropertyOverrideTestClass
        {
            private string val_Property1;

            private string val_Property2;

            public virtual string Property1
            {
                get { return val_Property1; }
                set
                {
                    val_Property1 = value;
                }
            }

            public string Property2
            {
                get { return val_Property2; }
                set
                {
                    val_Property2 = value;
                }
            }
        }

        private class Test : ICommitable
        {
            private Dictionary<Key<string>, List<Test>> testchen_Values;

            public Test()
            {
                Targets = new Collection<Test>();
            }

            public Collection<Test> Targets { get; set; }

            public string Testchen { get; set; }

            public bool Testchen_HasMultipleValues { get; set; }

            public Dictionary<Key<string>, List<Test>> Testchen_Values
            {
                get
                {
                    return this.Testchen_Values;
                }
            }

            public void Rollback()
            {
                Console.WriteLine("Whatever");
            }

            #region ICommitable Members

            void ICommitable.Commit()
            {
                throw new NotImplementedException();
            }

            void ICommitable.Rollback()
            {
                this.testchen_Values = new Dictionary<Key<string>, List<Test>>();
                foreach (var item in this.Targets)
                {
                    TransactionProxyHelper.AddValue<string, Test>(item.Testchen, item, this.testchen_Values);
                }
            }

            #endregion ICommitable Members
        }
    }
}