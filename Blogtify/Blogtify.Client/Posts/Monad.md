---
url: [/monad]
title: "Bạn đã từng nghe đến Monad chưa?"
$attribute: Category("Functional Programming")
$layout: Blogtify.Client.Layout.BlogPostLayout
---

Học FP không hề dễ. Nó đi kèm với cả đống từ vựng lạ hoắc, nghe cứ như mật mã đối với dân bước ra từ thế giới OOP kiểu C#. 
Trong cái danh sách đó, “monad” có lẽ là từ tai tiếng nhất, một từ vừa trừu tượng, vừa gây bối rối.

Một định nghĩa về monad chúng ta thường thấy đâu đó trên internet:
> Monad là một monoid trong category của endofunctor.

Đối với người mới học FP thì định nghĩa này  hoàn toàn không có ích gì. Để hiểu nổi câu này, bạn phải biết monoid là gì, category là gì, endofunctor là gì.
Tôi tự hỏi mình đang đọc cái quái gì khi mới học về monad. 

Các bài viết trên internet quá chú trọng vào “cách thức” – monad vận hành ra sao, bind thế nào – mà quên giải thích tại sao monad lại quan trọng, nó giải quyết vấn đề gì.


Trung tâm của lập trình hàm thì chỉ có một thứ thôi: Function
Trong C# thay vì function, ta thường nói đến method. Có 2 cách để mô phỏng một function

```c#
public static int MyFunc(string input) 
{
    // do something
    return 1;
}
```

```c#
public class ClassA
{
    public int MyFunc(string input) 
    {
        // do something
        return 1;
    }
}
```


```c#
class User
{
    Order GetLatestOrder()
    {
        // ...
    }
}

class Order
{
    Payment GetPayment() 
    {
        // ...
    }
}

class Payment
{
    Receipt GetReceipt() 
    {
        // ...
    }
}

class Receipt
{
    // ...
}

```

Trên lý thuyết, workflow để lấy receipt cho user

```c#
static Receipt GetLatestReceipt(User user)
{
    return user
        .GetLatestOrder()
        .GetPayment()
        .GetReceipt();
}
```