#!/bin/bash

# ========================================================================
# Script: test-authenticated.sh
# Description: Performance test with authentication
# Usage: ./test-authenticated.sh [email] [password]
# Example: ./test-authenticated.sh admin@soitmed.com password123
# ========================================================================

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

BASE_URL="http://localhost:5117"
EMAIL="${1:-admin@soitmed.com}"
PASSWORD="${2:-Admin@123}"

echo -e "${BLUE}========================================================================${NC}"
echo -e "${BLUE}Authenticated Performance Test${NC}"
echo -e "${BLUE}========================================================================${NC}"
echo ""
echo -e "${CYAN}Configuration:${NC}"
echo -e "  Base URL: ${YELLOW}$BASE_URL${NC}"
echo -e "  Email: ${YELLOW}$EMAIL${NC}"
echo ""

# Step 1: Login and get token
echo -e "${YELLOW}Step 1: Authenticating...${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/Account/login" \
  -H "Content-Type: application/json" \
  -d "{\"userName\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")

TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*' | sed 's/"token":"//')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "" ]; then
    echo -e "${RED}✗ Authentication failed!${NC}"
    echo -e "${YELLOW}Response: $LOGIN_RESPONSE${NC}"
    echo ""
    echo -e "${YELLOW}Please provide valid credentials:${NC}"
    echo -e "  ./test-authenticated.sh your-email@example.com your-password"
    echo ""
    echo -e "${CYAN}Or test without authentication (public endpoints only):${NC}"
    echo -e "  curl -i $BASE_URL/api/Governorate"
    exit 1
fi

echo -e "${GREEN}✓ Authentication successful${NC}"
echo -e "  Token: ${TOKEN:0:20}...${NC}"
echo ""

# Step 2: Test single request
echo -e "${YELLOW}Step 2: Testing single request...${NC}"
SINGLE_START=$(date +%s%N)
SINGLE_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "Authorization: Bearer $TOKEN" \
  "$BASE_URL/api/Governorate")
SINGLE_END=$(date +%s%N)
SINGLE_DURATION=$(( (SINGLE_END - SINGLE_START) / 1000000 ))

if [ "$SINGLE_RESPONSE" == "200" ]; then
    echo -e "${GREEN}✓ Single request successful (${SINGLE_DURATION}ms)${NC}"
else
    echo -e "${RED}✗ Request failed with status: $SINGLE_RESPONSE${NC}"
    exit 1
fi
echo ""

# Step 3: Concurrent requests test
echo -e "${YELLOW}Step 3: Running concurrent requests test...${NC}"
echo -e "${CYAN}Testing with 50 concurrent users, 10 requests each (500 total)${NC}"
echo ""

TEMP_RESULTS=$(mktemp)
PIDS=()
CONCURRENT_USERS=50
REQUESTS_PER_USER=10

TEST_START=$(date +%s)

for ((i=1; i<=CONCURRENT_USERS; i++)); do
    (
        for ((j=1; j<=REQUESTS_PER_USER; j++)); do
            start_time=$(date +%s%N)
            http_code=$(curl -s -o /dev/null -w "%{http_code}" \
              -H "Authorization: Bearer $TOKEN" \
              "$BASE_URL/api/Governorate" 2>&1)
            end_time=$(date +%s%N)
            duration=$(( (end_time - start_time) / 1000000 ))
            echo "$http_code,$duration" >> "$TEMP_RESULTS"
        done
    ) &
    PIDS+=($!)
    
    # Progress indicator
    if [ $(( i % 10 )) -eq 0 ]; then
        echo -e "  ${CYAN}Started $i users...${NC}"
    fi
done

# Wait for all to complete
echo -e "  ${YELLOW}Waiting for all requests to complete...${NC}"
for pid in "${PIDS[@]}"; do
    wait $pid
done

TEST_END=$(date +%s)
TOTAL_DURATION=$((TEST_END - TEST_START))

echo ""
echo -e "${BLUE}========================================================================${NC}"
echo -e "${BLUE}Test Results${NC}"
echo -e "${BLUE}========================================================================${NC}"

# Calculate statistics
TOTAL_REQUESTS=$(wc -l < "$TEMP_RESULTS" | tr -d ' ')
SUCCESS_COUNT=$(grep -c "^200," "$TEMP_RESULTS" 2>/dev/null || echo 0)
ERROR_COUNT=$(grep -cv "^200," "$TEMP_RESULTS" 2>/dev/null || echo 0)

