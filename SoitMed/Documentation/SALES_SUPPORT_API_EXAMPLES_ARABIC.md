# أمثلة API لدعم المبيعات لمطوري React

هذا المستند يوفر أمثلة شاملة لمطوري React لدمج API إنشاء مستخدمي دعم المبيعات.

## نقطة النهاية API

```
POST /api/RoleSpecificUser/sales-support
```

## المصادقة

جميع الطلبات تتطلب مصادقة. قم بتضمين رمز JWT في رأس Authorization:

```javascript
const token = localStorage.getItem('authToken'); // أو كيفما تخزن الرمز المميز
const headers = {
	Authorization: `Bearer ${token}`,
	// لا تقم بتعيين Content-Type لـ FormData - المتصفح سيقوم بتعيينه تلقائياً مع الحدود
};
```

## أمثلة مكونات React

### 1. مكون النموذج الأساسي

```jsx
import React, { useState } from 'react';
import axios from 'axios';

const CreateSalesSupportForm = () => {
	const [formData, setFormData] = useState({
		email: '',
		password: '',
		firstName: '',
		lastName: '',
		phoneNumber: '',
		personalMail: '',
		supportSpecialization: '',
		supportLevel: '',
		notes: '',
		altText: '',
	});
	const [profileImage, setProfileImage] = useState(null);
	const [loading, setLoading] = useState(false);
	const [message, setMessage] = useState('');

	const handleInputChange = (e) => {
		const { name, value } = e.target;
		setFormData((prev) => ({
			...prev,
			[name]: value,
		}));
	};

	const handleImageChange = (e) => {
		setProfileImage(e.target.files[0]);
	};

	const handleSubmit = async (e) => {
		e.preventDefault();
		setLoading(true);
		setMessage('');

		try {
			const formDataToSend = new FormData();

			// إضافة جميع حقول النموذج
			Object.keys(formData).forEach((key) => {
				if (formData[key]) {
					formDataToSend.append(
						key,
						formData[key]
					);
				}
			});

			// إضافة صورة الملف الشخصي إذا تم اختيارها
			if (profileImage) {
				formDataToSend.append(
					'profileImage',
					profileImage
				);
			}

			const response = await axios.post(
				'/api/RoleSpecificUser/sales-support',
				formDataToSend,
				{
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'authToken'
						)}`,
					},
				}
			);

			setMessage(`نجح: ${response.data.message}`);
			console.log(
				'تم إنشاء مستخدم دعم المبيعات:',
				response.data
			);

			// إعادة تعيين النموذج
			setFormData({
				email: '',
				password: '',
				firstName: '',
				lastName: '',
				phoneNumber: '',
				personalMail: '',
				supportSpecialization: '',
				supportLevel: '',
				notes: '',
				altText: '',
			});
			setProfileImage(null);
		} catch (error) {
			console.error(
				'خطأ في إنشاء مستخدم دعم المبيعات:',
				error
			);
			if (error.response?.data?.errors) {
				setMessage(
					`خطأ: ${JSON.stringify(
						error.response.data.errors
					)}`
				);
			} else {
				setMessage('خطأ في إنشاء مستخدم دعم المبيعات');
			}
		} finally {
			setLoading(false);
		}
	};

	return (
		<div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
			<h2 className="text-2xl font-bold mb-6">
				إنشاء مستخدم دعم المبيعات
			</h2>

			<form
				onSubmit={handleSubmit}
				className="space-y-4"
			>
				{/* المعلومات الأساسية */}
				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							البريد الإلكتروني *
						</label>
						<input
							type="email"
							name="email"
							value={formData.email}
							onChange={
								handleInputChange
							}
							required
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							كلمة المرور *
						</label>
						<input
							type="password"
							name="password"
							value={
								formData.password
							}
							onChange={
								handleInputChange
							}
							required
							minLength={6}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>
				</div>

				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							الاسم الأول
						</label>
						<input
							type="text"
							name="firstName"
							value={
								formData.firstName
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							الاسم الأخير
						</label>
						<input
							type="text"
							name="lastName"
							value={
								formData.lastName
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>
				</div>

				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							رقم الهاتف
						</label>
						<input
							type="tel"
							name="phoneNumber"
							value={
								formData.phoneNumber
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							البريد الإلكتروني الشخصي
						</label>
						<input
							type="email"
							name="personalMail"
							value={
								formData.personalMail
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>
				</div>

				{/* حقول خاصة بدعم المبيعات */}
				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							تخصص الدعم
						</label>
						<input
							type="text"
							name="supportSpecialization"
							value={
								formData.supportSpecialization
							}
							onChange={
								handleInputChange
							}
							placeholder="مثل: دعم العملاء، الدعم التقني"
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							مستوى الدعم
						</label>
						<select
							name="supportLevel"
							value={
								formData.supportLevel
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						>
							<option value="">
								اختر المستوى
							</option>
							<option value="Junior">
								مبتدئ
							</option>
							<option value="Senior">
								أقدم
							</option>
							<option value="Lead">
								رئيسي
							</option>
							<option value="Specialist">
								متخصص
							</option>
						</select>
					</div>
				</div>

				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1">
						ملاحظات
					</label>
					<textarea
						name="notes"
						value={formData.notes}
						onChange={handleInputChange}
						rows={3}
						placeholder="ملاحظات إضافية حول دور دعم المبيعات"
						className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
					/>
				</div>

				{/* رفع صورة الملف الشخصي */}
				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1">
						صورة الملف الشخصي
					</label>
					<input
						type="file"
						accept="image/*"
						onChange={handleImageChange}
						className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
					/>
					{profileImage && (
						<p className="text-sm text-gray-600 mt-1">
							المحدد:{' '}
							{profileImage.name}
						</p>
					)}
				</div>

				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1">
						النص البديل للصورة
					</label>
					<input
						type="text"
						name="altText"
						value={formData.altText}
						onChange={handleInputChange}
						placeholder="وصف صورة الملف الشخصي"
						className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
					/>
				</div>

				{/* زر الإرسال */}
				<button
					type="submit"
					disabled={loading}
					className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
				>
					{loading
						? 'جاري الإنشاء...'
						: 'إنشاء مستخدم دعم المبيعات'}
				</button>

				{/* عرض الرسالة */}
				{message && (
					<div
						className={`p-4 rounded-md ${
							message.startsWith(
								'نجح'
							)
								? 'bg-green-100 text-green-800'
								: 'bg-red-100 text-red-800'
						}`}
					>
						{message}
					</div>
				)}
			</form>
		</div>
	);
};

export default CreateSalesSupportForm;
```

### 2. استخدام React Hook Form (متقدم)

```jsx
import React from 'react';
import { useForm } from 'react-hook-form';
import axios from 'axios';

const CreateSalesSupportWithHookForm = () => {
	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
		watch,
	} = useForm();
	const [loading, setLoading] = useState(false);
	const [message, setMessage] = useState('');

	const onSubmit = async (data) => {
		setLoading(true);
		setMessage('');

		try {
			const formData = new FormData();

			// إضافة بيانات النموذج
			Object.keys(data).forEach((key) => {
				if (data[key] && key !== 'profileImage') {
					formData.append(key, data[key]);
				}
			});

			// إضافة صورة الملف الشخصي
			const profileImage = watch('profileImage');
			if (profileImage && profileImage[0]) {
				formData.append(
					'profileImage',
					profileImage[0]
				);
			}

			const response = await axios.post(
				'/api/RoleSpecificUser/sales-support',
				formData,
				{
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'authToken'
						)}`,
					},
				}
			);

			setMessage(`نجح: ${response.data.message}`);
			reset();
		} catch (error) {
			console.error('خطأ:', error);
			setMessage('خطأ في إنشاء مستخدم دعم المبيعات');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form
			onSubmit={handleSubmit(onSubmit)}
			className="space-y-4"
		>
			{/* البريد الإلكتروني */}
			<div>
				<label className="block text-sm font-medium text-gray-700 mb-1">
					البريد الإلكتروني *
				</label>
				<input
					{...register('email', {
						required: 'البريد الإلكتروني مطلوب',
						pattern: {
							value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
							message: 'عنوان بريد إلكتروني غير صحيح',
						},
					})}
					className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
				/>
				{errors.email && (
					<p className="text-red-600 text-sm mt-1">
						{errors.email.message}
					</p>
				)}
			</div>

			{/* كلمة المرور */}
			<div>
				<label className="block text-sm font-medium text-gray-700 mb-1">
					كلمة المرور *
				</label>
				<input
					type="password"
					{...register('password', {
						required: 'كلمة المرور مطلوبة',
						minLength: {
							value: 6,
							message: 'يجب أن تكون كلمة المرور 6 أحرف على الأقل',
						},
					})}
					className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
				/>
				{errors.password && (
					<p className="text-red-600 text-sm mt-1">
						{errors.password.message}
					</p>
				)}
			</div>

			{/* حقول أخرى... */}

			<button
				type="submit"
				disabled={loading}
				className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700"
			>
				{loading
					? 'جاري الإنشاء...'
					: 'إنشاء مستخدم دعم المبيعات'}
			</button>
		</form>
	);
};
```

### 3. فئة خدمة API

```javascript
// services/salesSupportApi.js
import axios from 'axios';

class SalesSupportApiService {
	constructor() {
		this.baseURL = process.env.REACT_APP_API_BASE_URL || '';
		this.api = axios.create({
			baseURL: this.baseURL,
		});

		// إضافة معترض الطلب لتضمين رمز المصادقة
		this.api.interceptors.request.use((config) => {
			const token = localStorage.getItem('authToken');
			if (token) {
				config.headers.Authorization = `Bearer ${token}`;
			}
			return config;
		});
	}

	async createSalesSupport(formData) {
		try {
			const response = await this.api.post(
				'/api/RoleSpecificUser/sales-support',
				formData
			);
			return {
				success: true,
				data: response.data,
				message: response.data.message,
			};
		} catch (error) {
			console.error('خطأ في إنشاء دعم المبيعات:', error);
			return {
				success: false,
				error: error.response?.data || error.message,
				message: 'فشل في إنشاء مستخدم دعم المبيعات',
			};
		}
	}

	// طريقة مساعدة لإنشاء FormData من كائن
	createFormData(data) {
		const formData = new FormData();

		Object.keys(data).forEach((key) => {
			if (
				data[key] !== null &&
				data[key] !== undefined &&
				data[key] !== ''
			) {
				if (
					key === 'profileImage' &&
					data[key] instanceof File
				) {
					formData.append(
						'profileImage',
						data[key]
					);
				} else if (key !== 'profileImage') {
					formData.append(key, data[key]);
				}
			}
		});

		return formData;
	}
}

export default new SalesSupportApiService();
```

### 4. Hook مخصص

```javascript
// hooks/useSalesSupport.js
import { useState } from 'react';
import salesSupportApi from '../services/salesSupportApi';

export const useSalesSupport = () => {
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState(null);

	const createSalesSupport = async (userData) => {
		setLoading(true);
		setError(null);

		try {
			const formData =
				salesSupportApi.createFormData(userData);
			const result = await salesSupportApi.createSalesSupport(
				formData
			);

			if (!result.success) {
				setError(result.error);
				return { success: false, error: result.error };
			}

			return { success: true, data: result.data };
		} catch (err) {
			setError(err.message);
			return { success: false, error: err.message };
		} finally {
			setLoading(false);
		}
	};

	return {
		createSalesSupport,
		loading,
		error,
	};
};
```

### 5. الاستخدام مع Hook المخصص

```jsx
import React, { useState } from 'react';
import { useSalesSupport } from '../hooks/useSalesSupport';

const SalesSupportManager = () => {
	const { createSalesSupport, loading, error } = useSalesSupport();
	const [formData, setFormData] = useState({
		email: '',
		password: '',
		firstName: '',
		lastName: '',
		phoneNumber: '',
		personalMail: '',
		supportSpecialization: '',
		supportLevel: '',
		notes: '',
		profileImage: null,
	});

	const handleSubmit = async (e) => {
		e.preventDefault();
		const result = await createSalesSupport(formData);

		if (result.success) {
			console.log('تم إنشاء دعم المبيعات:', result.data);
			// التعامل مع النجاح (عرض إشعار، إعادة توجيه، إلخ)
		} else {
			console.error('خطأ:', result.error);
		}
	};

	return (
		<div>
			{/* JSX النموذج الخاص بك هنا */}
			{loading && <p>جاري إنشاء مستخدم دعم المبيعات...</p>}
			{error && <p className="text-red-600">خطأ: {error}</p>}
		</div>
	);
};
```

## تنسيق الاستجابة

### استجابة النجاح (200)

```json
{
	"userId": "SalesSupport_John_Doe_Sales_001",
	"email": "john.doe@example.com",
	"role": "SalesSupport",
	"departmentName": "Sales",
	"createdAt": "2024-01-15T10:30:00Z",
	"profileImage": {
		"id": 123,
		"fileName": "profile.jpg",
		"filePath": "/uploads/sales-support/...",
		"contentType": "image/jpeg",
		"fileSize": 156789,
		"altText": "صورة الملف الشخصي لجون دو",
		"isProfileImage": true,
		"uploadedAt": "2024-01-15T10:30:00Z"
	},
	"message": "تم إنشاء دعم المبيعات 'john.doe@example.com' بنجاح مع صورة الملف الشخصي",
	"supportSpecialization": "دعم العملاء",
	"supportLevel": "أقدم",
	"notes": "خبرة في الدعم التقني"
}
```

### استجابة الخطأ (400)

```json
{
	"errors": {
		"Email": "البريد الإلكتروني مطلوب",
		"Password": "يجب أن تكون كلمة المرور 6 أحرف على الأقل"
	},
	"message": "فشل التحقق من الصحة"
}
```

## الحقول المطلوبة مقابل الاختيارية

### الحقول المطلوبة:

- `email` (نص، تنسيق بريد إلكتروني صحيح)
- `password` (نص، 6 أحرف على الأقل)

### الحقول الاختيارية:

- `firstName` (نص، 100 حرف كحد أقصى)
- `lastName` (نص، 100 حرف كحد أقصى)
- `phoneNumber` (نص، تنسيق هاتف صحيح، 20 حرف كحد أقصى)
- `personalMail` (نص، تنسيق بريد إلكتروني صحيح، 200 حرف كحد أقصى)
- `departmentId` (رقم، سيتم تعيينه تلقائياً لقسم المبيعات إذا لم يتم توفيره)
- `supportSpecialization` (نص، 200 حرف كحد أقصى)
- `supportLevel` (نص، 100 حرف كحد أقصى)
- `notes` (نص، 500 حرف كحد أقصى)
- `altText` (نص، 500 حرف كحد أقصى)
- `profileImage` (ملف، تنسيق صورة)

## التفويض

يمكن فقط للمستخدمين الذين لديهم الأدوار التالية إنشاء مستخدمي دعم المبيعات:

- `SuperAdmin`
- `Admin`
- `SalesManager`

## ملاحظات

1. **FormData**: استخدم دائماً FormData عند إرسال الملفات (صور الملف الشخصي)
2. **Content-Type**: لا تقم بتعيين رأس Content-Type يدوياً عند استخدام FormData - المتصفح سيقوم بتعيينه تلقائياً مع الحدود الصحيحة
3. **التحقق من صحة الملف**: API يقبل تنسيقات الصور الشائعة (jpg، jpeg، png، gif، إلخ)
4. **إنشاء معرف المستخدم**: النظام ينشئ تلقائياً معرفات مستخدم مخصصة تتبع النمط: `SalesSupport_FirstName_LastName_Department_Number`
5. **تعيين القسم**: إذا لم يتم توفير معرف القسم، يتم تعيين المستخدمين تلقائياً لقسم "المبيعات"

## التعامل مع الأخطاء

قم بتنفيذ التعامل الصحيح مع الأخطاء دائماً لـ:

- أخطاء الشبكة
- أخطاء التحقق من الصحة (400)
- أخطاء التفويض (401/403)
- أخطاء الخادم (500)
- أخطاء رفع الملفات

هذا يضمن تجربة مستخدم سلسة وإبلاغ صحيح عن الأخطاء.

## مثال سريع للاستخدام

```bash
# تثبيت التبعيات
npm install axios react-hook-form

# استخدام المكون الأساسي
import CreateSalesSupportForm from './components/CreateSalesSupportForm';

# أو استخدام الـ Hook المخصص
import { useSalesSupport } from './hooks/useSalesSupport';

# إجراء استدعاءات API
const { createSalesSupport, loading, error } = useSalesSupport();
```

## تفاصيل نقطة النهاية API

- **URL**: `POST /api/RoleSpecificUser/sales-support`
- **Content-Type**: `multipart/form-data` (لرفع الملفات)
- **Authorization**: `Bearer {token}`
- **Response**: JSON مع تفاصيل المستخدم ورسالة النجاح

التوثيق جاهز للإنتاج ويتضمن كل ما يحتاجه مطورو React لدمج نقطة النهاية الجديدة لدعم المبيعات في تطبيقات الواجهة الأمامية الخاصة بهم!


