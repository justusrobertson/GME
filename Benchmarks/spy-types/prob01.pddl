(define (problem 01)
	(:domain SPY-TYPES)
	(:objects
		snake boss - character
		elevator gear left right platform - location
		gears - gears
		pp7 rifle - gun
		c4 - explosive
		detonator - detonator
		wire - wire
		phone - phone
		trap - trap
		terminal1 terminal2 terminal3 terminal4 - computer
	)
	(:init    
		(player snake) (at snake elevator) (alive snake)
		(red c4) (detonates detonator c4)
		(has snake pp7) (has snake c4) (has snake detonator)
		(at boss gear) (alive boss)
		(has boss rifle) (has boss wire) (has boss phone)
		(at gears gear)
		(at terminal1 gear) (at terminal2 left) (at terminal3 right) (at terminal4 platform)
		(connected elevator gear)
		(connected gear left) (connected gear right)
		(connected left platform)
		(connected right platform)
	)
	(:goal 
		(and
			(alive snake)
			(not (alive boss))
			(at boss platform)
			(disabled-trap snake)
			(linked phone)
			(destroyed gears)
		)
	)
)