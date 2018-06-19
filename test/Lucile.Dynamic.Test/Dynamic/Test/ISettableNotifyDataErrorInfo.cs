using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Lucile.Dynamic.Test.Dynamic.Test
{
    /// <summary>
    /// Interface for validating objects
    /// </summary>
    public interface ISettableNotifyDataErrorInfo : INotifyDataErrorInfo
    {
        /// <summary>
        /// Clears all errors.
        /// </summary>
        /// <returns>List of property names with cleared errors.</returns>
        IEnumerable<string> ClearAllErrors();

        /// <summary>
        /// Clear all errors for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><code>true</code> if errors have been cleared</returns>
        bool ClearErrors(string propertyName);

        /// <summary>
        /// Sets errors for the specified properties.
        /// </summary>
        /// <param name="propertyErrors">Tuples which contain the property name and its errors.</param>
        void SetErrors(IEnumerable<Tuple<string, IEnumerable<string>>> propertyErrors);

        /// <summary>
        /// Sets errors for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="errors">Errors which will be set for the specified property.</param>
        void SetErrors(string propertyName, IEnumerable<string> errors);
    }
}