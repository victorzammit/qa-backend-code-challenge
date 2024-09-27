/// <summary>
/// Simple unit test for verifying 'Withdrawal' model functionality.
/// </summary>
using Betsson.OnlineWallets.Models;

namespace Betsson.OnlineWallets.Tests.UnitTests;

public class WithdrawalTests
{
    [Fact]
    public void Withdrawal_Amount_CanBeSetAndRetrieved()
    {
        // Create a new object of type Withdrawal
        var withdrawal = new Withdrawal();

        // Setting withdrawal amount to any valid value
        withdrawal.Amount = 30;

        // Checking that the correct value is stored in the withdrawal amount
        Assert.Equal(30, withdrawal.Amount);
    }
}
