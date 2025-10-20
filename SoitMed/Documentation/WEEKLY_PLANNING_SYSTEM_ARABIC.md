# نظام التخطيط الأسبوعي وإدارة العملاء - باللغة العربية

## نظرة عامة

هذا النظام يسمح للموظفين بكتابة خططهم الأسبوعية وتتبع العملاء بشكل شامل. النظام يدعم البحث عن العملاء الموجودين أو إنشاء عملاء جدد، وتتبع زياراتهم ونتائجها، وحساب أداء الموظفين.

## الميزات الرئيسية

### 1. التخطيط الأسبوعي

- إنشاء خطط أسبوعية للموظفين
- إضافة عناصر للخطة (زيارات العملاء)
- تتبع حالة كل عنصر في الخطة
- نظام موافقة على الخطط

### 2. إدارة العملاء

- البحث عن العملاء الموجودين
- إنشاء عملاء جدد تلقائياً
- تتبع معلومات العملاء التفصيلية
- تصنيف العملاء حسب النوع والأولوية

### 3. تتبع الزيارات

- تسجيل زيارات العملاء
- تتبع نتائج الزيارات
- تقييم رضا العملاء
- جدولة زيارات المتابعة

### 4. حساب الأداء

- حساب أداء الموظف بناءً على الزيارات
- تتبع معدلات الإنجاز
- إحصائيات شاملة

## الجداول الجديدة

### 1. جدول العملاء (Clients)

```sql
CREATE TABLE Clients (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Type NVARCHAR(100), -- Doctor, Hospital, Clinic, etc.
    Specialization NVARCHAR(200), -- For doctors
    Location NVARCHAR(500),
    Phone NVARCHAR(100),
    Email NVARCHAR(200),
    Status NVARCHAR(50) DEFAULT 'Active',
    Priority NVARCHAR(50) DEFAULT 'Medium',
    -- ... other fields
);
```

### 2. جدول الخطط الأسبوعية (WeeklyPlans)

```sql
CREATE TABLE WeeklyPlans (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId NVARCHAR(450) NOT NULL,
    WeekStartDate DATETIME2 NOT NULL,
    WeekEndDate DATETIME2 NOT NULL,
    PlanTitle NVARCHAR(200) NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Draft',
    -- ... other fields
);
```

### 3. جدول عناصر الخطة (WeeklyPlanItems)

```sql
CREATE TABLE WeeklyPlanItems (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    WeeklyPlanId BIGINT NOT NULL,
    ClientName NVARCHAR(200) NOT NULL,
    PlannedVisitDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Planned',
    -- ... other fields
);
```

## نقاط النهاية (API Endpoints)

### 1. إدارة الخطط الأسبوعية

#### إنشاء خطة أسبوعية

```http
POST /api/WeeklyPlan
Content-Type: application/json

{
  "weekStartDate": "2024-01-15T00:00:00Z",
  "weekEndDate": "2024-01-21T23:59:59Z",
  "planTitle": "الخطة الأسبوعية - الأسبوع الأول",
  "planDescription": "خطة زيارات العملاء للأسبوع الأول من يناير"
}
```

#### الحصول على الخطط الأسبوعية

```http
GET /api/WeeklyPlan?page=1&pageSize=20
```

#### الحصول على خطة محددة

```http
GET /api/WeeklyPlan/{id}
```

#### تحديث خطة أسبوعية

```http
PUT /api/WeeklyPlan/{id}
Content-Type: application/json

{
  "planTitle": "الخطة الأسبوعية المحدثة",
  "planDescription": "وصف محدث للخطة"
}
```

#### إرسال الخطة للموافقة

```http
POST /api/WeeklyPlan/{id}/submit
```

#### الموافقة على الخطة

```http
POST /api/WeeklyPlan/{id}/approve
Content-Type: application/json

{
  "notes": "موافقة على الخطة"
}
```

#### رفض الخطة

```http
POST /api/WeeklyPlan/{id}/reject
Content-Type: application/json

{
  "reason": "سبب الرفض"
}
```

### 2. إدارة عناصر الخطة

#### إضافة عنصر للخطة

```http
POST /api/WeeklyPlanItem
Content-Type: application/json

{
  "weeklyPlanId": 1,
  "clientName": "د. أحمد محمد",
  "clientType": "Doctor",
  "clientSpecialization": "أمراض القلب",
  "plannedVisitDate": "2024-01-16T10:00:00Z",
  "visitPurpose": "مناقشة عقد توريد أجهزة طبية",
  "priority": "High",
  "isNewClient": false
}
```

#### الحصول على عناصر الخطة

```http
GET /api/WeeklyPlanItem/plan/{planId}
```

