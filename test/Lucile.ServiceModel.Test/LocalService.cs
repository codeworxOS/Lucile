using System;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    public class LocalService : ILocalService
    {
        private readonly Guid _id;

        public LocalService()
        {
            _id = Guid.NewGuid();
        }

        public Task<Guid> WhateverAsync()
        {
            return Task.FromResult(_id);
        }
    }
}