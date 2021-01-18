# This script uses Apache Bench to simulate concurrent load.
# Download the Windows zip for apache (from https://www.apachelounge.com/download/)
# and extract ab.exe form the zip file.

ab -k -l -d -S `
-c 5 `
-n 50 `
http://localhost:3500/v1.0/invoke/governmentservice/method/rdw/A6k9D42L061Fx4Rm2K8/vehicle/21-KTG-4