---
url: [/graphql37678788]
title: GraphQL Phần 3 - Thiết kế Schema
$layout: Blogtify.Client.Layout.BlogPostLayout
---


# Design First

* Ưu tiên suy nghĩ và thiết kế schema ngay từ đầu để tránh phụ thuộc vào chi tiết triển khai nội bộ.
* Public API rất khó thay đổi sau khi phát hành, vì vậy cần đảm bảo thiết kế tốt để giảm thiểu rủi ro về breaking changes.

---

# Client First

* GraphQL là một API tập trung vào client, nên thiết kế dựa trên use case của client thay vì tập trung vào resource của backend.
* Làm việc với client ngay từ đầu, sử dụng mock server để kiểm tra và tinh chỉnh API trước khi phát hành.
* API nên cung cấp đủ tính năng để đáp ứng nhu cầu của client, không dư thừa hoặc tiết lộ những thông tin không cần thiết (YAGNI).
* Khi xử lý vấn đề của client, nên tìm hiểu nguyên nhân thay vì áp dụng ngay giải pháp mà họ đề xuất.

---

* Tránh thiết kế schema phụ thuộc vào database, ngôn ngữ lập trình hoặc các yếu tố nội bộ khác.
* Schema nên tập trung vào chức năng mà client cần, giúp việc thay đổi nội bộ không ảnh hưởng đến API bên ngoài.
* Hiệu suất (performance) và tính sẵn sàng (availability) đôi khi sẽ ảnh hưởng đến thiết kế nhưng vẫn phải đặt nhu cầu client lên hàng đầu.

---

**Công cụ tự động sinh schema từ database hoặc API REST thường dẫn đến thiết kế kém:**
* Coupled với backend, khó thay đổi.
* Quá chung chung, không phục vụ tốt cho client.
* Tiết lộ quá nhiều, vi phạm nguyên tắc YAGNI.
* Các công cụ này chỉ phù hợp cho prototype nhanh, không nên dùng cho hệ thống sản xuất.

---

# Naming

## Tầm quan trọng của việc đặt tên

* Tên trong API giống như ngôn ngữ mà API dùng để "nói chuyện" với người dùng, giúp họ hiểu chức năng ngay lập tức mà không cần đọc tài liệu.
* Một cái tên tốt có thể tự định hướng thiết kế và làm cho API dễ hiểu hơn.

---

## Nguyên tắc đặt tên hiệu quả

### Tính nhất quán là yếu tố quan trọng nhất

* API dễ học và sử dụng hơn khi cách đặt tên nhất quán. Ví dụ, các hành động như truy vấn hoặc tạo dữ liệu nên tuân theo một kiểu đặt tên đồng bộ trong toàn bộ schema.
* Tránh sử dụng các tên không nhất quán, như products và findPosts, hoặc addProduct và createPost.
* Đồng bộ hóa các khái niệm trong domain: Không nên đặt tên một loại dữ liệu với nhiều cách khác nhau trừ khi chúng thực sự khác biệt (VD: Post vs. BlogPost).

---

### API symmetry

* API cần đối xứng giữa các hành động tương tự.
* Ví dụ, nếu có publishPost, cũng nên có unpublishPost để giữ tính logic và dễ dự đoán.
* Sự đối xứng tạo cảm giác tự nhiên và tránh làm người dùng API ngạc nhiên.

---

### Nguyên tắc "Principle of least astonishment"

* API nên được thiết kế sao cho hành vi của nó phù hợp với kỳ vọng của người dùng.
* Tránh các trường hợp bất ngờ khi một hành động hoặc cấu trúc không như mong đợi.

---

## Hậu quả của việc đặt tên không tốt

* Gây khó khăn cho client khi khám phá và sử dụng API.
* Tăng khả năng xảy ra lỗi khi client dự đoán sai cách sử dụng các field hoặc mutation.
* Tạo ra một trải nghiệm không thân thiện, làm giảm giá trị của API.

---

## Ví dụ về cách đặt tên sai

* API ban đầu sử dụng User làm kiểu cho viewer (người dùng đã đăng nhập) và team members (thành viên nhóm).
* Sau đó nhận ra thông tin riêng tư của viewer không nên bị lộ khi dùng User cho các thành viên nhóm.
* Kết quả là phải thực hiện thay đổi lớn: thêm kiểu mới (Viewer và TeamMember) hoặc xử lý lỗi mỗi khi dữ liệu không hợp lệ, gây khó khăn và bất tiện.

