#!/bin/bash 

#check for input arguments 
#arguments accepted are: num_tests to run; query_name; strategy(singleThread|perPlayerThread); optional scheduler

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

./ImperativeSoccer.exe 0 $QUERY_NAME $STRATEGY $i  

sleep 5s

done 