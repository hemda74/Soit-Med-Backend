# وثائق نقاط النهاية لمنطق المبيعات - باللغة العربية

## نظرة عامة

هذا المستند يوضح جميع نقاط النهاية المتعلقة بمنطق المبيعات في النظام، بما في ذلك تتبع العملاء والتخطيط الأسبوعي.

## 1. إدارة العملاء (Client Management)

### 1.1 البحث عن العملاء

**النقطة:** `GET /api/Client/search`
**الوصف:** البحث عن العملاء بالاسم أو التخصص أو الموقع أو الهاتف أو البريد الإلكتروني
**المعاملات:**

- `query` (مطلوب): مصطلح البحث
- `page` (اختياري): رقم الصفحة (افتراضي: 1)
- `pageSize` (اختياري): عدد العناصر في الصفحة (افتراضي: 20)

**الاستجابة:**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "د. أحمد محمد",
			"type": "Doctor",
			"specialization": "أمراض القلب",
			"location": "الرياض",
			"phone": "+966501234567",
			"email": "ahmed.mohamed@hospital.com",
			"status": "Active",
			"priority": "High"
		}
	]
}
```

### 1.2 إنشاء عميل جديد

**النقطة:** `POST /api/Client`
**الوصف:** إنشاء عميل جديد في النظام
**البيانات المطلوبة:**

```json
{
	"name": "د. أحمد محمد",
	"type": "Doctor",
	"specialization": "أمراض القلب",
	"location": "الرياض",
	"phone": "+966501234567",
	"email": "ahmed.mohamed@hospital.com",
	"status": "Potential",
	"priority": "High",
	"potentialValue": 500000,
	"contactPerson": "د. أحمد محمد",
	"contactPersonPhone": "+966501234567",
	"contactPersonEmail": "ahmed.mohamed@hospital.com"
}
```

### 1.3 الحصول على تفاصيل العميل

**النقطة:** `GET /api/Client/{id}`
**الوصف:** الحصول على تفاصيل كاملة لعميل محدد
**الاستجابة:**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"name": "د. أحمد محمد",
		"type": "Doctor",
		"specialization": "أمراض القلب",
		"location": "الرياض",
		"phone": "+966501234567",
		"email": "ahmed.mohamed@hospital.com",
		"status": "Active",
		"priority": "High",
		"potentialValue": 500000,
		"lastContactDate": "2024-01-15T10:30:00Z",
		"nextContactDate": "2024-01-22T10:00:00Z",
		"satisfactionRating": 5
	}
}
```

### 1.4 تحديث العميل

**النقطة:** `PUT /api/Client/{id}`
**الوصف:** تحديث معلومات العميل
**البيانات المطلوبة:**

```json
{
	"name": "د. أحمد محمد",
	"type": "Doctor",
	"specialization": "أمراض القلب",
	"status": "Active",
	"priority": "High",
	"potentialValue": 750000
}
```

### 1.5 البحث عن عميل أو إنشاؤه

**النقطة:** `POST /api/Client/find-or-create`
**الوصف:** البحث عن عميل بالاسم، وإنشاؤه إذا لم يوجد
**البيانات المطلوبة:**

```json
{
	"name": "د. أحمد محمد",
	"type": "Doctor",
	"specialization": "أمراض القلب"
}
```

### 1.6 الحصول على عملائي

**النقطة:** `GET /api/Client/my-clients`
**الوصف:** الحصول على قائمة عملاء المستخدم الحالي
**المعاملات:**

- `page` (اختياري): رقم الصفحة
- `pageSize` (اختياري): عدد العناصر في الصفحة

### 1.7 العملاء المحتاجون لمتابعة

**النقطة:** `GET /api/Client/follow-up-needed`
**الوصف:** الحصول على العملاء المحتاجين لمتابعة بناءً على تاريخ الاتصال التالي

### 1.8 إحصائيات العملاء

**النقطة:** `GET /api/Client/statistics`
**الوصف:** الحصول على إحصائيات العملاء للمستخدم الحالي
**الاستجابة:**

```json
{
	"success": true,
	"data": {
		"myClientsCount": 25,
		"totalClientsCount": 150,
		"clientsByType": [
			{ "type": "Doctor", "count": 15 },
			{ "type": "Hospital", "count": 8 },
			{ "type": "Clinic", "count": 2 }
		],
		"clientsByStatus": [
			{ "status": "Active", "count": 20 },
			{ "status": "Potential", "count": 5 }
		]
	}
}
```

## 2. إدارة الخطط الأسبوعية (Weekly Plan Management)

