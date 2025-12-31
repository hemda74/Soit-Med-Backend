#!/bin/bash

# ========================================================================
# Script: quick-test.sh
# Description: Quick load test using Apache Bench (ab)
# Usage: ./quick-test.sh [base_url]
# Example: ./quick-test.sh http://localhost:58868
# ========================================================================

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

BASE_URL="${1:-http://localhost:58868}"
CONCURRENT=100
TOTAL_REQUESTS=1000

echo -e "${BLUE}========================================================================${NC}"
echo -e "${BLUE}Quick Performance Test with Apache Bench${NC}"
echo -e "${BLUE}========================================================================${NC}"
echo ""
echo -e "${CYAN}Configuration:${NC}"
echo -e "  Base URL: ${YELLOW}$BASE_URL${NC}"
echo -e "  Concurrent Users: ${YELLOW}$CONCURRENT${NC}"
echo -e "  Total Requests: ${YELLOW}$TOTAL_REQUESTS${NC}"
echo ""

# Check if ab is installed
if ! command -v ab &> /dev/null; then
    echo -e "${RED}Error: Apache Bench (ab) is not installed.${NC}"
    echo -e "${YELLOW}On macOS, it should be pre-installed. Try running: ab -V${NC}"
    exit 1
fi

# Check if server is running
echo -e "${YELLOW}Checking if server is running...${NC}"
if ! curl -s "$BASE_URL/health" > /dev/null 2>&1 && ! curl -s "$BASE_URL" > /dev/null 2>&1; then
    echo -e "${RED}✗ Server is not responding at $BASE_URL${NC}"
    echo -e "${YELLOW}Please start your API server first!${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Server is running${NC}"
echo ""

# Test endpoints
declare -a ENDPOINTS=(
    "/api/Products"
    "/api/ProductCategories"
    "/api/Clients"
    "/api/SalesOffers"
)

echo -e "${BLUE}Starting Tests...${NC}"
echo ""

for endpoint in "${ENDPOINTS[@]}"; do
    echo -e "${CYAN}========================================================================${NC}"
    echo -e "${CYAN}Testing: ${endpoint}${NC}"
    echo -e "${CYAN}========================================================================${NC}"
    
    ab -n $TOTAL_REQUESTS -c $CONCURRENT "${BASE_URL}${endpoint}" 2>&1 | grep -E "(Requests per second|Time per request|Transfer rate|Percentage|Failed)"
    
    echo ""
done

echo -e "${GREEN}========================================================================${NC}"
echo -e "${GREEN}✓ Test Completed${NC}"
echo -e "${GREEN}========================================================================${NC}"
echo ""
echo -e "${YELLOW}Key Metrics:${NC}"
echo -e "  • Requests per second: Higher is better"
echo -e "  • Time per request: Lower is better (avg response time)"
echo -e "  • Failed requests: Should be 0"
echo ""

