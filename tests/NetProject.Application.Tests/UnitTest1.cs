using Microsoft.Extensions.Logging.Abstractions;
using NetProject.Application;
using NetProject.Application.Mapping;
using Xunit;

namespace NetProject.Application.Tests;

public sealed class AutoMapperConfigurationTests
{
    [Fact]
    public void MappingConfiguration_is_valid()
    {
        var cfg = new AutoMapper.MapperConfigurationExpression();
        cfg.AddMaps(typeof(AssemblyMarker).Assembly);

        var config = new AutoMapper.MapperConfiguration(cfg, NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }
}
