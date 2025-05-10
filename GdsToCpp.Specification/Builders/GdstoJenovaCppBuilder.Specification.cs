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

        [Fact]
        public async Task Should_replace_gds_methods_with_c_style_methods()
        {
            // Arrange
            var gds = "func shake_camera_random(rangeX, rangeY):\r\n\tcamera_shake.apply_noise_shake(randf_range(-rangeX,rangeX),randf_range(-rangeY,rangeY))\r\n\r\n";
            var builder = new GdsToJenovaCppBuilder(gds);

            // Act
            builder.ReplaceMethods();
            var result = builder.Build();

            // Assert
            var expected = "void shake_camera_random(float rangeX, float rangeY) \r\n{\r\n    camera_shake->call(\"apply_noise_shake\", Array::make(Math::randf_range(-rangeX, rangeX), Math::randf_range(-rangeY, rangeY)));\r\n}";
            result.Should().Be(expected);
        }
    }
}