#!/bin/bash
cd ~/latest-backend/fubaza/API/Fubaza.API/
source venv/bin/activate
python Scripts/remove_bg.py "$@"
