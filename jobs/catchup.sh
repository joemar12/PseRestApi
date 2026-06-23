#!/bin/sh
STATE_FILE=/var/run/sync.last

NOW=$(date +%s)
LAST=$(cat $STATE_FILE 2>/dev/null || echo 0)

# Compute the most recent scheduled run (weekday at 19:00)
# If today is after 19:00, use today; otherwise use yesterday
if [ "$(date +%H)" -ge 19 ] && [ "$(date +%u)" -le 5 ]; then
    SCHEDULE=$(date -d "today 19:00" +%s)
else
    SCHEDULE=$(date -d "yesterday 19:00" +%s)
fi

# If we missed the scheduled run, replay it
if [ $NOW -gt $SCHEDULE ] && [ $LAST -lt $SCHEDULE ]; then
    echo "Missed sync job, running now..."
    docker start sync >> /var/log/cron.log 2>&1
fi

# Update last run timestamp
echo $NOW > $STATE_FILE