#### تحديث عنصر الخطة

```http
PUT /api/WeeklyPlanItem/{id}
Content-Type: application/json

{
  "clientName": "د. أحمد محمد",
  "plannedVisitDate": "2024-01-16T10:00:00Z",
  "visitPurpose": "مناقشة العقد النهائي",
  "priority": "High"
}
```

#### إكمال العنصر

```http
POST /api/WeeklyPlanItem/{id}/complete
Content-Type: application/json

{
  "results": "تم الاتفاق على العقد بنجاح",
  "feedback": "العميل راضي عن العرض",
  "satisfactionRating": 5,
  "nextVisitDate": "2024-01-23T10:00:00Z"
}
```

#### إلغاء العنصر

```http
POST /api/WeeklyPlanItem/{id}/cancel
Content-Type: application/json

{
  "reason": "العميل غير متاح"
}
```

#### تأجيل العنصر

```http
POST /api/WeeklyPlanItem/{id}/postpone
Content-Type: application/json

{
  "newDate": "2024-01-18T10:00:00Z",
  "reason": "تأجيل بناءً على طلب العميل"
}
```

### 3. إدارة العملاء

#### البحث عن العملاء

```http
GET /api/Client/search?query=د. أحمد&page=1&pageSize=20
```

#### إنشاء عميل جديد

```http
POST /api/Client
Content-Type: application/json

{
  "name": "د. أحمد محمد",
  "type": "Doctor",
  "specialization": "أمراض القلب",
  "phone": "+966501234567",
  "email": "ahmed.mohamed@hospital.com",
  "location": "الرياض",
  "status": "Potential",
  "priority": "High"
}
```

#### الحصول على عميل محدد

```http
GET /api/Client/{id}
```

#### تحديث بيانات العميل

```http
PUT /api/Client/{id}
Content-Type: application/json

{
  "name": "د. أحمد محمد",
  "phone": "+966501234567",
  "email": "ahmed.mohamed@hospital.com",
  "status": "Active",
  "priority": "High"
}
```

#### البحث عن أو إنشاء عميل

```http
POST /api/Client/find-or-create
Content-Type: application/json

{
  "name": "د. أحمد محمد",
  "type": "Doctor",
  "specialization": "أمراض القلب"
}
```

#### الحصول على عملائي

```http
GET /api/Client/my-clients?page=1&pageSize=20
```

#### العملاء المحتاجين لمتابعة

```http
GET /api/Client/follow-up-needed
```

#### إحصائيات العملاء

```http
GET /api/Client/statistics
```

## سير العمل (Workflow)

### 1. إنشاء الخطة الأسبوعية

1. الموظف ينشئ خطة أسبوعية جديدة
2. يضيف عناصر للخطة (زيارات العملاء)
3. لكل عنصر، يختار:
      - عميل موجود (يتم البحث عنه)
      - عميل جديد (يتم إنشاؤه تلقائياً)
4. يحدد موعد الزيارة والغرض منها
5. يرسل الخطة للموافقة

### 2. تنفيذ الخطة

1. الموظف ينفذ الزيارات حسب الجدول
2. بعد كل زيارة، يحدد:
      - نتائج الزيارة
      - تقييم رضا العميل
      - ملاحظات المتابعة
      - موعد الزيارة القادمة
3. يمكن تأجيل أو إلغاء الزيارات عند الحاجة

### 3. تتبع الأداء

1. النظام يحسب تلقائياً:
      - عدد الزيارات المكتملة
      - معدل الإنجاز
      - تقييمات رضا العملاء
      - العملاء الجدد المضافين

## الأدوار والصلاحيات

### 1. الموظف (Employee)

- إنشاء وتعديل خططه الأسبوعية
- إضافة عناصر للخطة
- تنفيذ الزيارات وتحديث نتائجها
- إدارة عملائه

### 2. مدير المبيعات (SalesManager)

- مراجعة خطط الموظفين
- الموافقة على الخطط أو رفضها
- مراقبة أداء الموظفين
- الوصول لجميع البيانات

### 3. المدير العام (Admin)

- إدارة جميع المستخدمين
- مراجعة جميع الخطط
- إحصائيات شاملة
- إدارة النظام

## أمثلة الاستخدام

### مثال 1: إنشاء خطة أسبوعية جديدة

