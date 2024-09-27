using AutoMapper;

using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Models;
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

    [Fact]
    public async Task PostWithdrawal_InsufficientBalance_ReturnsBadRequest()
    {
        // Set up a new withdrawal request exceeding available balance in online wallet
        var withdrawalRequest = new WithdrawalRequest {Amount = 1000};
        var withdrawal = _mapper.Map<Withdrawal>(withdrawalRequest);

        // Setting up lambda function to withdraw the funds specified in the withdrawal request.
        // Configure to throw Insufficient balance exception.
        _onlineWalletServiceMock.Setup(s => s.WithdrawFundsAsync(withdrawal))
            .ThrowsAsync(new InsufficientBalanceException("Insufficient funds available"));

        var content = new StringContent(
            JsonConvert.SerializeObject(withdrawalRequest), Encoding.UTF8, "application/json");

        // Send post request to withdraw from wallet
        var response = await _client.PostAsync("/OnlineWallet/Withdraw", content);

        // Evaluate response - should fail due to insufficient funds
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
