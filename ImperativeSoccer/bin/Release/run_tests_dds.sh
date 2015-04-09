#!/bin/bash 

#check for input arguments 
#arguments accepted are: num_tests to run; query_name; instrumentation(true|false); optional scheduler

if [ "$#" -lt 3 ]; then
echo "sh run_tests.sh <num_tests> <query_name> <strategy>"
exit 1
fi


NUM_TESTS=$1
QUERY_NAME=$2
STRATEGY=$3

echo "will run $NUM_TESTS tests for $QUERY_NAME and strategy $STRATEGY"

for i in `seq 1 $NUM_TESTS`;
do 

echo "RUNNING TEST INSTANCE $i"


echo "starting soccer publisher"
cd ../../../../soccer_publisher_x64/objs/x64Win64VS2012/ 
./soccer_publisher.exe 0 E:\\simulated_data\\full-game 0 > pub_output.txt & 


echo "starting imperative soccer processor"
cd ../../../ImperativeSoccer/ImperativeSoccer/bin/Release/

./ImperativeSoccer.exe 0 $QUERY_NAME $STRATEGY $i

sleep 5s

done 