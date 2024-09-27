using Xunit;
using Betsson.OnlineWallets.Models;

namespace Betsson.OnlineWallets.UnitTests;

public class DepositTests {

    [Fact]
    public void Deposit_Amount_CanBeSetAndRetrieved() {
        
        // Create a new object of type deposit
        var deposit = new Deposit();

        // Setting deposit amount to any valid quantity
        deposit.Amount = 150;

        // Checking that the correct quantity is stored in the deposit amount
        Assert.Equal(150, deposit.Amount);
    }
}
