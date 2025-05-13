using FluentAssertions;
using GdsToJenovaCpp.RestApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GdsToJenovaCpp.Specification.Controllers
{
    public class TranslateControllerTests
    {
        [Fact]
        public async Task Should_translate_gds_on_post()
        {
            // Arrange
            var controller = new TranslateController();
            var gdscriptPath = @"GDScriptSamples\script.gd";
            var gdscript = await File.ReadAllTextAsync(gdscriptPath);

            // Act
            var result = controller.Post(gdscript);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var cppCode = okResult.Value.ToString();
            var output = await File.ReadAllTextAsync("jenova-output.cpp");
            cppCode.Should().Be(output);
        }
    }
}