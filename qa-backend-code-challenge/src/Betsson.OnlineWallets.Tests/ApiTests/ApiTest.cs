using AutoMapper;

using Betsson.OnlineWallets.Services;
using Betsson.OnlineWallets.Web;
using Betsson.OnlineWallets.Web.Mappers;
using Betsson.OnlineWallets.Web.Models;

using Moq;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Text;
using System.Net;

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

        [Fact]
    public async Task PostDeposit_InvalidDepositRequest_ReturnsBadRequest() {
        
        // Create a new invalid Deposit request. Convert to Json format
        var depositRequest = new DepositRequest {Amount = -100};
        var content = new StringContent(
            JsonConvert.SerializeObject(depositRequest), Encoding.UTF8, "application/json");

        // Send Post deposit request
        var response = await _client.PostAsync("/OnlineWallet/Deposit", content);

        // Check that appropriate bad request response is received.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
