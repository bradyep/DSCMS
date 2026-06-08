using DSCMS.Controllers;
using DSCMS.Models;
using DSCMS.Models.DTOs;
using DSCMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSCMS.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<ILogger<UsersController>> _logger;
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

            _logger = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_userRepo.Object, _userManager.Object, _logger.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithUsers()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Email = "a@x.com" },
                new ApplicationUser { Id = "2", Email = "b@x.com" }
            };
            _userRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<UserListDto>>(ok.Value);
            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public async Task GetById_UserMissing_ReturnsNotFound()
        {
            _userRepo.Setup(r => r.GetByIdAsync("x")).ReturnsAsync((ApplicationUser?)null);

            var result = await _controller.GetById("x");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetById_UserFound_ReturnsOkWithDto()
        {
            var user = new ApplicationUser { Id = "u1", Email = "u1@x.com" };
            _userRepo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);

            var result = await _controller.GetById("u1");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<UserDetailDto>(ok.Value);
            Assert.Equal("u1", dto.Id);
        }

        [Fact]
        public async Task Create_Valid_CallsUserManagerAndReturnsCreated()
        {
            var dto = new UserCreateDto { Email = "new@x.com", Password = "Passw0rd!" };
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Create(dto);

            _userManager.Verify(m => m.CreateAsync(
                It.Is<ApplicationUser>(u => u.UserName == "new@x.com"),
                "Passw0rd!"), Times.Once);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetById), created.ActionName);
        }

        [Fact]
        public async Task Create_IdentityFailure_ReturnsBadRequest()
        {
            var dto = new UserCreateDto { Email = "new@x.com", Password = "weak" };
            var errors = new[] { new IdentityError { Description = "Too short" } };
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var result = await _controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Delete_UserFound_DeletesAndReturnsNoContent()
        {
            var user = new ApplicationUser { Id = "u1", Email = "u1@x.com" };
            _userRepo.Setup(r => r.GetByIdAsync("u1")).ReturnsAsync(user);
            _userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Delete("u1");

            _userManager.Verify(m => m.DeleteAsync(user), Times.Once);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_UserMissing_ReturnsNotFound()
        {
            _userRepo.Setup(r => r.GetByIdAsync("u404")).ReturnsAsync((ApplicationUser?)null);

            var result = await _controller.Delete("u404");

            _userManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
