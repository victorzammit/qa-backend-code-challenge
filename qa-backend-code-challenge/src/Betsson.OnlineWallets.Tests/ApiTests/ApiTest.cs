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
    public async Task PostDeposit_ValidDepositRequest_ReturnsUpdatedBalanceResponse() {
        // Set up a valid deposity amount and the expected balance after a successful deposit
        var depositRequest = new DepositRequest { Amount = 100 }; // Valid deposit
        var deposit = _mapper.Map<Deposit>(depositRequest);

        var balance = new Balance { Amount = 600 }; // Balance after the deposit
        
        // Configure to mock the valid deposit request and return the expected balance
        _onlineWalletServiceMock.Setup(s => s.DepositFundsAsync(It.IsAny<Deposit>())).ReturnsAsync(balance);

        var content = new StringContent(JsonConvert.SerializeObject(depositRequest), Encoding.UTF8, "application/json");

        // Send post request to deposit into wallet
        var response = await _client.PostAsync("/OnlineWallet/Deposit", content);

        // Evaluate response code, should be successful
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var balanceResponse = JsonConvert.DeserializeObject<BalanceResponse>(responseContent);

        // Verify the model is correctly mapped and the balance is updated
        Assert.Equal(600, balanceResponse.Amount);
    }

    [Fact]
    public async Task PostWithdrawal_InsufficientBalance_ReturnsBadRequest() {
        // Set up a new withdrawal request exceeding available balance in online wallet
        var withdrawalRequest = new WithdrawalRequest { Amount = 1000 }; // Requesting to withdraw 1000
        var withdrawal = _mapper.Map<Withdrawal>(withdrawalRequest);

        // Configure to mock the invalid withdrawal request and throw Insufficient balance exception.
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
    public async Task PostWithdrawal_ValidWithdrawalRequest_ReturnsUpdatedBalanceResponse() {
        // Set up a new valid withdrawal request
        var withdrawalRequest = new WithdrawalRequest { Amount = 50 }; // Valid withdrawal
        var withdrawal = _mapper.Map<Withdrawal>(withdrawalRequest);

        // Updated balance after withdrawal
        var updatedBalance = new Balance { Amount = 450 };

        _onlineWalletServiceMock.Setup(s => s.WithdrawFundsAsync(It.IsAny<Withdrawal>()))
            .ReturnsAsync(updatedBalance);

        var content = new StringContent(JsonConvert.SerializeObject(withdrawalRequest), Encoding.UTF8, "application/json");

        // Send post request
        var response = await _client.PostAsync("/OnlineWallet/Withdraw", content);

        // Check for valid response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var balanceResponse = JsonConvert.DeserializeObject<BalanceResponse>(responseContent);

        // Verify the model is correctly mapped and the balance is updated
        Assert.Equal(450, balanceResponse.Amount);
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
