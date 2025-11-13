## 📘 قصة المستخدم (العربية) - تدفق طلب العرض والعروض

### الأدوار المعنية

- مندوب المبيعات (Salesman)
- دعم المبيعات (SalesSupport)
- مدير المبيعات (SalesManager)

---

### 1) قصة المستخدم - مندوب المبيعات يطلب عرض

- كـ مندوب: أزور عميل وأسجل تقدم زيارة، ثم أطلب عرض أسعار.
- النظام يقوم تلقائياً بتعيين الطلب لأقرب/أول مستخدم دعم مبيعات موجود.

طلب إنشاء طلب عرض

```http
POST /api/OfferRequest
Authorization: Bearer {token}
Content-Type: application/json

{
  "clientId": 1,
  "taskProgressId": 123, // اختياري
  "requestedProducts": "X-Ray Machine, Ultrasound",
  "specialNotes": "Urgent"
}
```

استجابة متوقعة

```json
{
	"success": true,
	"data": {
		"id": 91,
		"status": "Assigned", // أو "Requested" إذا لا يوجد دعم نشط
		"assignedTo": "Ahmed_Hemdan_Engineering_001",
		"assignedToName": "Ahmed Hemdan",
		"clientId": 1,
		"requestedProducts": "X-Ray Machine, Ultrasound"
	},
	"message": "Offer request created successfully"
}
```

ملاحظات واجهة المستخدم

- إذا كانت الحالة "Assigned" اعرض اسم موظف الدعم.
- إذا كانت "Requested" اعرض تنبيه: بانتظار التعيين اليدوي.

---

### 2) قصة المستخدم - دعم المبيعات يعالج الطلب وينشئ عرض

- كـ دعم: أفتح الطلب المُسند لي، وأنشئ عرضاً، ويقوم النظام بإنشاء عناصر معدات تلقائياً من حقل المنتجات.

إنشاء عرض

```http
POST /api/Offer
Authorization: Bearer {token}
Content-Type: application/json

{
  "offerRequestId": 91, // اختياري؛ إذا وُجد يتم تحديث حالة الطلب إلى Ready
  "clientId": 1,
  "assignedTo": "salesman123", // مندوب سيتم إسناد العرض له
  "products": "X-Ray Machine, CT Scanner",
  "totalAmount": 500000,
  "validUntil": "2025-12-31T00:00:00Z",
  "notes": "..."
}
```

سلوك تلقائي

- يتم تحليل `products` (فواصل/سطور/فاصلة منقوطة) وإنشاء عناصر معدات لكل عنصر.
- يوضع مسار صورة افتراضي لكل عنصر: `offers/{offerId}/equipment-placeholder.png`.

رفع صورة لمعدة

```http
POST /api/Offer/{offerId}/equipment/{equipmentId}/upload-image
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: <IMAGE_FILE>
```

استجابة رفع الصورة

```json
{
	"success": true,
	"data": {
		"id": 1,
		"imagePath": "offers/45/equipment-1-<guid>.jpg"
	},
	"message": "Image uploaded and equipment updated successfully"
}
```

---

### 3) قصة المستخدم - إسناد/إعادة إسناد العرض لمندوب

- كـ دعم/مدير: أُسند العرض لمندوب محدد.

إسناد العرض لمندوب

```http
PUT /api/Offer/{offerId}/assign-to-salesman
Authorization: Bearer {token}
Content-Type: application/json

{ "salesmanId": "salesman123" }
```

أخطاء متوقعة

```json
{ "success": false, "message": "User must have Salesman role" }
```

---

### 4) قصة المستخدم - مندوب يطّلع على عروضه ويُصدّر PDF

- كـ مندوب: أشاهد العروض المسندة لي، وأفتح التفاصيل (مع المعدات والصور)، وأُصدّر PDF عند الحاجة.

الحصول على العروض المسندة لي

```http
GET /api/Offer/assigned-to-me
Authorization: Bearer {token}
```

تفاصيل عرض

```http
GET /api/Offer/{id}
Authorization: Bearer {token}
```

تصدير PDF

```http
GET /api/Offer/{offerId}/export-pdf
Authorization: Bearer {token}
```

---

### الحالات (Statuses)

- طلب العرض: `Requested` | `Assigned` | `InProgress` | `Ready` | `Sent` | `Cancelled`
- العرض: `Draft` → `Sent` → `UnderReview` → `Accepted/Rejected/Expired`

---

### ملاحظات تكامل الواجهة الأمامية (Frontend)

- بعد إنشاء طلب العرض افحص الحقل `status` لتحديد الرسالة المناسبة في الواجهة.
- بعد إنشاء العرض، اجلب قائمة المعدات لعرض الصور وتحديث الأسعار.
- عند الرفع استخدم `multipart/form-data` وحدد حجم ونوع الملف (JPG/PNG/GIF، حتى 5MB).
- إسناد العرض يتم عبر `PUT /api/Offer/{offerId}/assign-to-salesman` من دور الدعم/المدير.

### معالجة الأخطاء

- 400: فشل تحقق (بيانات ناقصة/نوع مستخدم خاطئ للإسناد)
- 401: توكن غير صالح
- 403: صلاحيات غير كافية
- 404: كيانات غير موجودة
- 500: خطأ خادم
