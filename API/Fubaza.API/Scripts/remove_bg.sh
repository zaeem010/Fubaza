#!/bin/bash
set -e

INPUT_IMAGE="$1"
OUTPUT_IMAGE="$2"

PROJECT_DIR="/home/devops/latest-backend/fubaza-backend/API/Fubaza.API"
SCRIPT_DIR="$PROJECT_DIR/Scripts"
PYTHON_SCRIPT="$SCRIPT_DIR/remove_bg.py"

cd "$PROJECT_DIR" || exit 1
source venv/bin/activate

if [ ! -f "$PYTHON_SCRIPT" ]; then
  echo "Python script not found: $PYTHON_SCRIPT" >&2
  exit 1
fi

python "$PYTHON_SCRIPT" "$INPUT_IMAGE" "$OUTPUT_IMAGE"

