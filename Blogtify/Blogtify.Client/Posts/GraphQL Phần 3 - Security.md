---
url: [/graphql-334]
title: GraphQL Phần 3 - Security
$attribute: Category("ios 17", "ios")
$layout: Blogtify.Client.Layout.BlogPostLayout
---


# Rate Limiting

Rate limiting là một tính năng bảo mật rất quan trọng đối với bất kỳ API web nào. Trong khi các API của chúng ta phải đáp ứng các use case của khách hàng, việc định nghĩa các giới hạn mà tại đó các API client phải tuân thủ cũng rất quan trọng. Đối với các API theo endpoint, những giới hạn này thường được thể hiện dưới dạng số lượng request mỗi phút. Điều này hợp lý vì tải trên các server của chúng ta thường liên quan đến số lượng các endpoint bị gọi trong một khoảng thời gian.

Vấn đề là, cách tiếp cận này không thực sự hiệu quả đối với GraphQL. Để hiểu tại sao, hãy xem xét hai truy vấn sau đây:

**Truy vấn A:**

```graphql
query A {
  me {
    name
  }
}
```

---

**Truy vấn B:**

```graphql
query B {
  me {
    posts(first: 100) {
      author {
        followers(first: 100) {
          name
        }
      }
    }
  }
}
```

---

Nếu chúng ta áp dụng cách tiếp cận đơn giản để giới hạn số lượng request mỗi phút, chúng ta sẽ gặp vấn đề ngay lập tức. Dù việc giới hạn request có vẻ hợp lý đối với truy vấn đầu tiên, nhưng với cùng số lượng request, truy vấn thứ hai lại quá tốn kém. Vấn đề ở đây là truy vấn thứ hai phức tạp và tốn kém hơn nhiều so với truy vấn đầu tiên. Vì vậy, không thể áp dụng cùng một giới hạn request mỗi phút cho cả hai truy vấn này. Điều này lý giải tại sao các API GraphQL cần phải xem xét lại hoàn toàn cách thức rate limit các client. Dưới đây là một số kỹ thuật giúp API GraphQL có thể thực hiện rate limiting hiệu quả.


## Dựa trên độ phức tạp

Một trong những kỹ thuật phổ biến nhất để rate limit API GraphQL là sử dụng cách tiếp cận dựa trên độ phức tạp. Ý tưởng cơ bản là: liệu chúng ta có thể ước tính chi phí của một truy vấn không? Nếu chúng ta có thể nói rằng truy vấn B trong ví dụ trước "tốn" 100 điểm và truy vấn A chỉ tốn 1 điểm, chúng ta có thể sử dụng chỉ số này thay vì số lượng request để rate limit theo thời gian. Ví dụ, API của chúng ta có thể cho phép chỉ 1000 điểm mỗi phút. Khi đó, truy vấn A có thể được thực thi 1000 lần trong một phút, nhưng truy vấn B chỉ có thể thực hiện 10 lần. Điều này giúp chúng ta thể hiện chính xác mức độ tốn kém khi thực thi một truy vấn trên server, bảo vệ resource của chúng ta.

Có nhiều cách để tính toán độ phức tạp, và phương pháp của bạn có thể ảnh hưởng đến cách bạn tính toán chi phí. Tuy nhiên, một phương pháp hiệu quả là tính độ phức tạp dựa trên số lượng nodes thay vì các fields. Các field kiểu scalar thường không tốn kém nhiều khi máy chủ tính toán, vì chúng đã được tải từ một đối tượng đã được giải mã. Thay vào đó, chúng ta có thể tính số lượng loại đối tượng hoặc "nodes" mà server phải tải. Ví dụ, truy vấn sau có thể có chi phí là 2:

```
query {
  viewer {  # tốn 1
    name # bỏ qua vì là scalar
    bio # bỏ qua vì là scalar
    bestFriend {  # tốn 1
      name  # bỏ qua vì là scalar
    }
  }
}
```

---

## Dựa trên thời gian

