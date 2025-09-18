---
url: [/webhook1234]
title: "Tìm hiểu về Webhook và cách triển khai"
$attribute: Category("Web")
$layout: Blogtify.Client.Layout.BlogPostLayout
---

# Tổng quan 

## Webhook là gì?

Webhook là một cơ chế truyền thông tin giữa các ứng dụng thông qua giao thức HTTP, cho phép một ứng dụng gửi thông báo thời gian thực đến ứng dụng khác khi một sự kiện cụ thể xảy ra. Thay vì phải liên tục gửi yêu cầu (polling) để kiểm tra thông tin mới, webhook giúp tiết kiệm tài nguyên và tăng tốc độ phản hồi bằng cách tự động gửi dữ liệu ngay khi cần thiết.

---

## Cách hoạt động của Webhook

1. **Đăng ký webhook:** Ứng dụng A (client) đăng ký một URL để nhận thông báo từ ứng dụng B (server).
2. **Kích hoạt sự kiện:** Khi một sự kiện xảy ra trong ứng dụng B, nó sẽ gửi một yêu cầu HTTP (thường là POST) đến URL đã đăng ký.
3. **Xử lý thông báo:** Ứng dụng A nhận thông báo từ ứng dụng B, xử lý dữ liệu được gửi đến và thực hiện các hành động cần thiết.

---

## Các thành phần chính của Webhook

* **URL Webhook:** Địa chỉ mà dữ liệu sẽ được gửi đến.
* **Payload:** Dữ liệu chi tiết về sự kiện (thường ở định dạng JSON).
* **HTTP Headers:** Chứa thông tin bổ sung như *Content-Type* hoặc mã xác thực để đảm bảo bảo mật.
* **HTTP Method:** Thông thường sử dụng phương thức POST.

---

## Ứng dụng phổ biến của Webhook

* **Thanh toán trực tuyến:** Nhận thông báo khi giao dịch thành công.
* **Quản lý đơn hàng:** Tự động cập nhật trạng thái đơn hàng.
* **Ứng dụng chat:** Gửi tin nhắn tự động từ bot.

---

# Triển khai

## RabbitMQ

```yaml
version: '3'
services:
  rabbitmq:
    image: "rabbitmq:3-management"
    hostname: "rabbit123"
    ports:
      - "15672:15672"
      - "5672:5672"
    labels:
      NAME: "my-rabbit"
```

---

## Triển khai Webhook Api (Server)

**Models**

```csharp
public class WebhookSubscription
{
    public int Id { get; set; }
    public string WebhookUri { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string WebhookType { get; set; } = string.Empty;
    public string WebhookPublisher { get; set; } = string.Empty;
}
```

```csharp
public class CreateWebhookSubscriptionRequest
{
    public string WebhookUri { get; set; } = string.Empty;
    public string WebhookType { get; set; } = string.Empty;
}

public class WebhookSubscriptionDto
{
    public string WebhookUri { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string WebhookType { get; set; } = string.Empty;
    public string WebhookPublisher { get; set; } = string.Empty;
}
```


```csharp
[ApiController]
[Route("api/[controller]")]
public class WebhookSubscriptionController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public WebhookSubscriptionController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActionResult<WebhookSubscriptionDto>> GetSubscription(string secret)
    {
        var subscription = await _dbContext.WebhookSubscriptions
                                    .FirstOrDefaultAsync(x => x.Secret == secret);

        if (subscription == null)
        {
            return NotFound();
        }

        return Ok(new WebhookSubscriptionDto()
        {
            WebhookUri = subscription.WebhookUri,
            WebhookType = subscription.WebhookType,
            Secret = subscription.Secret,
            WebhookPublisher = subscription.WebhookPublisher
        });
    }

    public async Task<ActionResult<WebhookSubscriptionDto>> CreateSubscription(CreateWebhookSubscriptionRequest request)
    {
        var subscription = await _dbContext.WebhookSubscriptions
                                    .FirstOrDefaultAsync(x => x.WebhookUri == request.WebhookUri);

        if (subscription != null)
        {
            return NoContent();
        }

        try
        {
            _dbContext.WebhookSubscriptions.Add(new WebhookSubscription()
            {
                WebhookUri = request.WebhookUri,
                WebhookType = request.WebhookType,
                Secret = Guid.NewGuid().ToString(),
                WebhookPublisher = "WebhookDemo"
            });

            await _dbContext.SaveChangesAsync();
            return Ok(new WebhookSubscriptionDto()
            {
                WebhookUri = request.WebhookUri,
                WebhookType = request.WebhookType,
                Secret = subscription.Secret
            });
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }
}
```