```javascript
// 1. إنشاء الخطة
const plan = await fetch('/api/WeeklyPlan', {
	method: 'POST',
	headers: { 'Content-Type': 'application/json' },
	body: JSON.stringify({
		weekStartDate: '2024-01-15T00:00:00Z',
		weekEndDate: '2024-01-21T23:59:59Z',
		planTitle: 'الخطة الأسبوعية - الأسبوع الأول',
		planDescription: 'خطة زيارات العملاء للأسبوع الأول من يناير',
	}),
});

// 2. إضافة عناصر للخطة
const item1 = await fetch('/api/WeeklyPlanItem', {
	method: 'POST',
	headers: { 'Content-Type': 'application/json' },
	body: JSON.stringify({
		weeklyPlanId: plan.id,
		clientName: 'د. أحمد محمد',
		clientType: 'Doctor',
		clientSpecialization: 'أمراض القلب',
		plannedVisitDate: '2024-01-16T10:00:00Z',
		visitPurpose: 'مناقشة عقد توريد أجهزة طبية',
		priority: 'High',
		isNewClient: false,
	}),
});

// 3. إرسال الخطة للموافقة
await fetch(`/api/WeeklyPlan/${plan.id}/submit`, {
	method: 'POST',
});
```

### مثال 2: تنفيذ زيارة

```javascript
// 1. إكمال الزيارة
await fetch(`/api/WeeklyPlanItem/${itemId}/complete`, {
	method: 'POST',
	headers: { 'Content-Type': 'application/json' },
	body: JSON.stringify({
		results: 'تم الاتفاق على العقد بنجاح',
		feedback: 'العميل راضي عن العرض',
		satisfactionRating: 5,
		nextVisitDate: '2024-01-23T10:00:00Z',
		followUpNotes: 'متابعة تسليم الأجهزة',
	}),
});
```

### مثال 3: البحث عن عميل

```javascript
// البحث عن عميل
const clients = await fetch(
	'/api/Client/search?query=د. أحمد&page=1&pageSize=10'
).then((res) => res.json());

if (clients.data.length > 0) {
	// العميل موجود
	const client = clients.data[0];
	console.log('العميل موجود:', client.name);
} else {
	// إنشاء عميل جديد
	const newClient = await fetch('/api/Client', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({
			name: 'د. أحمد محمد',
			type: 'Doctor',
			specialization: 'أمراض القلب',
			phone: '+966501234567',
			email: 'ahmed.mohamed@hospital.com',
		}),
	});
}
```

## الإحصائيات والتقارير

### 1. إحصائيات الموظف

- عدد الخطط الأسبوعية
- معدل إنجاز الخطط
- عدد الزيارات المكتملة
- تقييمات رضا العملاء
- عدد العملاء الجدد

### 2. إحصائيات العملاء

- إجمالي العملاء
- العملاء حسب النوع
- العملاء حسب الحالة
- العملاء المحتاجين لمتابعة

### 3. إحصائيات الزيارات

- الزيارات المكتملة
- الزيارات المتأخرة
- الزيارات القادمة
- معدل نجاح الزيارات

## الأمان والتحقق

### 1. المصادقة

- جميع الطلبات تتطلب مصادقة
- استخدام JWT tokens

### 2. التفويض

- الموظف يمكنه الوصول لخططه فقط
- المدير يمكنه مراجعة جميع الخطط
- المدير العام له صلاحيات كاملة

### 3. التحقق من البيانات

- التحقق من صحة البيانات المدخلة
- رسائل خطأ باللغة العربية
- التحقق من الصلاحيات

## الاستجابة للأخطاء

### 1. رموز الحالة

- `200 OK`: نجح الطلب
- `201 Created`: تم إنشاء العنصر بنجاح
- `400 Bad Request`: خطأ في البيانات
- `401 Unauthorized`: غير مصرح
- `403 Forbidden`: ممنوع الوصول
- `404 Not Found`: العنصر غير موجود
- `500 Internal Server Error`: خطأ في الخادم

### 2. تنسيق رسائل الخطأ

```json
{
	"success": false,
	"message": "حدث خطأ في معالجة الطلب",
	"errors": {
		"clientName": "اسم العميل مطلوب",
		"plannedVisitDate": "تاريخ الزيارة مطلوب"
	}
}
```

## التكامل مع الأنظمة الأخرى

### 1. نظام الإشعارات

- إشعارات عند إنشاء خطط جديدة
- إشعارات عند الموافقة على الخطط
- تذكيرات بالزيارات القادمة

### 2. نظام التقارير

- تقارير أداء الموظفين
- تقارير إحصائيات العملاء
- تقارير الزيارات

### 3. نظام المبيعات

- ربط الزيارات بالعروض
- تتبع الصفقات
- حساب العمولات

هذا النظام يوفر حلاً شاملاً لإدارة التخطيط الأسبوعي وتتبع العملاء، مع دعم كامل للغة العربية وواجهة سهلة الاستخدام.