Như đã thấy, việc tính toán "độ phức tạp" có thể khá khó khăn. Một phương pháp thay thế là giới hạn theo thời gian server. Cách này thực tế hơn vì chúng ta tính toán dựa trên thời gian server phản hồi. Thường thì điều này được thực hiện qua một middleware tính toán số mili-giây đã trôi qua từ khi request được nhận vào đến khi response được gửi đi.

So với phương pháp dựa trên độ phức tạp, phương pháp này khó hiểu hơn đối với các client. Với cách tiếp cận độ phức tạp, bạn có thể dễ dàng giải thích chi phí của một truy vấn. Bạn thậm chí có thể cung cấp công cụ cho phép client tính toán chi phí trước khi thực hiện. Tuy nhiên, với phương pháp này, client sẽ phải thử các truy vấn và xem chúng mất bao lâu hoặc điều chỉnh khi triển khai ứng dụng của họ.


Rất khó cho các client biết họ có đang trong giới hạn hay không, hoặc khi nào họ sẽ bị rate limited nếu tiếp tục gửi request. Vì lý do này, nhiều nhà cung cấp API muốn hiển thị trạng thái giới hạn cho client để giúp client tích hợp và đảm bảo rằng họ tuân thủ đúng giới hạn của API. Cách phổ biến nhất là thông qua các header trong response của API. Ví dụ:

```yaml
Status: 200
X-RateLimit-Limit: 5000
X-RateLimit-Remaining: 4999
X-RateLimit-Reset: 1372700873
```

---

* **RateLimit-Limit:** Đây là số lượng request tối đa mà người dùng có thể thực hiện trong một khoảng thời gian nhất định trước khi bị chặn.
* **RateLimit-Remaining:** Đây là số lượng request còn lại mà người dùng có thể thực hiện trước khi bị rate-limited. Tương tự như trong API REST, nhưng đối với GraphQL, có thể là số lượng chi phí độ phức tạp hoặc thời gian thực thi request còn lại.
* **RateLimit-Reset:** Đây là timestamp (dạng Unix) cho biết khi nào một chu kỳ mới sẽ bắt đầu, nghĩa là số lượng request còn lại sẽ được đặt lại về giới hạn ban đầu.

Ví dụ, để áp dụng chiến lược rate limiting, GitHub GraphQL API không chỉ sử dụng headers mà còn cung cấp field `rateLimit` trong GraphQL để truy vấn thông tin về chi phí, giới hạn, số lượng còn lại và thời gian đặt lại.

```graphql
query {
  rateLimit {
    cost
    limit
    remaining
    resetAt
  }
  user(id: "123") {
    login
  }
}
```

---

Một cách tiếp cận khác là sử dụng extensions trong response để cung cấp thông tin về rate limit. Shopify sử dụng trường extensions để cung cấp dữ liệu rate limit chi tiết, giúp người dùng có thể ước tính số lượng truy vấn còn lại:

```json
{
  "data": {
    "shop": {
      "name": "ProductionReadyGraphQL"
    }
  },
  "extensions": {
    "cost": {
      "requestedQueryCost": 1,
      "actualQueryCost": 1,
      "throttleStatus": {
        "maximumAvailable": 1000,
        "currentlyAvailable": 999,
        "restoreRate": 50
      }
    }
  }
}
```

---

Một cách tiếp cận khác nữa là không cung cấp thông tin chi tiết về trạng thái rate limit, mà thay vào đó là hướng dẫn và khuyến khích người dùng phản ứng đúng cách khi bị rate limited. Khi người dùng đạt giới hạn yêu cầu, server có thể trả về mã trạng thái `429 TOO MANY REQUESTS` kèm theo header `Retry-After`, thông báo thời gian mà người dùng có thể thử lại.

---

# Blocking Abusive Queries


## Giới hạn độ sâu của truy vấn

GraphQL cho phép xây dựng truy vấn lồng nhau rất sâu:

```graphql
query {
  product {
    variants {
      product {
        variants {
          product {
            variants {}
          }
        }
      }
    }
  }
}
```

---

Cần hạn chế độ sâu truy vấn để tránh truy vấn không hợp lý bằng cách sử dụng **depth validation**.

