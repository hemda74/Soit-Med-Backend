#!/bin/bash

# ========================================================================
# Script: run-test-data.sh
# Description: Execute the CREATE_TEST_DATA.sql script to populate test data
# Usage: ./run-test-data.sh
# ========================================================================

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Database connection settings
DB_SERVER="10.10.9.104\\SQLEXPRESS,1433"
DB_NAME="ITIWebApi44"
DB_USER="soitmed"
DB_PASSWORD="356120Ah"

echo -e "${BLUE}========================================================================${NC}"
echo -e "${BLUE}Creating Test Data for Performance Testing${NC}"
echo -e "${BLUE}========================================================================${NC}"

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
SQL_FILE="$SCRIPT_DIR/CREATE_TEST_DATA.sql"

if [ ! -f "$SQL_FILE" ]; then
    echo -e "${RED}Error: SQL file not found at $SQL_FILE${NC}"
    exit 1
fi

echo -e "${YELLOW}Connecting to database...${NC}"
echo -e "Server: $DB_SERVER"
echo -e "Database: $DB_NAME"
echo ""

# Check if sqlcmd is available
if ! command -v sqlcmd &> /dev/null; then
    echo -e "${RED}Error: sqlcmd is not installed.${NC}"
    echo -e "${YELLOW}Please install SQL Server command line tools:${NC}"
    echo -e "Visit: https://learn.microsoft.com/en-us/sql/tools/sqlcmd-utility"
    echo ""
    echo -e "${YELLOW}Alternatively, run this SQL script manually in Azure Data Studio or another SQL client.${NC}"
    exit 1
fi

echo -e "${GREEN}Executing CREATE_TEST_DATA.sql...${NC}"
echo -e "${YELLOW}This may take several minutes...${NC}"
echo ""

# Execute the SQL script
sqlcmd -S "$DB_SERVER" -d "$DB_NAME" -U "$DB_USER" -P "$DB_PASSWORD" -i "$SQL_FILE" -I

if [ $? -eq 0 ]; then
    echo ""
    echo -e "${GREEN}========================================================================${NC}"
    echo -e "${GREEN}✓ Test data created successfully!${NC}"
    echo -e "${GREEN}========================================================================${NC}"
    echo ""
    echo -e "Created:"
    echo -e "  • ${BLUE}10,000${NC} Test Clients"
    echo -e "  • ${BLUE}5,000${NC} Sales Offers"
    echo -e "  • ${BLUE}2,500${NC} Sales Deals"
    echo -e "  • ${BLUE}3,000${NC} Offer Requests"
    echo -e "  • ${BLUE}1,500${NC} Equipment Records"
    echo -e "  • ${BLUE}4,000${NC} Tasks"
    echo -e "  • ${BLUE}2,000${NC} Notifications"
    echo ""
else
    echo ""
    echo -e "${RED}========================================================================${NC}"
    echo -e "${RED}✗ Error occurred while creating test data${NC}"
    echo -e "${RED}========================================================================${NC}"
    echo ""
    exit 1
fi