---

## Tạo trang đăng ký webhook


Form cho phép nhập vào Webhook URI, chọn Webhook Type

---

## Triển khai endpoint cho Client

```csharp
public class WebhookSecret
{
    public int Id { get; set; }
    public string Secret { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
}

public class ReceivedDataDto
{
    public string Secret { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string WebhookType { get; set; } = string.Empty;

    // the payload received from the webhook
}
```

---

```csharp
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    public NotificationsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    [HttpPost]
    public async Task<ActionResult> SomethingChanged(ReceivedDataDto data)
    {
        Console.WriteLine($"Webhook received from {data.Publisher}!");
        var secretModel = await _dbContext.WebhookSecrets
                        .FirstOrDefaultAsync(x => x.Publisher == data.Publisher
                                && x.Secret == data.Secret);

        if (secretModel == null)
        {
            Console.WriteLine("Invalid secret.");
            return Ok();
        }

        // do something with the data

        return Ok();
    }
}
```

---

## Server gửi message tới Message Queue

```csharp
public class NotificationMessageDto
{
    public string Id { get; }
    public string WebhookType { get; set; } = string.Empty;

    // data from the webhook

    public NotificationMessageDto()
    {
        Id = Guid.NewGuid().ToString();
    }
}
```

---

```csharp
public interface IMessageBusClient
{
    void SendMessage(NotificationMessageDto message);
}

public class MessageBusClient : IMessageBusClient
{
    public async void SendMessage(NotificationMessageDto message)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672
        };

        using (var connection = await factory.CreateConnectionAsync())
        using (var channel = await connection.CreateChannelAsync())
        {
            await channel.ExchangeDeclareAsync("trigger", ExchangeType.Fanout);

            var serializedMessage = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(serializedMessage);

            await channel.BasicPublishAsync("trigger", "", body);
        }
    }
}
```

---

Khi có thay đổi, gửi message tới Message Queue.

---

## Gửi message trong Message Queue tới các Client

Nhận message từ Message Queue và gửi request tới Endpoint ở Client đã tạo.

```csharp
public class DataChangePayloadDto
{
    public string WebhookUri { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string WebhookType { get; set; } = string.Empty;

    // data need to sent to the subscribers
}
```

---

```csharp
public interface IWebhookClient
{
    Task SendWebhookNotification(DataChangePayloadDto changePayloadDto);
}

public class WebhookClient : IWebhookClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WebhookClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task SendWebhookNotification(DataChangePayloadDto changePayloadDto)
    {
        var content = new StringContent(JsonSerializer.Serialize(changePayloadDto), Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();

        var response = await httpClient.PostAsync(changePayloadDto.WebhookUri, content);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to send webhook notification.");
        }
        else
        {
            Console.WriteLine("Webhook notification sent successfully.");
        }
        
    }
}
```

---

```csharp
public class AppHost
{
    private readonly AppDbContext _dbContext;
    private readonly IWebhookClient _webhookClient;

    public AppHost(AppDbContext dbContext, IWebhookClient webhookClient)
    {
        _dbContext = dbContext;
        _webhookClient = webhookClient;
    }
    public async Task Run()
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672
        };

        using (var connection = await factory.CreateConnectionAsync())
        using (var channel = await connection.CreateChannelAsync())
        {
            await channel.ExchangeDeclareAsync("trigger", ExchangeType.Fanout);

            var queueName = (await channel.QueueDeclareAsync(
                "trigger",
                durable: true,
                exclusive: false,
                autoDelete: false)).QueueName;

            await channel.QueueBindAsync(queueName, "trigger", "");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var messageString = Encoding.UTF8.GetString(body);
                var messageDto = JsonSerializer.Deserialize<NotificationMessageDto>(messageString);

                var webhookToSend = new DataChangePayloadDto
                {
                    WebhookType = messageDto.WebhookType,
                    WebhookUri = string.Empty,
                    Secret = string.Empty,
                    Publisher = string.Empty
                    // other properties
                };

                var webhookSubscriptions = _dbContext.WebhookSubscriptions
                    .Where(x => x.WebhookType.Equals(messageDto.WebhookType));
                foreach (var webhookSubscription in webhookSubscriptions)
                {
                    webhookToSend.WebhookUri = webhookSubscription.WebhookUri;
                    webhookToSend.Secret = webhookSubscription.Secret;
                    webhookToSend.Publisher = webhookSubscription.WebhookPublisher;
                    await _webhookClient.SendWebhookNotification(webhookToSend);
                }

                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queueName, true, consumer);
        }
    }
}
```
