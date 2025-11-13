# Revert Arabic Support Changes Guide

## Important Note

The `EnableArabicSupport.sql` script **did NOT change any database settings**. It only verified Unicode support.

However, if you want to revert the **code changes** made in `Program.cs`, follow the steps below.

---

## What Was Changed

### 1. Program.cs - JSON Serialization Configuration

**Added code:**

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enable UTF-8 encoding for JSON responses (supports Arabic and Unicode)
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });
```

### 2. EnableArabicSupport.sql - Verification Only

**No database changes were made** - it only checked:

- Current collation
- Column types (NVARCHAR vs VARCHAR)
- Connection string info

---

## How to Revert

### Step 1: Revert Program.cs Code Changes

Edit `SoitMed/Program.cs` and change:

**FROM:**

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enable UTF-8 encoding for JSON responses (supports Arabic and Unicode)
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });
```

**TO:**

```csharp
builder.Services.AddControllers();
```

### Step 2: Database - No Changes Needed

The `EnableArabicSupport.sql` script did NOT modify the database, so **no SQL revert is needed**.

However, if you manually changed the database collation, you can check using:

```sql
-- Check current collation
SELECT name, collation_name
FROM sys.databases
WHERE name = 'ITIWebApi44';
```

If collation was manually changed to Arabic and you want to revert:

```sql
-- Close all connections first, then:
ALTER DATABASE [ITIWebApi44] COLLATE SQL_Latin1_General_CP1_CI_AS;
```

### Step 3: Restart Application

After reverting Program.cs, restart your API application.

---

## Verification

After reverting:

1. **Check Program.cs** - Should only have `builder.Services.AddControllers();`
2. **Check Database** - Should be unchanged (no revert needed)
3. **Test API** - Should work normally (JSON encoding will use default .NET 8 settings)

---

## Note About Arabic Support

Even after reverting the code changes, your database **still supports Arabic** because:

- ✅ Entity Framework Core uses `NVARCHAR` for all `string` properties (automatic)
- ✅ `.NET 8` uses UTF-8 by default (even without explicit configuration)
- ✅ SQL Server `NVARCHAR` supports Unicode

The explicit JSON encoding configuration in Program.cs was just an **enhancement** to ensure perfect Arabic character handling, but the default .NET 8 settings also support Arabic.

---

**Last Updated:** 2025-01-02


