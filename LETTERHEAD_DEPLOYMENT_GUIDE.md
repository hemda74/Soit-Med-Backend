# دليل نشر ملف Letterhead.pdf - Letterhead Deployment Guide

## الغرض (Purpose)

هذا الدليل يشرح كيفية إعداد ملف `Letterhead.pdf` لاستخدامه كخلفية في تصدير العروض.

## المتطلبات (Requirements)

1. ملف `Letterhead.pdf` بحجم A4 (595 × 842 points)
2. الملف يجب أن يكون بصيغة PDF قياسية
3. يفضل أن يكون الملف بدقة عالية (300 DPI أو أكثر)

## المسارات المدعومة (Supported Paths)

يبحث النظام عن الملف في المسارات التالية بالترتيب:

### 1. المسار الرئيسي (Recommended) ✅

```
Soit-Med-Backend/Letterhead.pdf
```

**الحالة:** ✅ الملف موجود بالفعل

**لماذا هذا المسار؟**

- سهل الوصول والإدارة
- خارج مجلد المشروع مما يسهل التحديثات
- لا يتأثر بعملية Build

### 2. مسار المشروع (Alternative)

```
Soit-Med-Backend/SoitMed/Letterhead.pdf
```

**متى تستخدمه؟**

- إذا كنت تريد تضمين الملف في المشروع مباشرة

### 3. مسار wwwroot (Fallback)

```
Soit-Med-Backend/SoitMed/wwwroot/templates/Letterhead.pdf
```

**متى تستخدمه؟**

- كنسخة احتياطية
- إذا كنت تريد نشر الملف مع الـ static assets

## خطوات النشر (Deployment Steps)

### للتطوير المحلي (Local Development)

1. ✅ **التحقق من وجود الملف:**

```powershell
Test-Path "d:\Soit-Med\Soit-Med-Backend\Letterhead.pdf"
```

2. **إذا لم يكن الملف موجوداً:**

```powershell
# Copy your Letterhead.pdf to the Backend directory
Copy-Item "path\to\your\Letterhead.pdf" "d:\Soit-Med\Soit-Med-Backend\Letterhead.pdf"
```

3. **تشغيل Backend:**

```powershell
cd "d:\Soit-Med\Soit-Med-Backend\SoitMed"
dotnet run
```

4. **مراجعة الـ logs للتأكد:**

```
[Info] Letterhead PDF found at: D:\Soit-Med\Soit-Med-Backend\Letterhead.pdf
```

### للنشر على السيرفر (Server Deployment)

#### الطريقة 1: نسخ الملف مباشرة

```bash
# SSH to server
ssh user@your-server.com

# Navigate to backend directory
cd /path/to/Soit-Med-Backend/

# Copy letterhead file
scp user@local:/path/to/Letterhead.pdf ./Letterhead.pdf

# Set permissions
chmod 644 Letterhead.pdf
```

#### الطريقة 2: تضمين في Build

1. **أضف الملف إلى `.csproj`:**

```xml
<ItemGroup>
  <None Update="Letterhead.pdf">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

2. **أو ضعه في `wwwroot/templates/`:**

```bash
mkdir -p wwwroot/templates
cp Letterhead.pdf wwwroot/templates/
```

## استخدام صورة كبديل (Image Fallback)

إذا لم يتوفر ملف PDF، يمكن استخدام صورة PNG كبديل:

### إنشاء letterhead.png من PDF:

**باستخدام ImageMagick:**

```bash
magick convert -density 300 Letterhead.pdf -quality 100 letterhead.png
```

**باستخدام GIMP:**

1. افتح `Letterhead.pdf` في GIMP
2. اختر Resolution: 300 DPI
3. Export as: `letterhead.png`
4. احفظ في: `wwwroot/templates/letterhead.png`

**باستخدام Photoshop:**

1. Open `Letterhead.pdf`
2. Image > Image Size > 300 DPI
3. Save As: `letterhead.png`
4. Format: PNG-24

## التحقق من عمل النظام (Verification)

### 1. اختبار API Endpoint

```bash
# Get an offer ID
curl http://localhost:5117/api/Offer