**Giải pháp là sử dụng tên cụ thể ngay từ đầu:** Tách riêng vai trò hoặc khái niệm khác biệt thành các kiểu riêng.

```graphql
interface User {
	name: String!
}

type Viewer implements User {
	name: String!
	hasTwoFactorAuthentication: Boolean
	billing: Billing!
}

type TeamMember implements User {
	name: String!
	isAdmin: Boolean!
}
```

---

# Description

Các thành phần trong GraphQL schema có thể được document bằng Description trực tiếp trong schema.
Trong SDL (Schema Definition Language), mô tả được định nghĩa bằng chuỗi trong dấu `"""`.

```graphql
"""
An order represents a `Checkout`
that was paid by a `Customer`
"""
type Order {
	items: [LineItem!]!
}
```

---

## Lợi ích của Descriptions

* **Truy cập nhanh chóng:** Thông tin mô tả được tích hợp vào schema, giúp người dùng nhanh chóng xem thông tin qua công cụ như GraphiQL.
* **Cải thiện khả năng hiểu:** Diễn giải mục đích của các type, field, và mutation trong schema.

---

## Hạn chế và lưu ý

* **Không nên phụ thuộc hoàn toàn vào mô tả:**
	* Một schema tốt nên tự nó thể hiện ý nghĩa và cách sử dụng thông qua tên và cấu trúc.
	* Mô tả chỉ nên bổ sung thông tin, không phải yếu tố cốt lõi để hiểu API.
* **Dấu hiệu thiết kế schema chưa tốt:** Các mô tả chứa nhiều edge case, điều kiện đặc biệt, hoặc hành vi phụ thuộc ngữ cảnh thường phản ánh thiết kế schema không rõ ràng.

---

# Tận dụng Schema trong GraphQL

## Sức mạnh của GraphQL Schema

* GraphQL cung cấp hệ thống type mạnh mẽ và biểu đạt rõ ràng, giúp thiết kế API dễ hiểu và giảm phụ thuộc vào hành vi runtime.
* Sử dụng đầy đủ khả năng của schema giúp API tự mô tả và cung cấp đảm bảo trong runtime.

---

## Ví dụ cải thiện schema với enum

**Schema ban đầu**: type là kiểu String, khiến người dùng khó hiểu và xử lý giá trị có thể nhận được.

```graphql
type Product {
	name: String!
	priceInCents: Int!
	type: String!
}
```

---

**Schema cải tiến**

```graphql
enum ProductType {
	APPAREL
	FOOD
	TOYS
}
type Product {
	name: String!
	priceInCents: Int!
	type: ProductType!
}
```


---

## Tránh dữ liệu không có cấu trúc

**Ví dụ không tốt:** Sử dụng kiểu JSON hoặc chuỗi không cấu trúc làm mất đi lợi thế của strong schema.

```graphql
type Product {
	metaAttributes: JSON!
}
type User {
	tags: String! # JSON encoded string with a list of user tags
}
```

---

**Cải thiện:** Typed Schema giúp client dễ xử lý dữ liệu, cải thiện tính ổn định và khả năng mở rộng mà không phá vỡ client.


```graphql
type ProductMetaAttribute {
	key: String!
	value: String!
}
type Product {
	metaAttributes: [ProductMetaAttribute!]!
}
```

---

## Sử dụng Custom Scalars

Thay vì sử dụng `String` cho dữ liệu phức tạp, custom scalars cung cấp thông tin ngữ nghĩa rõ ràng hơn:

```graphql
"""
Markdown là scalar tùy chỉnh, 
cho phép client hiểu rằng giá trị 
có thể được xử lý như markdown hợp lệ.
"""
type Product {
	description: Markdown 
}
scalar Markdown
```

---


# Expressive Schema

Một API có tính biểu diễn mạnh mẽ là một API mà client dễ dàng hiểu và sử dụng. Sử dụng các kiểu dữ liệu và quy ước chính xác sẽ giúp giảm thiểu lỗi và giúp việc sử dụng API trở nên trực quan hơn. Thay vì để client phải đoán trước cách sử dụng API, GraphQL sử dụng sơ đồ để diễn đạt rõ ràng cách thức sử dụng.

## Sử dụng Nullability

Giả sử chúng ta có một field `findProduct` có thể nhận cả `id` và `name` để tìm kiếm một sản phẩm:

