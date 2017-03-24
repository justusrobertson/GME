(define
	(domain batman)
	(:requirements :adl :typing :universal-preconditions)
	(:types 
		character location - thing
	)
	(:constants )
	(:predicates
		(player ?character - character)
		(at ?thing - character ?location - location)
		(has ?character - character ?character2 - character)
		(connected ?to - location ?from - location)
		(apprehended ?character - character)
		(alive ?character - character)
		(saved ?saved - character ?saver - character)
		(knows ?character - character)
		(captured ?character - character)
		(henchman ?henchman - character)
	)

	(:action move-location
		:parameters (?character - character ?to - location ?from - location )
		:precondition
			(and
				(player ?character)
				(connected ?to ?from)
				(at ?character ?from)
				(knows ?character)
			)
		:effect
			(and
				(not (at ?character ?from))
				(at ?character ?to)
			)
	)

	(:action interrogate-character
		:parameters (?interrogator - character ?interrogated - character ?location - location )
		:precondition
			(and
				(player ?interrogator)
				(at ?interrogator ?location)
				(at ?interrogated ?location)
				(apprehended ?interrogated)
			)
		:effect
				(knows ?interrogator)
	)

	(:action place-victim
		:parameters (?placer - character ?victim - character ?location - location ?placed - location )
		:precondition
			(and
				(henchman ?placer)
				(has ?placer ?victim)
				(at ?placer ?location)
				(connected ?placed ?location)
			)
		:effect
			(and
				(not (has ?placer ?victim))
				(at ?victim ?placed)
				(captured ?victim)
			)
	)

	(:action save-character
		:parameters (?saver - character ?saved - character ?notSaved - character ?location - location )
		:precondition
			(and
				(player ?saver)
				(at ?saver ?location)
				(at ?saved ?location)
				(captured ?saved)
				(captured ?notSaved)
				(not (at ?notSaved ?location))
			)
		:effect
			(and
				(saved ?saved ?saver)
				(not (alive ?notSaved))
			)
	)
)
