﻿namespace Lucile.Service
{
    public interface IConnected<TService>
        where TService : class
    {
        TService GetService();
    }
}