# Export PDF
curl -O -J -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5117/api/Offer/1/export-pdf
```

### 2. فحص ملف PDF الناتج

افتح الملف وتحقق من:

- ✅ ظهور الـ Letterhead في الخلفية
- ✅ وضوح النصوص والجداول
- ✅ عدم تداخل الـ Letterhead مع المحتوى

### 3. مراجعة Logs

```
[Info] Letterhead PDF found at: /path/to/Letterhead.pdf
[Info] Loaded Letterhead PDF successfully
```

إذا ظهرت رسائل تحذير:

```
[Warning] Letterhead.pdf not found. Checked paths: ...
[Warning] Could not add PDF letterhead, trying image fallback
```

## استكشاف الأخطاء (Troubleshooting)

### المشكلة: "Letterhead.pdf not found"

**الحل:**

1. تأكد من وجود الملف في أحد المسارات المدعومة
2. تحقق من صلاحيات القراءة:

```bash
ls -la Letterhead.pdf
chmod 644 Letterhead.pdf
```

### المشكلة: "Failed to load Letterhead PDF"

**الحل:**

1. تأكد من أن الملف PDF صالح:

```bash
# Test with pdf-info
pdfinfo Letterhead.pdf

# Or with pdftk
pdftk Letterhead.pdf dump_data
```

2. حاول إصلاح PDF:

```bash
# Using Ghostscript
gs -o Letterhead_fixed.pdf -sDEVICE=pdfwrite Letterhead.pdf
```

### المشكلة: PDF يظهر فارغاً أو مشوهاً

**الحل:**

1. تحقق من دقة الملف الأصلي
2. استخدم صورة PNG عالية الدقة كبديل
3. راجع هوامش المستند في `PdfExportService.cs`:

```csharp
// Current margins: left=50, right=50, top=80, bottom=50
Document document = new Document(PageSize.A4, 50, 50, 80, 50);
```

## المواصفات الموصى بها (Recommended Specifications)

### Letterhead.pdf:

- **الحجم:** A4 (210mm × 297mm)
- **Points:** 595 × 842
- **الدقة:** 300 DPI
- **حجم الملف:** < 2 MB
- **الألوان:** CMYK أو RGB
- **الخطوط:** Embedded أو Outlined

### letterhead.png (Fallback):

- **الأبعاد:** 2480 × 3508 pixels (A4 @ 300 DPI)
- **التنسيق:** PNG-24 with transparency
- **حجم الملف:** < 5 MB
- **Compression:** PNG optimized

## الأمان (Security)

1. **File Permissions:**

```bash
# Read-only for application
chmod 644 Letterhead.pdf
chown www-data:www-data Letterhead.pdf
```

2. **لا تخزن معلومات حساسة في Letterhead**

      - استخدم شعار الشركة والمعلومات العامة فقط
      - لا تضمن بيانات العملاء أو معلومات سرية

3. **نسخ احتياطي:**

```bash
# Backup letterhead
cp Letterhead.pdf Letterhead_backup_$(date +%Y%m%d).pdf
```

## الصيانة (Maintenance)

### تحديث الـ Letterhead:

```bash
# 1. Backup current
cp Letterhead.pdf Letterhead_old.pdf

# 2. Upload new
scp new_Letterhead.pdf server:/path/to/Soit-Med-Backend/Letterhead.pdf

# 3. Test
curl -O -J http://your-server/api/Offer/1/export-pdf

# 4. Verify
# Open PDF and check

# 5. If OK, remove backup
rm Letterhead_old.pdf
```

### مراقبة الاستخدام:

```bash
# Check file access logs
grep "Letterhead" /var/log/soitmed/application.log

# Monitor file size
du -h Letterhead.pdf
```

## للمطورين (For Developers)

### تعديل مسارات البحث:

في `PdfExportService.cs`:

```csharp
var possiblePaths = new[]
{
    Path.Combine(_environment.ContentRootPath, "..", "Letterhead.pdf"),
    Path.Combine(_environment.ContentRootPath, "Letterhead.pdf"),
    Path.Combine(_environment.WebRootPath, "templates", "Letterhead.pdf"),
    // Add more paths here if needed
};
```

### تخصيص موضع الـ Letterhead:

```csharp
// In PageEventHandler.OnEndPage()
canvas.AddTemplate(_letterheadPage, x_offset, y_offset);

// Or for images:
letterhead.SetAbsolutePosition(x, y);
```

## الخلاصة (Summary)

✅ **الوضع الحالي:**

- الملف موجود في: `Soit-Med-Backend/Letterhead.pdf`
- النظام يدعم PDF و PNG
- Logging مفعّل للتشخيص

🎯 **الخطوات التالية:**

1. اختبار تصدير عرض فعلي
2. التحقق من جودة PDF الناتج
3. نشر على بيئة الإنتاج

---

**آخر تحديث:** 2025-11-03  
**الإصدار:** 1.0
