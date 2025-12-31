# Performance Testing Guide

This guide will help you test your SoitMed application with heavy load to ensure it can handle 10,000+ concurrent users.

## 📋 Prerequisites

1. ✅ Database indexes are installed (ADD_PERFORMANCE_INDEXES.sql)
2. ✅ Caching layer is implemented (in-memory cache)
3. ✅ Connection pool is optimized (Max Pool Size = 300)
4. ✅ Application is running locally

## 🎯 Step 1: Create Test Data

First, populate the database with test data to simulate real load:

### Option A: Using SQL Script Manually

1. Open **Azure Data Studio** or **SQL Server Management Studio**
2. Connect to your database: `10.10.9.104\SQLEXPRESS`
3. Open the file: `backend/SoitMed/Scripts/CREATE_TEST_DATA.sql`
4. Execute the script (this will take 5-10 minutes)
5. Verify the data was created

### Option B: Using Shell Script (if sqlcmd is installed)

```bash
cd /Users/ahmedashraf/Desktop/soitmed/backend/SoitMed/Scripts
./run-test-data.sh
```

**What this creates:**
- 10,000 Test Clients
- 5,000 Sales Offers
- 2,500 Sales Deals
- 3,000 Offer Requests
- 1,500 Equipment Records
- 4,000 Tasks
- 2,000 Notifications

## 🚀 Step 2: Start Your Application

Make sure your SoitMed backend is running:

```bash
cd /Users/ahmedashraf/Desktop/soitmed/backend/SoitMed
dotnet run
```

Wait until you see:
```
Now listening on: http://localhost:58868
```

## 🧪 Step 3: Run Performance Tests

You have **three testing options**:

### Option 1: Quick Test (Recommended - Uses Apache Bench)

Apache Bench comes pre-installed on macOS and is the fastest way to test:

```bash
cd /Users/ahmedashraf/Desktop/soitmed/backend/SoitMed/Scripts
./quick-test.sh http://localhost:58868
```

**What it tests:**
- 1,000 requests per endpoint
- 100 concurrent users
- Tests: Products, Categories, Clients, SalesOffers

**Expected Results (with caching):**
- ✅ Requests per second: **500-2000+** (good performance)
- ✅ Time per request: **50-200ms** average (good)
- ✅ Failed requests: **0** (perfect)

### Option 2: Comprehensive Test (Tests Multiple Endpoints)

```bash
cd /Users/ahmedashraf/Desktop/soitmed/backend/SoitMed/Scripts
./test-performance.sh http://localhost:58868 100 10
```

Parameters:
- `100` = concurrent users
- `10` = requests per user
- Total = 1,000 requests per endpoint

Results will be saved in: `performance-results/summary_[timestamp].txt`

### Option 3: Extreme Load Test (10,000 concurrent users)

To simulate 10,000 users, increase the concurrency:

```bash
./test-performance.sh http://localhost:58868 1000 100
```

This will send **100,000 total requests** with **1,000 concurrent users**.

**Warning:** This is a heavy test. Make sure your Mac has enough resources!

## 📊 Understanding the Results

### Key Metrics to Watch:

1. **Requests per Second (RPS)**
   - ✅ Good: > 500 RPS
   - ⚠️ Acceptable: 200-500 RPS
   - ❌ Poor: < 200 RPS

2. **Average Response Time**
   - ✅ Excellent: < 100ms
   - ✅ Good: 100-500ms
   - ⚠️ Acceptable: 500-1000ms
   - ❌ Poor: > 1000ms

3. **Failed Requests**
   - ✅ Perfect: 0
   - ⚠️ Check logs: > 0

4. **95th Percentile (p95)**
   - This shows the response time for 95% of requests
   - Should be < 1000ms for good user experience

### Example Good Results:

```
Requests per second:    850.32 [#/sec] (mean)
Time per request:       117.615 [ms] (mean)
Time per request:       1.176 [ms] (mean, across all concurrent requests)
Failed requests:        0
```

