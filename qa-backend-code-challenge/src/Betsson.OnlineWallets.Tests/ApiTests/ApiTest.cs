using Betsson.OnlineWallets.Services;
using Betsson.OnlineWallets.Web;
using Betsson.OnlineWallets.Web.Mappers;
using AutoMapper;
using Moq;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Betsson.OnlineWallets.Tests.ApiTests;

public class OnlineWalletApiTests : IClassFixture<WebApplicationFactory<Startup>> {
    
    private readonly HttpClient _client;
    private readonly Mock<IOnlineWalletService> _onlineWalletServiceMock;
    private readonly IMapper _mapper;

    public OnlineWalletApiTests(WebApplicationFactory<Startup> factory) {

        _client = factory.CreateClient();
        _onlineWalletServiceMock = new Mock<IOnlineWalletService>();

        // AutoMapper setup using OnlineWalletMappingProfile
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new OnlineWalletMappingProfile()));
        _mapper = mapperConfig.CreateMapper();
    }
}
