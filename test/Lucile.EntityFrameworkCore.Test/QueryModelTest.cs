﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lucile.Linq;
using Lucile.Linq.Configuration;
using Lucile.Test.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Lucile.EntityFrameworkCore.Test
{
    public class QueryModelTest
    {
        private class DummyQuerySource : QuerySource
        {
            private readonly TestContext _context;

            public DummyQuerySource(TestContext context)
            {
                _context = context;
            }

            public override IQueryable<TEntity> Query<TEntity>()
            {
                return _context.Set<TEntity>();
            }
        }

        [Fact]
        public async Task GeneratedQueryWithSimpleModelEqualityTest()
        {
            using (var context = new TestContext())
            {
                var compiledQueryCache = GetCompiledQueryCache(context);
                var previousNumberOfCacheEntries = GetNumberOfCacheEntries(compiledQueryCache);
                var expectedNumberOfCacheEntries = previousNumberOfCacheEntries + 1;
                
                for (int i = 0; i < 10; i++)
                {
                    var queryModel = QueryModel.Create(
                        builder => builder.Get<ReceiptDetail>(),
                        builder => new ReceiptDetail
                        {
                            ReceiptId = builder.ReceiptId,
                            ArticleId = builder.ArticleId,
                        })
                        .HasKey(receipt => receipt.ReceiptId)
                        .Build();

                    var query = queryModel.GetQuery(new DummyQuerySource(context), new QueryConfiguration());
                    var result = await query.ToListAsync();

                    var currentNumberOfCacheEntries = GetNumberOfCacheEntries(compiledQueryCache);
                    Assert.Equal(expectedNumberOfCacheEntries, currentNumberOfCacheEntries);
                }
            }
        }

        [Fact]
        public async Task GeneratedQueryWithFiltersEqualityTest()
        {
            using (var context = new TestContext())
            {
                var compiledQueryCache = GetCompiledQueryCache(context);
                var previousNumberOfCacheEntries = GetNumberOfCacheEntries(compiledQueryCache);

                var querySource = new DummyQuerySource(context);

                var queryModel = QueryModel.Create(
                builder => builder.Get<ReceiptDetail>(),
                builder => new ReceiptDetail
                {
                    ReceiptId = builder.ReceiptId,
                    ArticleId = builder.ArticleId,
                })
                .HasKey(receipt => receipt.ReceiptId)
                .Build();

                FilterItem[] filterItems = {
                    new GuidBinaryFilterItem(new PathValueExpression(nameof(ReceiptDetail.ReceiptId)), new ConstantValueExpression<Guid>(Guid.NewGuid()), RelationalCompareOperator.Equal)
                };
                var queryConfiguration = new QueryConfiguration(filterItems);
                var query1 = queryModel.GetQuery(querySource, queryConfiguration);
                var result1 = await query1.ToListAsync();


                filterItems = new FilterItem[] {
                    new GuidBinaryFilterItem(new PathValueExpression(nameof(ReceiptDetail.ReceiptId)), new ConstantValueExpression<Guid>(Guid.NewGuid()), RelationalCompareOperator.Equal)
                };
                queryConfiguration = new QueryConfiguration(filterItems);
                var query2 = queryModel.GetQuery(querySource, queryConfiguration);
                var result2 = await query2.ToListAsync();

                var expectedNumberOfCacheEntries = previousNumberOfCacheEntries + 1;
                var currentNumberOfCacheEntries = GetNumberOfCacheEntries(compiledQueryCache);
                Assert.Equal(expectedNumberOfCacheEntries, currentNumberOfCacheEntries);
            }
        }

        private ICompiledQueryCache GetCompiledQueryCache(DbContext context)
        {
            var compiledQueryCache = context.GetService<ICompiledQueryCache>();
            return compiledQueryCache;
        }

        private int GetNumberOfCacheEntries(ICompiledQueryCache compiledQueryCache)
        {
            var memoryCacheField = compiledQueryCache.GetType().GetField(
                "_memoryCache",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var memoryCache = (MemoryCache)memoryCacheField.GetValue(compiledQueryCache);
            var numberOfCacheEntries = memoryCache.Count;
            return numberOfCacheEntries;
        }
    }
}
