using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.Dynamic.Test
{
    public interface IValidatable
    {
        /// <summary>
        /// Validates all properties.
        /// </summary>
        /// <returns><code>true</code> if validation is successful, otherwise <code>false</code></returns>
        bool ValidateAll();

        /// <summary>
        /// Validates all properties asynchronous.
        /// </summary>
        /// <returns><code>true</code> if validation is successful, otherwise <code>false</code></returns>
        Task<bool> ValidateAllAsync();

        /// <summary>
        /// Validates the property with the given name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><code>true</code> if validation is successful, otherwise <code>false</code></returns>
        /// <exception cref="System.InvalidOperationException">Occurres when property is not available on the type</exception>
        bool ValidateProperty(string propertyName);

        /// <summary>
        /// Validates the property with the given name asynchronous.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><code>true</code> if validation is successful, otherwise <code>false</code></returns>
        /// <exception cref="System.InvalidOperationException">Occurres when property is not available on the type</exception>
        Task<bool> ValidatePropertyAsync(string propertyName);
    }
}