---

## Giới hạn chiều rộng của truy vấn

Truy vấn rộng cũng có thể gây quá tải:

```graphql
query {
  product1: product(id: "1") { ... }
  product2: product(id: "1") { ... }
  product3: product(id: "1") { ... }
  # ...
}
```

---

Cần thiết lập max complexity cho truy vấn và kết hợp với rate limiting để ngăn chặn việc gửi truy vấn quá lớn hoặc quá nhiều trong một khoảng thời gian.

---

## Giới hạn số lượng node

Giới hạn số lượng đối tượng (node) được trả về bởi truy vấn. 
**Ví dụ:** GitHub sử dụng cả giới hạn số node và giới hạn độ phức tạp để kiểm soát API.

---

## Giới hạn kích thước truy vấn và biến

**Đặt giới hạn về:**
* Tổng kích thước của truy vấn và biến.
* Số lượng phần tử trong danh sách truyền vào truy vấn.
**Mục đích:** Ngăn chặn các truy vấn bất thường có thể gây quá tải server, như danh sách tham số quá lớn.

---

# Timeout

Timeouts là cơ chế cần thiết để ngăn chặn các truy vấn quá phức tạp hoặc mất quá nhiều thời gian xử lý.

## Tại sao cần timeouts?

* Bất kể các biện pháp giới hạn độ phức tạp hay số lượng node, vẫn có khả năng truy vấn vượt qua các giới hạn này và mất quá nhiều thời gian xử lý.
* Thiết lập timeouts giống như "cơ chế bảo vệ cuối cùng," đảm bảo không truy vấn nào chạy quá lâu.

---

## Thời gian timeout hợp lý


* Không có giá trị "hoàn hảo" cho timeout, nhưng cần có giá trị timeout phù hợp với hệ thống.
* Với GraphQL, timeouts có thể được xem như một tính năng hơn là ngoại lệ, bởi vì:
	* Các truy vấn phức tạp yêu cầu nhiều tài nguyên có thể xảy ra thường xuyên.
	* Timeout giúp đảm bảo không có truy vấn nào gây tê liệt hệ thống.

---

## Kết hợp với các giới hạn khác

Timeout giúp giới hạn thời gian xử lý, nhưng vẫn cần:
* Giới hạn độ phức tạp (max complexity): Ngăn chặn truy vấn phức tạp trước khi cần timeout.
* Giới hạn số lượng node: Hạn chế số lượng đối tượng được trả về để giảm thời gian xử lý.

---

## Lợi ích của timeouts

* Đảm bảo hệ thống không bị quá tải bởi một truy vấn duy nhất.
* Tăng độ tin cậy và ổn định cho server khi chấp nhận các truy vấn tùy ý.
* Giúp phát hiện và điều chỉnh các giới hạn khác để cải thiện hiệu suất tổng thể.


---


# Authentication và Authorization

**Không nên tích hợp Authentication trực tiếp vào GraphQL schema. Bởi vì:**
* Các truy vấn/mutation như login hay logout làm cho server trở nên stateful hoặc phức tạp hóa việc xác thực từng trường.
* Nên xử lý xác thực thông qua middleware, đảm bảo token hoặc thông tin người dùng đã có sẵn trong context của GraphQL.

**Lợi ích của cách tiếp cận này:**
* Tăng tính linh hoạt, cho phép thay đổi hoặc hỗ trợ nhiều cơ chế xác thực mà không phải sửa schema.
* Giảm độ phức tạp và tăng tính stateless cho schema.

**Nguyên tắc xử lý Authorization trong GraphQL:**
* Tránh thực hiện tất cả logic phân quyền ở tầng GraphQL. Thay vào đó, dựa vào các tầng sâu hơn của ứng dụng. Ví dụ: API scopes nên được xử lý ở tầng GraphQL.
* Logic nghiệp vụ như "Chỉ admin mới được đóng issue" nên đặt ở tầng nghiệp vụ.

