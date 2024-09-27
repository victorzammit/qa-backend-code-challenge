/// <summary>
/// Simple unit test for verifying 'OnlineWallet' model functionality.
/// </summary>

using Betsson.OnlineWallets.Data;
using Microsoft.EntityFrameworkCore;

namespace Betsson.OnlineWallets.Tests.UnitTests;

public class OnlineWalletRepoTests {
    private readonly DbContextOptions<OnlineWalletContext> _options;
}