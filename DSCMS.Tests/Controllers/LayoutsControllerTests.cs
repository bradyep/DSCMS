using DSCMS.Controllers;
using DSCMS.Models;
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
        public async Task Index_ReturnsViewWithLayouts()
        {
            var layouts = new List<Layout>
            {
                new Layout { LayoutId = 1, Name = "Bootstrap Blog", LayoutSource = "/Views/DSCMS/Layouts/_Blog.cshtml" }
            };
            _layoutRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(layouts);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Layout>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_LayoutNotFound_ReturnsNotFound()
        {
            _layoutRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Layout?)null);

            var result = await _controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_LayoutFound_ReturnsViewWithLayout()
        {
            var layout = new Layout { LayoutId = 1, Name = "My Layout" };
            _layoutRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(layout);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Layout>(viewResult.Model);
            Assert.Equal(1, model.LayoutId);
        }

        [Fact]
        public async Task Delete_NullId_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_LayoutNotFound_ReturnsNotFound()
        {
            _layoutRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Layout?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_LayoutExists_CallsDeleteAndRedirects()
        {
            var layout = new Layout { LayoutId = 1, Name = "Old Layout" };
            _layoutRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(layout);

            var result = await _controller.DeleteConfirmed(1);

            _layoutRepo.Verify(r => r.DeleteAsync(layout), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_LayoutMissing_RedirectsWithoutCallingDelete()
        {
            _layoutRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Layout?)null);

            var result = await _controller.DeleteConfirmed(99);

            _layoutRepo.Verify(r => r.DeleteAsync(It.IsAny<Layout>()), Times.Never);
            Assert.IsType<RedirectToActionResult>(result);
        }
    }
}
