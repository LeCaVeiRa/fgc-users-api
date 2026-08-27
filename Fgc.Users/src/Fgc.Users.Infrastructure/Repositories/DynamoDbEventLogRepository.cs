using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Fgc.Users.Application.Interfaces;
using System.Text.Json;

namespace Fgc.Users.Infrastructure.Repositories
{
    public class DynamoDbEventLogRepository(IAmazonDynamoDB dynamoDb) : IEventLogRepository
    {
        private const string TableName = "FgcEventLog";
        private const string ServiceName = "UsersApi";

        public async Task LogAsync(string eventType, object payload, CancellationToken cancellationToken)
        {
            var occurredAt = DateTime.UtcNow;
            var eventId = Guid.NewGuid();

            var item = new Dictionary<string, AttributeValue>
            {
                ["ServiceName"] = new AttributeValue { S = ServiceName },
                ["OccurredAtId"] = new AttributeValue { S = $"{occurredAt:O}#{eventId}" },
                ["EventType"] = new AttributeValue { S = eventType },
                ["Payload"] = new AttributeValue { S = JsonSerializer.Serialize(payload) }
            };

            await dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = TableName,
                Item = item
            }, cancellationToken);
        }
    }
}
