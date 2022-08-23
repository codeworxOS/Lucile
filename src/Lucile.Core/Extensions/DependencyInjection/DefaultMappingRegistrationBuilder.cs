using Lucile.Mapper;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DefaultMappingRegistrationBuilder<TSource> : IMappingRegistrationBuilder<TSource>
    {
        private readonly IServiceCollection _services;

        public DefaultMappingRegistrationBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IMappingRegistrationBuilder<TSource> Configure<TTarget>(System.Func<IMappingBuilder<TSource>, IMappingBuilder<TSource, TTarget>> builder)
        {
            _services.AddSingleton<IMappingConfiguration<TSource, TTarget>>(sp => builder(new DefaultMappingBuilder<TSource>(sp)).ToConfiguration());
            _services.AddTransient<IMappingConfiguration>(sp => sp.GetRequiredService<IMappingConfiguration<TSource, TTarget>>());

            return this;
        }
    }
}