```graphql
type Query {
  findProduct(id: ID, name: String): Product
}
```

---

Ở đây, vấn đề là nếu client không cung cấp bất kỳ tham số nào hoặc cung cấp cả hai tham số, API sẽ gặp lỗi. Thay vì vậy, ta có thể chia nó thành các field riêng biệt, mỗi field yêu cầu một tham số bắt buộc. Điều này giúp API trở nên dễ hiểu hơn, yêu cầu client phải cung cấp ít nhất một tham số (và không thể truyền cả hai), từ đó giảm thiểu sai sót trong việc sử dụng API.

```graphql
type Query {
  productByID(id: ID!): Product
  productByName(name: String!): Product
}
```

---

## Cải tiến sơ đồ thanh toán

Trong sơ đồ thanh toán, giả sử ta có một kiểu `Payment` với các field như số thẻ tín dụng và mã thẻ quà tặng:


```graphql
type Payment {
  creditCardNumber: String
  creditCardExp: String
  giftCardCode: String
}
```

---

Vấn đề là các field này đều có thể là `null`, khiến cho việc xác định dữ liệu cần cung cấp trở nên khó khăn cho client. Ta có thể cải thiện sơ đồ này bằng cách sử dụng các kiểu dữ liệu rõ ràng hơn:

```graphql
type Payment {
  creditCard: CreditCard
  giftCardCode: String
}

type CreditCard {
  number: CreditCardNumber!
  expiration: CreditCardExpiration!
}

scalar CreditCardNumber

type CreditCardExpiration {
  isExpired: Boolean!
  month: Int!
  year: Int!
}
```

---

Ở đây, ta sử dụng kiểu đối tượng CreditCard để nhóm các field liên quan đến thẻ tín dụng. Các field như number và expiration giờ trở thành bắt buộc (!), giúp đảm bảo tính nhất quán và tránh các trường hợp không hợp lệ khi client gửi dữ liệu.

---


## Nguyên tắc "Làm cho các trạng thái không thể có trở nên không thể"

Một trong những nguyên tắc quan trọng trong thiết kế GraphQL là đảm bảo rằng các trạng thái không hợp lệ không thể tồn tại trong hệ thống. Ví dụ, nếu một giỏ hàng đã được thanh toán, thì `amountPaid` không thể là `null`.

```graphql
type Cart {
  paid: Boolean
  amountPaid: Money
  items: [CartItem!]!
}
```

---

Trong ví dụ này, nếu giỏ hàng đã thanh toán nhưng amountPaid là null, hoặc nếu giỏ hàng chưa thanh toán nhưng amountPaid có giá trị, đây sẽ là các trường hợp không hợp lý. Để tránh điều này, ta có thể cấu trúc lại sơ đồ như sau:

```graphql
type Cart {
  payment: Payment
  items: [CartItem!]!
}

type Payment {
  paid: Boolean!
  amountPaid: Money!
}
```

---

Với cấu trúc này, nếu có field `payment`, thì chắc chắn giỏ hàng đã được thanh toán và cả `paid` và `amountPaid` đều phải có giá trị hợp lệ.

---

## Sắp xếp

Một ví dụ khác là khi cần lấy danh sách sản phẩm với tham số sắp xếp (sort), ta có thể cung cấp tham số này dưới dạng tùy chọn. Tuy nhiên, nếu không có tài liệu rõ ràng, client sẽ không biết giá trị mặc định là gì:

```graphql
type Query {
  products(sort: SortOrder): [Product!]!
}
```

---

Để làm rõ vấn đề này, ta có thể thiết lập giá trị mặc định cho tham số sort trong Schema. Điều này giúp client biết được rằng mặc định, sản phẩm sẽ được sắp xếp theo thứ tự giảm dần (`DESC`) nếu không có tham số sắp xếp.

```graphql
type Query {
  products(sort: SortOrder = DESC): [Product!]!
}
```

---

## Cụ thể hay tổng quát?

Việc chọn cách thiết kế API cụ thể hay tổng quát luôn là một chủ đề gây tranh cãi.

**API Cụ thể:**
* Ưu điểm: Tối ưu hóa tốt cho những trường hợp sử dụng cụ thể, dễ đọc, dễ tối ưu và dễ cache.
* Nhược điểm: Thiếu tính tùy chỉnh cho các client có nhu cầu khác.

