# Weekly Plan API Documentation

## نظرة عامة (Overview)

تم عمل refactor كامل لنظام Sales Report ليصبح نظام **Weekly Plan (To-Do List)** أسبوعي حديث حيث:

- **الموظف (Salesman)** يقوم بإنشاء خطة أسبوعية في بداية كل أسبوع تحتوي على مهام (Tasks)
- كل يوم، الموظف يضيف تحديث يومي (Daily Progress) عن ما تم إنجازه
- **المدير (Sales Manager)** يمكنه مراجعة الخطط الأسبوعية وإعطاء تقييم ومراجعة

---

## هيكل النظام (System Architecture)

### 1. **WeeklyPlan** - الخطة الأسبوعية

الخطة الأساسية التي يتم إنشاؤها في بداية الأسبوع

### 2. **WeeklyPlanTask** - المهمة

المهام الفردية داخل الخطة الأسبوعية

### 3. **DailyProgress** - التقدم اليومي

التحديثات اليومية التي يضيفها الموظف

---

## الصلاحيات (Authorization)

| العملية                   | الدور المطلوب                |
| ------------------------- | ---------------------------- |
| إنشاء خطة أسبوعية         | `Salesman`                   |
| تعديل/حذف الخطة           | `Salesman` (خطته فقط)        |
| إضافة/تعديل المهام        | `Salesman` (خطته فقط)        |
| إضافة/تعديل التقدم اليومي | `Salesman` (خطته فقط)        |
| عرض جميع الخطط            | `SalesManager`, `SuperAdmin` |
| مراجعة/تقييم الخطة        | `SalesManager`, `SuperAdmin` |

---

## 📡 API Endpoints

### Base URL: `/api/WeeklyPlan`

---

## 1️⃣ إدارة الخطط الأسبوعية (Weekly Plan Management)

### 1.1 إنشاء خطة أسبوعية جديدة

**Create Weekly Plan**

```http
POST /api/WeeklyPlan
Authorization: Bearer {token}
Role: Salesman
```

#### Request Body:

```json
{
	"title": "خطة الأسبوع الأول من أكتوبر",
	"description": "خطة مبيعات لزيارة المستشفيات في القاهرة",
	"weekStartDate": "2024-10-01",
	"weekEndDate": "2024-10-07",
	"tasks": [
		{
			"title": "زيارة مستشفى 57357",
			"description": "عرض المعدات الطبية الجديدة",
			"displayOrder": 1
		},
		{
			"title": "متابعة عرض مستشفى دار الفؤاد",
			"description": "متابعة العرض المقدم الأسبوع الماضي",
			"displayOrder": 2
		},
		{
			"title": "إعداد تقرير المبيعات الشهري",
			"description": "تجميع بيانات مبيعات سبتمبر",
			"displayOrder": 3
		}
	]
}
```

#### Response (201 Created):

```json
{
	"success": true,
	"message": "Weekly plan created successfully",
	"data": {
		"id": 1,
		"title": "خطة الأسبوع الأول من أكتوبر",
		"description": "خطة مبيعات لزيارة المستشفيات في القاهرة",
		"weekStartDate": "2024-10-01",
		"weekEndDate": "2024-10-07",
		"employeeId": "emp123",
		"employeeName": "أحمد محمد",
		"rating": null,
		"managerComment": null,
		"managerReviewedAt": null,
		"createdAt": "2024-10-01T08:00:00Z",
		"updatedAt": "2024-10-01T08:00:00Z",
		"isActive": true,
		"tasks": [
			{
				"id": 1,
				"weeklyPlanId": 1,
				"title": "زيارة مستشفى 57357",
				"description": "عرض المعدات الطبية الجديدة",
				"isCompleted": false,
				"displayOrder": 1,
				"createdAt": "2024-10-01T08:00:00Z",
				"updatedAt": "2024-10-01T08:00:00Z"
			},
			{
				"id": 2,
				"weeklyPlanId": 1,
				"title": "متابعة عرض مستشفى دار الفؤاد",
				"description": "متابعة العرض المقدم الأسبوع الماضي",
				"isCompleted": false,
				"displayOrder": 2,
				"createdAt": "2024-10-01T08:00:00Z",
				"updatedAt": "2024-10-01T08:00:00Z"
			}
		],
		"dailyProgresses": [],
		"totalTasks": 3,
		"completedTasks": 0,
		"completionPercentage": 0.0
	}
}
```

#### Validation Rules:

