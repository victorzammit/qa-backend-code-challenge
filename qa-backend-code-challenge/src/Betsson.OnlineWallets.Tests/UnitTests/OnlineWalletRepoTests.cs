/// <summary>
/// Simple unit test for verifying 'OnlineWallet' model functionality.
/// </summary>

using Betsson.OnlineWallets.Data;
using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Betsson.OnlineWallets.Tests.UnitTests;

public class OnlineWalletRepoTests {
    private readonly DbContextOptions<OnlineWalletContext> _options;
    private readonly OnlineWalletContext _context;
    private readonly OnlineWalletRepository _repository;

    public OnlineWalletRepoTests() {

        // Set up in-memory database
        _options = new DbContextOptionsBuilder<OnlineWalletContext>()
            .UseInMemoryDatabase(databaseName: "TesterDb").Options;

        // Create online wallet context with in-memory db
        _context = new OnlineWalletContext(_options);

        // Start the wallet repo using the context
        _repository = new OnlineWalletRepository(_context);

        // Ensure the database is clean before running tests
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetLastOnlineWalletEntryAsync_ReturnsNull_WhenNoEntriesExist() {

        // Retrieves last entry on online wallet, should return null since no entries
        // have been made
        var result = await _repository.GetLastOnlineWalletEntryAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task AddOnlineWalletEntryAsync_AddsNewEntrySuccessfully() {

        // Creates a new entry to be added to the online wallet db
        var entry = new OnlineWalletEntry {Amount = 50, BalanceBefore = 0};

        // Adds the new entry to the online wallet
        await _repository.InsertOnlineWalletEntryAsync(entry);

        // Checks that entry has been successfully registered in online wallet
        var lastEntry = await _repository.GetLastOnlineWalletEntryAsync();
        Assert.NotNull(lastEntry);
        Assert.Equal(50, lastEntry.Amount);
    }

    [Fact]
    public async Task GetLastOnlineWalletEntryAsync_ReturnsMostRecentEntry() {

        // Create two entries to be added to the online wallet. Add a time offset to
        // the first entry of 1 hour
        var entry1 = new OnlineWalletEntry {
            Amount = 50, BalanceBefore = 0,
            EventTime = DateTimeOffset.Now.AddHours(-1)
            };
            
        var entry2 = new OnlineWalletEntry {
            Amount = 100,
            BalanceBefore = 50,
            EventTime = DateTimeOffset.Now
            };

        // Add the new entries in the online wallet.
        await _repository.InsertOnlineWalletEntryAsync(entry1);
        await _repository.InsertOnlineWalletEntryAsync(entry2);

        // Retrieve the last entry added to the online wallet. Check that value matches
        // that of entry2
        var lastEntry = await _repository.GetLastOnlineWalletEntryAsync();
        Assert.Equal(100, lastEntry.Amount);
    }
}