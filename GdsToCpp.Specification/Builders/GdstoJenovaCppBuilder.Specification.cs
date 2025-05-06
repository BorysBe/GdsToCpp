using FluentAssertions;
using GdsToCpp.Controllers;
using Microsoft.AspNetCore.Mvc;
using GdsToJenovaCpp.Builders;

namespace GdsToJenovaCpp.Specification.Controllers
{
    public class GdsToJenovaCppBuilderSpecification
    {
        [Fact]
        public async Task Should_replace_gds_comments_with_cpp_comments()
        {
            // Arrange
            var code = "#cam.offset.x = randf_range(-rangeX,rangeX)";
            var builder = new GdsToJenovaCppBuilder(code);

            // Act
            builder.ReplaceComments();
            var result = builder.Build();

            // Assert
            result.Should().Be("// cam.offset.x = randf_range(-rangeX,rangeX)");
        }
    }
}