**Ưu tiên Authorization theo Object hơn là Field. Lý do là:**
* Dễ dàng thiết lập API scopes cho từng object type.
* Tránh việc bỏ sót quyền khi truy cập vào object thông qua các trường khác trong schema.


```graphql
type Query {
  adminThings: AdminOnlyType!
    @authorization(scopes: ["read:admin_only_types"])
  product: Product!
    @authorization(scopes: ["read:products"])
}

type Product {
  name: String
  settings: AdminOnlyType!
}
```

---

```csharp
public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.Field(x => x.Name).Authorize("read:products");
        descriptor.Field(x => x.Settings).Authorize("read:admin_only_types"); 
    }
}
```

---

Kiểm tra phân quyền

```csharp
public class ProductResolver
{
    public async Task<Product> GetProductAsync(string id, [Service] IAuthorizationService authService, [Service] IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext.Items["CurrentUser"] as User;
        
        if (user == null || !authService.HasPermission(user, "read:products"))
        {
            throw new UnauthorizedAccessException("You do not have access to this product.");
        }

        // Tiếp tục lấy dữ liệu sản phẩm từ database
        var product = await _productService.GetProductByIdAsync(id);
        return product;
    }
}
```

---


# Blocking Instrospective

Để giải quyết vấn đề về việc chặn hoặc giới hạn khả năng introspection trong GraphQL, dưới đây là cách tiếp cận có cấu trúc tùy thuộc vào môi trường của bạn, cho dù là API nội bộ hay công khai.

## Tắt Introspection trong Production Environment

Introspection thường được sử dụng trong development environment để khám phá schema của GraphQL, nhưng trong production environment, nó có thể tiết lộ thông tin nhạy cảm về API mà bạn có thể không muốn công khai.

**Development environment:** Bật introspection để các lập trình viên và các công cụ như GraphiQL hoặc Apollo Client có thể tương tác với API và khám phá các kiểu và truy vấn có sẵn.
**Production environment:** Cân nhắc tắt introspection cho các API nội bộ hoặc API có dữ liệu nhạy cảm.

```csharp
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production") 
{
    services.AddGraphQLServer()
        .AddQueryType<Query>()
        .AddMutationType<Mutation>()
        .AddInMemorySubscriptions()
        .ConfigureSchema(options => options.SetIntrospectionEnabled(false));
}
```

---

## Query Whitelisting

Trong một số trường hợp, đặc biệt là đối với API riêng tư hoặc nội bộ, bạn có thể muốn giới hạn các loại truy vấn mà client có thể thực thi. Điều này có thể được thực hiện thông qua Query Whitelisting, nơi chỉ các truy vấn đã đăng ký trước mới được phép thực thi.

Với Query Whitelisting, bạn sẽ xác định một bộ truy vấn đã biết và bất kỳ truy vấn nào không có trong danh sách sẽ bị chặn. Điều này có thể giúp ngăn người dùng truy cập các trường không được phép hoặc thực hiện các truy vấn không mong muốn. Đây là một tính năng hữu ích cho các ứng dụng nội bộ, nơi bạn không muốn cho phép các truy vấn tùy ý.

```csharp
public class QueryWhitelistingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISet<string> _whitelistedQueries;

    public QueryWhitelistingMiddleware(RequestDelegate next)
    {
        _next = next;
        _whitelistedQueries = new HashSet<string> { "query1", "query2" }; // Danh sách truy vấn đã phê duyệt
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var query = httpContext.Request.Body.ToString(); // Lấy truy vấn GraphQL
        if (!_whitelistedQueries.Contains(query))
        {
            httpContext.Response.StatusCode = 400; // Chặn truy vấn
            await httpContext.Response.WriteAsync("Truy vấn này không được phép.");
            return;
        }

        await _next(httpContext);
    }
}
```

---

## Visibility và Feature Flags

Nếu bạn muốn ẩn một số type hoặc field trong schema dựa trên các feature flags hoặc cài đặt cụ thể cho client, bạn có thể quản lý tính hiển thị của schema. Ví dụ, trong một hệ thống quản lý feature flags, bạn có thể chỉ hiển thị một phần của schema khi một feature flag được bật cho một client.