- `title`: مطلوب، الحد الأقصى 200 حرف
- `description`: اختياري، الحد الأقصى 1000 حرف
- `weekStartDate`: مطلوب
- `weekEndDate`: مطلوب، يجب أن يكون بعد `weekStartDate`
- لا يمكن إنشاء أكثر من خطة واحدة لنفس الأسبوع لنفس الموظف

---

### 1.2 تعديل خطة أسبوعية

**Update Weekly Plan**

```http
PUT /api/WeeklyPlan/{id}
Authorization: Bearer {token}
Role: Salesman
```

#### Request Body:

```json
{
	"title": "خطة الأسبوع الأول من أكتوبر - محدثة",
	"description": "تم تعديل الخطة بناءً على الأولويات الجديدة"
}
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Weekly plan updated successfully",
	"data": {
		"id": 1,
		"title": "خطة الأسبوع الأول من أكتوبر - محدثة",
		"description": "تم تعديل الخطة بناءً على الأولويات الجديدة"
		// ... rest of the plan data
	}
}
```

---

### 1.3 حذف خطة أسبوعية

**Delete Weekly Plan**

```http
DELETE /api/WeeklyPlan/{id}
Authorization: Bearer {token}
Role: Salesman
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Weekly plan deleted successfully",
	"data": null
}
```

---

### 1.4 عرض خطة أسبوعية محددة

**Get Weekly Plan by ID**

```http
GET /api/WeeklyPlan/{id}
Authorization: Bearer {token}
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Weekly plan retrieved successfully",
	"data": {
		"id": 1,
		"title": "خطة الأسبوع الأول من أكتوبر"
		// ... full plan data with tasks and daily progresses
	}
}
```

---

### 1.5 عرض جميع الخطط الأسبوعية مع فلترة

**Get All Weekly Plans (with filtering)**

```http
GET /api/WeeklyPlan?employeeId={employeeId}&startDate={date}&endDate={date}&hasManagerReview={bool}&minRating={1-5}&maxRating={1-5}&page={page}&pageSize={pageSize}
Authorization: Bearer {token}
```

#### Query Parameters:

| Parameter          | Type      | Required | Description                                   |
| ------------------ | --------- | -------- | --------------------------------------------- |
| `employeeId`       | string    | No       | فلترة حسب معرف الموظف                         |
| `startDate`        | date      | No       | تاريخ البداية (YYYY-MM-DD)                    |
| `endDate`          | date      | No       | تاريخ النهاية (YYYY-MM-DD)                    |
| `hasManagerReview` | boolean   | No       | فلترة حسب وجود مراجعة من المدير               |
| `minRating`        | int (1-5) | No       | الحد الأدنى للتقييم                           |
| `maxRating`        | int (1-5) | No       | الحد الأقصى للتقييم                           |
| `page`             | int       | No       | رقم الصفحة (default: 1)                       |
| `pageSize`         | int       | No       | عدد العناصر في الصفحة (default: 10, max: 100) |

#### Example Requests:

**للموظف - عرض خططه فقط:**

```http
GET /api/WeeklyPlan?page=1&pageSize=10
Authorization: Bearer {salesman_token}
```

**للمدير - عرض جميع خطط موظف معين:**

```http
GET /api/WeeklyPlan?employeeId=emp123&page=1&pageSize=10
Authorization: Bearer {manager_token}
```

**فلترة حسب الخطط التي لم تُراجع بعد:**

```http
GET /api/WeeklyPlan?hasManagerReview=false&page=1&pageSize=10
Authorization: Bearer {manager_token}
```

**فلترة حسب التقييم:**

```http
GET /api/WeeklyPlan?minRating=4&page=1&pageSize=10
Authorization: Bearer {manager_token}
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Found 15 weekly plan(s)",
	"data": {
		"data": [
			{
				"id": 1,
				"title": "خطة الأسبوع الأول من أكتوبر"
				// ... plan data
			}
		],
		"totalCount": 15,
		"page": 1,
		"pageSize": 10,
		"totalPages": 2,
		"hasNextPage": true,
		"hasPreviousPage": false
	}
}
```

---

## 2️⃣ إدارة المهام (Task Management)

### 2.1 إضافة مهمة جديدة لخطة أسبوعية

**Add Task to Weekly Plan**

```http
POST /api/WeeklyPlan/{weeklyPlanId}/tasks
Authorization: Bearer {token}
Role: Salesman
```

#### Request Body:

