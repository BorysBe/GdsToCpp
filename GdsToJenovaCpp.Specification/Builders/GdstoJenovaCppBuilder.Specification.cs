using FluentAssertions;
using GdsToJenovaCpp.Main.Builders;

namespace GdsToJenovaCpp.Specification.Controllers
{
    public class GdsToJenovaCppBuilderSpecification
    {
        [Fact]
        public void Should_replace_gds_comments_with_cpp_comments()
        {
            // Arrange
            var code = "#cam.offset.x = randf_range(-rangeX,rangeX)";
            var builder = new GdsToJenovaCppBuilder(code);

            // Act
            builder.ReplaceComments();

            // Assert
            var result = builder.Build();
            result.Should().Be("// cam.offset.x = randf_range(-rangeX,rangeX)");
        }

        [Fact]
        public void Should_replace_gds_methods_with_c_style_methods()
        {
            // Arrange
            var gds = "func shake_camera_random(rangeX, rangeY):\r\n\tcamera_shake.apply_noise_shake(randf_range(-rangeX,rangeX), randf_range(-rangeY,rangeY))\r\n\r\n";
            var builder = new GdsToJenovaCppBuilder(gds);

            // Act
            builder
                .ReplaceMethods()
                .AddGodotFunctionsUtilitiesHeader();

            // Assert
            var result = builder.Build();
            var expected = "void shake_camera_random(float rangeX, float rangeY)\r\n{\r\n\tcamera_shake.apply_noise_shake(UtilityFunctions::randf_range(-rangeX, rangeX), UtilityFunctions::randf_range(-rangeY, rangeY));\r\n}";
            result.Should().Contain(expected);
        }

        [Fact]
        public void Should_add_Godot_utilities_when_needed()
        {
            // Arrange
            var gds = File.ReadAllText(@"GDScriptSamples\many_intends.gd");
            var builder = new GdsToJenovaCppBuilder(gds);

            // Act
            builder.AddGodotFunctionsUtilitiesHeader();

            // Assert
            var result = builder.Build();
            result.Should().Contain("#include <Godot/variant/utility_functions.hpp>");
            result.Should().Contain("UtilityFunctions::randf_range(-rangeX,rangeX)");
            result.Should().Contain("UtilityFunctions::randf_range(-rangeY,rangeY)");
        }

        [Fact]
        public void Should_change_gdscript_function_parameters_into_c_style_methods()
        {
            // Arrange
            var gds = File.ReadAllText(@"GDScriptSamples\many_intends.gd");
            var builder = new GdsToJenovaCppBuilder(gds);

            // Act
            builder.ReplaceMethods();

            // Assert
            var result = builder.Build();
            result.Should().Contain("(Area2D area)");
            result.Should().Contain("(float rangeX, float rangeY)");
            result.Should().Contain("(-rangeX, rangeX)");
            result.Should().Contain("(-rangeY, rangeY)");
        }

        [Fact]
        public void Should_change_gdscript_function_parameters_and_add_utilities()
        {
            // Arrange
            var gds = File.ReadAllText(@"GDScriptSamples\many_intends.gd");
            var builder = new GdsToJenovaCppBuilder(gds);

            // Act
            builder
                .ReplaceMethods()
                .AddGodotFunctionsUtilitiesHeader();

            // Assert
            var result = builder.Build();
            result.Should().Contain("(Area2D area)");
            result.Should().Contain("(float rangeX, float rangeY)");
            result.Should().Contain("UtilityFunctions::randf_range(-rangeX, rangeX)");
            result.Should().Contain("UtilityFunctions::randf_range(-rangeY, rangeY)");
        }

        [Fact]
        public void Should_change_gdscript_advanced_if_else()
        {
            // Arrange
            var gds = File.ReadAllText(@"GDScriptSamples\ifelseadvanced.gd");
            var builder = new GdsToJenovaCppBuilder(gds);

            // Act
            builder
                .ReplaceMethods()
                .AddGodotFunctionsUtilitiesHeader();

            // Assert
            var result = builder.Build();
        }
    }
}