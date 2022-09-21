using System;
using Lucile.Core.Test.Model.Target;

namespace Tests
{
    public class ContactListItem
    {
        public string FirstName { get; set; }

        public Guid Id { get; set; }

        public string LastName { get; set; }

        public string Street { get; set; }

        public CountryInfo Country { get; set; }
    }
}