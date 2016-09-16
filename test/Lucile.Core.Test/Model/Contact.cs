using System;

namespace Lucile.Core.Test.Model
{
    public class Contact
    {
        public Guid Id { get; set; }

        public string Identity { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Street { get; set; }

        public int CountryId { get; set; }

        public Country Country { get; set; }
    }
}