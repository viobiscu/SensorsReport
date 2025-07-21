git status --porcelain | grep "^ M" | while read status file; do
  echo "Checking $file..."
  # Check if the file has only whitespace/line-ending changes
  if git diff --quiet --ignore-all-space --ignore-blank-lines "$file"; then
    echo "  -> Only whitespace changes, reverting..."
    git restore "$file"
  else
    echo "  -> Has actual code changes, keeping..."
  fi
done