```json
{
	"title": "الاتصال بمستشفى الجلاء",
	"description": "متابعة طلب الأسبوع الماضي",
	"displayOrder": 4
}
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Task added successfully",
	"data": {
		"id": 4,
		"weeklyPlanId": 1,
		"title": "الاتصال بمستشفى الجلاء",
		"description": "متابعة طلب الأسبوع الماضي",
		"isCompleted": false,
		"displayOrder": 4,
		"createdAt": "2024-10-02T09:00:00Z",
		"updatedAt": "2024-10-02T09:00:00Z"
	}
}
```

---

### 2.2 تعديل مهمة

**Update Task**

```http
PUT /api/WeeklyPlan/{weeklyPlanId}/tasks/{taskId}
Authorization: Bearer {token}
Role: Salesman
```

#### Request Body:

```json
{
	"title": "الاتصال بمستشفى الجلاء - عاجل",
	"description": "متابعة طلب عاجل",
	"isCompleted": true,
	"displayOrder": 1
}
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Task updated successfully",
	"data": {
		"id": 4,
		"weeklyPlanId": 1,
		"title": "الاتصال بمستشفى الجلاء - عاجل",
		"description": "متابعة طلب عاجل",
		"isCompleted": true,
		"displayOrder": 1,
		"createdAt": "2024-10-02T09:00:00Z",
		"updatedAt": "2024-10-02T10:30:00Z"
	}
}
```

---

### 2.3 حذف مهمة

**Delete Task**

```http
DELETE /api/WeeklyPlan/{weeklyPlanId}/tasks/{taskId}
Authorization: Bearer {token}
Role: Salesman
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Task deleted successfully",
	"data": null
}
```

---

## 3️⃣ إدارة التقدم اليومي (Daily Progress Management)

### 3.1 إضافة تقدم يومي

**Add Daily Progress**

```http
POST /api/WeeklyPlan/{weeklyPlanId}/progress
Authorization: Bearer {token}
Role: Salesman
```

#### Request Body:

```json
{
	"progressDate": "2024-10-01",
	"notes": "اليوم قمت بزيارة مستشفى 57357 وتم عرض جميع المنتجات الجديدة. تم الاتفاق على موعد ثاني الأسبوع القادم لمناقشة العرض. أيضاً قمت بالاتصال بمستشفى دار الفؤاد وتم تأكيد الموعد ليوم الأربعاء.",
	"tasksWorkedOn": [1, 2]
}
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Daily progress added successfully",
	"data": {
		"id": 1,
		"weeklyPlanId": 1,
		"progressDate": "2024-10-01",
		"notes": "اليوم قمت بزيارة مستشفى 57357...",
		"tasksWorkedOn": [1, 2],
		"createdAt": "2024-10-01T18:00:00Z",
		"updatedAt": "2024-10-01T18:00:00Z"
	}
}
```

#### Validation Rules:

- `progressDate`: مطلوب، يجب أن يكون ضمن نطاق الأسبوع
- `notes`: مطلوب، الحد الأقصى 2000 حرف
- `tasksWorkedOn`: اختياري، قائمة من معرفات المهام
- لا يمكن إضافة أكثر من تقدم يومي واحد لنفس التاريخ

---

### 3.2 تعديل تقدم يومي

**Update Daily Progress**

```http
PUT /api/WeeklyPlan/{weeklyPlanId}/progress/{progressId}
Authorization: Bearer {token}
Role: Salesman
```

#### Request Body:

```json
{
	"notes": "تحديث: اليوم قمت بزيارة مستشفى 57357 وتم عرض جميع المنتجات. تم الاتفاق على موعد ثاني. أيضاً اتصلت بدار الفؤاد وتم التأكيد. بالإضافة إلى ذلك، بدأت في إعداد التقرير الشهري.",
	"tasksWorkedOn": [1, 2, 3]
}
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Daily progress updated successfully",
	"data": {
		"id": 1,
		"weeklyPlanId": 1,
		"progressDate": "2024-10-01",
		"notes": "تحديث: اليوم قمت بزيارة مستشفى 57357...",
		"tasksWorkedOn": [1, 2, 3],
		"createdAt": "2024-10-01T18:00:00Z",
		"updatedAt": "2024-10-01T19:30:00Z"
	}
}
```

---

### 3.3 حذف تقدم يومي

**Delete Daily Progress**

```http
DELETE /api/WeeklyPlan/{weeklyPlanId}/progress/{progressId}
Authorization: Bearer {token}
Role: Salesman
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Daily progress deleted successfully",
	"data": null
}
```

---

## 4️⃣ مراجعة المدير (Manager Review)

### 4.1 مراجعة/تقييم خطة أسبوعية

**Review/Rate Weekly Plan**

```http
POST /api/WeeklyPlan/{id}/review
Authorization: Bearer {token}
Role: SalesManager, SuperAdmin
```

