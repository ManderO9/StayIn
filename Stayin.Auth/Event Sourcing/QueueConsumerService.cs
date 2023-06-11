using Stayin.Core;

namespace Stayin.Auth;

/// <summary>
/// Background service for consuming new events from the event bus
/// </summary>
public class QueueConsumerService : BackgroundService
{
    #region Private Members

    /// <summary>
    /// The service scope factory for creating scopes to get scoped services.
    /// </summary>
    private IServiceScopeFactory mServiceScopeFactory;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="serviceScopeFactory">Factory for creating scopes for scoped services</param>
    public QueueConsumerService(IServiceScopeFactory serviceScopeFactory)
    {
        mServiceScopeFactory = serviceScopeFactory;
    }

    #endregion

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        // TODO: delete this when running in production
        // while(true) { await Task.Delay(100000, stoppingToken); }


        while(!stoppingToken.IsCancellationRequested)
        {
            // Create the scope to get services from
            using var scope = mServiceScopeFactory.CreateScope();

            // Get the event bus
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            // Get the data access service
            var db = scope.ServiceProvider.GetRequiredService<IDataAccess>();

            // Get all new events
            var events = await eventBus.GetNewEvents(db);

            // For each event
            foreach(var newEvent in events)
                // Handle it
                await newEvent.Handle(db);

            // Wait for a period of time before resuming
            try { await Task.Delay(5 * 1000, stoppingToken); } catch(TaskCanceledException ex) { _ = ex; }
        }
    }
}