**API Tổng quát:**
* Ưu điểm: Linh hoạt hơn, cho phép xử lý nhiều trường hợp sử dụng không lường trước.
* Nhược điểm: Khó tối ưu hóa, gây phức tạp cho server và khó sử dụng hơn đối với client.

**Phương pháp tiếp cận trong GraphQL:**
* GraphQL khuyến khích cung cấp dữ liệu mà client cần một cách chính xác.
* Nên xây dựng schema với các field đơn giản, phục vụ một mục đích cụ thể.
* Tránh các field quá tổng quát hoặc chứa tham số boolean vì chúng khó đọc và tối ưu.

### Ví dụ minh hoạ

#### Field quá tổng quát

```graphql
type Query {
  posts(first: Int!, includeArchived: Boolean): [Post!]!
}
```

---

`posts` xử lý cả bài viết thường và bài viết lưu trữ, làm tăng độ phức tạp.
**Giải pháp:** Tách thành các field cụ thể, giúp dễ hiểu hơn, tối ưu hóa và cache hiệu quả hơn.

```graphql
type Query {
  posts(first: Int!): [Post!]!
  archivedPosts(first: Int!): [Post!]!
}
```

---

#### Filter quá tổng quát

```graphql
query {
  posts(where: [
	{ attribute: DATE, gt: "2019-08-30" },
	{ attribute: TITLE, includes: "GraphQL" }
  ]) {
	id
	title
	date
  }
}
```

---

Filter phức tạp giống SQL, yêu cầu server xử lý nhiều trường hợp.
**Giải pháp:** Sử dụng field cụ thể cho từng use case, giúp dễ hiểu và dễ triển khai hơn.

```graphql
type Query {
  filterPostsByTitle(
	includingTitle: String!,
	afterDate: DateTime
  ): [Post!]!
}
```

---



### Anemic GraphQL

Anemic GraphQL là một khái niệm được lấy cảm hứng từ Anemic Domain Model, do Martin Fowler phổ biến. Trong bối cảnh GraphQL, nó ám chỉ việc thiết kế các schema như những "túi dữ liệu rỗng" mà không chú ý đến hành động, use case, hay chức năng. Thay vào đó, chúng ta nên thiết kế schema dựa trên domain thực tế, cung cấp đúng dữ liệu mà client cần, không chỉ dữ liệu thô. 

#### Anemic Schema

```graphql
type Discount {
  amount: Money!
}

type Product {
  price: Money!
  discounts: [Discount!]!
}
```

--- 

Client cần tính tổng giá trị thực tế sau khi áp dụng discount. Logic phía client như sau:

```javascript
const discountAmount = product.discounts.reduce((total, discount) => 
  total + discount.amount, 0);
const totalPrice = product.price - discountAmount;
```

---

Nhưng nếu schema thay đổi để thêm thuế, logic cũ sẽ không còn chính xác:

```graphql
type Product {
  price: Money!
  discounts: [Discount!]!
  taxes: Money!
}
```

---

**Giải pháp:** Thêm `totalPrice` vào schema

```graphql
type Product {
  price: Money!
  discounts: [Discount!]!
  taxes: Money!
  totalPrice: Money!
}
```

---

#### Anemic Mutation

```graphql
type Mutation {
  updateCheckout(input: UpdateCheckoutInput): UpdateCheckoutPayload
}

input UpdateCheckoutInput {
  email: Email
  address: Address
  items: [ItemInput!]
  creditCard: CreditCard
  billingAddress: Address
}
```

---

**Vấn đề:**
* Client phải đoán dữ liệu cần thay đổi để thực hiện một hành động cụ thể.
* Mutation khó đoán, dễ gây lỗi runtime.
* Schema phải làm mọi field nullable, không rõ ràng.

**Giải pháp:** Tạo mutation riêng biệt

```graphql
type Mutation {
  addItemToCheckout(input: AddItemToCheckoutInput): AddItemToCheckoutPayload
}

input AddItemToCheckoutInput {
  checkoutID: ID!
  item: ItemInput!
}
```

---

**Lợi ích:**
* Schema không còn nullable fields.
* Client không cần đoán mà chỉ cần gọi mutation addItemToCheckout.
* Dễ viết và bảo trì resolver hơn vì tập trung vào một hành động cụ thể.
* Giảm lỗi runtime và trạng thái không hợp lệ.

---

## Relay Specification