### 2.1 إنشاء خطة أسبوعية

**النقطة:** `POST /api/WeeklyPlan`
**الوصف:** إنشاء خطة أسبوعية جديدة للموظف الحالي
**البيانات المطلوبة:**

```json
{
	"weekStartDate": "2024-01-15T00:00:00Z",
	"weekEndDate": "2024-01-21T23:59:59Z",
	"planTitle": "الخطة الأسبوعية - الأسبوع الأول",
	"planDescription": "خطة زيارات العملاء للأسبوع الأول من يناير"
}
```

### 2.2 الحصول على الخطط الأسبوعية

**النقطة:** `GET /api/WeeklyPlan`
**الوصف:** الحصول على قائمة الخطط الأسبوعية للموظف الحالي
**المعاملات:**

- `page` (اختياري): رقم الصفحة
- `pageSize` (اختياري): عدد العناصر في الصفحة

### 2.3 الحصول على خطة أسبوعية محددة

**النقطة:** `GET /api/WeeklyPlan/{id}`
**الوصف:** الحصول على خطة أسبوعية محددة مع جميع عناصرها

### 2.4 تحديث الخطة الأسبوعية

**النقطة:** `PUT /api/WeeklyPlan/{id}`
**الوصف:** تحديث خطة أسبوعية موجودة (فقط إذا كانت الحالة Draft)

### 2.5 إرسال الخطة الأسبوعية

**النقطة:** `POST /api/WeeklyPlan/{id}/submit`
**الوصف:** إرسال الخطة الأسبوعية للموافقة عليها

### 2.6 الموافقة على الخطة الأسبوعية

**النقطة:** `POST /api/WeeklyPlan/{id}/approve`
**الوصف:** الموافقة على خطة أسبوعية مرسلة (Admin/SalesManager فقط)
**البيانات المطلوبة:**

```json
{
	"notes": "موافقة على الخطة"
}
```

### 2.7 رفض الخطة الأسبوعية

**النقطة:** `POST /api/WeeklyPlan/{id}/reject`
**الوصف:** رفض خطة أسبوعية مرسلة (Admin/SalesManager فقط)
**البيانات المطلوبة:**

```json
{
	"reason": "سبب الرفض"
}
```

### 2.8 الحصول على الخطة الأسبوعية الحالية

**النقطة:** `GET /api/WeeklyPlan/current`
**الوصف:** الحصول على خطة الأسبوع الحالي للموظف الحالي

## 3. إدارة عناصر الخطة الأسبوعية (Weekly Plan Items Management)

### 3.1 إنشاء عنصر خطة

**النقطة:** `POST /api/WeeklyPlanItem`
**الوصف:** إضافة عنصر جديد إلى الخطة الأسبوعية
**البيانات المطلوبة:**

