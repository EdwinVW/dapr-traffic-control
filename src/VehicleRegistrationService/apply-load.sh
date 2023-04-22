# This script uses Apache Bench to simulate concurrent load.
# Install with: sudo apt-get install apache2-utils

ab -k -l -d -S -c 10 -n 100 http://localhost:3602/v1.0/invoke/vehicleregistrationservice/method/vehicleinfo/21-KTG-4
