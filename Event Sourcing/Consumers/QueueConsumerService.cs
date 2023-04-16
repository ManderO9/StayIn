using System.Diagnostics;
using System.Text.Json;

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

        // TODO: try locking the database access,
        // cuz it could be used somewhere else during the same time and cause an exception

        while(!stoppingToken.IsCancellationRequested)
        {
            using(var scope = mServiceScopeFactory.CreateScope())
            {
                // Get the event bus
                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                // Get all messages
                var events = await eventBus.GetAllMessages();

                // Get the database context
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // For each event
                foreach(var @event in events)
                {
                    // Convert the event to base event
                    var baseEvent = JsonSerializer.Deserialize<BaseEvent>(@event.message.Span)!;

                    // If the event has not been handled yet
                    if(!db.ConsumedEvents.Any(x => x.EventId == baseEvent.EventId))
                    {
                        // Add it to the list of handled messages
                        await db.ConsumedEvents.AddAsync(baseEvent);

                        // Switch the type of the event
                        switch(@event.messageType)
                        {
                            // Handle each event
                            case nameof(UserCreatedEvent):
                                await JsonSerializer.Deserialize<UserCreatedEvent>(@event.message.Span)!.Handle(db);
                                break;

                            case nameof(UserDeletedEvent):
                                await JsonSerializer.Deserialize<UserDeletedEvent>(@event.message.Span)!.Handle(db);
                                break;

                            default:
                                Debugger.Break();
                                throw new NotImplementedException();

                        }
                    }


                }

                // Save the changes to the database
                await db.SaveChangesAsync();

            }

            // Wait for a period of time before resuming
            try { await Task.Delay(10 * 1000, stoppingToken); } catch(TaskCanceledException ex) { }
        }
    }
}
