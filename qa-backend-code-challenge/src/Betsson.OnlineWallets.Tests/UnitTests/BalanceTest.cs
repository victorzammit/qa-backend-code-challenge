/// <summary>
/// Simple unit test for verifying 'Balance' model functionality.
/// </summary>
using Betsson.OnlineWallets.Models;

namespace Betsson.OnlineWallets.Tests.UnitTests;

public class BalanceTests {
    
    [Fact]
    public void Balance_Amount_CanBeSetAndRetrieved() {
        
        // Creating a new object of type Balance
        var balance = new Balance();

        // Setting balance to any valid quantity
        balance.Amount = 150;

        // Checking that the correct quantity is stored in balance amount.
        Assert.Equal(150, balance.Amount);
    }
}