#### Request Body:

```json
{
	"rating": 5,
	"managerComment": "أداء ممتاز هذا الأسبوع! تم إنجاز جميع المهام في الوقت المحدد. التقدم اليومي واضح ومفصل. استمر على هذا المستوى."
}
```

#### Response (200 OK):

```json
{
	"success": true,
	"message": "Weekly plan reviewed successfully",
	"data": {
		"id": 1,
		"title": "خطة الأسبوع الأول من أكتوبر",
		"rating": 5,
		"managerComment": "أداء ممتاز هذا الأسبوع!...",
		"managerReviewedAt": "2024-10-08T10:00:00Z"
		// ... rest of plan data
	}
}
```

#### Validation Rules:

- `rating`: اختياري، يجب أن يكون بين 1 و 5
- `managerComment`: اختياري، الحد الأقصى 1000 حرف
- يجب إرسال `rating` أو `managerComment` على الأقل

---

## 📊 سيناريوهات الاستخدام (Use Cases)

### السيناريو الأول: موظف مبيعات يبدأ أسبوع جديد

#### يوم الأحد (بداية الأسبوع):

```javascript
// 1. إنشاء خطة أسبوعية مع المهام
POST /api/WeeklyPlan
{
  "title": "خطة أسبوع 1-7 أكتوبر",
  "description": "التركيز على مستشفيات القاهرة",
  "weekStartDate": "2024-10-01",
  "weekEndDate": "2024-10-07",
  "tasks": [
    { "title": "زيارة مستشفى A", "displayOrder": 1 },
    { "title": "متابعة مستشفى B", "displayOrder": 2 },
    { "title": "إعداد تقرير", "displayOrder": 3 }
  ]
}
```

#### كل يوم - نهاية اليوم:

```javascript
// 2. إضافة التقدم اليومي
POST /api/WeeklyPlan/1/progress
{
  "progressDate": "2024-10-01",
  "notes": "زرت مستشفى A وتم مناقشة العروض...",
  "tasksWorkedOn": [1]
}

// 3. تحديث حالة المهمة
PUT /api/WeeklyPlan/1/tasks/1
{
  "title": "زيارة مستشفى A",
  "isCompleted": true,
  "displayOrder": 1
}
```

#### خلال الأسبوع - إضافة مهمة جديدة:

```javascript
// 4. إضافة مهمة جديدة طارئة
POST /api/WeeklyPlan/1/tasks
{
  "title": "اتصال عاجل بمستشفى C",
  "description": "طلب عاجل من المدير",
  "displayOrder": 4
}
```

---

### السيناريو الثاني: مدير المبيعات يراجع الموظفين

```javascript
// 1. عرض جميع الخطط التي لم تُراجع بعد
GET /api/WeeklyPlan?hasManagerReview=false&page=1&pageSize=20

// 2. عرض خطة موظف محدد
GET /api/WeeklyPlan/1

// 3. مراجعة وتقييم الخطة
POST /api/WeeklyPlan/1/review
{
  "rating": 4,
  "managerComment": "أداء جيد، لكن يمكن تحسين التقدم اليومي بإضافة المزيد من التفاصيل."
}

// 4. عرض جميع الخطط لموظف معين في فترة محددة
GET /api/WeeklyPlan?employeeId=emp123&startDate=2024-09-01&endDate=2024-09-30&page=1&pageSize=10
```

---

## ❌ أكواد الأخطاء (Error Codes)

### 400 - Bad Request

```json
{
	"success": false,
	"message": "Validation failed",
	"errors": {
		"Title": ["Title is required."],
		"WeekEndDate": ["Week end date must be after week start date."]
	}
}
```

### 401 - Unauthorized

```json
{
	"success": false,
	"message": "Unauthorized access",
	"data": null
}
```

### 404 - Not Found

```json
{
	"success": false,
	"message": "Plan not found or you don't have permission to view it.",
	"data": null
}
```

### 409 - Conflict

```json
{
	"success": false,
	"message": "A plan already exists for this week or invalid data provided.",
	"data": null
}
```

---

## 🔄 الفرق بين النظام القديم والجديد

| الميزة            | النظام القديم (Sales Report) | النظام الجديد (Weekly Plan)    |
| ----------------- | ---------------------------- | ------------------------------ |
| **الهيكل**        | تقرير واحد (Title + Body)    | خطة أسبوعية + مهام + تقدم يومي |
| **التكرار**       | يومي/أسبوعي/شهري/مخصص        | أسبوعي فقط                     |
| **التفصيل**       | نص واحد                      | مهام متعددة + تحديثات يومية    |
| **التتبع**        | صعب                          | سهل ومنظم                      |
| **المهام**        | ❌ غير موجودة                | ✅ موجودة مع حالة الإنجاز      |
| **التقدم اليومي** | ❌ غير موجود                 | ✅ موجود                       |
| **نسبة الإنجاز**  | ❌ غير موجودة                | ✅ حساب تلقائي                 |

