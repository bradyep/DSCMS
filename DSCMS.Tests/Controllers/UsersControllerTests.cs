using System.Security.Claims;
using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userRepo = new Mock<IUserRepository>();

            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            _controller = new UsersController(_userRepo.Object, _userManager.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewWithUsers()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Email = "a@x.com" },
                new ApplicationUser { Id = "2", Email = "b@x.com" }
            };
            _userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<ApplicationUser>>(view.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_UserMissing_ReturnsNotFound()
        {
            _userRepo.Setup(r => r.GetByIdAsync("x")).ReturnsAsync((ApplicationUser?)null);

            var result = await _controller.Details("x");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_UserFound_ReturnsView()
        {
            var user = new ApplicationUser { Id = "u1", Email = "u1@x.com" };
            _userRepo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);

            var result = await _controller.Details("u1");

            var view = Assert.IsType<ViewResult>(result);
            Assert.Same(user, view.Model);
        }

        [Fact]
        public async Task Create_Valid_CallsUserManagerAndRedirects()
        {
            var user = new ApplicationUser { Email = "new@x.com" };
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Create(user, "Passw0rd!");

            _userManager.Verify(m => m.CreateAsync(It.Is<ApplicationUser>(u => u.UserName == "new@x.com"), "Passw0rd!"), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_MismatchedId_ReturnsNotFound()
        {
            var result = await _controller.Edit("a", new ApplicationUser { Id = "b" });
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_UserFound_DeletesAndRedirects()
        {
            var user = new ApplicationUser { Id = "u1", Email = "u1@x.com" };
            _userRepo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);
            _userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.DeleteConfirmed("u1");

            _userManager.Verify(m => m.DeleteAsync(user), Times.Once);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_UserMissing_Redirects()
        {
            _userRepo.Setup(r => r.GetByIdAsync("u404")).ReturnsAsync((ApplicationUser?)null);

            var result = await _controller.DeleteConfirmed("u404");

            _userManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
    }
}
