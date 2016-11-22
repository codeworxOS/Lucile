using System;
using System.Collections.Generic;

namespace Lucile.Test.Model
{
    public class Contact
    {
        public ICollection<Article> Articles { get; set; }

        public ContactType? ContactType { get; set; }

        public Country Country { get; set; }

        public int CountryId { get; set; }

        public string FirstName { get; set; }

        public Guid Id { get; set; }

        public string Identity { get; set; }

        public string LastName { get; set; }

        public ICollection<Receipt> Receipts { get; set; }

        public string Street { get; set; }
    }
}