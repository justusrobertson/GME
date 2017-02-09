(define (problem 01)
	(:domain CEDEL-TEST)
	(:objects 
		ARTHUR MERLIN
		BOOK RING
		CHAMBER
	)
	(:init    
		(player arthur) (character arthur) (at arthur chamber)
		(character merlin) (at merlin chamber)
		(book book) (thing book) (at book chamber)
		(ring ring) (thing ring) (at ring chamber)
	)
	(:goal 
		(and
			(magical merlin)
			(not (magical arthur))
		)
	)
)