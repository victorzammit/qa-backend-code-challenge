using AutoMapper;

using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Services;

using Betsson.OnlineWallets.Web;
using Betsson.OnlineWallets.Web.Mappers;
using Betsson.OnlineWallets.Web.Models;

using Moq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text;
using System.Net;

namespace Betsson.OnlineWallets.Tests.ApiTests;

public class OnlineWalletApiTests : IClassFixture<WebApplicationFactory<Startup>> {
    
    private readonly HttpClient _client;
    private readonly Mock<IOnlineWalletService> _onlineWalletServiceMock;
    private readonly IMapper _mapper;

    public OnlineWalletApiTests(WebApplicationFactory<Startup> factory)
        {
            var mapperConfig = new MapperConfiguration(cfg => 
            { cfg.AddProfile(new OnlineWalletMappingProfile()); }
            );
    
            // Assert the configuration to ensure all mappings are valid
            mapperConfig.AssertConfigurationIsValid();

            _mapper = mapperConfig.CreateMapper();

            _onlineWalletServiceMock = new Mock<IOnlineWalletService>();
            // Create an HTTP client for the test server
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Registration of Mock Service
                    services.AddSingleton(_onlineWalletServiceMock.Object);
                });
            }).CreateClient();
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
    public async Task PostWithdrawal_InsufficientBalance_ReturnsBadRequest() {
        // Set up a new withdrawal request exceeding available balance in online wallet
        var withdrawalRequest = new WithdrawalRequest { Amount = 1000 }; // Requesting to withdraw 1000
        var withdrawal = _mapper.Map<Withdrawal>(withdrawalRequest);

        // Configure to throw Insufficient balance exception.
        _onlineWalletServiceMock.Setup(s => s.WithdrawFundsAsync(It.IsAny<Withdrawal>()))
            .ThrowsAsync(new InsufficientBalanceException("Insufficient funds available"));
            
        var content = new StringContent(
            JsonConvert.SerializeObject(withdrawalRequest), Encoding.UTF8, "application/json");

        // Send post request to withdraw from wallet
        var response = await _client.PostAsync("/OnlineWallet/Withdraw", content);

        // Evaluate response - should fail due to insufficient funds
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBalance_ReturnsCorrectBalanceResponse() {
        
        // Arrange
        var balance = new Balance{Amount = 500};
        _onlineWalletServiceMock.Setup(s => s.GetBalanceAsync()).ReturnsAsync(balance);

        // Act
        var response = await _client.GetAsync("/OnlineWallet/Balance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var balanceResponse = JsonConvert.DeserializeObject<BalanceResponse>(responseContent);
        
        // Verify the model is correctly mapped and returned
        Assert.NotNull(balanceResponse);
        Assert.Equal(500, balanceResponse.Amount);
    }
}