### Example Bad Results (needs optimization):

```
Requests per second:    45.12 [#/sec] (mean)
Time per request:       2215.615 [ms] (mean)
Failed requests:        342
```

## 🔍 Monitoring During Tests

### 1. Watch Application Logs

Open a second terminal and run:

```bash
cd /Users/ahmedashraf/Desktop/soitmed/backend/SoitMed
dotnet run --verbosity detailed
```

Look for:
- ✅ `Cache hit for key: ...` (caching is working)
- ⚠️ Many database queries (might need more caching)
- ❌ Errors or exceptions

### 2. Monitor System Resources

Open **Activity Monitor** on Mac:
- Watch **CPU usage** (should not be 100% constantly)
- Watch **Memory** (should not swap heavily)
- Watch **dotnet** process

### 3. Check Database Performance

In **Azure Data Studio**, run:

```sql
-- Check active connections
SELECT 
    COUNT(*) as ActiveConnections,
    program_name,
    host_name
FROM sys.dm_exec_sessions
WHERE is_user_process = 1
GROUP BY program_name, host_name;

-- Check slow queries
SELECT TOP 10
    creation_time,
    last_execution_time,
    total_worker_time/execution_count as avg_cpu_time,
    total_elapsed_time/execution_count as avg_elapsed_time,
    execution_count,
    SUBSTRING(text, 1, 200) as query_text
FROM sys.dm_exec_query_stats
CROSS APPLY sys.dm_exec_sql_text(sql_handle)
ORDER BY avg_elapsed_time DESC;
```

## 🎯 Performance Optimization Checklist

### Before Testing:
- [ ] Database indexes are installed
- [ ] Connection pool Max Size = 300
- [ ] In-memory cache is configured
- [ ] Test data is loaded
- [ ] No other heavy applications running

### After Testing:
- [ ] Review response times (< 500ms avg is good)
- [ ] Check for failed requests (should be 0)
- [ ] Monitor CPU and memory usage
- [ ] Check cache hit rates in logs
- [ ] Identify bottlenecks

## 🔧 Troubleshooting

### Issue: High Response Times (> 1000ms)

**Solutions:**
1. Check if indexes are properly created:
   ```sql
   SELECT name, type_desc FROM sys.indexes WHERE name LIKE 'IX_%';
   ```

2. Verify caching is working (look for "Cache hit" in logs)

3. Check database connection pool:
   ```sql
   SELECT * FROM sys.dm_exec_sessions WHERE is_user_process = 1;
   ```

### Issue: Many Failed Requests

**Solutions:**
1. Check application logs for errors
2. Increase connection pool timeout
3. Check database server load
4. Reduce concurrent users in test

### Issue: Memory Issues

**Solutions:**
1. Reduce cache expiration times in `RedisCacheService.cs`
2. Implement cache size limits
3. Add more RAM to your Mac
4. Reduce concurrent test users

## 🚀 Next Steps

### If Tests Pass ✅

Your application is ready to handle heavy load! Next:
1. Commit your changes to git
2. Deploy to staging environment
3. Run production load tests
4. Consider adding Redis for distributed caching
5. Monitor production metrics

### If Tests Fail ❌

Optimize based on results:
1. Add more indexes to slow queries
2. Implement more aggressive caching
3. Optimize slow endpoints
4. Consider database query optimization
5. Profile the application with dotTrace or similar

## 📈 Scaling Beyond 10,000 Users

For even higher load:
1. **Add Redis** (for distributed caching across multiple servers)
2. **Load Balancing** (run multiple API instances)
3. **Database Replication** (read replicas for queries)
4. **CDN** (for static content)
5. **Microservices** (split into smaller services)
6. **Database Sharding** (for very large databases)

---

## 📝 Notes

- Tests run locally may differ from production performance
- Network latency will add to response times in production
- Always test with realistic data volumes
- Monitor production closely after deployment

Good luck with your testing! 🎉

