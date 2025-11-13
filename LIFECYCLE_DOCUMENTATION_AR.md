# دورة حياة طلب العرض والصفقة - الوثائق الكاملة

## جدول المحتويات

1. [دورة حياة طلب العرض (Offer Request Lifecycle)](#دورة-حياة-طلب-العرض)
2. [دورة حياة الصفقة (Deal Lifecycle)](#دورة-حياة-الصفقة)
3. [الأدوار والمسؤوليات](#الأدوار-والمسؤوليات)
4. [العلاقات والمنطق المرتبط (Related Entities Logic)](#العلاقات-والمنطق-المرتبط)

---

## دورة حياة طلب العرض (Offer Request Lifecycle)

### نظرة عامة

دورة حياة طلب العرض تبدأ عندما يقوم مندوب المبيعات (Salesman) بطلب عرض لعميل محتمل، وتنتهي عندما يتم إرسال العرض للمندوب أو إلغاء الطلب.

### جدول الحالات

| الحالة (Status) | الوصف بالعربية | الوصف بالإنجليزية                      | المسؤول               | الحالة التالية المحتملة |
| --------------- | -------------- | -------------------------------------- | --------------------- | ----------------------- |
| **Requested**   | تم الطلب       | Request has been created               | Salesman              | Assigned, Cancelled     |
| **Assigned**    | تم التعيين     | Request assigned to SalesSupport       | SalesSupport Manager  | InProgress, Cancelled   |
| **InProgress**  | قيد المعالجة   | SalesSupport is working on the request | SalesSupport          | Ready, Cancelled        |
| **Ready**       | جاهز           | Offer has been created and is ready    | SalesSupport          | Sent, Cancelled         |
| **Sent**        | تم الإرسال     | Offer has been sent to Salesman        | SalesSupport          | - (حالة نهائية)         |
| **Cancelled**   | ملغي           | Request has been cancelled             | Salesman/SalesSupport | - (حالة نهائية)         |

### جدول الانتقالات بين الحالات

| من الحالة      | إلى الحالة     | الشرط/الإجراء                    | المسؤول              | الوصف                                                                                              |
| -------------- | -------------- | -------------------------------- | -------------------- | -------------------------------------------------------------------------------------------------- |
| -              | **Requested**  | إنشاء طلب عرض جديد               | Salesman             | يتم إنشاء الطلب تلقائياً عند تسجيل زيارة مع `visitResult: "Interested"` و `nextStep: "NeedsOffer"` |
| **Requested**  | **Assigned**   | تعيين الطلب لمستخدم SalesSupport | SalesSupport Manager | يتم التعيين تلقائياً إذا كان هناك مستخدم SalesSupport واحد فقط، أو يدوياً من قبل المدير            |
| **Requested**  | **Cancelled**  | إلغاء الطلب                      | Salesman             | يمكن للمندوب إلغاء الطلب قبل التعيين                                                               |
| **Assigned**   | **InProgress** | بدء العمل على الطلب              | SalesSupport         | عند بدء إنشاء العرض من الطلب                                                                       |
| **Assigned**   | **Cancelled**  | إلغاء الطلب                      | SalesSupport         | يمكن إلغاء الطلب بعد التعيين                                                                       |
| **InProgress** | **Ready**      | اكتمال إنشاء العرض               | SalesSupport         | عند إنشاء العرض بنجاح من الطلب، يتم تحديث `CreatedOfferId`                                         |
| **InProgress** | **Cancelled**  | إلغاء الطلب                      | SalesSupport         | يمكن إلغاء الطلب أثناء المعالجة                                                                    |
| **Ready**      | **Sent**       | إرسال العرض للمندوب              | SalesSupport         | عند تصدير/إرسال العرض PDF للمندوب                                                                  |
| **Ready**      | **Cancelled**  | إلغاء الطلب                      | SalesSupport         | يمكن إلغاء الطلب حتى بعد الجاهزية                                                                  |
| **Sent**       | -              | لا يمكن الانتقال                 | -                    | حالة نهائية - لا يمكن تعديلها                                                                      |

### التدفق الكامل

```
1. Salesman (Mobile App)
   └─> يسجل زيارة عميل مع:
       - visitResult: "Interested"
       - nextStep: "NeedsOffer"
       - requestedProducts: "وصف المنتجات"

2. Backend: TaskProgressService
   └─> ينشئ OfferRequest تلقائياً
       - Status: "Requested" (أو "Assigned" إذا كان هناك SalesSupport واحد)
       - RequestedBy: Salesman ID
       - ClientId: العميل

3. Backend: NotificationService
   └─> يرسل إشعار لجميع مستخدمي SalesSupport النشطين

4. SalesSupport (Dashboard)
   └─> يستقبل الإشعار عبر SignalR
   └─> يفتح RequestsInboxPage
   └─> يعين الطلب لنفسه (إذا كان "Requested")
       Status: "Assigned" → "InProgress"

5. SalesSupport
   └─> ينشئ العرض (Offer) من الطلب
       - يختار العميل
       - يختار المندوب
       - يضيف المنتجات/المعدات
       - يحدد الشروط (دفع، توصيل، ضمان)
       - يحدد تاريخ الصلاحية (افتراضي: 30 يوماً)
   └─> Status: "Ready"
   └─> CreatedOfferId: يتم ربط العرض بالطلب

6. SalesSupport
   └─> يصدّر العرض كـ PDF
   └─> Status: "Sent"

7. Salesman (Mobile App)
   └─> يستقبل العرض
   └─> يعرضه للعميل
   └─> إذا قبل العميل → ينشئ صفقة (Deal)
```

### الحالات الخاصة

#### حالة Cancelled

- يمكن إلغاء الطلب في أي مرحلة (Requested, Assigned, InProgress, Ready)
- عند الإلغاء، يتم حفظ سبب الإلغاء في `CompletionNotes`
- لا يمكن استعادة الطلب بعد الإلغاء

#### حالة Ready

- تعني أن العرض تم إنشاؤه بنجاح
- يمكن للمستخدم SalesSupport مراجعة العرض قبل الإرسال
- عند الإرسال، يتم تحديث الحالة إلى "Sent"

---

## دورة حياة الصفقة (Deal Lifecycle)

### نظرة عامة

دورة حياة الصفقة تبدأ عندما يقوم مندوب المبيعات بإنشاء صفقة من عرض مقبول، وتنتهي بموافقة متعددة المستويات ثم إما النجاح أو الفشل.

### جدول الحالات

| الحالة (Status)               | الوصف بالعربية                | الوصف بالإنجليزية                  | المسؤول                     | الحالة التالية المحتملة                      |
| ----------------------------- | ----------------------------- | ---------------------------------- | --------------------------- | -------------------------------------------- |
| **PendingManagerApproval**    | في انتظار موافقة المدير       | Waiting for Sales Manager approval | Sales Manager               | RejectedByManager, PendingSuperAdminApproval |
| **RejectedByManager**         | مرفوض من المدير               | Rejected by Sales Manager          | Sales Manager               | - (حالة نهائية)                              |
| **PendingSuperAdminApproval** | في انتظار موافقة المدير العام | Waiting for Super Admin approval   | Super Admin                 | Approved, RejectedBySuperAdmin               |
| **RejectedBySuperAdmin**      | مرفوض من المدير العام         | Rejected by Super Admin            | Super Admin                 | - (حالة نهائية)                              |
| **Approved**                  | موافق عليه                    | Approved by all levels             | Super Admin                 | SentToLegal, Failed                          |
| **SentToLegal**               | تم الإرسال للقانوني           | Sent to Legal Department           | Sales Manager               | Success, Failed                              |
| **Success**                   | نجحت الصفقة                   | Deal completed successfully        | Sales Manager / Legal       | - (حالة نهائية)                              |
| **Failed**                    | فشلت الصفقة                   | Deal failed                        | Sales Manager / Super Admin | - (حالة نهائية)                              |

### جدول الانتقالات بين الحالات

| من الحالة                     | إلى الحالة                    | الشرط/الإجراء                    | المسؤول                     | الوصف                                                                                                     |
| ----------------------------- | ----------------------------- | -------------------------------- | --------------------------- | --------------------------------------------------------------------------------------------------------- |
| -                             | **PendingManagerApproval**    | إنشاء صفقة جديدة من عرض مقبول    | Salesman                    | يتم إنشاء الصفقة فقط من عرض بحالة "Accepted"                                                              |
| **PendingManagerApproval**    | **RejectedByManager**         | رفض المدير الصفقة                | Sales Manager               | يجب تحديد سبب الرفض: Money, CashFlow, OtherNeeds                                                          |
| **PendingManagerApproval**    | **PendingSuperAdminApproval** | موافقة المدير على الصفقة         | Sales Manager               | جميع الصفقات تحتاج موافقة Super Admin بعد موافقة المدير                                                   |
| **PendingSuperAdminApproval** | **SentToLegal**               | موافقة المدير العام              | Super Admin                 | بعد مراجعة الصفقة والموافقة عليها، يتم إرسالها تلقائياً للقسم القانوني (يتم المرور عبر Approved تلقائياً) |
| **PendingSuperAdminApproval** | **RejectedBySuperAdmin**      | رفض المدير العام الصفقة          | Super Admin                 | يجب تحديد سبب الرفض: Money, CashFlow, OtherNeeds                                                          |
| **SentToLegal**               | **Success**                   | اكتمال الصفقة بنجاح              | Sales Manager / Legal       | بعد إتمام جميع الإجراءات القانونية والتسليم                                                               |
| **SentToLegal**               | **Failed**                    | فشلت الصفقة بعد الإرسال للقانوني | Sales Manager / Super Admin | إذا فشلت الصفقة بعد الإرسال للقانوني                                                                      |
| **RejectedByManager**         | -                             | لا يمكن الانتقال                 | -                           | حالة نهائية - لا يمكن استعادة الصفقة                                                                      |
| **RejectedBySuperAdmin**      | -                             | لا يمكن الانتقال                 | -                           | حالة نهائية - لا يمكن استعادة الصفقة                                                                      |
| **Success**                   | -                             | لا يمكن الانتقال                 | -                           | حالة نهائية - الصفقة مكتملة                                                                               |
| **Failed**                    | -                             | لا يمكن الانتقال                 | -                           | حالة نهائية - الصفقة فاشلة                                                                                |

### التدفق الكامل

```
1. Salesman (Mobile App)
   └─> يستقبل عرضاً من SalesSupport
   └─> يعرضه للعميل
   └─> إذا قبل العميل العرض:
       - يغير حالة العرض إلى "Accepted"
       - ينشئ صفقة (Deal) من العرض

2. Backend: DealService
   └─> ينشئ SalesDeal
       - Status: "PendingManagerApproval"
       - OfferId: رابط للعرض المقبول
       - ClientId: العميل
       - SalesmanId: المندوب
       - DealValue: قيمة الصفقة

3. Sales Manager (Dashboard)
   └─> يستقبل إشعار بصفقة جديدة
   └─> يراجع الصفقة في DealApprovalForm
   └─> يختار أحد الخيارات:

       أ) الموافقة:
          Status: "PendingSuperAdminApproval"
          (جميع الصفقات تحتاج موافقة Super Admin)

       ب) الرفض:
          Status: "RejectedByManager"
          - يجب تحديد سبب: Money, CashFlow, OtherNeeds

4. Super Admin (Dashboard)
   └─> يستقبل إشعار بصفقة جديدة
   └─> يراجع الصفقة
   └─> يختار أحد الخيارات:

       أ) الموافقة:
          Status: "Approved" → يتم إرسالها تلقائياً
          Status: "SentToLegal" (تلقائياً)
          SentToLegalAt: تاريخ الإرسال التلقائي

       ب) الرفض:
          Status: "RejectedBySuperAdmin"
          - يجب تحديد سبب: Money, CashFlow, OtherNeeds

6. Legal Department
   └─> يستقبل الصفقة
   └─> يعد العقود
   └─> يتابع مع العميل

7. Sales Manager / Super Admin
   └─> بعد اكتمال جميع الإجراءات:
       Status: "Success"
       CompletedAt: تاريخ الإتمام
       CompletionNotes: ملاحظات الإتمام

   أو في حالة الفشل:
       Status: "Failed"
       CompletedAt: تاريخ الفشل
       CompletionNotes: ملاحظات الفشل
```

### سير الموافقات متعددة المستويات

```
┌─────────────────────────────────────────────────┐
│ Salesman creates Deal from Accepted Offer      │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│ Status: PendingManagerApproval                  │
│ Sales Manager reviews and decides:              │
│   ├─ Approve → Continue to Super Admin (if req)│
│   └─ Reject → RejectedByManager (END)          │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│ Status: PendingSuperAdminApproval (if required) │
│ Super Admin reviews and decides:                │
│   ├─ Approve → Approved                         │
│   └─ Reject → RejectedBySuperAdmin (END)        │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│ Status: PendingSuperAdminApproval              │
│ Super Admin reviews and decides:                │
│   ├─ Approve → Approved → SentToLegal (auto)   │
│   └─ Reject → RejectedBySuperAdmin (END)       │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────┐
│ Status: SentToLegal (تلقائياً بعد الموافقة)   │
│ Legal Department processes the deal             │
└─────────────────┬───────────────────────────────┘
                  │
                  ▼
        ┌─────────┴─────────┐
        │                   │
        ▼                   ▼
┌───────────────┐   ┌───────────────┐
│ Status:       │   │ Status:       │
│ Success       │   │ Failed        │
│ (END)         │   │ (END)         │
└───────────────┘   └───────────────┘
```

### أسباب الرفض (Rejection Reasons)

| السبب (Reason) | الوصف بالعربية | الوصف بالإنجليزية           |
| -------------- | -------------- | --------------------------- |
| **Money**      | مشاكل مالية    | Financial/Money issues      |
| **CashFlow**   | مشاكل السيولة  | Cash flow problems          |
| **OtherNeeds** | احتياجات أخرى  | Other requirements or needs |

### شروط الموافقة

- **مدير المبيعات (Sales Manager)**:

     - يجب أن يكون المسؤول عن المندوب الذي أنشأ الصفقة
     - يمكنه الموافقة أو الرفض مع ذكر السبب

- **المدير العام (Super Admin)**:
     - جميع الصفقات تحتاج موافقته بعد موافقة المدير
     - لديه صلاحية الموافقة النهائية أو الرفض
     - عند الموافقة، يتم إرسال الصفقة تلقائياً للقسم القانوني
     - يمكنه رؤية جميع الصفقات في النظام

---

## الأدوار والمسؤوليات

### جدول الأدوار

| الدور                             | المسؤوليات                                                                    | الصلاحيات                                                                                                    |
| --------------------------------- | ----------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| **Salesman (مندوب المبيعات)**     | - إنشاء طلبات العرض<br>- استقبال العروض<br>- إنشاء الصفقات من العروض المقبولة | - إنشاء OfferRequest<br>- عرض Offers<br>- إنشاء Deal من Accepted Offer                                       |
| **SalesSupport (دعم المبيعات)**   | - استقبال طلبات العرض<br>- إنشاء العروض<br>- إرسال العروض للمندوبين           | - عرض جميع طلبات العرض<br>- تعيين الطلبات<br>- إنشاء Offers<br>- تحديث حالة OfferRequest                     |
| **Sales Manager (مدير المبيعات)** | - مراجعة الصفقات<br>- الموافقة/الرفض على الصفقات<br>- إرسال الصفقات للقانوني  | - عرض جميع الصفقات<br>- الموافقة على الصفقات<br>- رفض الصفقات<br>- إرسال للقانوني<br>- تحديد نجاح/فشل الصفقة |
| **Super Admin (مدير عام)**        | - مراجعة الصفقات الكبيرة<br>- الموافقة/الرفض النهائي                          | - عرض جميع الصفقات<br>- الموافقة على الصفقات الكبيرة<br>- رفض الصفقات<br>- تحديد نجاح/فشل الصفقة             |
| **Legal (القسم القانوني)**        | - استقبال الصفقات الموافق عليها<br>- إعداد العقود                             | - عرض الصفقات المرسلة للقانوني<br>- متابعة الإجراءات القانونية                                               |

### الصلاحيات حسب الحالة

#### Offer Request

| الحالة     | Salesman   | SalesSupport            | Sales Manager |
| ---------- | ---------- | ----------------------- | ------------- |
| Requested  | عرض، إلغاء | عرض، تعيين              | عرض، تعيين    |
| Assigned   | عرض        | عرض، بدء العمل          | عرض           |
| InProgress | عرض        | تعديل، إنشاء عرض، إلغاء | عرض           |
| Ready      | عرض        | عرض، إرسال، إلغاء       | عرض           |
| Sent       | عرض        | عرض                     | عرض           |
| Cancelled  | عرض        | عرض                     | عرض           |

#### Deal

| الحالة                    | Salesman | Sales Manager                  | Super Admin         | Legal       |
| ------------------------- | -------- | ------------------------------ | ------------------- | ----------- |
| PendingManagerApproval    | عرض      | عرض، موافقة، رفض               | عرض                 | -           |
| RejectedByManager         | عرض      | عرض                            | عرض                 | -           |
| PendingSuperAdminApproval | عرض      | عرض                            | عرض، موافقة، رفض    | -           |
| RejectedBySuperAdmin      | عرض      | عرض                            | عرض                 | -           |
| Approved                  | عرض      | عرض، إرسال للقانوني، تحديد فشل | عرض، تحديد فشل      | عرض         |
| SentToLegal               | عرض      | عرض، تحديد نجاح/فشل            | عرض، تحديد نجاح/فشل | عرض، متابعة |
| Success                   | عرض      | عرض                            | عرض                 | عرض         |
| Failed                    | عرض      | عرض                            | عرض                 | عرض         |

---

## أمثلة عملية

### مثال 1: دورة حياة طلب عرض كاملة

```
1. مندوب المبيعات "أحمد" يزور مستشفى "النور"
   - visitResult: "Interested"
   - nextStep: "NeedsOffer"
   - requestedProducts: "جهاز أشعة X-Ray"

2. النظام ينشئ OfferRequest تلقائياً
   - Status: "Requested"
   - RequestedBy: أحمد

3. دعم المبيعات "سارة" تستقبل الإشعار
   - تعين الطلب لنفسها
   - Status: "Assigned" → "InProgress"

4. سارة تنشئ العرض
   - تضيف المنتجات والأسعار
   - تحدد شروط الدفع والتوصيل
   - Status: "Ready"
   - CreatedOfferId: 50

5. سارة تصدر العرض كـ PDF
   - Status: "Sent"

6. أحمد يستقبل العرض ويعرضه للمستشفى
```

### مثال 2: دورة حياة صفقة ناجحة

```
1. أحمد ينشئ صفقة من عرض مقبول
   - OfferId: 50
   - DealValue: 45,000 EGP
   - Status: "PendingManagerApproval"

2. مدير المبيعات "محمد" يراجع الصفقة
   - يوافق
   - Status: "PendingSuperAdminApproval"

3. المدير العام "خالد" يراجع الصفقة
   - يوافق
   - Status: "Approved" → يتم الإرسال تلقائياً
   - Status: "SentToLegal" (تلقائياً)

5. القسم القانوني يعد العقود
   - يتم التوقيع والتسليم

6. محمد يحدد الصفقة كنجاح
   - Status: "Success"
   - CompletionNotes: "تم التسليم بنجاح"
```

### مثال 3: صفقة مرفوضة

```
1. أحمد ينشئ صفقة
   - DealValue: 50,000 EGP
   - Status: "PendingManagerApproval"

2. محمد يراجع الصفقة
   - يرفضها
   - Status: "RejectedByManager"
   - ManagerRejectionReason: "CashFlow"
   - ManagerComments: "العميل لديه مشاكل سيولة"
```

---

## ملاحظات مهمة

### طلب العرض (Offer Request)

1. **الإنشاء التلقائي**: يتم إنشاء طلب العرض تلقائياً عند تسجيل زيارة مع `visitResult: "Interested"` و `nextStep: "NeedsOffer"`

2. **التعيين التلقائي**: إذا كان هناك مستخدم SalesSupport واحد فقط، يتم تعيين الطلب تلقائياً

3. **ربط العرض**: عند إنشاء العرض من الطلب، يتم ربطه عبر `CreatedOfferId`

4. **الإشعارات**: يتم إرسال إشعارات لجميع مستخدمي SalesSupport عند إنشاء طلب جديد

### الصفقة (Deal)

1. **الإنشاء من عرض مقبول فقط**: لا يمكن إنشاء صفقة إلا من عرض بحالة "Accepted"

2. **سير الموافقات**: جميع الصفقات تحتاج موافقة على مستويين (Manager → Super Admin)

3. **الرفض النهائي**: بمجرد رفض الصفقة من أي مستوى، لا يمكن استعادتها

4. **الإرسال للقانوني**: يتم إرسال الصفقة تلقائياً للقسم القانوني عند موافقة Super Admin

5. **تحديد النجاح/الفشل**: يمكن تحديد نجاح أو فشل الصفقة فقط بعد الموافقة عليها

---

## العلاقات والمنطق المرتبط (Related Entities Logic)

### نظرة عامة

هذا القسم يوضح العلاقات بين الكيانات المختلفة في النظام وكيفية ارتباطها بدورة حياة طلب العرض والصفقة.

---

### 1. العلاقة مع العملاء (Client Relationship)

#### جدول العلاقات

| الكيان           | العلاقة مع Client    | نوع العلاقة | الوصف                         |
| ---------------- | -------------------- | ----------- | ----------------------------- |
| **OfferRequest** | `ClientId` (مطلوب)   | Many-to-One | كل طلب عرض مرتبط بعميل واحد   |
| **SalesOffer**   | `ClientId` (مطلوب)   | Many-to-One | كل عرض مرتبط بعميل واحد       |
| **SalesDeal**    | `ClientId` (مطلوب)   | Many-to-One | كل صفقة مرتبطة بعميل واحد     |
| **TaskProgress** | `ClientId` (اختياري) | Many-to-One | الزيارات والتفاعلات مع العميل |

#### التدفق مع العميل

```
1. Salesman يزور عميل (Client)
   └─> ينشئ TaskProgress
       - ClientId: العميل
       - visitResult: "Interested"
       - nextStep: "NeedsOffer"

2. النظام ينشئ OfferRequest تلقائياً
   └─> ClientId: نفس العميل من TaskProgress
   └─> Client: العميل المرتبط

3. SalesSupport ينشئ SalesOffer من OfferRequest
   └─> ClientId: نفس العميل من OfferRequest
   └─> Client: العميل المرتبط

4. Salesman ينشئ SalesDeal من SalesOffer المقبول
   └─> ClientId: نفس العميل من SalesOffer
   └─> Client: العميل المرتبط
```

#### معلومات العميل المستخدمة

| الحقل                        | الاستخدام                                       | متى يتم استخدامه               |
| ---------------------------- | ----------------------------------------------- | ------------------------------ |
| **Name**                     | اسم العميل                                      | عرض في جميع الواجهات والتقارير |
| **Type**                     | نوع العميل (مستشفى، عيادة، طبيب)                | تصنيف وتصفية العملاء           |
| **Phone/Email**              | معلومات الاتصال                                 | التواصل مع العميل              |
| **Address/City/Governorate** | العنوان                                         | الشحن والتوصيل                 |
| **Status**                   | حالة العميل (Potential, Active, Inactive, Lost) | تتبع حالة العميل               |
| **Priority**                 | الأولوية (Low, Medium, High)                    | تحديد أولوية المتابعة          |
| **Classification**           | التصنيف (A, B, C, D)                            | تصنيف العملاء حسب القيمة       |
| **PotentialValue**           | القيمة المحتملة                                 | تقدير قيمة الصفقات المحتملة    |

#### قواعد العمل

- **لا يمكن حذف العميل**: إذا كان له طلبات عروض أو عروض أو صفقات نشطة
- **تحديث بيانات العميل**: يتم تحديثها تلقائياً في جميع السجلات المرتبطة
- **تاريخ العميل**: يمكن عرض جميع طلبات العروض والعروض والصفقات للعميل

---

### 2. العلاقة مع المنتجات والمعدات (Products/Equipment Relationship)

#### جدول العلاقات

| الكيان           | المنتجات/المعدات         | نوع التخزين | الوصف                            |
| ---------------- | ------------------------ | ----------- | -------------------------------- |
| **OfferRequest** | `RequestedProducts`      | نص أو JSON  | وصف المنتجات المطلوبة من المندوب |
| **SalesOffer**   | `Products`               | نص أو JSON  | وصف عام للمنتجات في العرض        |
| **SalesOffer**   | `Equipment` (Collection) | جدول منفصل  | تفاصيل المعدات في العرض          |

#### هيكل OfferEquipment

| الحقل           | النوع        | الوصف                 |
| --------------- | ------------ | --------------------- |
| **Id**          | long         | المعرف الفريد         |
| **OfferId**     | long         | معرف العرض المرتبط    |
| **Name**        | string (200) | اسم المعدة            |
| **Model**       | string (100) | الموديل               |
| **Provider**    | string (100) | المورد/الشركة المصنعة |
| **Country**     | string (100) | بلد الصنع             |
| **Year**        | int          | سنة الصنع             |
| **Price**       | decimal      | السعر                 |
| **Description** | string (500) | الوصف التفصيلي        |
| **InStock**     | bool         | متوفر في المخزن       |
| **ImagePath**   | string (500) | مسار صورة المعدة      |

#### التدفق مع المنتجات

```
1. Salesman يطلب عرض
   └─> RequestedProducts: "جهاز أشعة X-Ray، موديل XYZ"
   └─> يتم تخزينه كـ نص أو JSON

2. SalesSupport ينشئ العرض
   └─> Products: "جهاز أشعة X-Ray، موديل XYZ"
   └─> ينشئ Equipment Items:
       - Name: "جهاز أشعة X-Ray"
       - Model: "XYZ-2024"
       - Provider: "Company ABC"
       - Price: 50,000 EGP
       - InStock: true

3. SalesSupport يضيف المزيد من المعدات
   └─> يمكن إضافة عدة عناصر Equipment
   └─> كل عنصر له تفاصيله الكاملة

4. عند إنشاء الصفقة
   └─> يتم نسخ معلومات المنتجات من العرض
   └─> يتم استخدامها في العقود والتسليم
```

#### قواعد العمل

- **الحد الأدنى**: يجب أن يحتوي العرض على منتج واحد على الأقل
- **التعديل**: يمكن تعديل المعدات قبل إرسال العرض للعميل
- **التوفر**: يمكن تحديد ما إذا كانت المعدة متوفرة في المخزن (InStock)
- **الصور**: يمكن رفع صور للمعدات (ImagePath)

---

### 3. العلاقة مع الشروط (Terms Relationship)

#### جدول الشروط

| نوع الشروط         | الحقل في SalesOffer | نوع التخزين        | الوصف                     |
| ------------------ | ------------------- | ------------------ | ------------------------- |
| **شروط الدفع**     | `PaymentTerms`      | JSON Array         | قائمة بشروط الدفع         |
| **شروط التوصيل**   | `DeliveryTerms`     | JSON Array         | قائمة بشروط التوصيل       |
| **شروط الضمان**    | `WarrantyTerms`     | JSON Array         | قائمة بشروط الضمان        |
| **تاريخ الصلاحية** | `ValidUntil`        | JSON Array (Dates) | قائمة بتواريخ الصلاحية    |
| **نوع الدفع**      | `PaymentType`       | string             | Cash, Installments, Other |

#### هيكل OfferTerms (تفاصيل إضافية)

| الحقل                | النوع         | الوصف        |
| -------------------- | ------------- | ------------ |
| **WarrantyPeriod**   | string (500)  | فترة الضمان  |
| **DeliveryTime**     | string (500)  | وقت التوصيل  |
| **MaintenanceTerms** | string (2000) | شروط الصيانة |
| **OtherTerms**       | string (2000) | شروط أخرى    |

#### مثال على الشروط

```json
{
	"PaymentTerms": ["دفع 30% عند التوقيع", "دفع 70% عند التسليم"],
	"DeliveryTerms": ["التوصيل خلال 30 يوم", "التوصيل مجاني داخل القاهرة"],
	"WarrantyTerms": ["ضمان سنتان على المعدات", "صيانة مجانية لمدة سنة"],
	"ValidUntil": ["2025-12-31"],
	"PaymentType": "Installments"
}
```

#### التدفق مع الشروط

```
1. SalesSupport ينشئ العرض
   └─> يحدد PaymentTerms (شروط الدفع)
   └─> يحدد DeliveryTerms (شروط التوصيل)
   └─> يحدد WarrantyTerms (شروط الضمان)
   └─> يحدد ValidUntil (تاريخ الصلاحية)

2. SalesSupport يمكنه إضافة تفاصيل إضافية
   └─> إنشاء OfferTerms:
       - WarrantyPeriod: "سنتان"
       - DeliveryTime: "30 يوم"
       - MaintenanceTerms: "صيانة مجانية سنة"

3. عند الموافقة على الصفقة
   └─> يتم نسخ الشروط إلى الصفقة
   └─> يتم استخدامها في العقود
```

---

### 4. العلاقة مع خطط التقسيط (Installment Plans Relationship)

#### جدول InstallmentPlan

| الحقل                 | النوع        | الوصف                    |
| --------------------- | ------------ | ------------------------ |
| **Id**                | long         | المعرف الفريد            |
| **OfferId**           | long         | معرف العرض المرتبط       |
| **InstallmentNumber** | int          | رقم القسط (1, 2, 3, ...) |
| **Amount**            | decimal      | قيمة القسط               |
| **DueDate**           | DateTime     | تاريخ الاستحقاق          |
| **Status**            | string       | Pending, Paid, Overdue   |
| **Notes**             | string (500) | ملاحظات                  |

#### مثال على خطة التقسيط

```
العرض: 100,000 EGP

القسط 1: 30,000 EGP - تاريخ الاستحقاق: 2025-01-15
القسط 2: 30,000 EGP - تاريخ الاستحقاق: 2025-02-15
القسط 3: 40,000 EGP - تاريخ الاستحقاق: 2025-03-15
```

#### التدفق مع خطط التقسيط

```
1. SalesSupport ينشئ العرض
   └─> إذا كان PaymentType = "Installments"
   └─> ينشئ InstallmentPlans:
       - القسط 1: Amount, DueDate
       - القسط 2: Amount, DueDate
       - القسط 3: Amount, DueDate

2. عند الموافقة على الصفقة
   └─> يتم نسخ خطط التقسيط
   └─> يتم تتبع حالة كل قسط

3. عند الدفع
   └─> يتم تحديث Status للقسط إلى "Paid"
   └─> يتم تحديث Notes إذا لزم الأمر
```

---

### 5. المخطط الكامل للعلاقات

```
┌─────────────────────────────────────────────────────────────┐
│ Client (العميل)                                            │
│ - Name, Type, Phone, Email, Address                         │
│ - Status, Priority, Classification                          │
└──────────────┬──────────────────────────────────────────────┘
               │
               ├─────────────────┬─────────────────┬──────────────┐
               │                 │                 │              │
               ▼                 ▼                 ▼              ▼
    ┌──────────────────┐ ┌──────────────┐ ┌────────────┐ ┌──────────┐
    │ OfferRequest      │ │ SalesOffer    │ │ SalesDeal   │ │ TaskProgress │
    │ - ClientId        │ │ - ClientId    │ │ - ClientId  │ │ - ClientId   │
    │ - RequestedProducts│ │ - Products    │ │ - DealValue │ │ - VisitResult│
    │ - Status          │ │ - Equipment[] │ │ - Status    │ │             │
    └────────┬──────────┘ │ - Terms       │ └────────────┘ └────────────┘
             │             │ - Installment │
             │             │   Plans[]     │
             │             └───────┬───────┘
             │                     │
             └─────────────────────┘
                     │
                     ├───────────────────┬───────────────────┬──────────────────┐
                     │                   │                   │                  │
                     ▼                   ▼                   ▼                  ▼
            ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
            │ OfferEquipment│  │ OfferTerms    │  │ InstallmentPlan││ SalesOffer    │
            │ - Name        │  │ - Warranty    │  │ - Amount      │  │ (Created)     │
            │ - Model       │  │ - Delivery    │  │ - DueDate     │  │               │
            │ - Price       │  │ - Maintenance │  │ - Status      │  │               │
            │ - InStock     │  │               │  │               │  │               │
            └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘
```

---

### 6. قواعد التحقق والقيود (Validation Rules)

#### قواعد OfferRequest

| القاعدة                      | الوصف                            |
| ---------------------------- | -------------------------------- |
| **ClientId مطلوب**           | يجب تحديد عميل عند إنشاء الطلب   |
| **RequestedProducts مطلوب**  | يجب تحديد المنتجات المطلوبة      |
| **العميل يجب أن يكون موجود** | لا يمكن إنشاء طلب لعمل غير موجود |

#### قواعد SalesOffer

| القاعدة                                | الوصف                                        |
| -------------------------------------- | -------------------------------------------- |
| **ClientId مطلوب**                     | يجب تحديد عميل                               |
| **Products مطلوب**                     | يجب تحديد المنتجات                           |
| **TotalAmount > 0**                    | يجب أن يكون المبلغ الإجمالي أكبر من صفر      |
| **Equipment لا يمكن أن يكون فارغاً**   | يجب إضافة معدات واحدة على الأقل              |
| **ValidUntil يجب أن يكون في المستقبل** | تاريخ الصلاحية يجب أن يكون بعد تاريخ الإنشاء |

#### قواعد SalesDeal

| القاعدة                                    | الوصف                               |
| ------------------------------------------ | ----------------------------------- |
| **ClientId مطلوب**                         | يجب تحديد عميل                      |
| **OfferId مطلوب**                          | يجب أن يكون مرتبطاً بعرض            |
| **العرض يجب أن يكون "Accepted"**           | لا يمكن إنشاء صفقة من عرض غير مقبول |
| **DealValue > 0**                          | يجب أن تكون قيمة الصفقة أكبر من صفر |
| **DealValue يمكن أن تختلف عن TotalAmount** | يمكن تعديل قيمة الصفقة عند الإنشاء  |

#### قواعد OfferEquipment

| القاعدة                   | الوصف                             |
| ------------------------- | --------------------------------- |
| **OfferId مطلوب**         | يجب أن يكون مرتبطاً بعرض          |
| **Name مطلوب**            | يجب تحديد اسم المعدة              |
| **Price > 0**             | يجب أن يكون السعر أكبر من صفر     |
| **InStock افتراضي: true** | إذا لم يتم التحديد، يعتبر متوفراً |

#### قواعد InstallmentPlan

| القاعدة                         | الوصف                                           |
| ------------------------------- | ----------------------------------------------- |
| **OfferId مطلوب**               | يجب أن يكون مرتبطاً بعرض                        |
| **InstallmentNumber > 0**       | رقم القسط يجب أن يكون أكبر من صفر               |
| **Amount > 0**                  | قيمة القسط يجب أن تكون أكبر من صفر              |
| **DueDate في المستقبل**         | تاريخ الاستحقاق يجب أن يكون في المستقبل         |
| **مجموع الأقساط = TotalAmount** | مجموع جميع الأقساط يجب أن يساوي المبلغ الإجمالي |

---

### 7. أمثلة عملية على العلاقات

#### مثال 1: طلب عرض كامل مع المنتجات

```
1. Salesman يزور مستشفى "النور"
   └─> ClientId: 123
   └─> ClientName: "مستشفى النور"
   └─> ClientType: "Hospital"

2. Salesman يسجل زيارة
   └─> visitResult: "Interested"
   └─> nextStep: "NeedsOffer"
   └─> requestedProducts: "جهاز أشعة X-Ray، موديل ABC-2024"

3. النظام ينشئ OfferRequest
   └─> ClientId: 123
   └─> RequestedProducts: "جهاز أشعة X-Ray، موديل ABC-2024"
   └─> Status: "Requested"

4. SalesSupport ينشئ العرض
   └─> ClientId: 123 (نفس العميل)
   └─> Products: "جهاز أشعة X-Ray، موديل ABC-2024"
   └─> Equipment:
       - Name: "جهاز أشعة X-Ray"
       - Model: "ABC-2024"
       - Provider: "Medical Equipment Co."
       - Price: 80,000 EGP
       - InStock: true
   └─> PaymentTerms: ["دفع 50% مقدماً", "دفع 50% عند التسليم"]
   └─> ValidUntil: ["2025-12-31"]
```

#### مثال 2: صفقة مع خطة تقسيط

```
1. Salesman ينشئ صفقة من عرض مقبول
   └─> OfferId: 50
   └─> ClientId: 123 (نفس العميل من العرض)
   └─> DealValue: 80,000 EGP

2. الصفقة تحتوي على:
   └─> نفس المنتجات من العرض
   └─> نفس المعدات
   └─> نفس الشروط
   └─> خطط التقسيط:
       - القسط 1: 30,000 EGP - 2025-01-15
       - القسط 2: 30,000 EGP - 2025-02-15
       - القسط 3: 20,000 EGP - 2025-03-15
```

---

### 8. ملاحظات مهمة

#### حول العملاء

- **لا يمكن حذف العميل**: إذا كان له أي طلبات عروض أو عروض أو صفقات
- **تحديث بيانات العميل**: يؤثر على جميع السجلات المرتبطة
- **تاريخ العميل**: يمكن عرض جميع التفاعلات مع العميل

#### حول المنتجات والمعدات

- **RequestedProducts في OfferRequest**: نصي ومرن، يمكن أن يكون أي وصف
- **Equipment في SalesOffer**: منظم ومفصل، يحتوي على جميع التفاصيل
- **الصور**: يمكن رفع صور للمعدات لعرضها للعميل
- **التوفر**: يمكن تحديد ما إذا كانت المعدة متوفرة في المخزن

#### حول الشروط

- **التخزين**: يتم تخزين الشروط كـ JSON arrays للمرونة
- **التعديل**: يمكن تعديل الشروط قبل إرسال العرض
- **النسخ**: يتم نسخ الشروط إلى الصفقة عند الموافقة

#### حول خطط التقسيط

- **المرونة**: يمكن إنشاء أي عدد من الأقساط
- **التحقق**: يجب أن يكون مجموع الأقساط مساوياً للمبلغ الإجمالي
- **التتبع**: يمكن تتبع حالة كل قسط (Pending, Paid, Overdue)

---

---

## تقرير مراجعة المنطق في Backend

### نظرة عامة

تم مراجعة الكود في Backend وتم العثور على **3 مشاكل رئيسية** تحتاج إلى إصلاح:

### المشاكل المكتشفة

#### 1. تعيين OfferRequest يغير الحالة مباشرة إلى InProgress

**الموقع**: `SoitMed/Models/OfferRequest.cs` - السطر 52-56

**المشكلة**: عند استدعاء `AssignTo()`, يتم تغيير الحالة مباشرة إلى "InProgress" بدلاً من "Assigned"

**الكود الحالي**:

```csharp
public void AssignTo(string supportUserId)
{
    AssignedTo = supportUserId;
    Status = "InProgress";  // ❌ يجب أن تكون "Assigned"
}
```

**الحل المقترح**:

```csharp
public void AssignTo(string supportUserId)
{
    AssignedTo = supportUserId;
    Status = "Assigned";  // ✅ تصحيح الحالة
}
```

---

#### 2. UpdateStatusAsync يستدعي MarkAsCompleted بشكل خاطئ

**الموقع**: `SoitMed/Services/OfferRequestService.cs` - السطر 334-337

**المشكلة**: عند تحديث الحالة إلى "Sent", يتم استدعاء `MarkAsCompleted()` الذي يغيرها مرة أخرى إلى "Ready"

**الكود الحالي**:

```csharp
offerRequest.Status = status;
if (status == "Ready" || status == "Sent")
{
    offerRequest.MarkAsCompleted(notes);  // ❌ سيغير "Sent" إلى "Ready"
}
```

**الحل المقترح**:

```csharp
switch (status)
{
    case "Ready":
        offerRequest.MarkAsCompleted(notes);
        break;
    case "Sent":
        offerRequest.MarkAsSent();
        if (!string.IsNullOrEmpty(notes))
            offerRequest.CompletionNotes = notes;
        break;
    case "Cancelled":
        offerRequest.Cancel(notes);
        break;
    default:
        offerRequest.Status = status;
        break;
}
```

---

#### 3. SendToSalesmanAsync لا يستخدم MarkAsSent

**الموقع**: `SoitMed/Services/OfferService.cs` - السطر 412

**المشكلة**: يتم تعيين الحالة مباشرة بدلاً من استخدام الدالة المخصصة

**الكود الحالي**:

```csharp
offerRequest.Status = "Sent";  // ❌ يجب استخدام MarkAsSent()
```

**الحل المقترح**:

```csharp
offerRequest.MarkAsSent();  // ✅ استخدام الدالة المخصصة
```

---

### ما يعمل بشكل صحيح ✅

1. ✅ **إنشاء OfferRequest**: التحقق من العميل والتعيين التلقائي
2. ✅ **إنشاء Deal**: التحقق من أن العرض مقبول
3. ✅ **موافقة Deal**: جميع الانتقالات صحيحة
4. ✅ **إنشاء Offer**: تحديث حالة OfferRequest إلى "Ready"

---

### توصيات

**أولوية عالية**:

1. إصلاح `AssignTo()` لتعيين الحالة إلى "Assigned"
2. إصلاح `UpdateStatusAsync()` لاستخدام الدوال المناسبة
3. إصلاح `SendToSalesmanAsync()` لاستخدام `MarkAsSent()`

**أولوية متوسطة**: 4. إضافة تحقق من انتقالات الحالات الصحيحة 5. تحسين `CanModifyOfferRequestAsync()` للتحقق من الأدوار

---

## الخلاصة

هذه الوثائق توضح دورة حياة كاملة لطلب العرض والصفقة في نظام Soit-Med، مع جميع الحالات والانتقالات والمسؤوليات. يجب الرجوع لهذه الوثائق عند تطوير أو تعديل أي جزء من النظام.

**ملاحظة**: راجع ملف `BACKEND_LOGIC_REVIEW_REPORT.md` للحصول على تقرير تفصيلي باللغة الإنجليزية.