```json
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

### 3.2 الحصول على عناصر الخطة

**النقطة:** `GET /api/WeeklyPlanItem/plan/{planId}`
**الوصف:** الحصول على جميع عناصر خطة أسبوعية محددة

### 3.3 تحديث عنصر الخطة

**النقطة:** `PUT /api/WeeklyPlanItem/{id}`
**الوصف:** تحديث عنصر خطة موجود

### 3.4 إكمال عنصر الخطة

**النقطة:** `POST /api/WeeklyPlanItem/{id}/complete`
**الوصف:** تحديد عنصر الخطة كمكتمل مع النتائج والملاحظات
**البيانات المطلوبة:**

```json
{
	"results": "تم الاتفاق على العقد بنجاح",
	"feedback": "العميل راضي عن العرض",
	"satisfactionRating": 5,
	"nextVisitDate": "2024-01-23T10:00:00Z",
	"followUpNotes": "متابعة تسليم الأجهزة"
}
```

### 3.5 إلغاء عنصر الخطة

**النقطة:** `POST /api/WeeklyPlanItem/{id}/cancel`
**الوصف:** إلغاء عنصر خطة مع ذكر السبب
**البيانات المطلوبة:**

```json
{
	"reason": "العميل غير متاح"
}
```

### 3.6 تأجيل عنصر الخطة

**النقطة:** `POST /api/WeeklyPlanItem/{id}/postpone`
**الوصف:** تأجيل عنصر خطة إلى تاريخ جديد
**البيانات المطلوبة:**

```json
{
	"newDate": "2024-01-18T10:00:00Z",
	"reason": "تأجيل بناءً على طلب العميل"
}
```

### 3.7 العناصر المتأخرة

**النقطة:** `GET /api/WeeklyPlanItem/overdue`
**الوصف:** الحصول على جميع عناصر الخطة المتأخرة للموظف الحالي

### 3.8 العناصر القادمة

**النقطة:** `GET /api/WeeklyPlanItem/upcoming`
**الوصف:** الحصول على عناصر الخطة القادمة للموظف الحالي
**المعاملات:**

- `days` (اختياري): عدد الأيام القادمة (افتراضي: 7)

## 4. تتبع زيارات العملاء (Client Visit Tracking)

### 4.1 الحصول على زيارات العميل

**النقطة:** `GET /api/ClientTracking/{clientId}/visits`
**الوصف:** الحصول على جميع زيارات عميل محدد
**المعاملات:**

- `startDate` (اختياري): تاريخ البداية
- `endDate` (اختياري): تاريخ النهاية
- `salesmanId` (اختياري): معرف مندوب المبيعات
- `status` (اختياري): حالة الزيارة
- `page` (اختياري): رقم الصفحة
- `pageSize` (اختياري): عدد العناصر في الصفحة

### 4.2 إضافة زيارة عميل

**النقطة:** `POST /api/ClientTracking/visit`
**الوصف:** إضافة زيارة جديدة للعميل
**البيانات المطلوبة:**

```json
{
	"clientId": 1,
	"visitDate": "2024-01-16T10:00:00Z",
	"visitType": "Initial",
	"location": "عيادة العميل",
	"purpose": "مناقشة عقد توريد أجهزة طبية",
	"attendees": ["د. أحمد محمد", "مندوب المبيعات"],
	"notes": "زيارة مبدئية لمناقشة المتطلبات",
	"results": "تم الاتفاق على المبادئ الأساسية",
	"nextVisitDate": "2024-01-23T10:00:00Z",
	"status": "Completed",
	"salesmanId": "salesman-123"
}
```

### 4.3 تحديث زيارة العميل

**النقطة:** `PUT /api/ClientTracking/visit/{visitId}`
**الوصف:** تحديث زيارة عميل موجودة

### 4.4 حذف زيارة العميل

**النقطة:** `DELETE /api/ClientTracking/visit/{visitId}`
**الوصف:** حذف زيارة عميل

## 5. تتبع تفاعلات العملاء (Client Interaction Tracking)

### 5.1 الحصول على تاريخ تفاعلات العميل

**النقطة:** `GET /api/ClientTracking/{clientId}/interactions`
**الوصف:** الحصول على جميع تفاعلات عميل محدد
**المعاملات:**

- `page` (اختياري): رقم الصفحة
- `pageSize` (اختياري): عدد العناصر في الصفحة

### 5.2 إضافة تفاعل عميل

**النقطة:** `POST /api/ClientTracking/interaction`
**الوصف:** إضافة تفاعل جديد مع العميل
**البيانات المطلوبة:**

```json
{
	"clientId": 1,
	"interactionDate": "2024-01-16T14:00:00Z",
	"interactionType": "Call",
	"subject": "متابعة العرض المقدم",
	"description": "مكالمة هاتفية لمتابعة العرض المقدم للعميل",
	"participants": ["د. أحمد محمد", "مندوب المبيعات"],
	"outcome": "العميل مهتم بالعرض وسيراجع التفاصيل",
	"followUpRequired": true,
	"followUpDate": "2024-01-20T10:00:00Z",
	"priority": "High",
	"status": "Open"
}
```

## 6. تحليلات العملاء (Client Analytics)

### 6.1 الحصول على تحليلات العميل

**النقطة:** `GET /api/ClientTracking/{clientId}/analytics`
**الوصف:** الحصول على تحليلات عميل محدد
**المعاملات:**

- `period` (اختياري): الفترة الزمنية (daily, weekly, monthly, yearly)

### 6.2 الحصول على ملخص العميل

**النقطة:** `GET /api/ClientTracking/{clientId}/summary`
**الوصف:** الحصول على ملخص شامل للعميل

### 6.3 الحصول على الجدول الزمني للعميل

**النقطة:** `GET /api/ClientTracking/{clientId}/timeline`
**الوصف:** الحصول على الجدول الزمني لجميع أنشطة العميل
**المعاملات:**

- `limit` (اختياري): عدد العناصر (افتراضي: 50)

## 7. البحث والتصدير (Search and Export)

### 7.1 البحث عن العملاء

**النقطة:** `GET /api/ClientTracking/search`
**الوصف:** البحث عن العملاء بناءً على معايير مختلفة
**المعاملات:**

- `query` (مطلوب): مصطلح البحث
- `role` (اختياري): دور المستخدم
- `department` (اختياري): القسم
- `page` (اختياري): رقم الصفحة
- `pageSize` (اختياري): عدد العناصر في الصفحة

### 7.2 تصدير تاريخ العميل

**النقطة:** `GET /api/ClientTracking/{clientId}/export`
**الوصف:** تصدير تاريخ العميل بصيغة PDF أو Excel
**المعاملات:**

- `format` (اختياري): صيغة التصدير (pdf, excel)
- `startDate` (اختياري): تاريخ البداية
- `endDate` (اختياري): تاريخ النهاية
- `includeVisits` (اختياري): تضمين الزيارات
- `includeSales` (اختياري): تضمين المبيعات
- `includeInteractions` (اختياري): تضمين التفاعلات

## 8. رموز الحالة والأخطاء

### 8.1 رموز الحالة

- `200` - نجح الطلب
- `201` - تم إنشاء المورد بنجاح
- `400` - طلب غير صحيح
- `401` - غير مصرح
- `403` - ممنوع
- `404` - غير موجود
- `500` - خطأ في الخادم

### 8.2 رسائل الخطأ الشائعة

- "غير مصرح لك" - المستخدم غير مسجل الدخول
- "ليس لديك صلاحية" - المستخدم لا يملك الصلاحية المطلوبة
- "العميل غير موجود" - العميل المطلوب غير موجود
- "الخطة الأسبوعية غير موجودة" - الخطة المطلوبة غير موجودة
- "يوجد خطة أسبوعية بالفعل لهذا الأسبوع" - محاولة إنشاء خطة مكررة

## 9. المصادقة والتفويض

### 9.1 الرؤوس المطلوبة

```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

