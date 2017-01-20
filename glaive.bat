@echo off

java -jar %1/Glaive/glaive.jar -d %1/Benchmarks/%2/domain.pddl -p %1/Benchmarks/%2/prob%3.pddl -o %1/Glaive/output/output -tl 1000 -b