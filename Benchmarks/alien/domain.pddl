;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;; ALIEN world
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

(define 
	(domain ALIEN)
	(:requirements :strips :universal-preconditions)
	(:predicates 
		(character ?x)
		(at ?x ?y)
	    (has ?x ?y)
	    (object ?x)
	    (location ?x)
		(color ?x ?y)
	)
	
	(:action move-location
	    :parameters (?mover ?location ?oldlocation)
	    :precondition 
			(and 
				(character ?mover) (location ?location) (location ?oldlocation)
				(at ?mover ?oldlocation) (not (at ?mover ?location)) (alive ?mover) (not (sitting ?mover))
				(connected ?oldlocation ?location) (not (locked ?oldlocation ?location))
			)
	    :effect
			(and 
				(not (at ?mover ?oldlocation))
				(at ?mover ?location)
				(forall (?x)
					(when 
						(and
							(embarked shuttle)
							(stalking ?x)
							(at ?mover shuttle)
							(human ?mover)
						)
						(and
							(not (at pipe shuttle))
							(at ?x shuttle)
						)
					)
				)
			)
	)
	
	(:action stalk-prey
	    :parameters (?alien)
	    :precondition 
			(and 
				(alien ?alien) (at ?alien shuttle)
			)
	    :effect
			(and 
				(not (at ?alien shuttle))
				(stalking ?alien)
				(at pipe shuttle)
			)
	)
	
	(:action take-cat
	    :parameters (?taker ?cat ?location)
	    :precondition 
			(and 
				(character ?taker) (alive ?taker) (at ?taker ?location)
				(cat ?cat) (at ?cat ?location)
				(location ?location)
			)
	    :effect
			(and 
				(not (at ?cat ?location))
				(has ?taker ?cat)
			)
	)
	
	(:action take-suit
	    :parameters (?taker ?suit ?location)
	    :precondition 
			(and 
				(character ?taker) (alive ?taker) (at ?taker ?location)
				(spacesuit ?suit) (at ?suit ?location)
				(location ?location)
			)
	    :effect
			(and 
				(not (at ?suit ?location))
				(has ?taker ?suit)
			)
	)
	
	(:action wear-suit
	    :parameters (?wearer ?suit ?location)
	    :precondition 
			(and 
				(character ?wearer) (alive ?wearer) (not (wearing ?wearer ?suit)) (has ?wearer ?suit)
				(spacesuit ?suit)
				(location ?location) (at ?wearer ?location)
			)
	    :effect
			(and 
				(not (has ?wearer ?suit))
				(wearing ?wearer ?suit)
				(suited ?wearer)
			)
	)
	
	(:action take-gun
	    :parameters (?taker ?gun ?location)
	    :precondition 
			(and 
				(character ?taker) (alive ?taker) (at ?taker ?location)
				(gun ?gun) (at ?gun ?location)
				(location ?location)
			)
	    :effect
			(and 
				(not (at ?gun ?location))
				(has ?taker ?gun)
			)
	)
	
	(:action shoot-alien
	    :parameters (?shooter ?alien ?gun ?location)
	    :precondition 
			(and 
				(character ?shooter) (alive ?shooter) (at ?shooter ?location) (has ?shooter ?gun)
				(gun ?gun) (loaded ?gun)
				(alien ?alien) (at ?alien ?location)
				(location ?location)
			)
	    :effect
			(and 
				(injured ?alien)
				(angry ?alien)
				(not (loaded ?gun))
				(tethered ?shooter ?alien)
			)
	)
	
	(:action drop-gun
	    :parameters (?dropper ?gun ?location)
	    :precondition 
			(and 
				(character ?dropper) (alive ?dropper) (at ?dropper ?location) (has ?dropper ?gun)
				(gun ?gun)
				(location ?location)
			)
	    :effect
			(and 
				(not (has ?dropper ?gun))
				(at ?gun ?location)
				(forall (?x)
					(when 
						(and
							(tethered ?dropper ?x)
						)
						(not (tethered ?dropper ?x))
					)
				)
			)
	)
	
	(:action put-cat-container
	    :parameters (?putter ?cat ?container ?location)
	    :precondition 
			(and 
				(character ?putter) (alive ?putter)
				(cat ?cat) (has ?putter ?cat)
				(sleepchamber ?container) (not (full ?container))
				(location ?location) (at ?putter ?location) (at ?container ?location)
			)
	    :effect
			(and 
				(not (has ?putter ?cat))
				(in ?cat ?container)
				(full ?container)
				(forall (?x)
					(when 
						(and
							(embarked shuttle)
							(stalking ?x)
						)
						(and
							(not (at pipe shuttle))
							(at ?x shuttle)
						)
					)
				)
			)
	)
	
	(:action open-door
	    :parameters (?user ?door ?location1 ?location2)
	    :precondition 
			(and 
				(character ?user) (alive ?user) (at ?user ?location1) (human ?user)
				(location ?location1) (location ?location2)
				(door ?door) (between ?door ?location1 ?location2) (closed ?door)
			)
	    :effect
			(and 
				(open ?door)
				(not (closed ?door))
				(not (locked ?location1 ?location2))
				(not (locked ?location2 ?location1))
				(forall (?x)
					(when 
						(and
							(at ?x ?location1)
							(embarked ?location1)
							(not (strapped ?x))
							(alive ?x)
						)
						(and
							(at ?x space)
							(not (at ?x ?location1))
							(not (alive ?x))
						)
					)
				)
				(forall (?x)
					(when 
						(and
							(at ?x ?location1)
							(embarked ?location1)
							(strapped ?x)
							(not (suited ?x))
						)
						(not (alive ?x))
					)
				)
				(forall (?x ?y)
					(when 
						(and
							(at ?x ?location1)
							(embarked ?location1)
							(has ?x ?y)
							(alive ?y)
						)
						(and
							(not (alive ?y))
							(not (at ?y ?location1))
							(at ?y space)
						)
					)
				)
				(forall (?x ?y)
					(when 
						(and
							(at ?x ?location1)
							(embarked ?location1)
							(not (strapped ?x))
							(alive ?x)
							(tethered ?y ?x)
						)
						(and
							(at ?y space)
							(not (alive ?y))
						)
					)
				)
			)
	)
	
	(:action close-door
	    :parameters (?user ?door ?location1 ?location2)
	    :precondition 
			(and 
				(character ?user) (alive ?user) (at ?user ?location1) (human ?user)
				(location ?location1) (location ?location2)
				(door ?door) (between ?door ?location1 ?location2) (open ?door)
			)
	    :effect
			(and 
				(closed ?door)
				(not (open ?door))
				(locked ?location1 ?location2)
				(locked ?location2 ?location1)
			)
	)
	
	(:action launch-ship
	    :parameters (?user ?ship ?panel ?location)
	    :precondition 
			(and 
				(character ?user) (alive ?user) (at ?user ?location) (human ?user)
				(ship ?ship) (connected ?ship ?location)
				(controlpanel ?panel) (at ?panel ?location) (not (embarked ?ship))
			)
	    :effect
			(and 
				(embarked shuttle)
				(not (between door shuttle autodock))
				(not (between door autodock shuttle))
				(between door shuttle space)
				(between door space shuttle)
				(not (connected shuttle autodock))
				(not (connected autodock shuttle))
				(forall (?x)
					(when 
						(and
							(open door)
							(at ?x ?location)
							(not (strapped ?x))
							(alive ?x)
						)
						(and
							(at ?x space)
							(not (alive ?x))
						)
					)
				)
				(forall (?x)
					(when 
						(and
							(open door)
							(at ?x ?location)
							(not (strapped ?x))
							(not (alive ?x))
						)
						(at ?x space)
					)
				)
				(forall (?x)
					(when 
						(and
							(at ?x ?location)
							(open door)
							(strapped ?x)
							(not (suited ?x))
						)
						(not (alive ?x))
					)
				)
			)
	)
	
	(:action sit-chair
	    :parameters (?user ?chair ?location)
	    :precondition 
			(and 
				(character ?user) (alive ?user) (at ?user ?location) (human ?user)
				(chair ?chair) (not (used ?chair)) (at ?chair ?location)
			)
	    :effect
			(and 
				(sitting ?user)
				(used ?chair)
				(sitting-in ?user ?chair)
				(strapped ?user)
			)
	)
	
	(:action stand-chair
	    :parameters (?user ?chair)
	    :precondition 
			(and 
				(character ?user) (alive ?user) (sitting-in ?user ?chair)
			)
	    :effect
			(and 
				(not (sitting ?user))
				(not (used ?chair))
				(not (sitting-in ?user ?chair))
				(not (strapped ?user))
				(forall (?x)
					(when 
						(and
							(embarked shuttle)
							(open ?x)
							(door ?x)
						)
						(and
							(at ?user space)
							(not (at ?user shuttle))
							(not (alive ?user))
						)
					)
				)
			)
	)
)
