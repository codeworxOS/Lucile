using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucile.Data.Metadata;
using Lucile.Test.Model;

namespace Lucile.Core.Test.Metadata
{
    public class CustomAccessorFactory : IValueAccessorFactory
    {
        public IEntityValueAccessor GetAccessor(IEntityMetadata entityMetadata)
        {
            if (entityMetadata.ClrType == typeof(Article))
            {
                return new ArticleValueAccessor();
            }
            else if (entityMetadata.ClrType == typeof(ArticleName))
            {
                return new ArticleNameValueAccessor();
            }
            else if (entityMetadata.ClrType == typeof(Contact))
            {
                return new ContactValueAccessor();
            }

            throw new NotSupportedException();
        }
    }
}