### 9.2 الأدوار والصلاحيات

- **Employee**: إنشاء وتحديث خططه وعملائه
- **SalesManager**: الموافقة على الخطط، عرض جميع البيانات
- **Admin**: وصول كامل لجميع الموارد

## 10. التصفية والترقيم

### 10.1 معاملات التصفية

جميع نقاط النهاية التي تعرض قوائم تدعم:

- `page`: رقم الصفحة (افتراضي: 1)
- `pageSize`: عدد العناصر في الصفحة (افتراضي: 20، حد أقصى: 100)

### 10.2 تنسيق الاستجابة

```json
{
  "success": true,
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}
```

## 11. تنسيقات التاريخ

### 11.1 تنسيق التاريخ

جميع التواريخ بصيغة ISO 8601:

- **التنسيق**: `YYYY-MM-DDTHH:mm:ssZ`
- **مثال**: `2024-01-15T10:30:00Z`
- **المنطقة الزمنية**: UTC

## 12. أمثلة الاستخدام

### 12.1 سير عمل كامل

```javascript
// 1. إنشاء خطة أسبوعية
const plan = await fetch('/api/WeeklyPlan', {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
		'Content-Type': 'application/json',
	},
	body: JSON.stringify({
		weekStartDate: '2024-01-15T00:00:00Z',
		weekEndDate: '2024-01-21T23:59:59Z',
		planTitle: 'الخطة الأسبوعية - الأسبوع الأول',
		planDescription: 'خطة زيارات العملاء للأسبوع الأول من يناير',
	}),
});

// 2. إضافة عناصر الخطة
const item = await fetch('/api/WeeklyPlanItem', {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
		'Content-Type': 'application/json',
	},
	body: JSON.stringify({
		weeklyPlanId: plan.id,
		clientName: 'د. أحمد محمد',
		clientType: 'Doctor',
		plannedVisitDate: '2024-01-16T10:00:00Z',
		visitPurpose: 'مناقشة عقد توريد أجهزة طبية',
		priority: 'High',
		isNewClient: false,
	}),
});

// 3. إرسال الخطة للموافقة
await fetch(`/api/WeeklyPlan/${plan.id}/submit`, {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
	},
});

// 4. إكمال زيارة
await fetch(`/api/WeeklyPlanItem/${item.id}/complete`, {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
		'Content-Type': 'application/json',
	},
	body: JSON.stringify({
		results: 'تم الاتفاق على العقد بنجاح',
		feedback: 'العميل راضي عن العرض',
		satisfactionRating: 5,
		nextVisitDate: '2024-01-23T10:00:00Z',
	}),
});
```

هذا المستند يغطي جميع نقاط النهاية المتعلقة بمنطق المبيعات مع أمثلة مفصلة ورسائل باللغة العربية.