# Calculate response times
AVG_TIME=$(awk -F',' '{sum+=$2; count++} END {if(count>0) printf "%.2f", sum/count; else print 0}' "$TEMP_RESULTS")
MIN_TIME=$(awk -F',' '{print $2}' "$TEMP_RESULTS" | sort -n | head -1)
MAX_TIME=$(awk -F',' '{print $2}' "$TEMP_RESULTS" | sort -n | tail -1)

# Calculate percentiles
P50=$(awk -F',' '{print $2}' "$TEMP_RESULTS" | sort -n | awk '{a[NR]=$1} END {print a[int(NR*0.5)]}')
P95=$(awk -F',' '{print $2}' "$TEMP_RESULTS" | sort -n | awk '{a[NR]=$1} END {print a[int(NR*0.95)]}')
P99=$(awk -F',' '{print $2}' "$TEMP_RESULTS" | sort -n | awk '{a[NR]=$1} END {print a[int(NR*0.99)]}')

# Success rate
SUCCESS_RATE=$(echo "scale=2; $SUCCESS_COUNT * 100 / $TOTAL_REQUESTS" | bc)

# Requests per second
RPS=$(echo "scale=2; $TOTAL_REQUESTS / $TOTAL_DURATION" | bc)

echo ""
echo -e "${CYAN}Request Statistics:${NC}"
echo -e "  Total Requests: ${YELLOW}$TOTAL_REQUESTS${NC}"
echo -e "  Successful: ${GREEN}$SUCCESS_COUNT${NC} (${SUCCESS_RATE}%)"
echo -e "  Failed: ${RED}$ERROR_COUNT${NC}"
echo -e "  Total Duration: ${YELLOW}${TOTAL_DURATION}s${NC}"
echo -e "  Requests/Second: ${YELLOW}${RPS}${NC}"
echo ""
echo -e "${CYAN}Response Time (milliseconds):${NC}"
echo -e "  Average: ${YELLOW}${AVG_TIME}ms${NC}"
echo -e "  Min: ${YELLOW}${MIN_TIME}ms${NC}"
echo -e "  Max: ${YELLOW}${MAX_TIME}ms${NC}"
echo -e "  50th percentile (median): ${YELLOW}${P50}ms${NC}"
echo -e "  95th percentile: ${YELLOW}${P95}ms${NC}"
echo -e "  99th percentile: ${YELLOW}${P99}ms${NC}"
echo ""

# Performance assessment
echo -e "${CYAN}Performance Assessment:${NC}"
if (( $(echo "$AVG_TIME < 200" | bc -l) )); then
    echo -e "  ${GREEN}✓ Excellent${NC} - Average response time < 200ms"
elif (( $(echo "$AVG_TIME < 500" | bc -l) )); then
    echo -e "  ${GREEN}✓ Good${NC} - Average response time < 500ms"
elif (( $(echo "$AVG_TIME < 1000" | bc -l) )); then
    echo -e "  ${YELLOW}⚠ Acceptable${NC} - Average response time < 1000ms"
else
    echo -e "  ${RED}✗ Needs Optimization${NC} - Average response time > 1000ms"
fi

if [ "$ERROR_COUNT" -eq 0 ]; then
    echo -e "  ${GREEN}✓ Perfect${NC} - No failed requests"
else
    echo -e "  ${RED}✗ Warning${NC} - $ERROR_COUNT requests failed"
fi

if (( $(echo "$RPS > 50" | bc -l) )); then
    echo -e "  ${GREEN}✓ Good Throughput${NC} - ${RPS} requests/second"
else
    echo -e "  ${YELLOW}⚠ Low Throughput${NC} - ${RPS} requests/second"
fi

echo ""
echo -e "${BLUE}========================================================================${NC}"
echo -e "${GREEN}✓ Test Completed${NC}"
echo -e "${BLUE}========================================================================${NC}"
echo ""

# Clean up
rm "$TEMP_RESULTS"

# Recommendations
echo -e "${CYAN}Next Steps:${NC}"
echo -e "  1. Run the test multiple times to get consistent results"
echo -e "  2. Test with more concurrent users: ./test-authenticated.sh"
echo -e "  3. Monitor server logs in the other terminal"
echo -e "  4. Check database performance with the SQL queries in PERFORMANCE_TESTING_GUIDE.md"
echo ""

