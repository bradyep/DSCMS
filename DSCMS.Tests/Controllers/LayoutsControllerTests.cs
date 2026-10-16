using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class LayoutsControllerTests
    {
        private readonly Mock<ILayoutRepository> _layoutRepo;
        private readonly Mock<ILogger<LayoutsController>> _logger;
        private readonly LayoutsController _controller;

        public LayoutsControllerTests()
        {
            _layoutRepo = new Mock<ILayoutRepository>();
            _logger = new Mock<ILogger<LayoutsController>>();
            _controller = new LayoutsController(_layoutRepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithLayouts()
        {
            var layouts = new List<Layout>
            {
                new Layout { LayoutId = 1, Name = "Bootstrap Blog", LayoutSource = "/Views/DSCMS/Layouts/_Blog.cshtml" }
            };
            _layoutRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(layouts);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<LayoutListDto>>(ok.Value);
            Assert.Single(dtos);
        }

        [Fact]
        public async Task GetById_LayoutNotFound_ReturnsNotFound()
        {
            _layoutRepo.Setup(r => r.GetByIdWithValuesAsync(99)).ReturnsAsync((Layout?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_LayoutFound_ReturnsOkWithDto()
        {
            var layout = new Layout { LayoutId = 1, Name = "My Layout", LayoutSource = "/src", SourceTypeId = 1 };
            _layoutRepo.Setup(r => r.GetByIdWithValuesAsync(1)).ReturnsAsync(layout);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<LayoutDetailDto>(ok.Value);
            Assert.Equal(1, dto.LayoutId);
        }

        [Fact]
        public async Task GetById_LayoutHasValues_ReturnsThemInDto()
        {
            var layout = new Layout { LayoutId = 1, Name = "My Layout", LayoutSource = "/src", SourceTypeId = 1 };
            layout.LayoutValues.Add(new LayoutValue { LayoutValueId = 1, Key = "blog-title", Value = "bradyep.com", LayoutId = 1 });
            layout.LayoutValues.Add(new LayoutValue { LayoutValueId = 2, Key = "blog-description", Value = "A Vanity Website", LayoutId = 1 });
            _layoutRepo.Setup(r => r.GetByIdWithValuesAsync(1)).ReturnsAsync(layout);

            var result = await _controller.GetById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<LayoutDetailDto>(ok.Value);
            Assert.Equal(2, dto.LayoutValues.Count);
            Assert.Contains(dto.LayoutValues, v => v.Key == "blog-title" && v.Value == "bradyep.com");
        }

        [Fact]
        public async Task Delete_LayoutNotFound_ReturnsNotFound()
        {
            _layoutRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Layout?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_LayoutExists_CallsDeleteAndReturnsNoContent()
        {
            var layout = new Layout { LayoutId = 1, Name = "Old Layout" };
            _layoutRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(layout);

            var result = await _controller.Delete(1);

            _layoutRepo.Verify(r => r.DeleteAsync(layout), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Create_ValidDto_ReturnsCreatedAtAction()
        {
            var dto = new LayoutCreateDto { Name = "New Layout", LayoutSource = "/src", SourceTypeId = 1 };
            _layoutRepo.Setup(r => r.AddAsync(It.IsAny<Layout>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), created.ActionName);
        }

        [Fact]
        public async Task Create_WithLayoutValues_PersistsThemOnTheLayout()
        {
            var dto = new LayoutCreateDto
            {
                Name = "New Layout",
                LayoutSource = "/src",
                SourceTypeId = 1,
                LayoutValues = new List<LayoutValueDto>
                {
                    new LayoutValueDto { Key = "blog-title", Value = "My Site" }
                }
            };
            Layout? capturedLayout = null;
            _layoutRepo.Setup(r => r.AddAsync(It.IsAny<Layout>()))
                .Callback<Layout>(l => capturedLayout = l)
                .Returns(Task.CompletedTask);

            var result = await _controller.Create(dto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(capturedLayout);
            Assert.Single(capturedLayout!.LayoutValues);
            Assert.Equal("blog-title", capturedLayout.LayoutValues[0].Key);
            Assert.Equal("My Site", capturedLayout.LayoutValues[0].Value);
        }

        [Fact]
        public async Task Update_AddsNewValue_WhenNotPresentInExistingLayout()
        {
            var layout = new Layout { LayoutId = 1, Name = "My Layout", LayoutSource = "/src", SourceTypeId = 1 };
            _layoutRepo.Setup(r => r.GetByIdWithValuesAsync(1)).ReturnsAsync(layout);
            _layoutRepo.Setup(r => r.UpdateAsync(It.IsAny<Layout>())).Returns(Task.CompletedTask);

            var dto = new LayoutUpdateDto
            {
                Name = "My Layout",
                LayoutSource = "/src",
                SourceTypeId = 1,
                LayoutValues = new List<LayoutValueDto>
                {
                    new LayoutValueDto { LayoutValueId = 0, Key = "blog-title", Value = "New Title" }
                }
            };

            var result = await _controller.Update(1, dto);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Single(layout.LayoutValues);
            Assert.Equal("blog-title", layout.LayoutValues[0].Key);
        }

        [Fact]
        public async Task Update_RemovesValue_WhenOmittedFromDto()
        {
            var layout = new Layout { LayoutId = 1, Name = "My Layout", LayoutSource = "/src", SourceTypeId = 1 };
            layout.LayoutValues.Add(new LayoutValue { LayoutValueId = 5, Key = "blog-title", Value = "Old Title", LayoutId = 1 });
            _layoutRepo.Setup(r => r.GetByIdWithValuesAsync(1)).ReturnsAsync(layout);
            _layoutRepo.Setup(r => r.UpdateAsync(It.IsAny<Layout>())).Returns(Task.CompletedTask);

            var dto = new LayoutUpdateDto
            {
                Name = "My Layout",
                LayoutSource = "/src",
                SourceTypeId = 1,
                LayoutValues = new List<LayoutValueDto>()
            };

            var result = await _controller.Update(1, dto);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Empty(layout.LayoutValues);
        }

        [Fact]
        public async Task Update_UpdatesExistingValue_WhenIdMatches()
        {
            var layout = new Layout { LayoutId = 1, Name = "My Layout", LayoutSource = "/src", SourceTypeId = 1 };
            layout.LayoutValues.Add(new LayoutValue { LayoutValueId = 5, Key = "blog-title", Value = "Old Title", LayoutId = 1 });
            _layoutRepo.Setup(r => r.GetByIdWithValuesAsync(1)).ReturnsAsync(layout);
            _layoutRepo.Setup(r => r.UpdateAsync(It.IsAny<Layout>())).Returns(Task.CompletedTask);

            var dto = new LayoutUpdateDto
            {
                Name = "My Layout",
                LayoutSource = "/src",
                SourceTypeId = 1,
                LayoutValues = new List<LayoutValueDto>
                {
                    new LayoutValueDto { LayoutValueId = 5, Key = "blog-title", Value = "New Title" }
                }
            };

            var result = await _controller.Update(1, dto);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Single(layout.LayoutValues);
            Assert.Equal("New Title", layout.LayoutValues[0].Value);
        }
    }
}
