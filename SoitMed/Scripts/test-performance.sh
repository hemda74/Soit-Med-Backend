#!/bin/bash

# ========================================================================
# Script: test-performance.sh
# Description: Load test the SoitMed API with concurrent requests
# Usage: ./test-performance.sh [base_url] [concurrent_users] [requests_per_user]
# Example: ./test-performance.sh http://localhost:58868 100 10
# ========================================================================

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Default parameters
BASE_URL="${1:-http://localhost:58868}"
CONCURRENT_USERS="${2:-100}"
REQUESTS_PER_USER="${3:-10}"
TOTAL_REQUESTS=$((CONCURRENT_USERS * REQUESTS_PER_USER))

# Output files
RESULTS_DIR="performance-results"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
RESULTS_FILE="$RESULTS_DIR/results_${TIMESTAMP}.txt"
SUMMARY_FILE="$RESULTS_DIR/summary_${TIMESTAMP}.txt"

# Create results directory
mkdir -p "$RESULTS_DIR"

echo -e "${BLUE}========================================================================${NC}"
echo -e "${BLUE}SoitMed API Performance Test${NC}"
echo -e "${BLUE}========================================================================${NC}"
echo ""
echo -e "${CYAN}Configuration:${NC}"
echo -e "  Base URL: ${YELLOW}$BASE_URL${NC}"
echo -e "  Concurrent Users: ${YELLOW}$CONCURRENT_USERS${NC}"
echo -e "  Requests per User: ${YELLOW}$REQUESTS_PER_USER${NC}"
echo -e "  Total Requests: ${YELLOW}$TOTAL_REQUESTS${NC}"
echo -e "  Results Directory: ${YELLOW}$RESULTS_DIR${NC}"
echo ""

# Check if server is running
echo -e "${YELLOW}Checking if server is running...${NC}"
if ! curl -s "$BASE_URL/health" > /dev/null 2>&1 && ! curl -s "$BASE_URL" > /dev/null 2>&1; then
    echo -e "${RED}✗ Server is not responding at $BASE_URL${NC}"
    echo -e "${YELLOW}Please start your API server first!${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Server is running${NC}"
echo ""

# API endpoints to test (adjust based on your actual endpoints)
declare -a ENDPOINTS=(
    "/api/Products"
    "/api/ProductCategories"
    "/api/Clients"
    "/api/SalesOffers"
    "/api/Governorates"
    "/api/Departments"
)

