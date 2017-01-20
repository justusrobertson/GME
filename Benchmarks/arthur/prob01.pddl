(define (problem 01)
	(:domain ARTHUR)
	(:objects 
		ARTHUR MERLIN
		EXCALIBUR BOOK SPELLBOOK RING
		CLEARING WOODS
	)
	(:init    
		(player arthur) (character arthur) (at arthur woods)
		(character merlin) (at merlin clearing) (has merlin book) (asleep merlin)
		(book book) (is book thing)
		(book spellbook) (at spellbook woods) (is spellbook thing)
		(sword excalibur) (at excalibur clearing) (enchanted excalibur) (is excalibur thing)
		(ring ring) (at ring woods) (is ring thing)
		(location woods) (connected woods clearing)
		(location clearing) (connected clearing woods)
	)
	(:goal 
		(and
			(has arthur excalibur)
		)
	)
)