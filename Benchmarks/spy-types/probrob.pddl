(define (problem rob)
(:domain SPY-TYPES)
(:objects c4 - explosive
 pp7 rifle - gun
 elevator gear left right platform - location
 phone - phone
 snake boss - character
 gears - gears
 wire - wire
 detonator - detonator
 terminal1 terminal2 terminal3 terminal4 - computer
 trap - trap
)
(:init (at terminal1 gear)
 (connected left platform)
 (at terminal4 platform)
 (alive boss)
 (connected gear right)
 (connected gear left)
 (at terminal2 left)
 (detonates detonator c4)
 (at snake elevator)
 (connected elevator gear)
 (connected right platform)
 (has snake detonator)
 (has boss phone)
 (at boss right)
 (has boss rifle)
 (has boss wire)
 (green c4)
 (player snake)
 (has snake pp7)
 (has snake c4)
 (at terminal3 right)
 (at gears gear)
 (alive snake)
)
(:goal (AND (alive snake)
 (not (alive boss))
 (at boss platform)
 (disabled-trap snake)
 (linked phone)
 (destroyed gears)
)))