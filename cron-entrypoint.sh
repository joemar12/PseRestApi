#!/bin/sh
echo "Running catch-up check..."
/usr/local/bin/catchup.sh

# Then hand over to supercronic for ongoing scheduling
echo "Starting supercronic..."
exec supercronic /etc/crontab