# Function to test a single endpoint
test_endpoint() {
    local endpoint=$1
    local url="${BASE_URL}${endpoint}"
    
    echo -e "${CYAN}Testing: ${endpoint}${NC}"
    
    # Temporary file for this endpoint's results
    local temp_results=$(mktemp)
    
    # Run concurrent requests
    local pids=()
    for ((i=1; i<=CONCURRENT_USERS; i++)); do
        (
            for ((j=1; j<=REQUESTS_PER_USER; j++)); do
                start_time=$(date +%s%N)
                response=$(curl -s -o /dev/null -w "%{http_code},%{time_total}" "$url" 2>&1)
                end_time=$(date +%s%N)
                
                http_code=$(echo $response | cut -d',' -f1)
                time_total=$(echo $response | cut -d',' -f2)
                
                # Convert nanoseconds to milliseconds
                duration=$(( (end_time - start_time) / 1000000 ))
                
                echo "$http_code,$time_total,$duration" >> "$temp_results"
            done
        ) &
        pids+=($!)
        
        # Limit concurrent processes to avoid overwhelming the system
        if [ $(( i % 50 )) -eq 0 ]; then
            sleep 0.1
        fi
    done
    
    # Wait for all background jobs to complete
    echo -e "  ${YELLOW}Waiting for $CONCURRENT_USERS users to complete...${NC}"
    for pid in "${pids[@]}"; do
        wait $pid
    done
    
    # Calculate statistics
    local total_requests=$(wc -l < "$temp_results")
    local success_count=$(grep -c "^200," "$temp_results" 2>/dev/null || echo 0)
    local error_count=$(grep -cv "^200," "$temp_results" 2>/dev/null || echo 0)
    
    # Calculate response times (in seconds from curl)
    local avg_time=$(awk -F',' '{sum+=$2; count++} END {if(count>0) print sum/count; else print 0}' "$temp_results")
    local min_time=$(awk -F',' '{print $2}' "$temp_results" | sort -n | head -1)
    local max_time=$(awk -F',' '{print $2}' "$temp_results" | sort -n | tail -1)
    
    # Convert to milliseconds for display
    avg_time_ms=$(echo "$avg_time * 1000" | bc)
    min_time_ms=$(echo "$min_time * 1000" | bc)
    max_time_ms=$(echo "$max_time * 1000" | bc)
    
    # Success rate
    local success_rate=$(echo "scale=2; $success_count * 100 / $total_requests" | bc)
    
    # Output results
    echo -e "  ${GREEN}✓ Completed${NC}"
    echo -e "  Total Requests: ${YELLOW}$total_requests${NC}"
    echo -e "  Successful: ${GREEN}$success_count${NC} ($success_rate%)"
    echo -e "  Failed: ${RED}$error_count${NC}"
    echo -e "  Response Times:"
    echo -e "    Average: ${YELLOW}${avg_time_ms} ms${NC}"
    echo -e "    Min: ${YELLOW}${min_time_ms} ms${NC}"
    echo -e "    Max: ${YELLOW}${max_time_ms} ms${NC}"
    echo ""
    
    # Append to summary file
    {
        echo "==================================================================="
        echo "Endpoint: $endpoint"
        echo "==================================================================="
        echo "Total Requests: $total_requests"
        echo "Successful: $success_count ($success_rate%)"
        echo "Failed: $error_count"
        echo "Response Times:"
        echo "  Average: ${avg_time_ms} ms"
        echo "  Min: ${min_time_ms} ms"
        echo "  Max: ${max_time_ms} ms"
        echo ""
    } >> "$SUMMARY_FILE"
    
    # Clean up
    rm "$temp_results"
}

# Start testing
echo -e "${BLUE}========================================================================${NC}"
echo -e "${BLUE}Starting Load Tests${NC}"
echo -e "${BLUE}========================================================================${NC}"
echo ""

# Initialize summary file
{
    echo "========================================================================="
    echo "SoitMed API Performance Test Results"
    echo "========================================================================="
    echo "Date: $(date)"
    echo "Base URL: $BASE_URL"
    echo "Concurrent Users: $CONCURRENT_USERS"
    echo "Requests per User: $REQUESTS_PER_USER"
    echo "Total Requests: $TOTAL_REQUESTS"
    echo ""
} > "$SUMMARY_FILE"

# Test each endpoint
OVERALL_START=$(date +%s)
for endpoint in "${ENDPOINTS[@]}"; do
    test_endpoint "$endpoint"
done
OVERALL_END=$(date +%s)

TOTAL_DURATION=$((OVERALL_END - OVERALL_START))

# Final summary
echo -e "${BLUE}========================================================================${NC}"
echo -e "${BLUE}Test Completed${NC}"
echo -e "${BLUE}========================================================================${NC}"
echo -e "${GREEN}✓ All endpoint tests completed${NC}"
echo -e "  Total Duration: ${YELLOW}${TOTAL_DURATION} seconds${NC}"
echo -e "  Results saved to: ${YELLOW}$SUMMARY_FILE${NC}"
echo ""

# Add total duration to summary
echo "=========================================================================" >> "$SUMMARY_FILE"
echo "Total Test Duration: ${TOTAL_DURATION} seconds" >> "$SUMMARY_FILE"
echo "=========================================================================" >> "$SUMMARY_FILE"

# Display summary
cat "$SUMMARY_FILE"

echo ""
echo -e "${CYAN}Tip: You can view detailed results with:${NC}"
echo -e "  cat $SUMMARY_FILE"
echo ""