Bạn có thể triển khai cấu hình schema tùy chỉnh, nơi tính hiển thị của các field hoặc type có thể được thay đổi động dựa trên feature flag, user role, hoặc ID client.

```csharp
public class ProductType : ObjectType<Product>
{
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        var featureFlagEnabled = CheckFeatureFlag("newProductFeature"); // Logic kiểm tra cờ tính năng
        
        if (featureFlagEnabled)
        {
            descriptor.Field(x => x.NewFeature).Type<StringType>(); // Hiển thị nếu cờ tính năng bật
        }
        else
        {
            descriptor.Field(x => x.NewFeature).Ignore(); // Ẩn nếu cờ tính năng tắt
        }
    }
}
```

---


# Persisted Queries

Persisted queries là một kỹ thuật mạnh mẽ giúp tối ưu hóa API GraphQL bằng cách loại bỏ việc gửi toàn bộ tài liệu truy vấn trong mỗi yêu cầu. Dưới đây là cái nhìn chi tiết về cách persisted queries hoạt động, các lợi ích và cách triển khai chúng:

## Cách thức hoạt động của Persisted Queries


---

Thông thường, khi một client gửi yêu cầu đến máy chủ GraphQL, nó sẽ gửi toàn bộ tài liệu truy vấn (dưới dạng chuỗi) đến máy chủ. Sau đó, máy chủ sẽ xử lý truy vấn bằng cách phân tích cú pháp, xác thực và thực thi nó. Tuy nhiên, quá trình này có thể không hiệu quả, đặc biệt là khi truy vấn giống nhau được gửi đi nhiều lần. Persisted queries giải quyết vấn đề này bằng cách thay thế tài liệu truy vấn đầy đủ bằng một định danh duy nhất (ví dụ: một mã băm hoặc URL) tham chiếu đến truy vấn.

**Quy trình hoạt động:**
* **Đăng ký:** Client đăng ký một truy vấn với máy chủ, có thể thực hiện trong quá trình triển khai hoặc khi yêu cầu đầu tiên được gửi.
* **Trả về định danh:** Máy chủ cung cấp cho client một định danh (ví dụ: mã băm hoặc URL) cho truy vấn đã đăng ký.
* **Gửi định danh:** Thay vì gửi toàn bộ truy vấn, client sẽ chỉ gửi định danh của truy vấn cùng với các biến cần thiết khi thực hiện truy vấn.

---

## Lợi ích của Persisted Queries

* **Tiết kiệm băng thông:** Client không cần gửi toàn bộ tài liệu truy vấn trong mỗi yêu cầu, chỉ gửi định danh, giúp tiết kiệm băng thông.
* **Tối ưu hóa máy chủ:** Máy chủ có thể tiền xử lý các truy vấn (parsing, validation, và phân tích) trước, giúp giảm tải xử lý khi nhận yêu cầu.
* **Bảo mật:** Persisted queries giúp cải thiện bảo mật bằng cách chỉ cho phép thực thi các truy vấn đã đăng ký, giúp ngăn chặn việc thực thi các truy vấn không xác định hoặc không an toàn.
* **Tăng hiệu suất:** Việc tiền xử lý truy vấn và tối ưu hóa máy chủ giúp API GraphQL hoạt động hiệu quả hơn, đặc biệt khi phải xử lý các truy vấn lớn.


---

## Ứng dụng của Persisted Queries

Persisted queries rất hữu ích đối với các API nội bộ, vì chúng giúp kiểm soát và tối ưu hóa các truy vấn có thể được thực thi. Bằng cách chỉ cho phép các truy vấn đã đăng ký, các API nội bộ có thể tránh được các truy vấn không mong muốn hoặc có thể gây tốn kém về hiệu suất.

Các API công khai cũng có thể hưởng lợi từ persisted queries, mặc dù có thể ít phổ biến hơn đối với các trường hợp API công cộng mở rộng. Dù vậy, persisted queries có thể trở thành một phương pháp quan trọng cho các API công cộng trong tương lai.




