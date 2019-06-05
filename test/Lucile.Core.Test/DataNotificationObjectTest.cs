using System.Collections.Generic;
using System.Linq;
using Lucile;
using Xunit;

namespace Tests
{
    public class DataNotificationObjectTest
    {
        [Fact]
        public void DataValidationNotificationTest()
        {
            int NotifyCount = 0;
            var testClass = new ValidationTestClass();
            testClass.ErrorsChanged += (d, e) =>
            {
                if (e.PropertyName == nameof(testClass.ZipCode))
                    NotifyCount++;
            };

            Assert.True(testClass.HasErrors);
            var errors = testClass.GetErrors(nameof(testClass.ZipCode)).OfType<string>().ToArray();
            Assert.Single(errors);
            Assert.Contains("Empty", errors);
            Assert.Equal(0, NotifyCount);

            testClass.ZipCode = " 12345";
            errors = testClass.GetErrors(nameof(testClass.ZipCode)).OfType<string>().ToArray();
            Assert.True(testClass.HasErrors);
            Assert.Equal(2, errors.Length);
            Assert.Contains("Length", errors);
            Assert.Contains("Invalid", errors);
            Assert.Equal(1, NotifyCount);

            testClass.ZipCode = "12345";
            errors = testClass.GetErrors(nameof(testClass.ZipCode)).OfType<string>().ToArray();
            Assert.True(testClass.HasErrors);
            Assert.Single(errors);
            Assert.Contains("Length", errors);
            Assert.Equal(2, NotifyCount);

            testClass.ZipCode = "1234";
            errors = testClass.GetErrors(nameof(testClass.ZipCode))?.OfType<string>().ToArray();
            Assert.False(testClass.HasErrors);
            Assert.Null(errors);
        }

        internal class ValidationTestClass : DataNotificationObject
        {
            private string _checkStr = "0123456789";

            public ValidationTestClass()
            {
                UpdateValidation(ValidateZipCode(), nameof(ZipCode));
            }

            private string _zipCode;
            public string ZipCode
            {
                get
                {
                    return _zipCode;
                }

                set
                {
                    if (value != _zipCode)
                    {
                        _zipCode = value;
                        UpdateValidation(ValidateZipCode());
                        RaisePropertyChanged();
                    }
                }
            }

            private IEnumerable<string> ValidateZipCode()
            {
                if (string.IsNullOrEmpty(ZipCode))
                {
                    yield return "Empty";
                    yield break;
                }

                if (ZipCode.Length != 4)
                {
                    yield return "Length";
                }

                if (!ZipCode.All(_checkStr.Contains))
                {
                    yield return "Invalid";
                }
            }
        }
    }
}
