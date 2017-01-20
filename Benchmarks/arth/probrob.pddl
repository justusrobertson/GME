(define (problem rob)
(:domain arth)
(:objects spellbook merlinbook - book
 arthur merlin - character
 clearing woods - location
 ring - ring
 thing - thing
 excalibur - sword
)
(:init (at merlin clearing)
 (at arthur woods)
 (at excalibur clearing)
 (player arthur)
 (has arthur ring)
 (at spellbook woods)
 (connected clearing woods)
 (asleep merlin)
 (enchanted excalibur)
 (has merlin merlinbook)
 (connected woods clearing)
 (intends arthur (has arthur excalibur))
 (intends merlin (has arthur excalibur))
)
(:goal (has arthur excalibur)
))