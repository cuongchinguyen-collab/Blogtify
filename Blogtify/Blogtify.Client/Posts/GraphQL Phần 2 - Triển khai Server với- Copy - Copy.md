---
url: [/graphql-311233231213212321314334]
title: GraphQL Phần 2 - Triển khai Server với HotChocolate
$layout: Blogtify.Client.Layout.BlogPostLayout
---


[Getting Started with Hot Chocolate](https://chillicream.com/docs/hotchocolate/v14/get-started-with-graphql-in-net-core)


# Tạo các SDL Artifact

## Các bước

HotChocolate cung cấp một phương thức ToString cho schema, cho phép in ra SDL của schema đã định nghĩa.

```csharp
using HotChocolate;
using HotChocolate.Execution;

public class SchemaPrinter
{
	public static string PrintSchema(ISchema schema)
	{
		return schema.ToString();
	}
}
```

---

```csharp
var schema = SchemaBuilder.New()
	.AddQueryType<QueryType>()
	.AddType<UserType>()
	.Create();

var sdl = SchemaPrinter.PrintSchema(schema);
Console.WriteLine(sdl);
```

---

**In SDL với Metadata hoặc Directives:**

Nếu muốn sử dụng directives hoặc thêm các metadata vào schema, bạn có thể định nghĩa một directive tùy chỉnh trong HotChocolate.

```csharp
public class FeatureFlagDirective : DirectiveType
{
	public FeatureFlagDirective()
	{
		Name = "featureFlagged";
		Location = DirectiveLocation.FieldDefinition | DirectiveLocation.TypeDefinition;
		Argument("name").Type<StringType>();
	}
}

public class UserType : ObjectType<User>
{
	protected override void Configure(IObjectTypeDescriptor<User> descriptor)
	{
		descriptor.Field(x => x.Name).Type<StringType>();
		descriptor.Field(x => x.SecretField)
				  .Type<StringType>()
				  .Directive("featureFlagged", d => d.Argument("name", "secret"));
	}
}

public class QueryType : ObjectType<Query>
{
	protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
	{
		descriptor.Field(x => x.GetUser())
				  .Type<UserType>();
	}
}
```

---

## Lợi ích của việc tạo SDL Artifact

* **Đồng bộ Schema:** Việc tạo SDL từ code giúp đảm bảo rằng schema trong tài liệu luôn được cập nhật với các thay đổi trong mã nguồn.
* **Dễ dàng kiểm tra:** SDL artifact có thể được kiểm tra trong hệ thống quản lý mã nguồn (version control), giúp các lập trình viên khác dễ dàng thấy các thay đổi đối với schema.
* **Hỗ trợ nhiều công cụ:** Bạn có thể sử dụng các công cụ như linter, documentation generators, và breaking change detectors để làm việc với SDL.

---

## Middleware

```csharp
public static class UseToUpperObjectFieldDescriptorExtensions
{
	public static IObjectFieldDescriptor UseToUpper(this IObjectFieldDescriptor descriptor)
	{
		return descriptor.Use(next => async context =>
		{
			await next(context);
			if (context.Result is string str)
			{
				context.Result = str.ToUpperInvariant();
			}
		});
	}
}

public class UseToUpperAttribute : ObjectFieldDescriptorAttribute
{
	public UseToUpperAttribute([CallerLineNumber] int order = default)
	{
		Order = order;
	}
	protected override void OnConfigure(
		IDescriptorContext context,
		IObjectFieldDescriptor descriptor,
		MemberInfo member)
	{
		descriptor.UseToUpper();
	}
}
```



---


# Relay Pattern

## Các thành phần chính

Relay Pattern được xây dựng dựa trên một số khái niệm và quy ước cơ bản:

* **Connection Pattern:** Relay sử dụng **Connection Pattern** để đảm bảo tính đồng nhất trong việc xử lý dữ liệu dạng danh sách.
* **Global Object Identification:** Relay yêu cầu mỗi đối tượng trong hệ thống phải có một định danh toàn cục (Global ID), thường được xử lý bằng field `id`.
* **Mutations:** Relay cũng định nghĩa một cấu trúc tiêu chuẩn cho các mutation, đảm bảo rằng các thay đổi dữ liệu được xử lý thống nhất.
	* **Input Object:** Mutation nhận một đối tượng input.
	* **Payload Object:** Kết quả trả về của mutation bao gồm một payload chứa các field liên quan.
* **Fragments:** Relay khuyến khích sử dụng fragments để tối ưu hóa và tái sử dụng các phần của truy vấn.

---

## Lợi ích

* Giảm thiểu over-fetching và under-fetching dữ liệu.
* Fragments giúp tái sử dụng các phần truy vấn dễ dàng.
* Định danh toàn cục và cấu trúc chuẩn hóa giúp quản lý dữ liệu trên client hiệu quả.
* Connection Pattern hỗ trợ tốt các danh sách lớn bằng việc phân trang.


---

# Entity Pattern




