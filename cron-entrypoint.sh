#!/bin/sh
echo "Running catch-up check..."
max_retries=5
retry_delay=2
attempt=1

while [ $attempt -le $max_retries ]; do
  if [ -x /usr/local/bin/catchup.sh ]; then
    echo "Found catchup.sh, executing..."
    sh /usr/local/bin/catchup.sh
    break
  else
    echo "Attempt $attempt/$max_retries: catchup.sh not found, retrying in $retry_delay seconds..."
    sleep $retry_delay
    attempt=$((attempt+1))
  fi
done

if [ $attempt -gt $max_retries ]; then
  echo "catchup.sh not found after $max_retries attempts, skipping."
fi

# Then hand over to supercronic for ongoing scheduling
echo "Starting supercronic..."
exec supercronic /etc/crontab