---

## 💡 نصائح للتطوير (Development Tips)

### Frontend Best Practices

#### 1. **State Management**

```javascript
// مثال باستخدام React + Redux Toolkit
const weeklyPlanSlice = createSlice({
	name: 'weeklyPlan',
	initialState: {
		currentPlan: null,
		plans: [],
		loading: false,
		error: null,
	},
	reducers: {
		// ... reducers
	},
});
```

#### 2. **API Calls**

```javascript
// مثال باستخدام Axios
const api = axios.create({
	baseURL: 'https://api.example.com/api',
	headers: {
		'Content-Type': 'application/json',
		Authorization: `Bearer ${token}`,
	},
});

// Create weekly plan
export const createWeeklyPlan = async (planData) => {
	const response = await api.post('/WeeklyPlan', planData);
	return response.data;
};

// Add daily progress
export const addDailyProgress = async (planId, progressData) => {
	const response = await api.post(
		`/WeeklyPlan/${planId}/progress`,
		progressData
	);
	return response.data;
};
```

#### 3. **Date Handling**

```javascript
// استخدام date-fns أو moment.js
import { format, startOfWeek, endOfWeek } from 'date-fns';

const getWeekDates = (date) => {
	return {
		weekStartDate: format(
			startOfWeek(date, { weekStartsOn: 0 }),
			'yyyy-MM-dd'
		),
		weekEndDate: format(
			endOfWeek(date, { weekStartsOn: 0 }),
			'yyyy-MM-dd'
		),
	};
};
```

#### 4. **Task Completion Tracking**

```javascript
// حساب نسبة الإنجاز
const calculateProgress = (tasks) => {
	const completed = tasks.filter((t) => t.isCompleted).length;
	return (completed / tasks.length) * 100;
};
```

### React Native Specific

```javascript
// مثال لشاشة عرض الخطة
import React, { useEffect } from 'react';
import { View, Text, FlatList } from 'react-native';

const WeeklyPlanScreen = ({ route }) => {
	const { planId } = route.params;
	const [plan, setPlan] = useState(null);

	useEffect(() => {
		fetchWeeklyPlan(planId);
	}, [planId]);

	return (
		<View>
			<Text>{plan?.title}</Text>
			<FlatList
				data={plan?.tasks}
				renderItem={({ item }) => (
					<TaskItem task={item} />
				)}
			/>
		</View>
	);
};
```

---

## 🧪 أمثلة الاختبار (Testing Examples)

### Postman Collection

```json
{
	"info": {
		"name": "Weekly Plan API",
		"schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
	},
	"item": [
		{
			"name": "Create Weekly Plan",
			"request": {
				"method": "POST",
				"header": [
					{
						"key": "Authorization",
						"value": "Bearer {{token}}"
					}
				],
				"url": "{{baseUrl}}/api/WeeklyPlan",
				"body": {
					"mode": "raw",
					"raw": "{\n  \"title\": \"Test Plan\",\n  \"weekStartDate\": \"2024-10-01\",\n  \"weekEndDate\": \"2024-10-07\",\n  \"tasks\": []\n}"
				}
			}
		}
	]
}
```

---

## 📝 ملاحظات مهمة (Important Notes)

1. **التواريخ**: جميع التواريخ بصيغة `YYYY-MM-DD` (ISO 8601)
2. **التوقيت**: جميع التواريخ والأوقات بتوقيت UTC
3. **الترميز**: UTF-8 لدعم اللغة العربية
4. **الحد الأقصى للصفحات**: 100 عنصر في الصفحة الواحدة
5. **النظام القديم**: Sales Report API لا يزال متاحاً للتوافق المؤقت

---

## 🔗 الموارد الإضافية (Additional Resources)

- [API Base URL]: `https://your-api-url.com/api`
- [Swagger Documentation]: `https://your-api-url.com/swagger`
- [Support Email]: support@example.com

---

## 📞 الدعم الفني (Technical Support)

لأي استفسارات أو مشاكل، يرجى التواصل مع فريق Backend عبر:

- Email: backend@example.com
- Slack: #backend-support

---

**آخر تحديث**: 4 أكتوبر 2025
**الإصدار**: 1.0.0
