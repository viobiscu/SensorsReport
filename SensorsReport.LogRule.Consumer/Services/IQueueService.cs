﻿using System.Text.Json;

namespace SensorsReport.LogRule.Consumer;

public interface IQueueService
{
    Task StartConsumingAsync(Func<string, ulong, Task> onMessageReceived, CancellationToken cancellationToken);
    void AcknowledgeMessage(ulong deliveryTag);
    void RejectMessage(ulong deliveryTag, bool requeue = false);
}
