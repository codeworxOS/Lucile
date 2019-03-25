using System;
using System.Threading.Tasks;

namespace Lucile.ServiceModel.Test
{
    public interface ILocalService
    {
        Task<Guid> WhateverAsync();
    }
}