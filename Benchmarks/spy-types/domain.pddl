;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; SPY world
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(define 
	(domain SPY-TYPES)
	(:requirements :typing :adl :universal-preconditions)
	(:types character location - stat
			gears computer - object
			trap gun explosive detonator wire phone - item)
	(:constants )
	(:predicates (player ?character - character)
				 (at ?character - character ?location - location)
				 (at ?object - object ?location - location)
				 (at ?item - item ?location - location)
				 (has ?character - character ?item - item)
				 (red ?explosive - explosive)
				 (green ?explosive - explosive)
				 (connected ?location1 - location ?location2 - location)
				 (alive ?character - character)
				 (disabled-trap ?character - character)
				 (destroyed ?object - object)
				 (destroyed ?item - item)
				 (linked ?phone - phone)
				 (used ?computer - computer ?user - character)
				 (on ?explosive - explosive ?object - object))
	
	(:action move-location
	    :parameters (?mover - character ?to - location ?from - location)
	    :precondition 
			(and 
				(at ?mover ?from) (alive ?mover) (connected ?from ?to)
			)
	    :effect
			(and 
				(not (at ?mover ?from)) 
				(at ?mover ?to)
			)
	)
	
	(:action shoot-character
	    :parameters (?shooter - character ?shot - character ?gun - gun ?location - location)
	    :precondition 
			(and 
				(has ?shooter ?gun) (at ?shooter ?location) (at ?shot ?location) 
				(alive ?shooter) (alive ?shot) (not (has ?shot ?gun))
			)
	    :effect
			(and 
				(not (alive ?shot))
			)
	)
	
	(:action make-trap-wire
	    :parameters (?maker - character ?gun - gun ?wire - wire)
	    :precondition 
			(and 
				(alive ?maker) (has ?maker ?gun) (has ?maker ?wire)
			)
	    :effect
			(and 
				(not (has ?maker ?gun))
				(not (has ?maker ?wire))
				(has ?maker trap)
			)
	)
	
	(:action set-trap
	    :parameters (?setter - character ?trap - trap ?location - location)
	    :precondition 
			(and 
				(alive ?setter) (at ?setter ?location) (has ?setter ?trap)
				(not (at ?setter gear)) (not (at ?setter platform))
			)
	    :effect
			(and 
				(not (has ?setter ?trap))
				(at ?trap ?location)
			)
	)
	
	(:action disable-trap
	    :parameters (?disabler - character ?trap - trap ?location - location)
	    :precondition 
			(and 
				(alive ?disabler)
				(at ?disabler ?location)
				(at ?trap ?location)
			)
	    :effect
			(and 
				(not (at ?trap ?location))
				(disabled-trap ?disabler)
			)
	)
	
	(:action use-computer
	    :parameters (?user - character ?computer - computer ?location - location)
	    :precondition 
			(and 
				(alive ?user) (at ?user ?location)
				(at ?computer ?location)
			)
	    :effect
			(and 
				(used ?computer ?user)
			)
	)
	
	(:action link-phone
	    :parameters (?user - character ?phone - phone ?computer - computer ?location - location)
	    :precondition 
			(and 
				(alive ?user) (at ?user ?location) (at ?computer ?location)
				(has ?user ?phone) (used ?computer ?user)
			)
	    :effect
			(and 
				(linked ?phone)
			)
	)
	
	(:action place-explosive-thing
	    :parameters (?actor - character ?bomb - explosive ?thing - object ?location - location)
	    :precondition 
			(and 
				(alive ?actor)
				(at ?actor ?location)
				(has ?actor ?bomb)
				(at ?thing ?location)
			)
	    :effect
			(and 
				(not (has ?actor ?bomb))
				(on ?bomb ?thing)
			)
	)
	
	(:action detonate-explosive
	    :parameters (?actor - character ?bomb - explosive ?detonator - detonator ?thing - object)
	    :precondition 
			(and 
				(alive ?actor)
				(on ?bomb ?thing)
				(detonates ?detonator ?bomb) (has ?actor ?detonator)
			)
	    :effect
			(and 
				(when
					(red ?bomb)
					(and
						(destroyed ?thing)
						(not (on ?bomb ?thing))
					)
				)
				(when
					(green ?bomb)
					(and
						(destroyed ?thing)
						(not (on ?bomb ?thing))
					)
				)
			)
	)
	
	(:action toggle-green
	    :parameters (?toggler - character ?bomb - explosive)
	    :precondition 
			(and 
				(alive ?toggler)
				(has ?toggler ?bomb)
				(red ?bomb)
			)
	    :effect
			(and 
				(not (red ?bomb))
				(green ?bomb)
			)
	)
	
	(:action toggle-red
	    :parameters (?toggler - character ?bomb - explosive)
	    :precondition 
			(and 
				(alive ?toggler)
				(has ?toggler ?bomb)
				(green ?bomb)
			)
	    :effect
			(and 
				(not (green ?bomb))
				(red ?bomb)
			)
	)
	
	(:action take-explosive
	    :parameters (?taker - character ?bomb - explosive ?detonator - detonator ?thing - object ?location - location)
	    :precondition 
			(and 
				(alive ?taker)
				(on ?bomb ?thing)
				(at ?taker ?location) (at ?thing ?location)
				(has ?taker ?detonator) (detonates ?detonator ?bomb)
			)
	    :effect
			(and 
				(not (on ?bomb ?thing))
				(has ?taker ?bomb)
			)
	)
)